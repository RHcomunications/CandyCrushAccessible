using System;
using CandyCrushAccessible.Engine;

namespace CandyCrushAccessible.Audio
{
    public enum MusicTrack
    {
        Menu,
        Intro,
        Win,
        Lose,
        ScoreAndJelly,
        Order,
        Timed,
        Ingredient
    }

    public static class MusicMap
    {
        public static string FileName(MusicTrack track)
        {
            switch (track)
            {
                case MusicTrack.Menu: return "candy_crush_saga_loop_1";
                case MusicTrack.Intro: return "candy_crush_intro2";
                case MusicTrack.Win: return "candy_crush_outro1";
                case MusicTrack.Lose: return null;
                case MusicTrack.ScoreAndJelly: return "candy_crush_loop5";
                case MusicTrack.Order: return "candy_crush_soundtrack2";
                case MusicTrack.Timed: return "candy_crush_soundtrack3";
                case MusicTrack.Ingredient: return "candy_crush_soundtrack4";
            }
            return "candy_crush_saga_loop_1";
        }

        public static string GetTrackForLevelType(LevelType type)
        {
            switch (type)
            {
                case LevelType.Score:
                case LevelType.Jelly:
                    return "candy_crush_loop5";
                case LevelType.Order:
                    return "candy_crush_soundtrack2";
                case LevelType.Timed:
                    return "candy_crush_soundtrack3";
                case LevelType.Ingredient:
                    return "candy_crush_soundtrack4";
                default:
                    return "candy_crush_loop5";
            }
        }
    }
}