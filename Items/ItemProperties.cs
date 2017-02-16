namespace Randio_2
{
    class ItemProperties
    {
        #region Public variables
        public string Name;
        public string Adjective;
        public int HPBonus;
        public float StrengthBonus;
        public float ArmorBonus;
        public float SpeedBonus;
        //etc.
        //item-specific actions?
        #endregion

        #region Public methods
        public ItemProperties(string name = "?", string adjective = "", int hp = 0, int strength = 0, int armor = 0, int speed = 0)
        {
            Name = name;
            Adjective = adjective;
            HPBonus = hp;
            StrengthBonus = strength;
            ArmorBonus = armor;
            SpeedBonus = speed;
        }
        #endregion
    }
}
