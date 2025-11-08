using System;
using System.IO;

namespace QudJP
{
    public static class ModPathResolver
    {
        private const string ModFolderName = "QudJP";

        public static string ResolveModPath()
        {
            var assemblyDir = Path.GetDirectoryName(typeof(ModPathResolver).Assembly.Location);
            if (assemblyDir == null)
            {
                throw new InvalidOperationException("Mod assembly location could not be determined.");
            }

            // Development: Mods/QudJP/Assemblies/bin/... -> go up one level to reach mod root
            var candidate = Path.GetFullPath(Path.Combine(assemblyDir, ".."));
            if (Directory.Exists(Path.Combine(candidate, "Docs")))
            {
                return candidate;
            }

            // Player install: %LocalLow%/Freehold Games/CavesOfQud/ModAssemblies/... -> find Mods/QudJP
            var parent = Directory.GetParent(assemblyDir);
            if (parent != null)
            {
                var playerModsPath = Path.Combine(parent.FullName, "Mods", ModFolderName);
                if (Directory.Exists(Path.Combine(playerModsPath, "Docs")))
                {
                    return playerModsPath;
                }
            }

            // Fallback to the original candidate if Docs was not found
            return candidate;
        }
    }
}
