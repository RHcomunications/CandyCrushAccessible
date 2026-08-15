using System;

namespace CandyCrushAccessible.Engine
{
    public enum CandyColor
    {
        Red,
        Blue,
        Green,
        Yellow,
        Orange,
        Purple
    }

    public enum SpecialType
    {
        None,
        Striped,
        Wrapped,
        ColorBomb,
        Fish
    }

    public enum IngredientType
    {
        None,
        Cherry,
        Nut
    }

    public class Candy
    {
        public CandyColor Color;
        public SpecialType Special = SpecialType.None;
        public bool StripedVertical;
        public IngredientType Ingredient = IngredientType.None;
        public int BombTimer;
        public bool IsTimeCandy;
        public bool IsLicorice;

        public Candy(CandyColor color)
        {
            Color = color;
        }

        public bool IsSpecial
        {
            get { return Special != SpecialType.None; }
        }

        public bool IsIngredient
        {
            get { return Ingredient != IngredientType.None; }
        }

        public Candy Clone()
        {
            Candy c = new Candy(Color);
            c.Special = Special;
            c.StripedVertical = StripedVertical;
            c.Ingredient = Ingredient;
            c.BombTimer = BombTimer;
            c.IsTimeCandy = IsTimeCandy;
            c.IsLicorice = IsLicorice;
            return c;
        }

        public override string ToString()
        {
            if (IsIngredient)
            {
                return Ingredient.ToString();
            }
            string s = Color.ToString();
            if (Special != SpecialType.None) s += " " + Special;
            return s;
        }
    }
}