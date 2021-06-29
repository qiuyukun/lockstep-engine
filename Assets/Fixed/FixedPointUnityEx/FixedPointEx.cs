// ==============================================
// Author：qiuyukun
// Date:2019-05-29 23:24:24
// ==============================================

using Fixed;

public static class FixedPointEx
{
    public static UnityEngine.Vector3 ToUnityVector3(this Vector3 v)
    {
        return new UnityEngine.Vector3(v.x.Single, v.y.Single, v.z.Single);
    }

    public static UnityEngine.Quaternion ToUnityQuaternion(this Quaternion q)
    {
        //计算方式不一样,所以得从欧拉角转
        var eulerAngles = q.eulerAngles;
        return UnityEngine.Quaternion.Euler(eulerAngles.x.Single, eulerAngles.y.Single, eulerAngles.z.Single);
    }
}
