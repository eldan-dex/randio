﻿/*
The MIT License(MIT)
Copyright(c) mxgmn 2016.
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
The software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the authors or copyright holders be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or other dealings in the software.
*/

using System;
using System.Xml;
using System.Drawing;
using System.ComponentModel;

namespace CC {
    static class CC {
        public static Bitmap ExpandBitmap(Bitmap bitmap, short N, short width, short height, double temperature, short outputSize, short iterations) {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            Bitmap result = ConvChain(bitmap.ToArray(), bitmap.Width, bitmap.Height, N, temperature, outputSize, iterations, rnd.Next()).ToBitmap(outputSize);
            return result;
        }

        static bool[] ConvChain(bool[] sample, int sampleWidth, int sampleHeight, int N, double temperature, int size, int iterations, int seed) {
            bool[] field = new bool[size * size];
            double[] weights = new double[1 << (N * N)];
            Random random = new Random(seed);

            Func<Func<int, int, bool>, bool[]> pattern = f => {
                bool[] result = new bool[N * N];
                for (int y = 0; y < N; y++) for (int x = 0; x < N; x++) result[x + y * N] = f(x, y);
                return result;
            };

            Func<bool[], bool[]> rotate = p => pattern((x, y) => p[N - 1 - y + x * N]);
            Func<bool[], bool[]> reflect = p => pattern((x, y) => p[N - 1 - x + y * N]);

            Func<bool[], int> index = p => {
                int result = 0, power = 1;
                for (int i = 0; i < p.Length; i++) {
                    result += p[p.Length - 1 - i] ? power : 0;
                    power *= 2;
                }
                return result;
            };

            for (int y = 0; y < sampleHeight; y++) for (int x = 0; x < sampleWidth; x++) {
                    bool[][] ps = new bool[8][];

                    ps[0] = pattern((dx, dy) => sample[(x + dx) % sampleWidth + ((y + dy) % sampleHeight) * sampleWidth]);
                    ps[1] = rotate(ps[0]);
                    ps[2] = rotate(ps[1]);
                    ps[3] = rotate(ps[2]);
                    ps[4] = reflect(ps[0]);
                    ps[5] = reflect(ps[1]);
                    ps[6] = reflect(ps[2]);
                    ps[7] = reflect(ps[3]);

                    for (int k = 0; k < 8; k++) weights[index(ps[k])] += 1;
                }

            for (int k = 0; k < weights.Length; k++) if (weights[k] <= 0) weights[k] = 0.1;
            for (int i = 0; i < field.Length; i++) field[i] = random.Next(2) == 1;

            for (int k = 0; k < iterations * size * size; k++) {
                int r = random.Next(field.Length);
                int x = r % size, y = r / size;

                double q = 1;
                for (int sy = y - N + 1; sy <= y + N - 1; sy++) for (int sx = x - N + 1; sx <= x + N - 1; sx++) {
                        int ind = 0, difference = 0;
                        for (int dy = 0; dy < N; dy++) for (int dx = 0; dx < N; dx++) {
                                int X = sx + dx;
                                if (X < 0) X += size;
                                else if (X >= size) X -= size;

                                int Y = sy + dy;
                                if (Y < 0) Y += size;
                                else if (Y >= size) Y -= size;

                                bool value = field[X + Y * size];
                                int power = 1 << (dy * N + dx);
                                ind += value ? power : 0;
                                if (X == x && Y == y) difference = value ? power : -power;
                            }

                        q *= weights[ind - difference] / weights[ind];
                    }

                if (q >= 1) { field[r] = !field[r]; continue; }
                if (temperature != 1) q = Math.Pow(q, 1.0 / temperature);
                if (q > random.NextDouble()) field[r] = !field[r];
            }

            return field;
        }
    }

    static class Stuff {
        public static T Get<T>(this XmlNode node, string attribute, T defaultT = default(T)) {
            string s = ((XmlElement)node).GetAttribute(attribute);
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return s == "" ? defaultT : (T)converter.ConvertFromInvariantString(s);
        }

        public static bool[] ToArray(this Bitmap bitmap) {
            bool[] result = new bool[bitmap.Width * bitmap.Height];
            for (int i = 0; i < result.Length; i++) result[i] = bitmap.GetPixel(i % bitmap.Width, i / bitmap.Width).R > 0;
            return result;
        }

        public static Bitmap ToBitmap(this bool[] array, int size) {
            Bitmap result = new Bitmap(size, size);
            for (int y = 0; y < result.Height; y++) for (int x = 0; x < result.Width; x++) result.SetPixel(x, y, array[x + y * size] ? Color.LightGray : Color.Black);
            return result;
        }
    }
}