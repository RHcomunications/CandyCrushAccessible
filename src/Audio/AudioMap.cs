using System;
using System.Collections.Generic;

namespace CandyCrushAccessible.Audio
{
    public static class AudioMap
    {
        private static readonly Dictionary<string, string> Map = new Dictionary<string, string>
        {
            {"candy", "candy_land1.wav"},
            {"candy2", "candy_land2.wav"},
            {"candy3", "candy_land3.wav"},
            {"candy4", "candy_land4.wav"},
            {"switch", "switch_sound1.wav"},
            {"invalid", "negative_switch_sound1.wav"},
            {"match1", "combo_sound1.wav"},
            {"match2", "combo_sound2.wav"},
            {"match3", "combo_sound3.wav"},
            {"match4", "combo_sound4.wav"},
            {"match5", "combo_sound5.wav"},
            {"match6", "combo_sound6.wav"},
            {"match7", "combo_sound7.wav"},
            {"match8", "combo_sound8.wav"},
            {"match9", "combo_sound9.wav"},
            {"match10", "combo_sound10.wav"},
            {"match11", "combo_sound11.wav"},
            {"match12", "combo_sound12.wav"},
            {"striped_created", "striped_candy_created1.wav"},
            {"wrapped_created", "wrapped_candy_created1.wav"},
            {"colorbomb_created", "colour_bomb_created.wav"},
            {"fish", "swedish_fish_candy1.wav"},
            {"fish_eating", "swedish_fish_candy_loop1.wav"},
            {"fish_swim", "swedish_fish_candy1.wav"},
            {"fish_bite", "swedish_fish_candy2.wav"},
            {"lineblast", "line_blast1.wav"},
            {"wrapped_explosion", "bomb_sound1.wav"},
            {"square", "square_removed1.wav"},
            {"square2", "square_removed2.wav"},
            {"colorbomb", "colour_bomb1.wav"},
            {"supercolorbomb", "super_colour_bomb1.wav"},
            {"bomb", "bomb_sound1.wav"},
            {"chocolate_grow", "chocolate_grows.wav"},
            {"chocolate_removed", "chocolate_removed.wav"},
            {"licorice", "liqourice_lock_broken.wav"},
            {"jelly", "square_removed1.wav"},
            {"jelly2", "square_removed2.wav"},
            {"frosting1", "frosting_cleared1.wav"},
            {"frosting2", "frosting_cleared2.wav"},
            {"ingredient", "nut_out1.wav"},
            {"ingredient2", "nut_out1.wav"},
            {"nut", "nut_out1.wav"},
            {"all_aboard", "all_aboard1.wav"},
            {"tickets_please", "tickets_please1.wav"},
            {"sugar", "sugar_crush.wav"},
            {"win", "level_completed.wav"},
            {"lose", "level_failed1.wav"},
            {"star1", "1_star.wav"},
            {"star2", "2_star.wav"},
            {"star3", "3_star.wav"},
            {"button", "button_press.wav"},
            {"button_down", "button_down.wav"},
            {"button_release", "button_release.wav"},
            {"unlock", "level_unlocked1.wav"},
            {"fanfare", "episode_unlocked_fanfare.wav"},
            {"footsteps", "footsteps1.wav"},
            {"swoop_in", "info_swoop_in1.wav"},
            {"swoop_out", "info_swoop_out1.wav"},
            {"sign", "sign_falls_in1.wav"},
            {"time_warning", "time_warning.wav"},
            {"sweet", "sweet.wav"},
            {"tasty", "tasty.wav"},
            {"delicious", "delicious.wav"},
            {"divine", "divine.wav"},
            {"swoosh", "swoosh_ut.wav"},
            {"level_update", "level_update1.wav"},
            {"superline", "super_line_blast_created.wav"},
            {"klubb", "klubb_kross1.wav"},
            {"shop_buy", "button_press.wav"},
            {"shop_error", "negative_switch_sound1.wav"},
            {"daily_bonus", "episode_unlocked_fanfare.wav"},
            {"bobber_bell", "SFX - Bobber Bell.mp3"},
            {"bobber_splash", "SFX - Bobber Splash.mp3"},
            {"win_screen", "SFX - Arch Win Screen.mp3"},
            {"flash", "flash_loop.wav"},
            {"owl_freeze_break", "owl_freeze_break.wav"},
            {"owl_freeze_ice", "owl_freeze_ice.wav"},
            {"moonstruck", "moonstruck.wav"},
            {"moonstruck_msr", "moonstruck_msr.wav"},
            {"tornado_explosion", "tornado_explosion_v2.wav"},
            {"tornado_ground", "tornado_ground_v2.wav"},
            {"tornado", "tornado_v4.wav"},
            {"conveyor", "conveyor_belt_move.wav"}
        };

        private static readonly Dictionary<string, string> Fallback = new Dictionary<string, string>
        {
            {"candy", "candy_land1.wav"},
            {"candy2", "candy_land2.wav"},
            {"candy3", "candy_land3.wav"},
            {"candy4", "candy_land4.wav"},
            {"switch", "switch_sound1.wav"},
            {"invalid", "negative_switch_sound1.wav"},
            {"match1", "combo_sound1.wav"},
            {"match2", "combo_sound2.wav"},
            {"match3", "combo_sound3.wav"},
            {"match4", "combo_sound4.wav"},
            {"match5", "combo_sound5.wav"},
            {"match6", "combo_sound6.wav"},
            {"match7", "combo_sound7.wav"},
            {"match8", "combo_sound8.wav"},
            {"match9", "combo_sound9.wav"},
            {"match10", "combo_sound10.wav"},
            {"match11", "combo_sound11.wav"},
            {"match12", "combo_sound12.wav"},
            {"striped_created", "striped_candy_created1.wav"},
            {"wrapped_created", "wrapped_candy_created1.wav"},
            {"colorbomb_created", "colour_bomb_created.wav"},
            {"colorbomb", "colour_bomb1.wav"},
            {"supercolorbomb", "super_colour_bomb1.wav"},
            {"bomb", "bomb_sound1.wav"},
            {"wrapped_explosion", "bomb_sound1.wav"},
            {"lineblast", "line_blast1.wav"},
            {"superline", "super_line_blast_created.wav"},
            {"jelly", "square_removed1.wav"},
            {"jelly2", "square_removed2.wav"},
            {"square", "square_removed1.wav"},
            {"square2", "square_removed2.wav"},
            {"frosting1", "frosting_cleared1.wav"},
            {"frosting2", "frosting_cleared2.wav"},
            {"chocolate_grow", "chocolate_grows.wav"},
            {"chocolate_removed", "chocolate_removed.wav"},
            {"licorice", "liqourice_lock_broken.wav"},
            {"ingredient", "nut_out1.wav"},
            {"ingredient2", "nut_out1.wav"},
            {"nut", "nut_out1.wav"},
            {"sugar", "sugar_crush.wav"},
            {"win", "level_completed.wav"},
            {"lose", "level_failed1.wav"},
            {"star1", "1_star.wav"},
            {"star2", "2_star.wav"},
            {"star3", "3_star.wav"},
            {"fish", "swedish_fish_candy1.wav"},
            {"fish_swim", "swedish_fish_candy1.wav"},
            {"fish_eating", "swedish_fish_candy_loop1.wav"},
            {"fish_bite", "swedish_fish_candy2.wav"},
            {"time_warning", "time_warning.wav"},
            {"sweet", "sweet.wav"},
            {"tasty", "tasty.wav"},
            {"delicious", "delicious.wav"},
            {"divine", "divine.wav"},
            {"unlock", "level_unlocked1.wav"},
            {"fanfare", "episode_unlocked_fanfare.wav"},
            {"klubb", "klubb_kross1.wav"}
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
            return "button_press.wav";
        }
    }
}