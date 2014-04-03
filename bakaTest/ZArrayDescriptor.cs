using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace bakaTest
{
    public class ZArrayDescriptor
    {
        public float[,] array;
        public int width, height;

        public static ZArrayDescriptor createRandom(int width, int height)
        {
            ZArrayDescriptor desc = new ZArrayDescriptor();
            desc.array = new float[height, width];
            desc.width = width;
            desc.height = height;

            Random r = new Random();
            for(int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    desc.array[j, i] = (float)(r.NextDouble() * 1);
                }

            return desc;
        }

        //---------------------------------------------------------------------

        static int[] primes = 
        {
            15731,
            789221,
            1376312589,
            15733,
            789227,
            1376312627,
            15737,
            789251,
            1376312629,
            15739,
            789311,
            1376312657,
            15749,
            789323,
            1376312687,
            15761,
            789331,
            1376312689,
            15767,
            789343,
            1376312753,
            15773,
            789367,
            1376312783,
            15787,
            789377,
            1376312789,
            15791,
            789389,
            1376312813,
            15797,
            789391,
            1376312857,
            15803,
            789407,
            1376312879,
            15809,
            789419,
            1376312881,
            15817,
            789443,
            1376312897,
            15823,
            789473,
            1376312909,
            15859,
            789491,
            1376312929,
            15877,
            789493,
            1376312953,
            15881,
            789511,
            1376312989,
            15887,
            789527,
            1376313007,
            15889,
            789533,
            1376313011
        };

        static int getNextPrime(int from)
        {
            for (int j = from + 1; ; ++j)
            {
                Boolean prime = true;
                if ((j & 0x01) == 0)
                    continue;

                for (int k = 3; k < j / 2 + 1; ++k)
                {
                    if ((k & 0x01) == 0)
                        continue;
                    if (j % k == 0)
                    {
                        prime = false;
                        break;
                    }
                }
                if (prime)
                    return j;
            }
        }

        static float noiseFunc(int x, int y, int octaves)
        {
            x += y * 57;
            x = (x<<13) ^ x;
            return ( 1.0f - (float)( (x * (x * x * primes[3*octaves] + primes[3*octaves + 1]) + primes[3*octaves + 2]) & 0x7fffffff) / 1073741824.0f);
        }

        static float interpolate(float v1, float v2, float lerp)
        {
            float f = (float) (1.0 - Math.Cos(Math.PI * lerp)) + 0.5f;
            return v1 * (1 - f) + v2 * f;
        }

        static float smoothedNoise(int x, int y, int octaves)
        {
            float corners = (noiseFunc(x - 1, y - 1, octaves) + noiseFunc(x + 1, y - 1, octaves) + noiseFunc(x - 1, y + 1, octaves) + noiseFunc(x + 1, y + 1, octaves)) / 16.0f;
            float sides = (noiseFunc(x - 1, y, octaves) + noiseFunc(x + 1, y, octaves) + noiseFunc(x, y - 1, octaves) + noiseFunc(x, y + 1, octaves)) / 8.0f;
            float center = (noiseFunc(x, y, octaves)) / 4.0f;
            return corners + sides + center;
        }

        static float interpolatedNoise(float x, float y, int octaves)
        {
            int xint = (int) Math.Floor(x);
            int yint = (int)Math.Floor(y);
            float xfrac = x - xint;
            float yfrac = y - yint;

            float v1 = smoothedNoise(xint, yint, octaves);
            float v2 = smoothedNoise(xint + 1, yint, octaves);
            float v3 = smoothedNoise(xint, yint + 1, octaves);
            float v4 = smoothedNoise(xint + 1, yint + 1, octaves);

            return interpolate(interpolate(v1, v2, xfrac), interpolate(v3, v4, xfrac), yfrac);
        }

        static float perlinNoise(float x, float y, int octaves)
        {
            float rv = 0, freq = 1, amp = 128, persistence = 0.3f;

            for (int i = 0; i < octaves; ++i, freq *= 2.0f, amp *= persistence)
                rv += interpolatedNoise(x * freq, y * freq, i) * amp;

            return rv;
        }

        public static ZArrayDescriptor createPerlin1d(int width, int height, int octaves)
        {
            ZArrayDescriptor z = new ZArrayDescriptor();
            z.width = height;
            z.height = width;
            z.array = new float[z.width, z.height];

            float y = 0, x = 0;
            for (int i = 0; i < z.width; ++i, x += 0.02f)
            {
                for (int j = 0; j < z.height; ++j, y += 0.02f)
                {
                    z.array[i, j] = perlinNoise(x, y, octaves);
                }
                y = 0.0f;
                Console.WriteLine(i + "/" + z.width);
            }

            return z;
        }
    }
}
