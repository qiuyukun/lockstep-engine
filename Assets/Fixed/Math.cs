// ==============================================
// Author：qiuyukun
// Date:2019-05-23 22:53:14
// ==============================================

namespace Fixed
{
    public static class Math
    {
        //小数精度
        public const int PRECISION_FACTOR = 10000;
        public static readonly FixedPoint64 Pi = new FixedPoint64(31416, PRECISION_FACTOR);
        public static readonly FixedPoint64 HalPi = Pi / 2;
        public static readonly FixedPoint64 twoPi = Pi * 2;
        public static readonly FixedPoint64 AngleMax = new FixedPoint64(360, 1);
        public static readonly FixedPoint64 HalfAngleMax = AngleMax / 2;
        public static readonly FixedPoint64 QuarterAngleMax = HalfAngleMax / 2;
        public static readonly FixedPoint64 Deg2Rad = new FixedPoint64(175, PRECISION_FACTOR);
        public static readonly FixedPoint64 Rad2Deg = new FixedPoint64(572958, PRECISION_FACTOR);
        public static readonly FixedPoint64 DivAngle = new FixedPoint64(150000, PRECISION_FACTOR);
        public static readonly FixedPoint64 HalfDivAngle = new FixedPoint64(75000, PRECISION_FACTOR);
        public static readonly FixedPoint64 Log2Max = new FixedPoint64(RAW_LOG2MAX);
        public static readonly FixedPoint64 Log2Min = new FixedPoint64(RAW_LOG2MIN);
        public static readonly FixedPoint64 Ln2 = new FixedPoint64(RAW_LN2);

        private const long RAW_LOG2MAX = 0x1F0000;
        private const long RAW_LOG2MIN = -0x200000;
        private const long RAW_LN2 = 0xB172;


        public static int Clamp(int a, int min, int max)
        {
            if (a < min)
            {
                return min;
            }
            if (a > max)
            {
                return max;
            }
            return a;
        }

        public static FixedPoint64 Clamp(FixedPoint64 a, FixedPoint64 min, FixedPoint64 max)
        {
            if (a < min)
            {
                return min;
            }
            if (a > max)
            {
                return max;
            }
            return a;

        }

        public static FixedPoint64 Lerp(FixedPoint64 a, FixedPoint64 b, FixedPoint64 c)
        {
            return (b - a) * c + a;
        }

        public static FixedPoint64 InverseLerp(FixedPoint64 a, FixedPoint64 b, FixedPoint64 c)
        {
            if (a == b)
                return FixedPoint64.zero;
            return Clamp((c - a) / (b - a), FixedPoint64.zero, FixedPoint64.one);
        }

        public static FixedPoint64 Min(FixedPoint64 a, FixedPoint64 b)
        {
            return a < b ? a : b;
        }

        public static FixedPoint64 Max(FixedPoint64 a, FixedPoint64 b)
        {
            return a < b ? b : a;
        }

        public static int Max(int a, int b)
        {
            return a < b ? b : a;
        }

        public static int Min(int a, int b)
        {
            return a > b ? b : a;
        }

        public static int Sign(FixedPoint64 value)
        {
            return value >= 0 ? 1 : -1;
        }

        public static long Abs(long val)
        {
            if (val < 0)
                return -val;
            return val;
        }

        public static int Abs(int val)
        {
            if (val < 0)
                return -val;
            return val;
        }

        public static FixedPoint64 Abs(FixedPoint64 v)
        {
            return new FixedPoint64(Abs(v.RawValue));
        }

        public static long Divide(long a, long b)
        {
            if (a == 0)
            {
                return 0;
            }
            if (b == 0)
            {
                return long.MaxValue;
            }
            return a / b;
        }

        #region 三角函数


        public static FixedPoint64 Sin(FixedPoint64 sinVal)
        {
            //索引值求的原理见对应函数内注释 
            int index = FixedSinCosTable.getIndex(sinVal.RawValue, FixedPoint64.FRACTION_RANGE);
            return new FixedPoint64(FixedSinCosTable.sin_table[index], PRECISION_FACTOR);
        }

        public static FixedPoint64 Cos(FixedPoint64 cosVal)
        {
            //索引值求的原理见对应函数内注释 
            int index = FixedSinCosTable.getIndex(cosVal.RawValue, FixedPoint64.FRACTION_RANGE);
            return new FixedPoint64(FixedSinCosTable.cos_table[index], PRECISION_FACTOR);
        }

        public static FixedPoint64 Tan(FixedPoint64 val)
        {
            FixedPoint64 sin = Sin(val);
            FixedPoint64 cos = Cos(val);
            if (cos == 0)
            {
                return FixedPoint64.MaxValue;
            }
            return sin / cos;

        }

        public static FixedPoint64 Atan(FixedPoint64 cosVal)
        {
            return Atan2(cosVal.RawValue, FixedPoint64.FRACTION_RANGE);
        }

        public static FixedPoint64 Atan2(long y, long x)
        {
            int num;
            int num2;
            if (x < 0)
            {
                if (y < 0)
                {
                    //第三象限
                    x = -x;
                    y = -y;
                    num = 1;
                }
                else
                {
                    //第二象限
                    x = -x;
                    num = -1;
                }
                //-PI 乘以10000
                num2 = -31416;
            }
            else
            {
                if (y < 0)
                {
                    //第四象限
                    y = -y;
                    num = -1;
                }
                else
                {
                    //第一象限
                    num = 1;
                }
                num2 = 0;
            }
            int dIM = FixedAtanTable.DIM;   //2^7 = 128
            long num3 = (long)(dIM - 1);  //127
            //下边这段的意思是，把xy归一化后映射到0-127闭区间上，然后去查表
            //y做行，x做列去查询设置好的二维表
            long b = (long)((x >= y) ? x : y);
            int num4 = (int)Divide((long)x * num3, b);
            int num5 = (int)Divide((long)y * num3, b);
            int num6 = FixedAtanTable.table[num5 * dIM + num4];
            return new FixedPoint64(((num6 + num2) * num), PRECISION_FACTOR);
        }

        public static FixedPoint64 Acos(FixedPoint64 cosVal)
        {
            //计算acos就比较简单了，因为cos的取值就是-1~1，不存在无穷的问题
            //如果把cos比作x/length,当length等于1的时候，x就是cos值，所以只需要把-1~1平均分成IntAcosTable.COUNT份
            //然后做一个-1~1与0~IntAcosTable.COUNT的映射即可，该函数的功能就是做这个映射 
            //由于cos函数与角度不是线性的，即两者的变化率不一样，所以多少有点值分配不均匀的问题，不过分成1024份，影响不大
            int num = (cosVal * FixedAcosTable.HALF_COUNT).Integer + FixedAcosTable.HALF_COUNT;
            num = Clamp(num, 0, FixedAcosTable.COUNT);
            return new FixedPoint64(FixedAcosTable.table[num], PRECISION_FACTOR);
        }

        /// <summary>
        /// 返回的是-pi/2 到 pi/2的弧度值
        /// </summary>
        /// <param name="nom"></param>
        /// <param name="den"></param>
        /// <returns></returns>
        public static FixedPoint64 Asin(FixedPoint64 sinVal)
        {
            int num = (sinVal * FixedAcosTable.HALF_COUNT).Integer + FixedAsinTable.HALF_COUNT;
            num = Clamp(num, 0, FixedAsinTable.COUNT);
            return new FixedPoint64(FixedAsinTable.table[num], PRECISION_FACTOR);
        }

        #endregion


        #region sqrt
        public static uint Sqrt32(uint a)
        {
            uint num = 0u;
            uint num2 = 0u;
            for (int i = 0; i < 16; i++)
            {
                num2 <<= 1;
                num <<= 2;
                num += a >> 30;
                a <<= 2;
                if (num2 < num)
                {
                    num2 += 1u;
                    num -= num2;
                    num2 += 1u;
                }
            }
            return num2 >> 1 & 65535u;
        }

        public static ulong Sqrt64(ulong a)
        {
            ulong num = 0uL;
            ulong num2 = 0uL;
            for (int i = 0; i < 32; i++)
            {
                num2 <<= 1;
                num <<= 2;
                num += a >> 62;
                a <<= 2;
                if (num2 < num)
                {
                    num2 += 1uL;
                    num -= num2;
                    num2 += 1uL;
                }
            }
            return num2 >> 1 & unchecked((ulong)-1);
        }

        public static int Sqrt(long a)
        {
            if (a <= 0L)
            {
                return 0;
            }
            if (a <= unchecked((long)(unchecked((ulong)-1))))
            {
                return (int)Sqrt32((uint)a);
            }
            return (int)Sqrt64((ulong)a);
        }

        public static FixedPoint64 Sqrt(FixedPoint64 a)
        {
            if (a < FixedPoint64.zero)
                return FixedPoint64.zero;
            long num = Sqrt(a.RawValue << FixedPoint64.FRACTIONAL_BITS);
            return new FixedPoint64(num);
        }
        #endregion

    }
}

