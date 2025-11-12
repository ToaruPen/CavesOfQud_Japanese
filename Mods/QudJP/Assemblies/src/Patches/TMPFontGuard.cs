using TMPro;
using UnityEngine;

namespace QudJP.Patches
{
    internal static class TMPFontGuard
    {
        public static void ApplyToHierarchy(Component? root, bool forceReplace = false, bool includeInactive = true)
        {
            if (root == null)
            {
                return;
            }

            ApplyToHierarchy(root.gameObject, forceReplace, includeInactive);
        }

        public static void ApplyToHierarchy(GameObject? root, bool forceReplace = false, bool includeInactive = true)
        {
            if (root == null)
            {
                return;
            }

            var tmps = root.GetComponentsInChildren<TMP_Text>(includeInactive);
            if (tmps == null || tmps.Length == 0)
            {
                return;
            }

            foreach (var tmp in tmps)
            {
                FontManager.Instance.ApplyToText(tmp, forceReplace);
            }
        }
    }
}
