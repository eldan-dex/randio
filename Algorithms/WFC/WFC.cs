/*
The MIT License(MIT)
Copyright(c) mxgmn 2016.
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
The software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the authors or copyright holders be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or other dealings in the software.
*/

using System;
using System.Drawing;
namespace WFC {
    static class WFC {
        public static Bitmap ExpandBitmap(Bitmap bitmap, short N, short width, short height, bool periodic, short symmetry, short ground) {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            Model model = new OverlappingModel(bitmap, N, width, height, periodic, periodic, symmetry, ground);
            bool result = model.Run(rnd.Next(), 0);
            if (result)
                return model.Graphics();
            else
                return null; //or return bitmap; ?
        }
    }
}