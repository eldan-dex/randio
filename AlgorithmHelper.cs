using System;

namespace Randio_2 {
    class AlgorithmHelper {

        //Public methods
        //********************************************************************************//
        public static Random GetNewRandom() {
            return new Random((int)DateTime.Now.Ticks);
        }
    }
}
