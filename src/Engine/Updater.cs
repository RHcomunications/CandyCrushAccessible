using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CandyCrushAccessible.Engine
{
    public class UpdateInfo
    {
        public string Version { get; set; }
        public string ReleaseNotes { get; set; }
        public string DownloadUrl { get; set; }
    }

    public static class Updater
    {
        public static string CurrentVersion => GetLocalVersionString();
        public static UpdateInfo AvailableUpdate = null;

        public static string GetLocalVersionString()
        {
            var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            int build = v.Build >= 0 ? v.Build : 0;
            return $"{v.Major}.{v.Minor}.{build}";
        }

        public static async Task<bool> CheckConnectionAsync()
        {
            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) })
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "CandyCrushAccessible-Updater");
                    using (var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "https://github.com")))
                    {
                        return response.IsSuccessStatusCode;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CheckConnection error: " + ex.Message);
                return false;
            }
        }

        public static async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            if (!await CheckConnectionAsync()) return null;
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "CandyCrushAccessible-Updater");
                    string json = await client.GetStringAsync("https://api.github.com/repos/RHcomunications/CandyCrushAccessible/releases/latest");
                    using (JsonDocument doc = JsonDocument.Parse(json))
                    {
                        var root = doc.RootElement;
                        string tag = root.GetProperty("tag_name").GetString();
                        string notes = root.TryGetProperty("body", out var bodyElem) ? bodyElem.GetString() : "";

                        string cleanTag = tag.Replace("v", "").Replace("V", "").Trim();
                        if (cleanTag.Contains("-"))
                        {
                            cleanTag = cleanTag.Split('-')[0];
                        }

                        if (!Version.TryParse(cleanTag, out Version onlineVersion))
                        {
                            System.Diagnostics.Debug.WriteLine("Failed to parse online version tag: " + tag);
                            return null;
                        }

                        var asmVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                        Version strictLocal = new Version(asmVer.Major, asmVer.Minor, asmVer.Build > 0 ? asmVer.Build : 0);

                        string downloadUrl = "";
                        if (root.TryGetProperty("assets", out var assetsElem) && assetsElem.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var asset in assetsElem.EnumerateArray())
                            {
                                string name = asset.GetProperty("name").GetString();
                                if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                                {
                                    downloadUrl = asset.GetProperty("browser_download_url").GetString();
                                    break;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(downloadUrl) && onlineVersion > strictLocal)
                        {
                            AvailableUpdate = new UpdateInfo
                            {
                                Version = tag,
                                ReleaseNotes = notes,
                                DownloadUrl = downloadUrl
                            };
                            return AvailableUpdate;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CheckForUpdatesAsync error: " + ex.ToString());
            }
            return null;
        }
    }
}
