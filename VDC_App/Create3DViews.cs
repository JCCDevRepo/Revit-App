using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDC_App
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]



        
    public class Create3DViews : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            UIDocument uidoc = new UIDocument(doc);

            // get list of all levels
            //IList<Level> levels = new FilteredElementCollector(doc)
            //    .OfClass(typeof(Level))
            //    .SkipWhile(s => s.Name == "INSERTION LEVEL")
            //    .Where(s => s.Name.Contains("LEV"))
            //    .Cast<Level>()
            //    .OrderBy(l => l.Elevation)
            //    .ToList();

            ////SimpleForm form = new SimpleForm(levels);
            ////form.Show();

            //var prjInfo = doc.ProjectInformation;

            ////TaskDialog.Show("sfa", prjInfo.Number);

            //if (prjInfo.Number == "**VDC to Fill Out**" || prjInfo.BuildingName == "**Trade Name - VDC to Fill Out**")
            //{

            //    TaskDialog.Show("Missing Info", "Please Fill Out The Project Number And Trade Name in Project Information");

            //    return Result.Cancelled;
            //}



            //var getImportGrid = new FilteredElementCollector(doc)
            //    .OfClass(typeof(ImportInstance))
            //    // where method to access the import grid under Snooped caterogry > Name
            //    .Where(s => s.Category.Name.Contains("GRID"))
            //    .ToList();


            //// trying to isolate just one grid 
            //if (getImportGrid.Count > 1)
            //{
            //    TaskDialog.Show("Grid Error", "Please check if grid is imported or Multiple grids are imported");

            //    return Result.Cancelled;
            //}





            //var max = getImportGrid.Where(s => s.)

            //foreach (var i in getImportGrid) 
            //{
            //    var max = i.get_BoundingBox(null).Max.ToString().Remove(0,1);
            //    var min = i.get_BoundingBox(null).Min.ToString().Remove(0,1);

            //    var maxXValue = max.Remove(max.IndexOf("."));

            //    var getCommaIndex = max.IndexOf(",")+1;
            //    var maxYTemp = max.Remove(0, getCommaIndex);
            //    var maxYValue = maxYTemp.Remove(maxYTemp.IndexOf("."));

            //    var minXValue = min.Remove(min.IndexOf("."));

            //    var getMinCommaIndex = min.IndexOf(",") + 1;
            //    var minYTemp = min.Remove(0, getMinCommaIndex);
            //    var minYValue = minYTemp.Remove(minYTemp.IndexOf("."));

            //    //maxXValue.Remove(0, 2);

            //    //TaskDialog.Show("sdfs", maxXValue);
            //    //TaskDialog.Show("sdfs", minYValue);
            //    //TaskDialog.Show("sdfs", minXValue);


            //    //TaskDialog.Show("sdfs", min.ToString());



            //}








            //SimpleForm form = new SimpleForm(levelNames);
            //form.Show();

            //var view3D = new FilteredElementCollector (doc)
            //    .OfClass(typeof(View3D))
            //    .ToList();

            //foreach (var v in view3D) 
            //{
            //    if (v.Name.Contains("PRJ"))
            //    {
            //        //TaskDialog.Show("fds", v.Name);

            //    }

            //}
            //var level = new FilteredElementCollector(doc)
            //    .OfClass(typeof(Level))
            //    .SkipWhile(s => s.Name == "INSERTION LEVEL")
            //    .Cast<Level>()
            //    .OrderBy(l => l.Elevation);

            //foreach (var l in level) 
            //{
            //    TaskDialog.Show("level", l.Name);
            //}

            //SimpleForm form = new SimpleForm(levels);
            //form.Show();



            //// get a ViewFamilyType for a 3D View
            //ViewFamilyType viewFamilyType = (from v in new FilteredElementCollector(doc).
            //                                 OfClass(typeof(ViewFamilyType)).
            //                                 Cast<ViewFamilyType>()
            //                                 where v.ViewFamily == ViewFamily.ThreeDimensional
            //                                 select v).First();







            //using (Transaction t = new Transaction(doc, "Create view"))
            //{
            //    try
            //    {
            //        int ctr = 0;
            //        // loop through all levels
            //        foreach (Level level in levels)
            //        {
            //            t.Start();

            //            // LEVEL xxxxx
            //            int indexOfSpace = level.Name.IndexOf(" ") + 1;

            //            // remove everything but the level name
            //            var editedLevelStr = level.Name.Remove(0, indexOfSpace);



            //            // Create the 3d view
            //            //View3D viewForGc = View3D.CreateIsometric(doc, viewFamilyType.Id);



            //            //View3D viewIso = View3D.CreateIsometric(doc, viewFamilyType.Id);



            //            //View3D viewJcc = View3D.CreateIsometric(doc, viewFamilyType.Id);


            //            View3D view = View3D.CreateIsometric(doc, viewFamilyType.Id);


            //            view.Name = "3D-Lev" + editedLevelStr;

            //            //viewForGc.Name = "PRJ_TRADE_Lev" + editedLevelStr;


            //            //viewIso.Name = "ISO_TRADE-Lev" + editedLevelStr;


            //            //viewJcc.Name = prjInfo.Number + "_TRADE-Lev" + editedLevelStr;



            //            // Set the name of the view
            //            //viewForGc.Name = prjInfo.Number + level.Name + " Section Box";

            //            foreach (var i in getImportGrid)
            //            {
            //                // Logic to isolate the X and Y coordinates: (82.2069046655, 405.257102627, 0.0000000)

            //                // Max is front right upper limit of the section box
            //                var max = i.get_BoundingBox(null).Max.ToString().Remove(0, 1);

            //                // Min is bottom left & lower limit.
            //                var min = i.get_BoundingBox(null).Min.ToString().Remove(0, 1);

            //                var maxXValue = Convert.ToDouble(max.Remove(max.IndexOf("."))) + 100;

            //                var getCommaIndex = max.IndexOf(",") + 1;
            //                var maxYTemp = max.Remove(0, getCommaIndex);
            //                var maxYValue = Convert.ToDouble(maxYTemp.Remove(maxYTemp.IndexOf("."))) + 100;


            //                var minXValue = Convert.ToDouble(min.Remove(min.IndexOf("."))) - 100;

            //                var getMinCommaIndex = min.IndexOf(",") + 1;
            //                var minYTemp = min.Remove(0, getMinCommaIndex);
            //                var minYValue = Convert.ToDouble(minYTemp.Remove(minYTemp.IndexOf("."))) - 100;


            //                //TaskDialog.Show("sdfs", maxXValue);
            //                //TaskDialog.Show("sdfs", maxYValue);

            //                //TaskDialog.Show("sdfs", min.ToString());

            //                // Set the name of the transaction
            //                // A transaction can be renamed after it has been started
            //                //t.SetName("Create view " + view.Name);

            //                // Create a new BoundingBoxXYZ to define a 3D rectangular space
            //                BoundingBoxXYZ boundingBoxXYZ = new BoundingBoxXYZ();

            //                // Set the lower left bottom corner of the box
            //                // Use the Z of the current level.
            //                // X & Y values have been hardcoded based on this RVT geometry


            //                if (ctr == 0)
            //                {

            //                    boundingBoxXYZ.Min = new XYZ(minXValue, minYValue, level.Elevation - 50.0);
            //                    //TaskDialog.Show("asdf", boundingBoxXYZ.ToString());
            //                }

            //                else
            //                {
            //                    boundingBoxXYZ.Min = new XYZ(minXValue, minYValue, level.Elevation);
            //                    //TaskDialog.Show("asdf", "it was skipped");

            //                }

            //                //TaskDialog.Show("min z", boundingBoxXYZ.Min.ToString());

            //                // Determine the height of the bounding box
            //                double zOffset = 0;



            //                // If there is another level above this one, use the elevation of that level
            //                if (levels.Count > ctr + 1)
            //                    zOffset = levels.ElementAt(ctr + 1).Elevation;
            //                // If this is the top level, use an offset of 10 feet
            //                else
            //                    zOffset = level.Elevation + 50;
            //                boundingBoxXYZ.Max = new XYZ(maxXValue, maxYValue, zOffset);

            //                //TaskDialog.Show("min z", boundingBoxXYZ.Max.ToString());


            //                // Apply this bouding box to the view's section box
            //                //view.BoundingBox = boundingBoxXYZ;

            //                //viewForGc.SetSectionBox(boundingBoxXYZ);

            //                //viewIso.SetSectionBox(boundingBoxXYZ);

            //                //viewJcc.SetSectionBox(boundingBoxXYZ);

            //                view.SetSectionBox(boundingBoxXYZ);



            //            }







            //            t.Commit();

            //            // Open the just-created view
            //            // There cannot be an open transaction when the active view is set
            //            //uidoc.ActiveView = view;

            //            ctr++;
            //        }

            //    }

            //    catch (Exception ex)
            //    {
            //        message = ex.Message;
            //        var errorMessage = "Please check for Duplicate Views";
            //        TaskDialog.Show("Error", errorMessage);
            //        t.RollBack();
            //        return Result.Failed;

            //    }

            //}



            try
            {
                var window = new Create3DViewsUI(doc);
                window.ShowDialog();


                return Result.Succeeded;


            }
            catch (Exception e)
            {
                message = e.Message;
                var errorMessage = "ffsfdsf";
                TaskDialog.Show("Error", errorMessage);
                return Result.Failed;
            }



        }
    }
}
