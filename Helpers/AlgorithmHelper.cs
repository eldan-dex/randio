using System;

namespace Randio_2 {
    //Assists with random generation
    static class AlgorithmHelper {

        #region Public variables
        public static Random Rand = new Random((int)DateTime.Now.Ticks);
        #endregion

        #region Public methods
        //Returns a random integer between min (inclusive) and max (exclusive)
        public static int GetRandom(int minInc, int maxExc)
        {
            return Rand.Next(minInc, maxExc);
        }

        //Overloaded GetRandom for floats
        public static float GetRandom(float min, float max)
        {
            return (float)(min + (Rand.NextDouble() * (max - min)));
        }

        //Returns a random integer with a certain bias toward one extreme (0 = left edge, 1 = neutral, 2 = right edge)
        public static int BiasedRandom(int min, int max, double probabilityPower = 2)
        {
            var randomDouble = Rand.NextDouble();

            var result = Math.Floor(min + (max + 1 - min) * (Math.Pow(randomDouble, probabilityPower)));
            return (int)result;
        }
        #endregion
    }
}
