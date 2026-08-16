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
                Path.Combine(Directory.GetCurrentDirectory(), "sounds_legacy"),
                Path.Combine(exeDir, "sounds"),
                Path.Combine(exeDir, "..", "sounds"),
                Path.Combine(exeDir, "..", "..", "sounds"),
                Path.Combine(exeDir, "..", "..", "..", "sounds"),
                Path.Combine(Directory.GetCurrentDirectory(), "sounds")
            };
            foreach (string c in legacyCandidates)
            {
                if (Directory.Exists(c))
                {
                    SoundsLegacyDir = Path.GetFullPath(c);
                    SoundsDir = SoundsLegacyDir;
                    break;
                }
            }

            string[] musicCandidates = {
                Path.Combine(exeDir, "sounds_legacy"),
                Path.Combine(exeDir, "..", "sounds_legacy"),
                Path.Combine(exeDir, "..", "..", "sounds_legacy"),
                Path.Combine(exeDir, "..", "..", "..", "sounds_legacy"),
                Path.Combine(Directory.GetCurrentDirectory(), "sounds_legacy"),
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
                    if (name.Contains(".sounds.")) { sub = "sounds_legacy"; marker = ".sounds."; }
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
                string sd = Path.Combine(assetRoot, "sounds_legacy");
                string md = Path.Combine(assetRoot, "music");
                if (Directory.Exists(sd))
                {
                    SoundsLegacyDir = sd;
                    SoundsDir = sd;
                }
                if (Directory.Exists(md)) MusicDir = md;
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
                    if (!string.IsNullOrEmpty(SoundsDir))
                    {
                        string p2 = Path.Combine(SoundsDir, candidate);
                        if (File.Exists(p2)) return p2;
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
                    if (!string.IsNullOrEmpty(MusicDir))
                    {
                        string p2 = Path.Combine(MusicDir, candidate);
                        if (File.Exists(p2)) return p2;
                    }
                    if (!string.IsNullOrEmpty(SoundsDir))
                    {
                        string p3 = Path.Combine(SoundsDir, candidate);
                        if (File.Exists(p3)) return p3;
                    }
                }
            }

            return null;
        }
    }
}