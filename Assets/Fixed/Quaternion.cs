// ==============================================
// Author：qiuyukun
// Date:2019-05-23 21:07:38
// ==============================================

namespace Fixed
{
    public struct Quaternion
    {
        public static readonly Quaternion zero;

        public FixedPoint64 x;
        public FixedPoint64 y;
        public FixedPoint64 z;
        public FixedPoint64 w;

        public Vector3 V { get { return new Vector3(x, y, z); } }

        public Quaternion(FixedPoint64 x, FixedPoint64 y, FixedPoint64 z, FixedPoint64 w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        //x
        public FixedPoint64 pitch
        {
            get
            {
                var n1 = x * z - w * y;
                var n2 = FixedPoint64.half - x * x - y * y;
                return Math.Atan2(n1.RawValue, n2.RawValue) * Math.Rad2Deg;
            }
        }

        //y
        public FixedPoint64 yaw
        {
            get
            {
                FixedPoint64 sp = -2 * (y * z + w * x);
                return Math.Asin(sp) * Math.Rad2Deg;
            }
        }

        //z
        public FixedPoint64 roll
        {
            get
            {
                var n1 = x * y - w * z;
                var n2 = FixedPoint64.half - x * x - z * z;
                return Math.Atan2(n1.RawValue, n2.RawValue) * Math.Rad2Deg;
            }
        }

        //转欧拉角
        public Vector3 eulerAngles
        {
            get
            {
                return new Vector3(pitch, yaw, roll);
            }
        }

        //欧拉角转四元数
        public static Quaternion Euler(int x, int y, int z)
        {
            return Euler((FixedPoint64)x, (FixedPoint64)y, (FixedPoint64)z);
        }

        //欧拉角转四元数
        public static Quaternion Euler(FixedPoint64 x, FixedPoint64 y, FixedPoint64 z)
        {
            FixedPoint64 eulerX = (x >> 1) * Math.Deg2Rad;
            FixedPoint64 cX = Math.Cos(eulerX);
            FixedPoint64 sX = Math.Sin(eulerX);
            FixedPoint64 eulerY = (y >> 1) * Math.Deg2Rad;
            FixedPoint64 cY = Math.Cos(eulerY);
            FixedPoint64 sY = Math.Sin(eulerY);
            FixedPoint64 eulerZ = (z >> 1) * Math.Deg2Rad;
            FixedPoint64 cZ = Math.Cos(eulerZ);
            FixedPoint64 sZ = Math.Sin(eulerZ);

            FixedPoint64 iw = cX * cY * cZ + sX * sY * sZ;
            FixedPoint64 ix = (-cX * sY * cZ) - (sX * cY * sZ);
            FixedPoint64 iy = cX * sY * sZ - sX * cY * cZ;
            FixedPoint64 iz = sX * sY * cZ - cX * cY * sZ;

            Quaternion q = new Quaternion(ix, iy, iz, iw);
            return q;
        }

        //叉乘
        public static Quaternion operator *(Quaternion l, Quaternion r)
        {
            FixedPoint64 w = l.w * r.w - (l.V * r.V);
            Vector3 v = l.w * r.V + r.w * l.V + Vector3.Cross(r.V, l.V);
            return new Quaternion()
            {
                x = v.x,
                y = v.y,
                z = v.z,
                w = w
            };
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", x.ToString(), y.ToString(), z.ToString(), w.ToString());
        }
    }
}