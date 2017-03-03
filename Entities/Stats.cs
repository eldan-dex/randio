using System;
namespace Randio_2
{
    //Keeps track of game statistics, is displayed on the end screen
    class Stats
    {
        #region Public variables
        public int EnemiesKilled = 0;
        public int DamageSustained = 0;
        public int TimesDead = 0;
        public int QuestsCompleted = 0;
        public DateTime gameStarted;
        #endregion

        #region Public methods
        //default ctor
        public Stats()
        {
            gameStarted = DateTime.Now;
        }

        //Copy ctor
        public Stats(Stats other)
        {
            EnemiesKilled = other.EnemiesKilled;
            DamageSustained = other.DamageSustained;
            TimesDead = other.TimesDead;
            QuestsCompleted = other.QuestsCompleted;
            gameStarted = other.gameStarted;
        }
        #endregion
    }
}
