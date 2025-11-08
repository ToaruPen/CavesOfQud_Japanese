using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using HarmonyLib;
using UnityEngine;

namespace QudJP
{
    [HarmonyPatch(typeof(XRL.Help.XRLManual))]
    internal static class ManualLocalizationPatch
    {
        private static readonly Lazy<IReadOnlyDictionary<string, string>> LocalizedTopics = new(LoadLocalizedTopics);
        private static readonly Func<object, IDictionary?> TopicsAccessor = BuildTopicsAccessor();
        private static readonly Dictionary<Type, List<MemberInfo>> TopicBodyMemberCache = new();

        [HarmonyPatch(MethodType.Constructor, typeof(ConsoleLib.Console.TextConsole))]
        private static void Postfix(object __instance)
        {
            var topics = TopicsAccessor(__instance);
            if (topics == null || topics.Count == 0)
            {
                Debug.LogWarning("[QudJP] Manual topics dictionary not found; localization skipped.");
                return;
            }

            var localized = LocalizedTopics.Value;
            if (localized.Count == 0)
            {
                return;
            }

            var updated = 0;
            foreach (DictionaryEntry entry in topics)
            {
                if (entry.Key is not string key)
                {
                    continue;
                }

                if (!localized.TryGetValue(key, out var body))
                {
                    continue;
                }

                if (TrySetTopicBody(entry.Value, body))
                {
                    updated++;
                }
            }

            Debug.Log($"[QudJP] Manual topics localized: {updated}/{localized.Count}");
        }

        private static Func<object, IDictionary?> BuildTopicsAccessor()
        {
            var manualType = typeof(XRL.Help.XRLManual);

            var field = manualType
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(f => IsTopicDictionaryType(f.FieldType));
            if (field != null)
            {
                return instance => field.GetValue(instance) as IDictionary;
            }

            var property = manualType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(p => p.CanRead && IsTopicDictionaryType(p.PropertyType));
            if (property != null)
            {
                return instance => property.GetValue(instance, null) as IDictionary;
            }

            return _ => null;
        }

        private static bool IsTopicDictionaryType(Type type)
        {
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return true;
            }

            if (!type.IsGenericType)
            {
                return false;
            }

            var definition = type.GetGenericTypeDefinition();
            if (definition != typeof(Dictionary<,>))
            {
                return false;
            }

            var args = type.GetGenericArguments();
            if (args.Length != 2)
            {
                return false;
            }

            if (args[0] != typeof(string))
            {
                return false;
            }

            var valueTypeName = args[1].FullName ?? string.Empty;
            return valueTypeName.IndexOf("Manual", StringComparison.OrdinalIgnoreCase) >= 0
                || valueTypeName.IndexOf("Topic", StringComparison.OrdinalIgnoreCase) >= 0
                || valueTypeName.IndexOf("Page", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool TrySetTopicBody(object topicInstance, string body)
        {
            if (topicInstance == null)
            {
                return false;
            }

            var type = topicInstance.GetType();
            if (!TopicBodyMemberCache.TryGetValue(type, out var members))
            {
                members = ResolveBodyMembers(type);
                TopicBodyMemberCache[type] = members;
            }

            if (members.Count == 0)
            {
                Debug.LogWarning($"[QudJP] No writable string members found for manual topic type '{type.FullName}'.");
                return false;
            }

            var updated = false;
            foreach (var member in members)
            {
                switch (member)
                {
                    case FieldInfo field:
                        field.SetValue(topicInstance, body);
                        updated = true;
                        break;
                    case PropertyInfo property:
                        property.SetValue(topicInstance, body, null);
                        updated = true;
                        break;
                }
            }

            return updated;
        }

        private static List<MemberInfo> ResolveBodyMembers(Type type)
        {
            var members = new List<MemberInfo>();

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!property.CanWrite || property.PropertyType != typeof(string))
                {
                    continue;
                }

                if (IsLikelyBodyMember(property.Name))
                {
                    members.Add(property);
                }
            }

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.FieldType != typeof(string))
                {
                    continue;
                }

                if (IsLikelyBodyMember(field.Name))
                {
                    members.Add(field);
                }
            }

            if (members.Count == 0)
            {
                var fallbackField = type
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(field => field.FieldType == typeof(string) && !IsNameMember(field.Name));
                if (fallbackField != null)
                {
                    members.Add(fallbackField);
                }

                var fallbackProperty = type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(property => property.CanWrite && property.PropertyType == typeof(string) && !IsNameMember(property.Name));
                if (fallbackProperty != null)
                {
                    members.Add(fallbackProperty);
                }
            }

            return members;
        }

        private static bool IsLikelyBodyMember(string name)
        {
            var lowered = name.ToLowerInvariant();
            if (IsNameMember(lowered))
            {
                return false;
            }

            return lowered.Contains("text") || lowered.Contains("body") || lowered.Contains("content") || lowered.Contains("description");
        }

        private static bool IsNameMember(string name)
        {
            var lowered = name.ToLowerInvariant();
            return lowered.Contains("name") || lowered.Contains("title") || lowered.Contains("category") || lowered.Contains("id");
        }

        private static IReadOnlyDictionary<string, string> LoadLocalizedTopics()
        {
            try
            {
                var manualPath = Path.Combine(ModPathResolver.ResolveModPath(), "Docs", "ManualPatch.jp.xml");
                if (!File.Exists(manualPath))
                {
                    Debug.LogWarning($"[QudJP] Manual localization file not found: {manualPath}");
                    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                var settings = new XmlReaderSettings
                {
                    CheckCharacters = false,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true
                };

                using var stream = File.OpenRead(manualPath);
                using var reader = XmlReader.Create(stream, settings);
                var document = new XmlDocument { PreserveWhitespace = true };
                document.Load(reader);
                var root = document.DocumentElement;
                if (root == null)
                {
                    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                var topics = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node is not XmlElement topicElement || !string.Equals(topicElement.Name, "topic", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var name = topicElement.GetAttribute("name");
                    if (string.IsNullOrEmpty(name))
                    {
                        name = topicElement.GetAttribute("Name");
                    }

                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    var body = NormalizeTopicBody(topicElement);
                    topics[name] = body;
                }

                return topics;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[QudJP] Failed to load Manual.jp.xml: {ex}");
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static string NormalizeTopicBody(XmlElement topicElement)
        {
            var sb = new StringBuilder();
            foreach (XmlNode node in topicElement.ChildNodes)
            {
                sb.Append(node.OuterXml);
            }

            var text = sb.ToString();
            text = text.Replace("\r\n", "\n");
            text = text.Trim('\n');

            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].TrimEnd('\r');
                lines[i] = lines[i].TrimStart('\t');
            }

            return string.Join("\n", lines);
        }
    }
}
