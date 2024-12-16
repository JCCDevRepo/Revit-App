using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Drawing;

namespace VDC_App
{




    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class LinkDwgAll : IExternalCommand
    {
        ExternalCommandData m_revit;
        public ForgeTypeId UnitTypeId;
        List<LevelsDataSource> systemLevelsDatum;
        public List<LevelsDataSource> SystemLevelsDatum
        {
            get
            {
                return systemLevelsDatum;
            }
            set
            {
                systemLevelsDatum = value;
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get application and documnet objects
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            m_revit = commandData;
            UnitTypeId = m_revit.Application.ActiveUIDocument.Document.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();

            //Get model name 
            //(ex: Drawing1_JCC.Cadsystems)
            string rvtName = doc.Title;
            //TaskDialog.Show("Revit Model Name", rvtName);

            //Get model path
            //(ex: "C:\users\knoelte\Documents\Drawing1_JCC.Cadsystems.rvt")
            string rvtPath = doc.PathName;
            //TaskDialog.Show("Revit Model Path", rvtPath);

            //Get central model path
            //(ex: "\\cannistraro.local\storage\PROJECTS\##### - TEST PRJ\01 Field Management\04 Coordination\BIM\Revit\Drawing1_JCC.Cadsystems.rvt")
            var rvtCen = doc.GetWorksharingCentralModelPath();
            var rvtCenPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(rvtCen);
            //TaskDialog.Show("Revit Central Model Path", rvtCenPath);

            //if project path found than adjust for Xrefs

            string rvtDir = rvtCenPath.Substring(0, 37);
            string prjCordDir = "\\01 Field Management\\04 Coordination\\";
            string prjFolder = null;
            string prjNumber = null;
            if (rvtDir == "\\\\cannistraro.local\\storage\\PROJECTS\\")
            {
                //Get project Folder
                if (rvtCenPath.Contains(rvtDir) && rvtCenPath.Contains(prjCordDir))
                {
                    int Start, End;
                    Start = rvtCenPath.IndexOf(rvtDir, 0) + rvtDir.Length;
                    End = rvtCenPath.IndexOf(prjCordDir, Start);
                    prjFolder = rvtCenPath.Substring(Start, End - Start);
                    //TaskDialog.Show("Project Folder", prjFolder);
                    prjNumber = prjFolder.Substring(0, 6);
                    //TaskDialog.Show("Project Folder", prjNumber);
                }
                else
                {
                    TaskDialog.Show("TROUBLESHOOT", "project folder not found");
                }
            }
            else
            {
                TaskDialog.Show("TROUBLESHOOT", "project directory not found");
            }

            string coordFolder = rvtDir + prjFolder + prjCordDir;




            //filter the elements to select the "Level" class 
            systemLevelsDatum = new List<LevelsDataSource>();
            var collector = new FilteredElementCollector(doc);
            var collection = collector.OfClass(typeof(Level)).ToElements();

            //itterate to get the level's IDs, names and Elevations Parameters 
            foreach (var element in collection)
            {
                Level systemLevel = element as Level;
                LevelsDataSource levelsDataSourceRow = new LevelsDataSource();

                levelsDataSourceRow.LevelIDValue = systemLevel.Id.IntegerValue;
                levelsDataSourceRow.Name = systemLevel.Name;

                Parameter elevationPara = systemLevel.get_Parameter(BuiltInParameter.LEVEL_ELEV);

                double temValue = Unit.CovertFromAPI(UnitTypeId, elevationPara.AsDouble());
                //can format .ToString("#.0") for significant number of zeros
                // needed #.0000000000 because encountered issue where not enough sig zero caused visibility issues
                double temValue2 = double.Parse(temValue.ToString("#.0000000000"));



                levelsDataSourceRow.Elevation = temValue2;

                systemLevelsDatum.Add(levelsDataSourceRow);


            }

            //var collectorCADImport = new FilteredElementCollector(doc);
            //var collectionCADImport = collectorCADImport.OfClass(typeof(ImportInstance)).ToElements();


            //var cadList = new List<string>();

            //foreach (var cad in collectionCADImport)
            //{


            //    cadList.Add(cad.Category.Name.ToString());



            //}

            //var SimpleForm = new SimpleForm(cadList);
            //SimpleForm.Show();


            //Extension methods from the IEnumerable (Enumerable Interface)
            //EXT method with LINQ to skip the insertion level
            //LINQ query to order by ascending elevations then select the elevations with the associated level name
            var groupByElevation = systemLevelsDatum
                .SkipWhile(s => s.Name == "INSERTION LEVEL")
                .OrderBy(s => s.Elevation)
                .Select(s => new { s.Elevation, s.Name });


            //var SimpleLevels = new SimpleForm(groupByElevation);
            //SimpleLevels.Show();


            //create a list of the ordered elevations
            var elevationsList = new List<double>();

            foreach (var el in groupByElevation)
            {
                var elevations = el.Elevation;
                elevationsList.Add(elevations);

            }

            //list of ordered levels associated with elevations above
            var levelsList = new List<string>();
            foreach (var lev in groupByElevation)
            {
                var levels = lev.Name;

                //used string builder to modify the string to show LEV instead of level
                var removeLevelStr = new StringBuilder(levels).Remove(3, 3).ToString();


                var strFormatLev = removeLevelStr.Substring(0, 1) + removeLevelStr.Substring(1, 2).ToLower() + removeLevelStr.Substring(3);

                levelsList.Add(strFormatLev);

            }


            //var SimpleForm = new SimpleForm(levelsList);
            //SimpleForm.Show();

            //var SimpleLevels = new SimpleForm(levelsList);
            //SimpleLevels.Show();


            //Create a default import option
            DWGImportOptions dwgOpt = new DWGImportOptions();
            dwgOpt.Unit = ImportUnit.Default;
            dwgOpt.Placement = ImportPlacement.Origin;
            dwgOpt.AutoCorrectAlmostVHLines = true;
            dwgOpt.ThisViewOnly = false;
            dwgOpt.VisibleLayersOnly = true;
            //dwgOpt.OrientToView = false;
            //dwgOpt.ColorMode = ImportColorMode.Preserved;
            //dwgOpt.CustomScale = 0.0;

            //var wsAF = new FilteredWorksetCollector(doc)
            //    .OfKind(WorksetKind.UserWorkset)
            //    .Where(s => s.Name.Contains("MISC-Link_AF"));

            //var wsAR = new FilteredWorksetCollector(doc)
            //    .OfKind(WorksetKind.UserWorkset)
            //    .Where(s => s.Name.Contains("MISC-Link_AR"));

            //var wsEL = new FilteredWorksetCollector(doc)
            //    .OfKind(WorksetKind.UserWorkset)
            //    .Where(s => s.Name.Contains("MISC-Link_EL"));



            // get all worksets in project to apply to the linked cads
            var worksets = new FilteredWorksetCollector(doc)
                .OfKind(WorksetKind.UserWorkset);


            var wsAfId = 0;
            var wsArId = 0;
            var wsElId = 0;
            var wsFpId = 0;
            var wsHdId = 0;
            var wsHpId = 0;
            var wsPlId = 0;
            var wsStcId = 0;
            var wsSteId = 0;

            // get workset Ids for applying the change
            foreach (var w in worksets)
            {
                var cadType = w.Name;
                
                switch(cadType)
                {
                    case "MISC-Link_AF":
                        wsAfId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_AR":
                        wsArId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_EL":
                        wsElId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_FP":
                        wsFpId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_HD":
                        wsHdId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_HP":
                        wsHpId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_PL":
                        wsPlId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_STC":
                        wsStcId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_STE":
                        wsSteId = w.Id.IntegerValue;
                        break;
                }
                    
                //TaskDialog.Show("sfs", cadType);
            }

            //TaskDialog.Show("dsfs", $"{wsAfId}\n{wsArId}\n{wsElId}\n{wsFpId}\n{wsHdId}\n{wsHpId}\n{wsPlId}\n{wsStcId}\n{wsSteId}\n"); 

            ElementId linkId = ElementId.InvalidElementId;
            using (Transaction tran = new Transaction(doc, "Quick Link"))
            {
                tran.Start();
                try
                {


                    //itterate through each level and elevate it according to the index of the lists
                    for (int i = 0; i < levelsList.Count; i++)
                    {
                        string dwgAF = coordFolder + "Architecturals\\" + prjNumber + "_AF-" + levelsList[i] + ".dwg";
                        string dwgAR = coordFolder + "Architecturals\\" + prjNumber + "_AR-" + levelsList[i] + ".dwg";
                        string dwgHD = coordFolder + "HVAC-DUCT\\" + prjNumber + "_HD-" + levelsList[i] + ".dwg";
                        string dwgHP = coordFolder + "HVAC-PIPE\\" + prjNumber + "_HP-" + levelsList[i] + ".dwg";
                        string dwgPL = coordFolder + "Plumbing\\" + prjNumber + "_PL-" + levelsList[i] + ".dwg";
                        string dwgFP = coordFolder + "Fire Protection\\" + prjNumber + "_FP-" + levelsList[i] + ".dwg";
                        string dwgEL = coordFolder + "Electrical\\" + prjNumber + "_EL-" + levelsList[i] + ".dwg";
                        //TaskDialog.Show("tdsfds", dwgAF);

                        //set import option to pull elevations from the sorted list of ELV
                        var zElevationTemp = new XYZ(0, 0, elevationsList[i]);
                        dwgOpt.ReferencePoint = zElevationTemp;
                        //TaskDialog.Show("fdsf", zElevationTemp.ToString());



                        doc.Link(dwgAF, dwgOpt, doc.ActiveView, out linkId);
                        doc.Link(dwgAR, dwgOpt, doc.ActiveView, out linkId);
                        doc.Link(dwgHD, dwgOpt, doc.ActiveView, out linkId);
                        doc.Link(dwgHP, dwgOpt, doc.ActiveView, out linkId);
                        doc.Link(dwgPL, dwgOpt, doc.ActiveView, out linkId);
                        doc.Link(dwgFP, dwgOpt, doc.ActiveView, out linkId);
                        doc.Link(dwgEL, dwgOpt, doc.ActiveView, out linkId);


                    }

                    //offset the starting index by 1 since the lowest level is not needed for steel
                    for (int i = 1; i < levelsList.Count; i++)
                    {

                        string dwgSTC = coordFolder + "Structurals\\" + prjNumber + "_STC-" + levelsList[i] + ".dwg";
                        string dwgSTE = coordFolder + "Structurals\\" + prjNumber + "_STE-" + levelsList[i] + ".dwg";

                        //index i-1 means start at one index below or in this case elevation. 
                        var zElevationTemp = new XYZ(0, 0, elevationsList[i - 1]);
                        dwgOpt.ReferencePoint = zElevationTemp;

                        doc.Link(dwgSTC, dwgOpt, doc.ActiveView, out linkId);
                        doc.Link(dwgSTE, dwgOpt, doc.ActiveView, out linkId);

                    }

                    #region get each type of linked cads

                    var getImportAF = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.Category.Name.Contains("AF-Lev"))
                        .ToList();

                    var getImportAR = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.Category.Name.Contains("AR-Lev"))
                        .ToList();

                    var getImportEL = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.Category.Name.Contains("EL-Lev"))
                        .ToList();

                    var getImportFP = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.Category.Name.Contains("FP-Lev"))
                        .ToList();

                    var getImportHD = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.Category.Name.Contains("HD-Lev"))
                        .ToList();

                    var getImportHP = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.Category.Name.Contains("HP-Lev"))
                        .ToList();

                    var getImportPL = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.Category.Name.Contains("PL-Lev"))
                        .ToList();

                    var getImportSTC = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.Category.Name.Contains("STC-Lev"))
                        .ToList();

                    var getImportSTE = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.Category.Name.Contains("STE-Lev"))
                        .ToList();
                    #endregion

                    #region set the CAD elements to the respective workset via Ids

                    foreach (var i in getImportAF)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsAfId);

                    }

                    foreach (var i in getImportAR)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsArId);

                    }

                    foreach (var i in getImportEL)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsElId);

                    }

                    foreach (var i in getImportFP)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsFpId);

                    }

                    foreach (var i in getImportHD)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsHdId);

                    }

                    foreach (var i in getImportHP)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsHpId);

                    }

                    foreach (var i in getImportPL)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsPlId);

                    }

                    foreach (var i in getImportSTC)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsStcId);

                    }

                    foreach (var i in getImportSTE)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsSteId);

                    }
                    #endregion

                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    var errorMessage = "DWG does not exist\nOr\nDWG naming does not match Revit's";
                    TaskDialog.Show("Error", errorMessage);
                    tran.RollBack();
                    return Result.Failed;
                }
                tran.Commit();
                return Result.Succeeded;

            }


        }




    }

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

    //public class Unit
    //{
    //    public static double CovertFromAPI(ForgeTypeId to, double value)
    //    {
    //        return UnitUtils.ConvertFromInternalUnits(value, to);
    //    }

    //    public static double CovertToAPI(double value, ForgeTypeId from)
    //    {
    //        return UnitUtils.ConvertToInternalUnits(value, from);
    //    }
    //}

}
