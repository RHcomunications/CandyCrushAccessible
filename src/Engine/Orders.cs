using System;
using System.Collections.Generic;
using System.Text;

namespace CandyCrushAccessible.Engine
{
    public enum OrderKind
    {
        Candy,
        Striped,
        Wrapped,
        ColorBomb,
        Fish
    }

    public class LevelOrder
    {
        public OrderKind Kind;
        public CandyColor Color;
        public int Count;
        public int Filled;

        public bool Fulfilled
        {
            get { return Filled >= Count; }
        }

        public int Remaining
        {
            get { return Math.Max(0, Count - Filled); }
        }

        public string Describe()
        {
            switch (Kind)
            {
                case OrderKind.Candy:
                    return string.Format(Localization.Get("order.candy"), Count, Localization.C(Color));
                case OrderKind.Striped:
                    return string.Format(Localization.Get("order.striped"), Count);
                case OrderKind.Wrapped:
                    return string.Format(Localization.Get("order.wrapped"), Count);
                case OrderKind.ColorBomb:
                    return string.Format(Localization.Get("order.colorbomb"), Count);
                case OrderKind.Fish:
                    return string.Format(Localization.Get("order.fish"), Count);
            }
            return "";
        }

        public string DescribeProgress()
        {
            string name = Describe();
            return string.Format(Localization.Get("order.progress.item"), Filled, Count, name);
        }
    }

    public static class OrderFactory
    {
        public static List<LevelOrder> Create(params LevelOrder[] orders)
        {
            return new List<LevelOrder>(orders);
        }

        public static LevelOrder Candy(CandyColor color, int count)
        {
            return new LevelOrder { Kind = OrderKind.Candy, Color = color, Count = count };
        }

        public static LevelOrder Striped(int count)
        {
            return new LevelOrder { Kind = OrderKind.Striped, Count = count };
        }

        public static LevelOrder Wrapped(int count)
        {
            return new LevelOrder { Kind = OrderKind.Wrapped, Count = count };
        }

        public static LevelOrder ColorBomb(int count)
        {
            return new LevelOrder { Kind = OrderKind.ColorBomb, Count = count };
        }

        public static LevelOrder Fish(int count)
        {
            return new LevelOrder { Kind = OrderKind.Fish, Count = count };
        }
    }
}
