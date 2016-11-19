using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randio_2 {
    class AlgorithmHelper {
        public static Random GetNewRandom() {
            return new Random((int)DateTime.Now.Ticks);
        }
    }
}
