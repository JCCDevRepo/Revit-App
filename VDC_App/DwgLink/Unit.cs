using Autodesk.Revit.DB;

namespace VDC_App
{
    public class Unit
    {
        public static double CovertFromAPI(ForgeTypeId to, double value)
        {
            return UnitUtils.ConvertFromInternalUnits(value, to);
        }

        public static double CovertToAPI(double value, ForgeTypeId from)
        {
            return UnitUtils.ConvertToInternalUnits(value, from);
        }
    }
}
