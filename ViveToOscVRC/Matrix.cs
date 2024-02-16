using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace ViveToOscVRC
{
    public static class MatrixExtensions
    {
        public static Matrix4x4 ToMatrix4x4(this HmdMatrix34_t matrix)
        {
            return new Matrix4x4(
                matrix.m0,  matrix.m1,  matrix.m2,  matrix.m3,
                matrix.m4,  matrix.m5,  matrix.m6,  matrix.m7,
                matrix.m8,  matrix.m9,  matrix.m10,  matrix.m11,
                0,          0,          0,          1);
        }

        public static Vector3 ToDirectionVector(this Matrix4x4 matrix)
        {
            return new Vector3(matrix.M41, matrix.M42, matrix.M43);
        }
    }
}
