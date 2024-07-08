using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Grid = Autodesk.Revit.DB.Grid;

namespace VDC_App
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ModifyGridExtentsManual : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;



            //var viewPlanCollector = new FilteredElementCollector(doc)
            //    .OfClass(typeof(ViewPlan))
            //    //.Where(e => e.Name.Contains("LEVEL 08 - Sheet_Hangers - 1"));
            //    //.Where(e => e.Name.Contains("SHEET"));
            //    .Where(e => e.Name.ToLower().Contains("grid extents - 02"));


            //var listViewIds = new List<ElementId>();
            //foreach (var e in viewPlanCollector)
            //{
            //    listViewIds.Add(e.Id);

            //}


            //var SimpleForm = new SimpleForm(list);
            //SimpleForm.Show();

            // use ViewsElementId class to return the list of filteredelementcollector of IDs
            var sourceViewId = new ViewsElementId(doc, "grid extents area 02").ElementList();

            var getAreaTwoPlanIds = new ViewsElementId(doc, "- 02 test").ElementList();

            //var SimpleForm = new SimpleForm(getAreaTwoPlanIds);
            //SimpleForm.Show();

            var test = new ModifyArc(sourceViewId, getAreaTwoPlanIds, doc);

            test.MoveGrid();


            /* this was the working code before turning it into a class
            #region transaction
            using (Transaction t = new Transaction(doc))
            {
                t.Start("test");


                foreach (var id in sourceViewId)
                {
                    var iterationView = doc.GetElement(id) as View;
                    var gridCurrentView = new FilteredElementCollector(doc, id)
                    .OfClass(typeof(Grid))
                    .ToElements();

                    foreach(var e in gridCurrentView)
                    {
                        
                        var curveInView = (e as Grid).GetCurvesInView(DatumExtentType.ViewSpecific, iterationView);
                        var toGrid = e as Grid;

                        if (toGrid.IsCurved == false)
                        {
                            continue;
                        }
                        //foreach (var curve in curveInView)
                        //{
                        //    TaskDialog.Show("sdfs", $"0:{curve.GetEndPoint(0)}\n1:{curve.GetEndPoint(1)}");



                        //}

                        foreach (var c in curveInView)
                        {


                            
                            var viewArc = c as Arc;


                            // create curve object based on Area 2 reference views
                            var curveProp2 = new CurveProperties()
                            {
                                Name = toGrid.Name, 
                                Center = viewArc.Center,
                                CenterZ = viewArc.Center.Z,
                                Radius = viewArc.Radius,
                                StartAngle = viewArc.GetEndParameter(0),
                                EndAngle = viewArc.GetEndParameter(1),
                                DirectionX = viewArc.XDirection,
                                DirectionY = viewArc.YDirection
                            };

                            foreach (var a2 in getAreaTwoPlanIds)
                            {
                                var iterationViewA2 = doc.GetElement(a2) as View;
                                var gridCurrentViewA2 = new FilteredElementCollector(doc, a2)
                                .OfClass(typeof(Grid))
                                .ToElements();



                                // views ids for area 2 views
                                foreach(var g in gridCurrentViewA2)
                                {
                                    var gridInA2 = g as Grid;

                                    //TaskDialog.Show("asdas", gridInA2.Id.ToString());
                                    //TaskDialog.Show("asdas", toGrid.Id.ToString());
                                    if (gridInA2.Id == toGrid.Id)
                                    {
                                        // needed to adjust the Z value of the Center because it changes based the level of the view.
                                        // the boundingbox max.z value contains the levels elevation value.
                                        var adjustedCenter = new XYZ(curveProp2.Center.X, curveProp2.Center.Y, gridInA2.get_BoundingBox(iterationViewA2).Min.Z);

                                        var arc = Arc.Create(adjustedCenter, curveProp2.Radius, curveProp2.StartAngle, curveProp2.EndAngle, curveProp2.DirectionX, curveProp2.DirectionY);

                                        //TaskDialog.Show("sdfsd", $"{arc.Center}\n{arc.Radius}\n{curveProp2.StartAngle}\n{curveProp2.EndAngle}\n{arc.XDirection}\n{arc.YDirection}");
                                        //TaskDialog.Show("sadas", gridInA2.get_BoundingBox(iterationViewA2).Min.Z.ToString());

                                        //doc.Create.NewDetailCurve(doc.ActiveView, arc);


                                        gridInA2.SetCurveInView(DatumExtentType.ViewSpecific, iterationViewA2, arc);
                                    }
                                }






                            }


                        }
                    }




                }

                

                t.Commit();

            }
            #endregion
            */
            return Result.Succeeded;
        }


    }
}

public class ViewsElementId
{

    public Document Doc { get; set; }
    public string ViewName { get; set; }

    public ViewsElementId(Document doc, string viewName)
    {
        Doc = doc;
        ViewName = viewName;

        
    }

    public List<ElementId> ElementList()
    {
        var viewPlanCollector = new FilteredElementCollector(Doc)
        .OfClass(typeof(ViewPlan))
        //.Where(e => e.Name.Contains("LEVEL 08 - Sheet_Hangers - 1"));
        //.Where(e => e.Name.Contains("SHEET"));
        .Where(e => e.Name.ToLower().Contains(ViewName));


        var listViewIds = new List<ElementId>();
        foreach (var e in viewPlanCollector)
        {

            listViewIds.Add(e.Id);

        }

        return listViewIds;
    }
}

public class ModifyArc
{
    public Document Doc { get; set; }
    public List<ElementId> ViewIds { get; set; }
    public List<ElementId> AreaIds { get; set; }

    public List<Arc> Arcs { get; set; }
    public ModifyArc(List<ElementId> viewIds, List<ElementId> areaIds, Document doc)
    {

        ViewIds = viewIds;
        AreaIds = areaIds;
        Doc = doc;
    }
    public void MoveGrid()
    {
        using (Transaction t = new Transaction(Doc))
        {
            t.Start("test");

            // ids of source views used for extracting the source grids
            foreach (var id in ViewIds)
            {
                var iterationView = Doc.GetElement(id) as View;
                var gridCurrentView = new FilteredElementCollector(Doc, id)
                .OfClass(typeof(Grid))
                .ToElements();

                // get source grids and only select curved grids
                foreach (var e in gridCurrentView)
                {

                    var curveInView = (e as Grid).GetCurvesInView(DatumExtentType.ViewSpecific, iterationView);
                    var sourceGrid = e as Grid;

                    if (sourceGrid.IsCurved == false)
                    {
                        continue;
                    }

                    foreach (var c in curveInView)
                    {

                        var viewArc = c as Arc;

                        // this is for the grids that will be proccessed in each area view
                        // get the Ids with the respective area.
                        foreach (var i in AreaIds)
                        {
                            var iterationViewArea = Doc.GetElement(i) as View;
                            var gridCurrentViewArea = new FilteredElementCollector(Doc, i)
                            .OfClass(typeof(Grid))
                            .ToElements();



                            // views ids for area views
                            foreach (var g in gridCurrentViewArea)
                            {
                                var gridInArea = g as Grid;


                                if (gridInArea.Id == sourceGrid.Id)
                                {
                                    // needed to adjust the Z value of the Center because it changes based the level of the view.
                                    // the boundingbox max.z value contains the levels elevation value.
                                    var adjustedCenter = new XYZ(viewArc.Center.X, viewArc.Center.Y, gridInArea.get_BoundingBox(iterationViewArea).Min.Z);

                                    var arc = Arc.Create(adjustedCenter, viewArc.Radius, viewArc.GetEndParameter(0), viewArc.GetEndParameter(1), viewArc.XDirection, viewArc.YDirection);


                                    gridInArea.SetCurveInView(DatumExtentType.ViewSpecific, iterationViewArea, arc);
                                }
                            }
                        }
                    }
                }
            }
            t.Commit();
        }

    }




}

//public class CurveProperties
//{
//    public string Name { get; set; }
//    public double Radius { get; set; }
//    public double StartAngle { get; set; }
//    public double EndAngle { get; set; }
//    public XYZ Center { get; set; }
//    public double CenterZ { get; set; }
//    public XYZ DirectionX { get; set; }
//    public XYZ DirectionY { get; set; }

//}
