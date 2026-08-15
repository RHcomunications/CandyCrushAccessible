using System;
using System.Collections.Generic;

namespace CandyCrushAccessible.Audio
{
    public enum MusicTrack
    {
        Menu,
        Gameplay,
        Win,
        Lose
    }

    public static class MusicMap
    {
        private static readonly string[] GameplayTracks =
        {
            "Candy Crush Loop 5.mp3",
            "Candy Crush Soundtrack 2.mp3",
            "Candy Crush Soundtrack 3.mp3",
            "Candy Crush Soundtrack 4.mp3",
            "Sa Game Mode Mixed Modes Loop.mp3"
        };

        private static readonly Random Rng = new Random();

        public static string FileName(MusicTrack track)
        {
            switch (track)
            {
                case MusicTrack.Menu: return "Candy Crush Menu.mp3";
                case MusicTrack.Gameplay:
                    return GameplayTracks[Rng.Next(GameplayTracks.Length)];
                case MusicTrack.Win: return "Candy Crush Win.mp3";
                case MusicTrack.Lose: return "Candy Crush Lose.mp3";
            }
            return null;
        }

        public static string EpisodeFileName(int episodeNumber)
        {
            int idx = ((episodeNumber - 1) % GameplayTracks.Length + GameplayTracks.Length) % GameplayTracks.Length;
            return GameplayTracks[idx];
        }
    }
}