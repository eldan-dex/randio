using System;
namespace Randio_2
{
    class Stats
    {
        public int EnemiesKilled = 0;
        public int DamageSustained = 0;
        public int TimesDead = 0;
        public int QuestsCompleted = 0;
        public DateTime gameStarted;
        public string PlayerEval = ""; //todo: maybe remove this

        public Stats()
        {
            gameStarted = DateTime.Now;
        }

        public Stats(Stats other)
        {
            EnemiesKilled = other.EnemiesKilled;
            DamageSustained = other.DamageSustained;
            TimesDead = other.TimesDead;
            QuestsCompleted = other.QuestsCompleted;
            gameStarted = other.gameStarted;
            PlayerEval = other.PlayerEval;
        }
    }
}
