using System;
using System.Linq;

namespace Calculator
{
    public class SimpleCalculator
    {
        static public int StaticAdd(int left, int right)
        {
            return left + right;
        }

        static private int Sum(params int[] args)
        {
            return args.Sum();
        }

        public int AddViaMethodChainAndLinq(int i1, int i2, int i3)
        {
            return Sum(i1, i2, i3);
        }

        public int Add(int left, int right)
        {
            return left + right;
        }

        public int MultiAdd(int i1, int i2, int i3, int i4)
        {
            return i1 + i2 + i3 + i4;
        }

        public int MixedAdd(short i1, short i2, short i3, int i4, int i5, int i6)
        {
            int shortSum = i1 + i2 + i3;
            int intSum = i4 + i5 + i6;
            int sum = shortSum + intSum;
            return sum;
        }

        public int Divide(int left, int right)
        {
            if (right == 0)
            {
                throw new ArgumentException();
            }
            return left / right;
        }

        public int Subtract(int left, int right)
        {
            return left - right;
        }
    }
}

