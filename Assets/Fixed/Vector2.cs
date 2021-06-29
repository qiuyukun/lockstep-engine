// ==============================================
// Author：qiuyukun
// Date:2019-07-24 11:44:10
// ==============================================

using UnityEngine;
using System.Collections;

namespace Fixed
{
    /// <summary>
    /// 2D向量
    /// </summary>
    public struct Vector2
    {
        public static readonly Vector2 zero;
        public static readonly Vector2 one = new Vector2(FixedPoint64.one, FixedPoint64.one);

        public FixedPoint64 x;
        public FixedPoint64 y;

        public Vector2(FixedPoint64 x, FixedPoint64 y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2(int x, int y)
        {
            this.x = (FixedPoint64)x;
            this.y = (FixedPoint64)y;
        }

        public FixedPoint64 Magnitude
        {
            get
            {
                var p = x * x + y * y;
                return Math.Sqrt(p);
            }
        }

        public FixedPoint64 SqrtMagnitude
        {
            get
            {
                return x * x + y * y;
            }
        }

        public Vector2 Normalized
        {
            get
            {
                return this / Magnitude;
            }
        }


        public void Normalize()
        {
            var v = this / Magnitude;
            x = v.x;
            y = v.y;
        }

        #region static

        //点乘
        public static FixedPoint64 operator *(Vector2 l, Vector2 r)
        {
            return l.x * r.x + l.y * r.y;
        }

        public static Vector2 operator *(Vector2 l, FixedPoint64 r)
        {
            return new Vector2(l.x * r, l.y * r);
        }

        public static Vector2 operator *(FixedPoint64 l, Vector2 r)
        {
            return r * l;
        }

        public static Vector2 operator /(Vector2 l, FixedPoint64 r)
        {
            return new Vector2(l.x / r, l.y / r);
        }

        public static implicit operator Vector3(Vector2 v)
        {
            return new Vector3(v.x, v.y, FixedPoint64.zero);
        }

        public static bool operator ==(Vector2 l, Vector2 r)
        {
            return l.x == r.x && l.y == r.y;
        }

        public static bool operator !=(Vector2 l, Vector2 r)
        {
            return l.x != r.x || l.y != r.y;
        }

        #endregion


        public override string ToString()
        {
            return string.Format("x:{0}, y:{1}", x, y);
        }
    }
}

