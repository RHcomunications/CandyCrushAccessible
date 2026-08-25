using System;
using System.Collections.Generic;
using System.IO;

namespace CandyCrushAccessible.Audio
{
    public static class ContentResolver
    {
        public static string SoundsDir;
        public static string SoundsLegacyDir;
        public static string MusicDir;

        public static void Initialize()
        {
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Priorizamos sounds_legacy donde residen los audios completos del juego
            string[] legacyCandidates = {
                Path.Combine(exeDir, "sounds_legacy"),
                Path.Combine(exeDir, "..", "sounds_legacy"),
                Path.Combine(exeDir, "..", "..", "sounds_legacy"),
                Path.Combine(exeDir, "..", "..", "..", "sounds_legacy"),
                Path.Combine(Directory.GetCurrentDirectory(), "sounds_legacy")
            };
            foreach (string c in legacyCandidates)
            {
                if (Directory.Exists(c))
                {
                    SoundsLegacyDir = Path.GetFullPath(c);
                    SoundsDir = SoundsLegacyDir;
                    MusicDir = SoundsLegacyDir;
                    break;
                }
            }

            if (string.IsNullOrEmpty(SoundsLegacyDir))
            {
                ExtractEmbeddedAssets();
            }
        }

        private static void ExtractEmbeddedAssets()
        {
            try
            {
                string assetRoot = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "CandyCrushAccessible", "assets");
                System.Reflection.Assembly asm = typeof(ContentResolver).Assembly;
                string[] names = asm.GetManifestResourceNames();
                foreach (string name in names)
                {
                    string marker = ".sounds.";
                    if (!name.Contains(marker)) continue;
                    string rel = name.Substring(name.IndexOf(marker) + marker.Length);
                    string dir = Path.Combine(assetRoot, "sounds_legacy");
                    Directory.CreateDirectory(dir);
                    string dest = Path.Combine(dir, rel);
                    if (File.Exists(dest)) continue;
                    using (Stream s = asm.GetManifestResourceStream(name))
                    {
                        if (s == null) continue;
                        using (FileStream fs = File.Create(dest))
                        {
                            s.CopyTo(fs);
                        }
                    }
                }
                string sd = Path.Combine(assetRoot, "sounds_legacy");
                if (Directory.Exists(sd))
                {
                    SoundsLegacyDir = sd;
                    SoundsDir = sd;
                    MusicDir = sd;
                }
            }
            catch
            {
            }
        }

        public static string SoundPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;

            string[] extensions = { "", ".wav", ".ogg", ".mp3" };
            string baseWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string[] variations = {
                fileName,
                baseWithoutExt,
                baseWithoutExt.Replace("-", "_"),
                baseWithoutExt.Replace("_", "-"),
                baseWithoutExt.Replace(" ", "_"),
                baseWithoutExt.Replace(" ", "-"),
                baseWithoutExt.ToLowerInvariant().Replace(" ", "_"),
                baseWithoutExt.ToLowerInvariant().Replace(" ", "-")
            };

            foreach (string v in variations)
            {
                foreach (string ext in extensions)
                {
                    string candidate = ext.Length > 0 ? v + ext : v;
                    if (!string.IsNullOrEmpty(SoundsLegacyDir))
                    {
                        string p = Path.Combine(SoundsLegacyDir, candidate);
                        if (File.Exists(p)) return p;
                    }
                }
            }

            return null;
        }

        public static string MusicPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;

            string[] extensions = { "", ".ogg", ".mp3", ".wav" };
            string baseWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            string[] variations = {
                fileName,
                baseWithoutExt,
                baseWithoutExt.Replace("-", "_"),
                baseWithoutExt.Replace("_", "-"),
                baseWithoutExt.Replace(" ", "_"),
                baseWithoutExt.Replace(" ", "-"),
                baseWithoutExt.ToLowerInvariant().Replace(" ", "_"),
                baseWithoutExt.ToLowerInvariant().Replace(" ", "-")
            };

            foreach (string v in variations)
            {
                foreach (string ext in extensions)
                {
                    string candidate = ext.Length > 0 ? v + ext : v;
                    if (!string.IsNullOrEmpty(SoundsLegacyDir))
                    {
                        string p = Path.Combine(SoundsLegacyDir, candidate);
                        if (File.Exists(p)) return p;
                    }
                }
            }

            return null;
        }
    }
}