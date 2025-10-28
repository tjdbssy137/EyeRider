using System;
using System.Linq;

public class RandomManager
{
    private class MT19937
    {
        private const int N = 624;
        private const int M = 397;
        private const uint MATRIX_A = 0x9908b0df;
        private const uint UPPER_MASK = 0x80000000;
        private const uint LOWER_MASK = 0x7fffffff;

        private uint[] mt;
        private int mti;

        public MT19937(int seed)
        {
            mt = new uint[N];
            mt[0] = (uint)seed;
            for (mti = 1; mti < N; mti++)
            {
                mt[mti] = (uint)(1812433253 * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + mti);
            }
        }

        public uint GenerateUInt()
        {
            uint y;
            uint[] mag01 = new uint[2] { 0x0, MATRIX_A };

            if (mti >= N)
            {
                int kk;
                for (kk = 0; kk < N - M; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1];
                }
                for (; kk < N - 1; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1];
                }
                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1];

                mti = 0;
            }

            y = mt[mti++];
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680;
            y ^= (y << 15) & 0xefc60000;
            y ^= (y >> 18);

            return y;
        }

        public int GenerateInt(int minInclude, int maxExclude)
        {
            uint range = (uint)(maxExclude - minInclude);
            uint value = GenerateUInt();
            return (int)(minInclude + (value % range));
        }

        public double GenerateDouble()
        {
            return GenerateUInt() * (1.0 / 4294967296.0);
        }
    }

    private MT19937 _random;

    public RandomManager()
    {
        _random = new MT19937((int)DateTime.Now.Ticks);
    }

    public int GetInt(int minInclude, int maxExclude)
    {
        return _random.GenerateInt(minInclude, maxExclude);
    }

    public double GetDouble(double minInclude, double maxExclude)
    {
        return _random.GenerateDouble() * (maxExclude - minInclude) + minInclude;
    }

    public string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
                          .Select(s => s[_random.GenerateInt(0, s.Length)]).ToArray());
    }
}