using System;

namespace CandyCrushAccessible.Engine
{
    public class EpisodeDefinition
    {
        public int Number;
        public int StartLevel;
        public int EndLevel;
        public string NameKey;

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(NameKey)) return Episodes.GeneratedName(Number);
                return Localization.Get(NameKey);
            }
        }
    }

    public static class Episodes
    {
        private const int LevelsPerEpisode = 10;

        public static readonly EpisodeDefinition[] All =
        {
            new EpisodeDefinition { Number = 1, StartLevel = 1, EndLevel = 10, NameKey = "episode.1" },
            new EpisodeDefinition { Number = 2, StartLevel = 11, EndLevel = 20, NameKey = "episode.2" },
            new EpisodeDefinition { Number = 3, StartLevel = 21, EndLevel = 30, NameKey = "episode.3" },
            new EpisodeDefinition { Number = 4, StartLevel = 31, EndLevel = 40, NameKey = "episode.4" },
            new EpisodeDefinition { Number = 5, StartLevel = 41, EndLevel = 50, NameKey = "episode.5" },
            new EpisodeDefinition { Number = 6, StartLevel = 51, EndLevel = 60, NameKey = "episode.6" },
            new EpisodeDefinition { Number = 7, StartLevel = 61, EndLevel = Levels.TotalLevels, NameKey = "episode.7" }
        };

        public static EpisodeDefinition GetForLevel(int levelNumber)
        {
            foreach (var ep in All)
            {
                if (levelNumber >= ep.StartLevel && levelNumber <= ep.EndLevel)
                    return ep;
            }
            int epNum = 8 + (levelNumber - (Levels.TotalLevels + 1)) / LevelsPerEpisode;
            int start = (Levels.TotalLevels + 1) + (epNum - 8) * LevelsPerEpisode;
            return new EpisodeDefinition { Number = epNum, StartLevel = start, EndLevel = start + LevelsPerEpisode - 1, NameKey = null };
        }

        public static int IndexOf(EpisodeDefinition ep)
        {
            return ep.Number - 1;
        }

        public static EpisodeDefinition Next(EpisodeDefinition ep)
        {
            return GetForLevel(ep.EndLevel + 1);
        }

        public static bool IsEndLevel(int levelNumber)
        {
            var ep = GetForLevel(levelNumber);
            return levelNumber == ep.EndLevel;
        }

        public static string GeneratedName(int num)
        {
            int adj = (num * 7) % 8 + 1;
            int noun = (num * 11 + 3) % 10 + 1;
            return Localization.Get("ep.name.adj." + adj) + " " + Localization.Get("ep.name.noun." + noun);
        }
    }
}