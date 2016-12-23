using System;

namespace Randio_2 {
    static class AlgorithmHelper {
        public static Random Rand = new Random((int)DateTime.Now.Ticks);


        //Public methods
        //********************************************************************************//
        public static int GetRandom(int min, int max)
        {
            return Rand.Next(min, max);
        }


        public static int BiasedRandom(int min, int max, double probabilityPower = 2)
        {
            var randomDouble = Rand.NextDouble();

            var result = Math.Floor(min + (max + 1 - min) * (Math.Pow(randomDouble, probabilityPower)));
            return (int)result;
        }
    }
}
