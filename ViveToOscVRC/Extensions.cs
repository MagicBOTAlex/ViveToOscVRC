using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

// From OpenVR.NET

namespace ViveToOscVRC
{
    public static class Extensions
    {
        public static Vector3 ExtractPosition(this HmdMatrix34_t mat)
        {
            return new Vector3(mat.m3, mat.m7, -mat.m11);
        }

        public static Quaternion ExtractRotation(this HmdMatrix34_t mat)
        {
            Quaternion q = default;
            q.W = MathF.Sqrt(MathF.Max(0, 1 + mat.m0 + mat.m5 + mat.m10)) / 2;
            q.X = MathF.Sqrt(MathF.Max(0, 1 + mat.m0 - mat.m5 - mat.m10)) / 2;
            q.Y = MathF.Sqrt(MathF.Max(0, 1 - mat.m0 + mat.m5 - mat.m10)) / 2;
            q.Z = MathF.Sqrt(MathF.Max(0, 1 - mat.m0 - mat.m5 + mat.m10)) / 2;
            q.X = MathF.CopySign(q.X, mat.m9 - mat.m6);
            q.Y = MathF.CopySign(q.Y, mat.m2 - mat.m8);
            q.Z = MathF.CopySign(q.Z, mat.m1 - mat.m4);

            var scale = 1 / q.LengthSquared();
            return new Quaternion(q.X * -scale, q.Y * -scale, q.Z * -scale, q.W * scale);
        }

        public static Vector3 ToDegrees(this Vector3 vector)
        {
            return new Vector3(
                (float)(vector.X * (180.0 / Math.PI)),
                (float)(vector.Y * (180.0 / Math.PI)),
                (float)(vector.Z * (180.0 / Math.PI))
            );
        }

        public static Vector3 Qua2Eul(this Vector3 direction)
        {
            // Calculate yaw (rotation around Y-axis)
            float yaw = (float)Math.Atan2(direction.X, direction.Z);

            // Calculate pitch (rotation around X-axis)
            float pitch = (float)Math.Atan2(direction.Y, Math.Sqrt(direction.X * direction.X + direction.Z * direction.Z));

            // Calculate roll (rotation around Z-axis)
            float roll = 0; // Default roll is 0 when up is +Y and forward is +Z
            if (direction.X != 0 || direction.Z != 0) // Ensure not dividing by zero
            {
                roll = (float)Math.Atan2(direction.Z, direction.X);
            }

            // Convert radians to degrees
            yaw = MathHelper.ToDegrees(yaw);
            pitch = MathHelper.ToDegrees(pitch);
            roll = MathHelper.ToDegrees(roll);

            return new Vector3(yaw, pitch, roll);
        }

        public static Vector3 GetDirection(this Matrix4x4 matrix)
        {
            // Extract translation component
            Vector3 translation = matrix.Translation;

            // Extract rotation component
            Matrix4x4 rotationMatrix = Matrix4x4.CreateScale(matrix.GetScale()) * Matrix4x4.CreateTranslation(-translation);
            Quaternion rotationQuaternion = Quaternion.CreateFromRotationMatrix(rotationMatrix);

            // Convert rotation quaternion to direction vector
            return Vector3.Transform(Vector3.UnitZ, Matrix4x4.CreateFromQuaternion(rotationQuaternion));
        }

        // Helper method to get the scale of the matrix
        private static Vector3 GetScale(this Matrix4x4 matrix)
        {
            return new Vector3(
                (float)Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12 + matrix.M13 * matrix.M13),
                (float)Math.Sqrt(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22 + matrix.M23 * matrix.M23),
                (float)Math.Sqrt(matrix.M31 * matrix.M31 + matrix.M32 * matrix.M32 + matrix.M33 * matrix.M33)
            );
        }
    }
}
