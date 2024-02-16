using System.Numerics;
//using Valve.VR;
public static class QuaternionExtensions
{
    public static Vector3 ToEulerAngles(this Quaternion q)
    {
        // roll (x-axis rotation)
        float sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
        float cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        float roll = (float)Math.Atan2(sinr_cosp, cosr_cosp);

        // pitch (y-axis rotation)
        float sinp = 2 * (q.W * q.Y - q.Z * q.X);
        float pitch = (float)Math.Asin(sinp);

        // yaw (z-axis rotation)
        float siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
        float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        float yaw = (float)Math.Atan2(siny_cosp, cosy_cosp);

        return new Vector3(MathHelper.ToDegrees(pitch) + 180, MathHelper.ToDegrees(roll) + 180, MathHelper.ToDegrees(yaw) + 180);
    }

    // https://stackoverflow.com/questions/1031005/is-there-an-algorithm-for-converting-quaternion-rotations-to-euler-angle-rotatio
    public static Vector3 Qua2Eul(this Quaternion q)
    {
        float f1 = -2 * (q.X * q.Y - q.W * q.Z);
        float f2 = q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z;
        float f3 = 2 * (q.Y * q.Z + q.W * q.X);
        float f4 = -2 * (q.X * q.Z - q.W * q.Y);
        float f5 = q.W* q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z;

        Vector3 euler = default;
        euler.X = (float)Math.Atan2(f4, f5);
        euler.Y = (float)Math.Asin(f3);
        euler.Z = (float)Math.Atan2(f1, f2);

        return euler;
    }
}
