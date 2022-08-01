using Autodesk.Revit.DB;
using System;

namespace AlfaMap.Converter2d
{
    public static class ConvertUtils
    {
        public static double Convert(double value)
        {
            //return UnitUtils.ConvertFromInternalUnits(value, DisplayUnitType.DUT_METERS);
            return Math.Round(value, 3);
        }

        public static (double X, double Y) ConvertPoint(XYZ point)
        {
            double x = ConvertUtils.Convert(point.X);
            double y = ConvertUtils.Convert(point.Y);

            return (x, y);
        }
    }
}
