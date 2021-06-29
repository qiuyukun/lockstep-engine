// ==============================================
// Author：qiuyukun
// Date:2019-05-23 11:48:12
// ==============================================

namespace Fixed
{
    /// <summary>
    /// 3D向量
    /// </summary>
    public struct Vector3
    {
        public static readonly Vector3 zero;
        public static readonly Vector3 one = new Vector3(FixedPoint64.one, FixedPoint64.one, FixedPoint64.one);
        public static readonly Vector3 forward = new Vector3(FixedPoint64.zero, FixedPoint64.zero, FixedPoint64.one);
        public static readonly Vector3 back = -forward;
        public static readonly Vector3 up = new Vector3(FixedPoint64.zero, FixedPoint64.one, FixedPoint64.zero);
        public static readonly Vector3 down = -up;
        public static readonly Vector3 right = new Vector3(FixedPoint64.one, FixedPoint64.zero, FixedPoint64.zero);
        public static readonly Vector3 left = -right;

        public FixedPoint64 x;
        public FixedPoint64 y;
        public FixedPoint64 z;

        public FixedPoint64 Magnitude
        {
            get
            {
                var p = x * x + y * y + z * z;
                return Math.Sqrt(p);
            }
        }

        public FixedPoint64 SqrtMagnitude
        {
            get
            {
                return x * x + y * y + z * z;
            }
        }

        public Vector3 Normalized
        {
            get
            {
                return this / Magnitude;
            }
        }

        public Vector3(FixedPoint64 x, FixedPoint64 y, FixedPoint64 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void Normalize()
        {
            var sqrtMagnitude = SqrtMagnitude;
            if (sqrtMagnitude == 0)
                return;
            var v = this / Magnitude;
            x = v.x;
            y = v.y;
            z = v.z;
        }

        #region static
        //叉乘
        public static Vector3 Cross(Vector3 l, Vector3 r)
        {
            return new Vector3()
            {
                x = l.y * r.z - l.z * r.y,
                y = l.z * r.x - l.x * r.z,
                z = l.x * r.y - l.y * r.x,
            };
        }
        //点乘
        public static FixedPoint64 operator *(Vector3 l, Vector3 r)
        {
            return l.x * r.x + l.y * r.y + l.z * r.z;
        }

        public static Vector3 operator *(Vector3 l, FixedPoint64 r)
        {
            return new Vector3(l.x * r, l.y * r, l.z * r);
        }

        public static Vector3 operator *(Vector3 l, int r)
        {
            return new Vector3(l.x * r, l.y * r, l.z * r);
        }

        public static Vector3 operator *(FixedPoint64 l, Vector3 r)
        {
            return r * l;
        }

        public static Vector3 operator /(Vector3 l, FixedPoint64 r)
        {
            return new Vector3(l.x / r, l.y / r, l.z / r);
        }

        public static Vector3 operator +(Vector3 l, Vector3 r)
        {
            return new Vector3
            {
                x = l.x + r.x,
                y = l.y + r.y,
                z = l.z + r.z,
            };
        }

        public static Vector3 operator -(Vector3 v)
        {
            v = v * -FixedPoint64.one;
            return v;
        }


        public static bool operator ==(Vector3 l, Vector3 r)
        {
            return l.x == r.x && l.y == r.y && l.z == r.z;
        }

        public static bool operator !=(Vector3 l, Vector3 r)
        {
            return !(l == r);
        }

        public static explicit operator Vector2(Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x.ToString(), y.ToString(), z.ToString());
        }
    }
}

