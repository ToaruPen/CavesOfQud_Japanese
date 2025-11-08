using System;
using System.IO;

namespace QudJP
{
    public static class ModPathResolver
    {
        public static string ResolveModPath()
        {
            var dir = Path.GetDirectoryName(typeof(ModPathResolver).Assembly.Location);
            if (dir == null)
            {
                throw new InvalidOperationException("Mod assembly location could not be determined.");
            }

            // Assemblies フォルダの一段上が Mod ルート
            return Path.GetFullPath(Path.Combine(dir, ".."));
        }
    }
}
