using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitWrapper
{
    public static class Utils
    {
        public static double FootToMeter(double value)
        {
            return Math.Round(value * 0.3048, 4);
        }

        public static double SquareFootToMeter(double value)
        {
            var converted = Math.Round(value / 10.764, 4);
            //var converted = UnitUtils.ConvertFromInternalUnits(value, DisplayUnitType.DUT_SQUARE_METERS);
            return Math.Round(converted, 4);
        }

        public static Parameter GetParameter(Element element, string name)
        {
            var parameter = element.LookupParameter(name);
            if (parameter == null)
            {
                throw new ParameterNotFoundException($"У #{element.Id} нет параметра \"{name}\"");
            }
            return parameter;
        }
    }

    public class ParameterNotFoundException : Exception
    {
        public ParameterNotFoundException() { }
        public ParameterNotFoundException(string message) : base(message) { }
        public ParameterNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
