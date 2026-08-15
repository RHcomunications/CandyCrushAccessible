using System;
using System.Collections.Generic;

namespace CandyCrushAccessible.Audio
{
    public static class AudioMap
    {
        private static readonly Dictionary<string, string> Map = new Dictionary<string, string>
        {
            {"candy", "candy-land1.mp3"},
            {"candy2", "candy-land2.mp3"},
            {"candy3", "candy-land3.mp3"},
            {"candy4", "candy-land4.mp3"},
            {"switch", "switch-sound1.mp3"},
            {"invalid", "negative-switch-sound1.mp3"},
            {"match1", "combo-sound1.mp3"},
            {"match2", "combo-sound2.mp3"},
            {"match3", "combo-sound3.mp3"},
            {"match4", "combo-sound4.mp3"},
            {"match5", "combo-sound5.mp3"},
            {"match6", "combo-sound6.mp3"},
            {"match7", "combo-sound7.mp3"},
            {"match8", "combo-sound8.mp3"},
            {"match9", "combo-sound9.mp3"},
            {"match10", "combo-sound10.mp3"},
            {"match11", "combo-sound11.mp3"},
            {"match12", "combo-sound12.mp3"},
            {"striped_created", "striped-candy-created1.mp3"},
            {"wrapped_created", "wrapped-candy-created1.mp3"},
            {"colorbomb_created", "colour-bomb-created.mp3"},
            {"fish", "swedish-fish-candy1.mp3"},
            {"fish_eating", "swedish-fish-candy-loop1.mp3"},
            {"fish_swim", "swedish-fish-candy1.mp3"},
            {"fish_bite", "swedish-fish-candy2.mp3"},
            {"lineblast", "super-line-blast-created.mp3"},
            {"wrapped_explosion", "bomb-sound1.mp3"},
            {"square", "square-removed1.mp3"},
            {"square2", "square-removed2.mp3"},
            {"colorbomb", "colour-bomb1.mp3"},
            {"supercolorbomb", "super-colour-bomb1.mp3"},
            {"bomb", "bomb-sound1.mp3"},
            {"chocolate_grow", "chocolate-grows.mp3"},
            {"chocolate_removed", "chocolate-removed.mp3"},
            {"licorice", "liqourice-lock-broken.mp3"},
            {"jelly", "square-removed2.mp3"},
            {"jelly2", "square-removed2.mp3"},
            {"frosting1", "frosting-cleared1.mp3"},
            {"frosting2", "frosting-cleared2.mp3"},
            {"ingredient", "all-aboard1.mp3"},
            {"ingredient2", "tickets-please1.mp3"},
            {"nut", "nut-out1.mp3"},
            {"sugar", "sugar-crush.mp3"},
            {"win", "level-completed.mp3"},
            {"lose", "level-failed1.mp3"},
            {"star1", "1-star.mp3"},
            {"star2", "2-star.mp3"},
            {"star3", "3-star.mp3"},
            {"button", "button-press.mp3"},
            {"button_down", "button-down.mp3"},
            {"button_release", "button-release.mp3"},
            {"unlock", "level-unlocked1.mp3"},
            {"fanfare", "episode-unlocked-fanfare.mp3"},
            {"footsteps", "footsteps1.mp3"},
            {"swoop_in", "info-swoop-in1.mp3"},
            {"swoop_out", "info-swoop-out1.mp3"},
            {"sign", "sign-falls-in1.mp3"},
            {"time_warning", "time-warning.mp3"},
            {"sweet", "sweet.mp3"},
            {"tasty", "tasty.mp3"},
            {"delicious", "delicious.mp3"},
            {"divine", "divine.mp3"},
            {"swoosh", "swoosh-ut.mp3"},
            {"level_update", "level-update1.mp3"},
            {"superline", "super-line-blast-created.mp3"},
            {"klubb", "klubb-kross1.mp3"},
            {"shop_buy", "button-press.mp3"},
            {"shop_error", "negative-switch-sound1.mp3"},
            {"daily_bonus", "episode-unlocked-fanfare.mp3"},
            {"bobber_bell", "SFX - Bobber Bell.mp3"},
            {"bobber_splash", "SFX - Bobber Splash.mp3"},
            {"win_screen", "SFX - Arch Win Screen.mp3"},
            {"flash", "flash-loop.mp3"}
        };

        private static readonly Dictionary<string, string> Fallback = new Dictionary<string, string>
        {
            {"candy", "button-press.mp3"},
            {"match1", "button-press.mp3"},
            {"match2", "button-press.mp3"},
            {"match3", "button-press.mp3"},
            {"match4", "button-press.mp3"},
            {"match5", "button-press.mp3"},
            {"match6", "button-press.mp3"},
            {"match7", "button-press.mp3"},
            {"match8", "button-press.mp3"},
            {"match9", "button-press.mp3"},
            {"match10", "button-press.mp3"},
            {"match11", "button-press.mp3"},
            {"match12", "button-press.mp3"},
            {"jelly", "swoosh-ut.mp3"},
            {"jelly2", "swoosh-ut.mp3"},
            {"win", "button-press.mp3"},
            {"lose", "button-press.mp3"},
            {"colorbomb_created", "striped-candy-created1.mp3"},
            {"wrapped_created", "striped-candy-created1.mp3"},
            {"supercolorbomb", "colorbomb_created"},
            {"ingredient", "candy-land1.mp3"},
            {"ingredient2", "candy-land1.mp3"},
            {"square", "swoosh-ut.mp3"},
            {"square2", "swoosh-ut.mp3"},
            {"lineblast", "swoosh-ut.mp3"},
            {"sugar", "swoosh-ut.mp3"},
            {"fish", "candy-land1.mp3"},
            {"bomb", "swoosh-ut.mp3"},
            {"chocolate_grow", "candy-land1.mp3"},
            {"chocolate_removed", "candy-land1.mp3"},
            {"licorice", "candy-land1.mp3"},
            {"time_warning", "button-press.mp3"}
        };

        public static string FileName(string key)
        {
            string name;
            if (Map.TryGetValue(key, out name)) return name;
            return null;
        }

        public static string FileNameWithFallback(string key)
        {
            string name = FileName(key);
            if (name != null) return name;
            string fb;
            if (Fallback.TryGetValue(key, out fb)) return fb;
            return "button-press.mp3";
        }
    }
}