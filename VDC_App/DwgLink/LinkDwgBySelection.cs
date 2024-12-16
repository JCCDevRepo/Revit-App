using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Forms;

namespace VDC_App
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LinkDwgBySelection : IExternalCommand
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
            //var rvtCen = doc.GetWorksharingCentralModelPath();
            //var rvtCenPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(rvtCen);
            //TaskDialog.Show("Revit Central Model Path", rvtCenPath);

            //if project path found than adjust for Xrefs

            //string rvtDir = rvtCenPath.Substring(0, 37);
            //string prjCordDir = "\\01 Field Management\\04 Coordination\\";
            //string prjFolder = null;
            //string prjNumber = null;
            //if (rvtDir == "\\\\cannistraro.local\\storage\\PROJECTS\\")
            //{
            //    //Get project Folder
            //    if (rvtCenPath.Contains(rvtDir) && rvtCenPath.Contains(prjCordDir))
            //    {
            //        int Start, End;
            //        Start = rvtCenPath.IndexOf(rvtDir, 0) + rvtDir.Length;
            //        End = rvtCenPath.IndexOf(prjCordDir, Start);
            //        prjFolder = rvtCenPath.Substring(Start, End - Start);
            //        //TaskDialog.Show("Project Folder", prjFolder);
            //        prjNumber = prjFolder.Substring(0, 6);
            //        //TaskDialog.Show("Project Folder", prjNumber);
            //    }
            //    else
            //    {
            //        TaskDialog.Show("TROUBLESHOOT", "project folder not found");
            //    }
            //}
            //else
            //{
            //    TaskDialog.Show("TROUBLESHOOT", "project directory not found");
            //}

            //string coordFolder = rvtDir + prjFolder + prjCordDir;




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
                double temValue2 = double.Parse(temValue.ToString("#.0000000000"));



                levelsDataSourceRow.Elevation = temValue2;

                systemLevelsDatum.Add(levelsDataSourceRow);


            }

            //Extension methods from the IEnumerable (Enumerable Interface)
            //EXT method with LINQ to skip the insertion level
            //LINQ query to order by ascending elevations then select the elevations with the associated level name
            var groupByElevation = systemLevelsDatum
                //.SkipWhile(s => s.Name == "INSERTION LEVEL")
                .Where(s => s.Name.Contains("LEVEL"))
                .SkipWhile(s => s.Name.Contains("INSERTION"))
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

                var strFormatLev = removeLevelStr.Substring(0,1) + removeLevelStr.Substring(1,2).ToLower() + removeLevelStr.Substring(3);

                levelsList.Add(strFormatLev);

            }




            //window.BtnClicked += (sender, EventArgs) =>
            //{
            //    while (window.Levels ==  null)
            //    {
            //        //TaskDialog.Show("sdad", window.Levels);


            //    }

            //    window.Close();

            //    foreach(var level in window.Levels)
            //    {
            //        TaskDialog.Show("sdad", level.ToString());

            //    }








            try
            {
                LinkDwgBySelectionUI window = new LinkDwgBySelectionUI(doc);

                //window.lvLevels.ItemsSource = groupByElevation;



                foreach (var e in groupByElevation)
                {
                    // this is how to add items to a row with two columns (Level & Elevation)
                    window.lvLevels.Items.Add(new { Level = e.Name, Elevation = e.Elevation });



                }





                window.ShowDialog();


                return Result.Succeeded;

            }

            catch (Exception e)
            {
                message = e.Message;
                var errorMessage = "Transaction Cancelled\nOr\nDWG does not exist\nOr\nDWG naming does not match Revit's";
                TaskDialog.Show("Error", errorMessage);
                return Result.Failed;
            }




        }


        //static void EventMessage()
        //{

        //    var test = "test";
        //    TaskDialog.Show("fsdf", test);

            
        //    var window2 = new ListLevelsWindow();

        //    //if (window2.GetLevels() != null)
        //    //{
        //    //    var SimpleLevels = new SimpleForm(window2.GetLevels());
        //    //    SimpleLevels.Show();
        //    //}

        //    TaskDialog.Show("sdf", window2.GetLevels());




        //}




    }
}
