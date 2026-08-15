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
        public static string CurrentVersion = "1.0.0";
        public static UpdateInfo AvailableUpdate = null;

        public static bool CheckConnection()
        {
            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) })
                {
                    var response = client.Send(new HttpRequestMessage(HttpMethod.Head, "https://github.com"));
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            if (!CheckConnection()) return null;
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
                        string cleanTag = tag.StartsWith("v") ? tag.Substring(1) : tag;
                        if (cleanTag.Contains("-")) cleanTag = cleanTag.Split('-')[0];

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

                        if (!string.IsNullOrEmpty(downloadUrl) && string.Compare(cleanTag, CurrentVersion, StringComparison.OrdinalIgnoreCase) > 0)
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
            catch
            {
            }
            return null;
        }
    }
}
