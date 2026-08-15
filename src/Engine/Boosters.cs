using System;

namespace CandyCrushAccessible.Engine
{
    public enum BoosterType
    {
        LollipopHammer,
        ExtraMoves,
        ExtraTime,
        ColorBomb,
        JellyFish
    }

    public static class Boosters
    {
        public static readonly BoosterType[] All =
        {
            BoosterType.LollipopHammer,
            BoosterType.ExtraMoves,
            BoosterType.ExtraTime,
            BoosterType.ColorBomb,
            BoosterType.JellyFish
        };

        public static string Name(BoosterType type)
        {
            switch (type)
            {
                case BoosterType.LollipopHammer: return Localization.Get("booster.hammer");
                case BoosterType.ExtraMoves: return Localization.Get("booster.moves");
                case BoosterType.ExtraTime: return Localization.Get("booster.time");
                case BoosterType.ColorBomb: return Localization.Get("booster.colorbomb");
                case BoosterType.JellyFish: return Localization.Get("booster.fish");
            }
            return "";
        }

        public static string Description(BoosterType type)
        {
            switch (type)
            {
                case BoosterType.LollipopHammer: return Localization.Get("desc.hammer");
                case BoosterType.ExtraMoves: return Localization.Get("desc.extramoves");
                case BoosterType.ExtraTime: return Localization.Get("desc.extratime");
                case BoosterType.ColorBomb: return Localization.Get("desc.colorbomb");
                case BoosterType.JellyFish: return Localization.Get("desc.jellyfish");
            }
            return "";
        }
    }
}