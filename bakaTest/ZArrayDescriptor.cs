using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
