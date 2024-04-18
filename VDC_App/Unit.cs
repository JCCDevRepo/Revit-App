using Autodesk.Revit.DB;

namespace VDC_App
{
    //public class LevelsDataSource
    //{
    //    string m_levelName;
    //    double m_levelElevation;
    //    int m_levelIDValue;

    //    public string Name
    //    {
    //        get
    //        {
    //            return m_levelName;
    //        }
    //        set
    //        {
    //            m_levelName = value;
    //        }
    //    }

    //    public double Elevation
    //    {
    //        get
    //        {
    //            return m_levelElevation;
    //        }
    //        set
    //        {
    //            m_levelElevation = value;
    //        }

    //    }

    //    public int LevelIDValue
    //    {
    //        get
    //        {
    //            return m_levelIDValue;
    //        }
    //        set
    //        {
    //            m_levelIDValue = value;
    //        }
    //    }
    //}
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
