using System;
using System.Collections.Generic;
using System.Text;

namespace CandyCrushAccessible.Engine
{
    public enum LevelType
    {
        Score,
        Jelly,
        Ingredient,
        Timed,
        Order
    }

    public class LevelDefinition
    {
        public int Number;
        public LevelType Type = LevelType.Score;
        public int Moves = 30;
        public int TimeSeconds;
        public int TargetScore = 1000;
        public int TargetJelly;
        public int TargetIngredients;
        public IngredientType TargetIngredientType = IngredientType.Cherry;
        public int NumColors = 5;
        public bool AllBoardJelly;
        public bool HasBombs;
        public bool HasChocolate;
        public int TargetFrosting;
        public int TargetLicorice;
        public int BombTimerBase = 8;
        public bool TimeCandies;
        public List<LevelOrder> Orders;

        public int OneStarScore;
        public int TwoStarScore;
        public int ThreeStarScore;

        public string ObjectiveText
        {
            get
            {
                switch (Type)
                {
                    case LevelType.Score:
                        return string.Format(Localization.Get("obj.score"), TargetScore, Moves);
                    case LevelType.Jelly:
                        return string.Format(Localization.Get("obj.jelly"), Moves);
                    case LevelType.Ingredient:
                        string ingName = TargetIngredientType == IngredientType.Nut ? Localization.Get("ingredient.nut") : Localization.Get("ingredient.cherry");
                        return string.Format(Localization.Get("obj.ingredient"), TargetIngredients, ingName, Moves);
                    case LevelType.Timed:
                        return string.Format(Localization.Get("obj.timed"), TargetScore, TimeSeconds);
                    case LevelType.Order:
                        {
                            if (Orders != null && Orders.Count > 0)
                            {
                                List<string> parts = new List<string>();
                                foreach (LevelOrder o in Orders) parts.Add(o.Describe());
                                return string.Format(Localization.Get("obj.order.specific"), string.Join(", ", parts), Moves);
                            }
                            return string.Format(Localization.Get("obj.order"), Moves);
                        }
                }
                return "";
            }
        }
    }

    public static class Levels
    {
        public const int TotalLevels = 65;

        public static LevelDefinition Get(int number)
        {
            if (number < 1) number = 1;

            switch (number)
            {
                case 1: return Score(1, 1000, 30, 5);
                case 2: return Score(2, 3000, 30, 5);
                case 3: return Score(3, 5000, 30, 5);
                case 4: return Score(4, 8000, 35, 5);
                case 5: return Score(5, 10000, 35, 5);
                case 6: return Score(6, 20000, 50, 6);
                case 7: return Jelly(7, 20, 5, false);
                case 8: LevelDefinition l8 = Score(8, 25000, 50, 6); l8.TargetLicorice = 3; return l8;
                case 9: return Score(9, 25000, 50, 6);
                case 10: return Score(10, 15000, 45, 6);
                case 11: return Score(11, 20000, 45, 6);
                case 12: LevelDefinition l12 = Jelly(12, 30, 6, true); l12.TargetFrosting = 6; return l12;
                case 13: LevelDefinition l13 = Score(13, 20000, 40, 6); l13.TargetLicorice = 4; return l13;
                case 14: LevelDefinition l14 = Jelly(14, 25, 6, false); l14.TargetFrosting = 5; return l14;
                case 15: return Score(15, 30000, 50, 6);
                case 16: LevelDefinition l16 = Jelly(16, 35, 6, false); l16.TargetFrosting = 6; return l16;
                case 17: LevelDefinition l17 = Score(17, 30000, 50, 6); l17.TargetLicorice = 4; return l17;
                case 18: return Score(18, 50000, 60, 6);
                case 19: LevelDefinition l19 = Ingredient(19, 1, 40, 5, false); l19.TargetLicorice = 5; return l19;
                case 20: return Timed(20, 2000, 60, 6);
                case 21: return Score(21, 50000, 50, 5);
                case 22: LevelDefinition l22 = Ingredient(22, 2, 40, 5, false); l22.TargetLicorice = 6; return l22;
                case 23: LevelDefinition l23 = JellyBombs(23, 30, 6); l23.TargetFrosting = 8; return l23;
                case 24: LevelDefinition l24 = Ingredient(24, 1, 45, 5, true); l24.TargetFrosting = 4; l24.TargetLicorice = 5; return l24;
                case 25: LevelDefinition l25 = OrderLevel(25, 40, 5, 30000); l25.Orders = OrderFactory.Create(
                    OrderFactory.Striped(2), OrderFactory.Wrapped(2), OrderFactory.Candy(CandyColor.Red, 10)); return l25;
                case 26: LevelDefinition l26 = Jelly(26, 40, 6, true); l26.HasChocolate = true; l26.TargetFrosting = 8; return l26;
                case 27: return Timed(27, 3000, 75, 6);
                case 28: LevelDefinition l28 = Ingredient(28, 2, 45, 6, true); l28.TargetFrosting = 5; l28.TargetLicorice = 6; return l28;
                case 29: LevelDefinition l29 = OrderLevel(29, 45, 5, 40000); l29.Orders = OrderFactory.Create(
                    OrderFactory.Striped(3), OrderFactory.ColorBomb(2), OrderFactory.Candy(CandyColor.Purple, 10)); l29.HasBombs = true; l29.TargetFrosting = 6; return l29;
                case 30: LevelDefinition l30 = Score(30, 120000, 60, 6); l30.HasChocolate = true; return l30;
                
                // Episodio 4: Laguna de Limonada (31 - 40)
                case 31: return Score(31, 35000, 35, 5);
                case 32: LevelDefinition l32 = Jelly(32, 30, 5, true); l32.TargetFrosting = 8; return l32;
                case 33: LevelDefinition l33 = Ingredient(33, 2, 35, 5, false); l33.TargetLicorice = 6; return l33;
                case 34: LevelDefinition l34 = OrderLevel(34, 40, 5, 35000); l34.Orders = OrderFactory.Create(
                    OrderFactory.Striped(4), OrderFactory.Candy(CandyColor.Yellow, 15)); return l34;
                case 35: LevelDefinition l35 = Score(35, 45000, 40, 6); l35.HasChocolate = true; return l35;
                case 36: return Timed(36, 4000, 60, 5);
                case 37: LevelDefinition l37 = Jelly(37, 35, 5, true); l37.TargetLicorice = 8; return l37;
                case 38: LevelDefinition l38 = JellyBombs(38, 30, 6); l38.BombTimerBase = 12; l38.TargetFrosting = 10; return l38;
                case 39: LevelDefinition l39 = Ingredient(39, 3, 40, 5, true); l39.HasChocolate = true; return l39;
                case 40: LevelDefinition l40 = OrderLevel(40, 45, 5, 50000); l40.Orders = OrderFactory.Create(
                    OrderFactory.Wrapped(3), OrderFactory.ColorBomb(1), OrderFactory.Candy(CandyColor.Green, 20)); return l40;

                // Episodio 5: Montaña de Mentebruma (41 - 50)
                case 41: return Score(41, 60000, 40, 6);
                case 42: LevelDefinition l42 = Jelly(42, 35, 5, true); l42.HasChocolate = true; l42.TargetFrosting = 10; return l42;
                case 43: LevelDefinition l43 = Ingredient(43, 3, 45, 6, true); l43.TargetLicorice = 8; return l43;
                case 44: LevelDefinition l44 = OrderLevel(44, 40, 5, 45000); l44.Orders = OrderFactory.Create(
                    OrderFactory.Striped(3), OrderFactory.Wrapped(3)); l44.HasBombs = true; return l44;
                case 45: return Timed(45, 5000, 60, 6);
                case 46: LevelDefinition l46 = JellyBombs(46, 30, 5); l46.BombTimerBase = 10; l46.HasChocolate = true; return l46;
                case 47: LevelDefinition l47 = Score(47, 75000, 45, 6); l47.TargetLicorice = 10; return l47;
                case 48: LevelDefinition l48 = Ingredient(48, 4, 40, 5, true); l48.TargetFrosting = 12; return l48;
                case 49: LevelDefinition l49 = OrderLevel(49, 45, 5, 60000); l49.Orders = OrderFactory.Create(
                    OrderFactory.ColorBomb(2), OrderFactory.Candy(CandyColor.Blue, 25)); return l49;
                case 50: LevelDefinition l50 = Jelly(50, 40, 6, true); l50.HasChocolate = true; l50.TargetFrosting = 14; l50.TargetLicorice = 10; return l50;

                // Episodio 6: Cañón de Caramelo (51 - 60)
                case 51: return Score(51, 80000, 35, 5);
                case 52: LevelDefinition l52 = Jelly(52, 35, 5, true); l52.TargetFrosting = 12; l52.HasChocolate = true; return l52;
                case 53: LevelDefinition l53 = OrderLevel(53, 40, 5, 55000); l53.Orders = OrderFactory.Create(
                    OrderFactory.Striped(5), OrderFactory.Wrapped(2)); l53.HasBombs = true; l53.BombTimerBase = 10; return l53;
                case 54: LevelDefinition l54 = Ingredient(54, 4, 45, 6, true); l54.TargetLicorice = 12; return l54;
                case 55: return Timed(55, 6000, 75, 5);
                case 56: LevelDefinition l56 = JellyBombs(56, 30, 5); l56.BombTimerBase = 9; l56.TargetFrosting = 14; return l56;
                case 57: LevelDefinition l57 = Score(57, 90000, 45, 6); l57.HasChocolate = true; return l57;
                case 58: LevelDefinition l58 = OrderLevel(58, 45, 5, 70000); l58.Orders = OrderFactory.Create(
                    OrderFactory.Wrapped(4), OrderFactory.ColorBomb(2)); return l58;
                case 59: LevelDefinition l59 = Ingredient(59, 5, 40, 5, true); l59.TargetFrosting = 15; l59.TargetLicorice = 12; return l59;
                case 60: LevelDefinition l60 = Jelly(60, 40, 6, true); l60.HasChocolate = true; l60.HasBombs = true; l60.BombTimerBase = 10; return l60;

                // Episodio 7: Valle del Malvavisco (61 - 65)
                case 61: return Score(61, 100000, 40, 6);
                case 62: LevelDefinition l62 = Jelly(62, 35, 5, true); l62.TargetFrosting = 16; l62.TargetLicorice = 15; return l62;
                case 63: LevelDefinition l63 = OrderLevel(63, 45, 5, 85000); l63.Orders = OrderFactory.Create(
                    OrderFactory.Striped(5), OrderFactory.Wrapped(4), OrderFactory.ColorBomb(2)); l63.HasBombs = true; l63.BombTimerBase = 8; return l63;
                case 64: LevelDefinition l64 = Ingredient(64, 5, 40, 5, true); l64.HasChocolate = true; l64.TargetFrosting = 16; return l64;
                case 65: LevelDefinition l65 = Jelly(65, 45, 6, true); l65.HasChocolate = true; l65.HasBombs = true; l65.BombTimerBase = 8; l65.TargetFrosting = 18; l65.TargetLicorice = 16; return l65;

                default: return Generate(number);
            }
        }

        private static LevelDefinition Generate(int n)
        {
            Random rng = new Random(n * 1009 + 31);
            int d = n - TotalLevels;
            LevelDefinition l = new LevelDefinition();
            l.Number = n;
            int pattern = (n - 1) % 10;
            LevelType[] types =
            {
                LevelType.Score, LevelType.Jelly, LevelType.Score, LevelType.Ingredient, LevelType.Order,
                LevelType.Timed, LevelType.Score, LevelType.Jelly, LevelType.Ingredient, LevelType.Order
            };
            l.Type = types[pattern];
            l.NumColors = rng.Next(2) == 0 ? 5 : 6;
            int moves = Math.Max(15, 40 - d / 4 + rng.Next(0, 5));
            int scoreBase = 50000 + d * 1500;
            switch (l.Type)
            {
                case LevelType.Score:
                    l.TargetScore = scoreBase + rng.Next(0, 25000);
                    l.Moves = moves;
                    break;
                case LevelType.Jelly:
                    l.Moves = moves;
                    l.AllBoardJelly = rng.Next(100) < Math.Min(60, 10 + d);
                    l.TargetJelly = l.AllBoardJelly ? 64 : 24;
                    l.TargetScore = 20000 + d * 500;
                    l.HasBombs = rng.Next(3) == 0;
                    if (l.HasBombs) l.BombTimerBase = Math.Max(5, 9 - d / 10);
                    break;
                case LevelType.Ingredient:
                    l.TargetIngredients = Math.Min(3, 1 + d / 6 + rng.Next(0, 2));
                    l.TargetIngredientType = rng.Next(2) == 0 ? IngredientType.Nut : IngredientType.Cherry;
                    l.Moves = moves + 5;
                    l.TargetScore = 15000 + d * 300;
                    l.HasBombs = rng.Next(3) == 0;
                    if (l.HasBombs) l.BombTimerBase = Math.Max(5, 9 - d / 10);
                    break;
                case LevelType.Timed:
                    l.TargetScore = 3000 + d * 100;
                    l.TimeSeconds = Math.Max(45, 90 - d / 5) + rng.Next(0, 15);
                    l.TimeCandies = true;
                    break;
                case LevelType.Order:
                    l.Moves = moves + 3;
                    l.NumColors = 5;
                    l.TargetScore = 30000 + d * 600;
                    l.Orders = MakeRandomOrders(rng, d);
                    break;
            }
            if (d > 5 && rng.Next(100) < 35) l.HasChocolate = true;
            if (rng.Next(100) < 40) l.TargetFrosting = 3 + rng.Next(0, 6);
            if (rng.Next(100) < 35) l.TargetLicorice = 3 + rng.Next(0, 4);
            SetStars(l);
            return l;
        }

        private static LevelDefinition Score(int n, int target, int moves, int colors)
        {
            LevelDefinition l = new LevelDefinition();
            l.Number = n;
            l.Type = LevelType.Score;
            l.TargetScore = target;
            l.Moves = moves;
            l.NumColors = colors;
            SetStars(l);
            return l;
        }

        private static LevelDefinition Jelly(int n, int moves, int colors, bool allBoard)
        {
            LevelDefinition l = new LevelDefinition();
            l.Number = n;
            l.Type = LevelType.Jelly;
            l.Moves = moves;
            l.NumColors = colors;
            l.AllBoardJelly = allBoard;
            l.TargetJelly = allBoard ? 64 : 24;
            l.TargetScore = 20000;
            SetStars(l);
            return l;
        }

        private static LevelDefinition JellyBombs(int n, int moves, int colors)
        {
            LevelDefinition l = new LevelDefinition();
            l.Number = n;
            l.Type = LevelType.Jelly;
            l.Moves = moves;
            l.NumColors = colors;
            l.AllBoardJelly = false;
            l.TargetJelly = 32;
            l.HasBombs = true;
            l.HasChocolate = true;
            l.BombTimerBase = 9;
            l.TargetScore = 20000;
            SetStars(l);
            return l;
        }

        private static LevelDefinition Ingredient(int n, int count, int moves, int colors, bool nut)
        {
            LevelDefinition l = new LevelDefinition();
            l.Number = n;
            l.Type = LevelType.Ingredient;
            l.TargetIngredients = count;
            l.TargetIngredientType = nut ? IngredientType.Nut : IngredientType.Cherry;
            l.Moves = moves;
            l.NumColors = colors;
            l.TargetScore = 15000;
            SetStars(l);
            return l;
        }

        private static LevelDefinition Timed(int n, int target, int seconds, int colors)
        {
            LevelDefinition l = new LevelDefinition();
            l.Number = n;
            l.Type = LevelType.Timed;
            l.TargetScore = target;
            l.TimeSeconds = seconds;
            l.NumColors = colors;
            l.TimeCandies = true;
            SetStars(l);
            return l;
        }

        private static LevelDefinition OrderLevel(int n, int moves, int colors, int targetScore)
        {
            LevelDefinition l = new LevelDefinition();
            l.Number = n;
            l.Type = LevelType.Order;
            l.Moves = moves;
            l.NumColors = colors;
            l.TargetScore = targetScore;
            SetStars(l);
            return l;
        }

        private static List<LevelOrder> MakeRandomOrders(Random rng, int d)
        {
            CandyColor[] colors = { CandyColor.Red, CandyColor.Blue, CandyColor.Green, CandyColor.Yellow, CandyColor.Orange, CandyColor.Purple };
            List<LevelOrder> orders = new List<LevelOrder>();
            int spec = rng.Next(2) + 1;
            int baseCount = Math.Max(1, 2 + d / 10);
            int candyCount = 10 + d / 5;
            for (int i = 0; i < spec; i++)
            {
                switch (rng.Next(3))
                {
                    case 0: orders.Add(OrderFactory.Striped(Math.Max(1, baseCount + rng.Next(0, 2)))); break;
                    case 1: orders.Add(OrderFactory.Wrapped(Math.Max(1, baseCount + rng.Next(0, 2)))); break;
                    case 2: orders.Add(OrderFactory.ColorBomb(Math.Max(1, baseCount / 2))); break;
                }
            }
            orders.Add(OrderFactory.Candy(colors[rng.Next(colors.Length)], candyCount));
            return orders;
        }

        private static void SetStars(LevelDefinition l)
        {
            l.OneStarScore = l.TargetScore;
            l.TwoStarScore = (int)(l.TargetScore * 1.5);
            l.ThreeStarScore = (int)(l.TargetScore * 2.5);
        }
    }
}