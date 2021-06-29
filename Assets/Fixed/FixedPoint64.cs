// ==============================================
// Author：qiuyukun
// Date:2019-05-23 10:07:21
// ==============================================

namespace Fixed
{
    /// <summary>
    /// 定点数Q47.16
    /// </summary>
    public struct FixedPoint64
    {
        //小数部分占用的位数
        public const int FRACTIONAL_BITS = 16;
        //整数部分占用的位数
        private const int INTEGER_BITS = sizeof(long) * 8 - FRACTIONAL_BITS;
        //小数部分的位遮罩
        public const int FRACTIONAL_BIT_MASK = (int)(ulong.MaxValue >> INTEGER_BITS);
        //小数范围
        public const int FRACTION_RANGE = FRACTIONAL_BIT_MASK + 1;

        public static readonly FixedPoint64 MaxValue = new FixedPoint64(long.MaxValue);
        public static readonly FixedPoint64 MinValue = new FixedPoint64(long.MinValue);
        public static readonly FixedPoint64 zero = new FixedPoint64(0);
        public static readonly FixedPoint64 one = new FixedPoint64(RAW_ONE);
        public static readonly FixedPoint64 half = new FixedPoint64(1,2);

        public const long RAW_ONE = 1L << FRACTIONAL_BITS;

        public long RawValue => _rawValue;
        private long _rawValue;

        public FixedPoint64(long rawValue)
        {
            _rawValue = rawValue;
        }

        public FixedPoint64(long numerator, long denominator)
        {
            if (denominator == 0)
                _rawValue = MaxValue._rawValue;
            else
                _rawValue = (((numerator << (FRACTIONAL_BITS + 1)) / denominator) + 1) >> 1;
        }

        //整数
        public int Integer
        {
            get
            {
                return (int)(_rawValue >> FRACTIONAL_BITS);
            }
        }

        //单精度浮点数
        public float Single
        {
            get
            {
                return (_rawValue >> FRACTIONAL_BITS) + (_rawValue & FRACTIONAL_BIT_MASK) / (float)FRACTION_RANGE;
            }
        }

        //双精度浮点数
        public double Double
        {
            get
            {
                return (_rawValue >> FRACTIONAL_BITS) + (_rawValue & FRACTIONAL_BIT_MASK) / (double)FRACTION_RANGE;
            }
        }

        #region operator

        public static FixedPoint64 operator *(FixedPoint64 l, FixedPoint64 r)
        {
            return new FixedPoint64((l._rawValue * r._rawValue) >> FRACTIONAL_BITS);
        }

        public static FixedPoint64 operator *(FixedPoint64 l, int r)
        {
            return new FixedPoint64(l._rawValue * r);
        }

        public static FixedPoint64 operator *(int l, FixedPoint64 r)
        {
            return r * l;
        }

        public static FixedPoint64 operator /(FixedPoint64 l, FixedPoint64 r)
        {
            if (r == FixedPoint64.zero) return FixedPoint64.zero;
            return new FixedPoint64((l._rawValue << FRACTIONAL_BITS) / r._rawValue);
        }

        public static FixedPoint64 operator /(FixedPoint64 l, int r)
        {
            if (r == 0) return FixedPoint64.zero;
            return new FixedPoint64(l._rawValue / r);
        }

        public static FixedPoint64 operator /(int l, FixedPoint64 r)
        {
            if (r == FixedPoint64.zero) return FixedPoint64.zero;
            return new FixedPoint64((long)l << FRACTIONAL_BITS) / r;
        }

        public static FixedPoint64 operator +(FixedPoint64 l, FixedPoint64 r)
        {
            return new FixedPoint64(l._rawValue + r._rawValue);
        }

        public static FixedPoint64 operator +(FixedPoint64 l, int r)
        {
            return new FixedPoint64(l._rawValue + (((long)r) << FRACTIONAL_BITS));
        }

        public static FixedPoint64 operator -(FixedPoint64 l, FixedPoint64 r)
        {
            return new FixedPoint64(l._rawValue - r._rawValue);
        }

        public static FixedPoint64 operator -(FixedPoint64 l, int r)
        {
            return new FixedPoint64(l._rawValue - ((long)r << FRACTIONAL_BITS));
        }

        public static FixedPoint64 operator -(int l, FixedPoint64 r)
        {
            return new FixedPoint64(((long)l << FRACTIONAL_BITS) - r._rawValue);
        }

        public static bool operator <(FixedPoint64 l, FixedPoint64 r)
        {
            return l._rawValue < r._rawValue;
        }

        public static bool operator <(int l, FixedPoint64 r)
        {
            return ((long)l << FRACTIONAL_BITS) < r._rawValue;
        }

        public static bool operator <(FixedPoint64 l, int r)
        {
            return l._rawValue < ((long)r << FRACTIONAL_BITS);
        }

        public static bool operator >(FixedPoint64 l, FixedPoint64 r)
        {
            return l._rawValue > r._rawValue;
        }

        public static bool operator >(int l, FixedPoint64 r)
        {
            return ((long)l << FRACTIONAL_BITS) > r._rawValue;
        }

        public static bool operator >(FixedPoint64 l, int r)
        {
            return l._rawValue > ((long)r << FRACTIONAL_BITS);
        }

        public static bool operator ==(FixedPoint64 l, FixedPoint64 r)
        {
            return l._rawValue == r._rawValue;
        }

        public static bool operator ==(int l, FixedPoint64 r)
        {
            return ((long)l << FRACTIONAL_BITS) == r._rawValue;
        }

        public static bool operator ==(FixedPoint64 l, int r)
        {
            return l._rawValue == ((long)r << FRACTIONAL_BITS);
        }

        public static bool operator !=(FixedPoint64 l, FixedPoint64 r)
        {
            return l._rawValue != r._rawValue;
        }

        public static bool operator !=(FixedPoint64 l, int r)
        {
            return l._rawValue != ((long)r << FRACTIONAL_BITS);
        }

        public static bool operator !=(int l, FixedPoint64 r)
        {
            return ((long)l << FRACTIONAL_BITS) != r._rawValue;
        }

        public static bool operator <=(FixedPoint64 l, FixedPoint64 r)
        {
            return l._rawValue <= r._rawValue;
        }

        public static bool operator <=(int l, FixedPoint64 r)
        {
            return ((long)l << FRACTIONAL_BITS) <= r._rawValue;
        }

        public static bool operator <=(FixedPoint64 l, int r)
        {
            return l._rawValue <= ((long)r << FRACTIONAL_BITS);
        }

        public static bool operator >=(FixedPoint64 l, FixedPoint64 r)
        {
            return l._rawValue >= r._rawValue;
        }
        public static bool operator >=(int l, FixedPoint64 r)
        {
            return ((long)l << FRACTIONAL_BITS) >= r._rawValue;
        }

        public static bool operator >=(FixedPoint64 l, int r)
        {
            return l._rawValue >= ((long)r << FRACTIONAL_BITS);
        }

        public static FixedPoint64 operator >>(FixedPoint64 l, int r)
        {
            return new FixedPoint64(l._rawValue >> r);
        }

        public static FixedPoint64 operator <<(FixedPoint64 l, int r)
        {
            return new FixedPoint64(l._rawValue << r);
        }

        #endregion

        #region explicit
        public static explicit operator FixedPoint64(long value)
        {
            return new FixedPoint64(value * RAW_ONE);
        }

        public static explicit operator FixedPoint64(int value)
        {
            return new FixedPoint64(value * RAW_ONE);
        }

        public static FixedPoint64 operator -(FixedPoint64 v)
        {
            v._rawValue = -v._rawValue;
            return v;
        }
        #endregion


        public override string ToString()
        {
            return Double.ToString("f4");
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
