using System;
using System.Numerics;

namespace ViveToOscVRC
{

    public static class MatrixExtension
    {
        /// <summary>
        /// Converts a <see cref="HmdMatrix34_t"/> to a <see cref="Matrix4x4"/>.
        /// <br/>
        /// <br/>
        /// From: <br/>
        /// 11 12 13 14 <br/>
        /// 21 22 23 24 <br/>
        /// 31 32 33 34
        /// <br/><br/>
        /// To: <br/>
        /// 11 12 13 XX <br/>
        /// 21 22 23 XX <br/>
        /// 31 32 33 XX <br/>
        /// 14 24 34 XX
        /// </summary>
        public static Matrix4x4 ToMatrix4x4(this HmdMatrix34_t matrix)
        {
            return new Matrix4x4(
                matrix.m0, matrix.m1, matrix.m2, 0,
                matrix.m4, matrix.m5, matrix.m6, 0,
                matrix.m8, matrix.m9, matrix.m10, 0,
                matrix.m3, matrix.m7, matrix.m11, 1
            );
        }
    }
}
