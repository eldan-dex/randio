namespace Randio_2
{
    class ItemProperties
    {
        #region Public variables
        public string Name;
        public int HPBonus;
        public float StrengthBonus;
        public float ArmorBonus;
        public float SpeedBonus;
        //etc.
        //item-specific actions?
        #endregion

        #region Public methods
        public ItemProperties(string name = "?", int hp = 0, int strength = 0, int armor = 0, int speed = 0)
        {
            Name = name;
            HPBonus = hp;
            StrengthBonus = strength;
            ArmorBonus = armor;
            SpeedBonus = speed;
        }
        #endregion
    }
}
