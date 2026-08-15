using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CandyCrushAccessible.Engine
{
    public class GameProgress
    {
        private const string FileName = "candycrush_progress.json";
        private static string _overrideSavePath = null;

        public static void SetSavePathForTesting(string path)
        {
            _overrideSavePath = path;
            IsDevMode = false;
        }

        public int CurrentLevel { get; set; } = 1;
        public int CurrentEpisode { get; set; } = 0;
        public int Lives { get; set; } = 5;
        public DateTime LivesRegenDue { get; set; } = DateTime.UtcNow;
        public int GoldBars { get; set; } = 0;
        public int Coins { get; set; } = 100;
        public DateTime DailyBonusDue { get; set; } = DateTime.MinValue;
        public Dictionary<int, int> BestStars { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, int> BestScores { get; set; } = new Dictionary<int, int>();
        public Dictionary<BoosterType, int> BoosterCounts { get; set; } = new Dictionary<BoosterType, int>();
        public bool LanguageSpanish { get; set; } = true;
        public bool BinauralAmbientEnabled { get; set; } = true;
        public float MusicVolume { get; set; } = 0.45f;
        public float SfxVolume { get; set; } = 0.8f;
        public float VoiceVolume { get; set; } = 0.9f;

        private static string SavePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_overrideSavePath))
                    return _overrideSavePath;
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CandyCrushAccessible");
                return Path.Combine(dir, FileName);
            }
        }

        public static bool IsDevMode = false;

        public static GameProgress Load()
        {
            GameProgress p = new GameProgress();
            try
            {
                string path = SavePath;
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    GameProgress loaded = JsonSerializer.Deserialize<GameProgress>(json);
                    if (loaded != null) p = loaded;
                }
            }
            catch
            {
            }
            p.GrantStarterBoosters();
            p.TryCollectDailyBonus();
            if (IsDevMode)
            {
                p.Lives = 99;
                if (p.GoldBars < 500) p.GoldBars = 999;
                if (p.Coins < 5000) p.Coins = 9999;
                if (p.CurrentLevel < Levels.TotalLevels) p.CurrentLevel = Levels.TotalLevels;
            }
            return p;
        }

        private void GrantStarterBoosters()
        {
            // No free boosters at start - they must be purchased or earned
        }

        public int GetBooster(BoosterType type)
        {
            int count;
            if (BoosterCounts.TryGetValue(type, out count)) return count;
            return 0;
        }

        public void AddBooster(BoosterType type, int amount)
        {
            BoosterCounts[type] = GetBooster(type) + amount;
        }

        public bool UseBooster(BoosterType type)
        {
            if (GetBooster(type) <= 0) return false;
            BoosterCounts[type] = GetBooster(type) - 1;
            return true;
        }

        public string AwardBoosters(int stars)
        {
            int amount = 0;
            if (stars >= 3) amount = 3;
            else if (stars == 2) amount = 2;
            else if (stars == 1) amount = 1;
            if (amount <= 0) return "";

            Random rng = new Random();
            BoosterType[] pool = { BoosterType.LollipopHammer, BoosterType.ExtraMoves, BoosterType.ExtraTime, BoosterType.ColorBomb, BoosterType.JellyFish };
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < amount; i++)
            {
                BoosterType t = pool[rng.Next(pool.Length)];
                AddBooster(t, 1);
                if (sb.Length > 0) sb.Append(". ");
                sb.Append(Boosters.Name(t));
            }
            return sb.ToString();
        }

        public void Save()
        {
            try
            {
                string path = SavePath;
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string json = JsonSerializer.Serialize(this);
                File.WriteAllText(path, json);
            }
            catch
            {
            }
        }

        public void RecordResult(int levelNumber, int score, int stars)
        {
            if (levelNumber > CurrentLevel)
            {
                CurrentLevel = levelNumber;
            }
            if (stars > 0 && levelNumber >= CurrentLevel)
            {
                CurrentLevel = Math.Max(CurrentLevel, levelNumber + 1);
            }
            int best;
            if (!BestStars.TryGetValue(levelNumber, out best) || stars > best)
            {
                BestStars[levelNumber] = stars;
            }
            int bestScore;
            if (!BestScores.TryGetValue(levelNumber, out bestScore) || score > bestScore)
            {
                BestScores[levelNumber] = score;
            }
            Save();
        }

        public bool IsUnlocked(int levelNumber)
        {
            return levelNumber <= CurrentLevel;
        }

        public static int GetBoosterUnlockLevel(BoosterType type)
        {
            switch (type)
            {
                case BoosterType.LollipopHammer: return 8;
                case BoosterType.ExtraMoves: return 10;
                case BoosterType.JellyFish: return 12;
                case BoosterType.ColorBomb: return 19;
                case BoosterType.ExtraTime: return 20;
                default: return 1;
            }
        }

        public bool IsBoosterUnlocked(BoosterType type)
        {
            return CurrentLevel >= GetBoosterUnlockLevel(type);
        }

        public static int GetBoosterPrice(BoosterType type)
        {
            switch (type)
            {
                case BoosterType.LollipopHammer: return 9;
                case BoosterType.ExtraMoves: return 9;
                case BoosterType.JellyFish: return 19;
                case BoosterType.ColorBomb: return 29;
                case BoosterType.ExtraTime: return 19;
                default: return 9;
            }
        }

        public void AddGoldBars(int amount)
        {
            if (amount > 0) GoldBars += amount;
        }

        public void AddCoins(int amount)
        {
            if (amount > 0) Coins += amount;
        }

        public bool BuyGoldPackage(int goldAmount, int coinCost)
        {
            if (Coins >= coinCost)
            {
                Coins -= coinCost;
                AddGoldBars(goldAmount);
                Save();
                return true;
            }
            return false;
        }

        public bool SpendGoldBars(int amount)
        {
            if (IsDevMode) return true;
            if (GoldBars >= amount)
            {
                GoldBars -= amount;
                return true;
            }
            return false;
        }

        public bool TryCollectDailyBonus()
        {
            DateTime now = DateTime.UtcNow;
            if (DailyBonusDue <= now)
            {
                DailyBonusDue = now.Date.AddDays(1);
                int bonus = 5;
                AddGoldBars(bonus);
                AddCoins(50);
                Save();
                return true;
            }
            return false;
        }

        public double DailyBonusTimeRemaining()
        {
            if (DailyBonusDue <= DateTime.UtcNow) return 0;
            return (DailyBonusDue - DateTime.UtcNow).TotalSeconds;
        }

        public void AwardLevelCompletion(int stars)
        {
            int goldEarned = stars;
            if (stars >= 3) goldEarned = 3;
            else if (stars == 2) goldEarned = 2;
            else if (stars == 1) goldEarned = 1;
            if (goldEarned > 0)
            {
                AddGoldBars(goldEarned);
                AddCoins(goldEarned * 20);
                Save();
            }
        }

        private const double LivesRegenSeconds = 1800.0;

        public void UpdateLives()
        {
            DateTime now = DateTime.UtcNow;
            if (LivesRegenDue < now.AddYears(-1))
            {
                LivesRegenDue = now;
            }
            if (Lives >= 5)
            {
                LivesRegenDue = now;
                return;
            }
            while (Lives < 5 && LivesRegenDue <= now)
            {
                Lives++;
                LivesRegenDue = LivesRegenDue.AddSeconds(LivesRegenSeconds);
            }
            if (Lives >= 5)
            {
                LivesRegenDue = now;
            }
        }

        public bool TryConsumeLife()
        {
            if (IsDevMode)
            {
                Lives = 99;
                return true;
            }
            UpdateLives();
            if (Lives <= 0) return false;
            Lives--;
            if (Lives < 5)
            {
                LivesRegenDue = DateTime.UtcNow.AddSeconds(LivesRegenSeconds);
            }
            return true;
        }

        public double NextLifeInSeconds()
        {
            UpdateLives();
            if (Lives >= 5) return 0;
            double s = (LivesRegenDue - DateTime.UtcNow).TotalSeconds;
            return s < 0 ? 0 : s;
        }
    }
}