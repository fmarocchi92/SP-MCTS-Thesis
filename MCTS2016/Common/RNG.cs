using System;

namespace Common
{
	public static class RNG
	{
		private static MersenneTwister rnd = new MersenneTwister((uint)DateTime.Now.Ticks & 0x0000FFFF);

		public static int Next()
		{
			return rnd.Next();
		}

		public static int Next(int minValue, int maxValue)
		{
			return rnd.Next(minValue, minValue);
		}

		public static double NextDouble()
		{
			return rnd.NextDouble();
		}

		public static int Next(int maxValue) {
			return rnd.Next (maxValue);
		}

		public static void Seed(uint seed) 
		{
			uint rseed = seed;
			if (seed == 0)
				rseed = (uint)DateTime.Now.Ticks & 0x0000FFFF;
				
			rnd = new MersenneTwister (rseed);
		}
	}
}