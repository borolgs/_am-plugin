using Autodesk.Revit.DB;
using RevitWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Matrix
    {
        private double[] elements;

        private Matrix(double[] elements)
        {
            this.elements = elements;
        }

        public double[] ToArray()
        {
            return elements;
        }

        public Matrix Multiply(Matrix other)
        {
            var dst = Matrix.Identity();

            var b00 = other.elements[0 * 4 + 0];
            var b01 = other.elements[0 * 4 + 1];
            var b02 = other.elements[0 * 4 + 2];
            var b03 = other.elements[0 * 4 + 3];
            var b10 = other.elements[1 * 4 + 0];
            var b11 = other.elements[1 * 4 + 1];
            var b12 = other.elements[1 * 4 + 2];
            var b13 = other.elements[1 * 4 + 3];
            var b20 = other.elements[2 * 4 + 0];
            var b21 = other.elements[2 * 4 + 1];
            var b22 = other.elements[2 * 4 + 2];
            var b23 = other.elements[2 * 4 + 3];
            var b30 = other.elements[3 * 4 + 0];
            var b31 = other.elements[3 * 4 + 1];
            var b32 = other.elements[3 * 4 + 2];
            var b33 = other.elements[3 * 4 + 3];

            var a00 = elements[0 * 4 + 0];
            var a01 = elements[0 * 4 + 1];
            var a02 = elements[0 * 4 + 2];
            var a03 = elements[0 * 4 + 3];
            var a10 = elements[1 * 4 + 0];
            var a11 = elements[1 * 4 + 1];
            var a12 = elements[1 * 4 + 2];
            var a13 = elements[1 * 4 + 3];
            var a20 = elements[2 * 4 + 0];
            var a21 = elements[2 * 4 + 1];
            var a22 = elements[2 * 4 + 2];
            var a23 = elements[2 * 4 + 3];
            var a30 = elements[3 * 4 + 0];
            var a31 = elements[3 * 4 + 1];
            var a32 = elements[3 * 4 + 2];
            var a33 = elements[3 * 4 + 3];

            dst.elements[0] = b00 * a00 + b01 * a10 + b02 * a20 + b03 * a30;
            dst.elements[1] = b00 * a01 + b01 * a11 + b02 * a21 + b03 * a31;
            dst.elements[2] = b00 * a02 + b01 * a12 + b02 * a22 + b03 * a32;
            dst.elements[3] = b00 * a03 + b01 * a13 + b02 * a23 + b03 * a33;
            dst.elements[4] = b10 * a00 + b11 * a10 + b12 * a20 + b13 * a30;
            dst.elements[5] = b10 * a01 + b11 * a11 + b12 * a21 + b13 * a31;
            dst.elements[6] = b10 * a02 + b11 * a12 + b12 * a22 + b13 * a32;
            dst.elements[7] = b10 * a03 + b11 * a13 + b12 * a23 + b13 * a33;
            dst.elements[8] = b20 * a00 + b21 * a10 + b22 * a20 + b23 * a30;
            dst.elements[9] = b20 * a01 + b21 * a11 + b22 * a21 + b23 * a31;
            dst.elements[10] = b20 * a02 + b21 * a12 + b22 * a22 + b23 * a32;
            dst.elements[11] = b20 * a03 + b21 * a13 + b22 * a23 + b23 * a33;
            dst.elements[12] = b30 * a00 + b31 * a10 + b32 * a20 + b33 * a30;
            dst.elements[13] = b30 * a01 + b31 * a11 + b32 * a21 + b33 * a31;
            dst.elements[14] = b30 * a02 + b31 * a12 + b32 * a22 + b33 * a32;
            dst.elements[15] = b30 * a03 + b31 * a13 + b32 * a23 + b33 * a33;
            return dst;
        }

        public static Matrix Identity()
        {
            return new Matrix(new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 });
        }

        public static Matrix Translation(double tx, double ty, double tz)
        {
            return new Matrix(
                new double[16]
                {
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    tx, ty, tz, 1
                }
            );
        }

        public static Matrix FromTransform(Transform t, XYZ facingOrientation = null) {
            // TODO: fix wrong rotation
            XYZ or = t.Origin;
            XYZ bx = t.BasisX;
            XYZ by = facingOrientation != null ? facingOrientation : t.BasisY; // Handle mirror
            XYZ bz = t.BasisZ;
            return new Matrix(
                new double[16]
                {
                    //by.X, bz.X, bx.X, 0,
                    //by.Y, bz.Y, bx.Y, 0,
                    //by.Z, bz.Z, bx.Z, 0,
                    //or.Y, or.Z, or.X, 1
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    //by.Y, bz.Y, bx.Y, 0,
                    //by.Z, bz.Z, bx.Z, 0,
                    //by.X, bz.X, bx.X, 0,
                    Utils.FootToMeter(or.Y), Utils.FootToMeter(or.Z), Utils.FootToMeter(or.X), 1
                }
            );
        }

        public static Matrix Scaling(double sx, double sy, double sz)
        {
            return new Matrix(new double[16] { sx, 0, 0, 0, 0, sy, 0, 0, 0, 0, sz, 0, 0, 0, 0, 1 });
        }

        public static Matrix RotationY(double angle)
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);
            return new Matrix(
                new double[16] {
                    cos, 0, sin, 0,
                    0, 1, 0, 0,
                    -sin, 0, cos, 0,
                    0, 0, 0, 1
                }
            );
        }

    }
}
