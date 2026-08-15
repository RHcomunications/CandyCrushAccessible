using System;
using System.Collections.Generic;
using System.IO;

namespace CandyCrushAccessible.Audio
{
    public static class ContentResolver
    {
        public static string SoundsDir;
        public static string MusicDir;

        public static void Initialize()
        {
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidates = {
                Path.Combine(exeDir, "sounds"),
                Path.Combine(exeDir, "..", "sounds"),
                Path.Combine(exeDir, "..", "..", "sounds"),
                Path.Combine(exeDir, "..", "..", "..", "sounds"),
                Path.Combine(Directory.GetCurrentDirectory(), "sounds")
            };
            foreach (string c in candidates)
            {
                if (Directory.Exists(c))
                {
                    SoundsDir = Path.GetFullPath(c);
                    break;
                }
            }

            string[] musicCandidates = {
                Path.Combine(exeDir, "music"),
                Path.Combine(exeDir, "..", "music"),
                Path.Combine(exeDir, "..", "..", "music"),
                Path.Combine(exeDir, "..", "..", "..", "music"),
                Path.Combine(Directory.GetCurrentDirectory(), "music")
            };
            foreach (string c in musicCandidates)
            {
                if (Directory.Exists(c))
                {
                    MusicDir = Path.GetFullPath(c);
                    break;
                }
            }

            if (string.IsNullOrEmpty(SoundsDir) || string.IsNullOrEmpty(MusicDir))
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
                    string sub = null;
                    string marker = null;
                    if (name.Contains(".sounds.")) { sub = "sounds"; marker = ".sounds."; }
                    else if (name.Contains(".music.")) { sub = "music"; marker = ".music."; }
                    else continue;
                    string rel = name.Substring(name.IndexOf(marker) + marker.Length);
                    string dir = Path.Combine(assetRoot, sub);
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
                string sd = Path.Combine(assetRoot, "sounds");
                string md = Path.Combine(assetRoot, "music");
                if (Directory.Exists(sd)) SoundsDir = sd;
                if (Directory.Exists(md)) MusicDir = md;
            }
            catch
            {
            }
        }

        public static string SoundPath(string fileName)
        {
            if (string.IsNullOrEmpty(SoundsDir)) return null;
            string p = Path.Combine(SoundsDir, fileName);
            return File.Exists(p) ? p : null;
        }

        public static string MusicPath(string fileName)
        {
            if (string.IsNullOrEmpty(MusicDir)) return null;
            string p = Path.Combine(MusicDir, fileName);
            return File.Exists(p) ? p : null;
        }
    }
}