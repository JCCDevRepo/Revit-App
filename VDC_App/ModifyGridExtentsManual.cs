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

                        //if (toGrid.IsCurved == false)
                        //foreach (var curve in curveInView)
                        //{
                        //    TaskDialog.Show("sdfs", $"0:{curve.GetEndPoint(0)}\n1:{curve.GetEndPoint(1)}");



                        //}

                        foreach (var c in curveInView)
                        {


                            
                            var viewArc = c as Arc;

                            //var curveProp = new List<CurveProperties>()
                            //{
                            //    new CurveProperties(){Name = toGrid.Name, Center = viewArc.Center, StartAngle = viewArc.GetEndParameter(0), EndAngle = viewArc.GetEndParameter(1), DirectionX = viewArc.XDirection, DirectionY = viewArc.YDirection}
                            //};

                            // create curve object based on Area 2 reference views
                            var curveProp2 = new CurveProperties()
                            {
                                Name = toGrid.Name, 
                                Center = viewArc.Center,
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

                                        var arc = Arc.Create(curveProp2.Center, curveProp2.Radius, curveProp2.StartAngle, curveProp2.EndAngle, curveProp2.DirectionX, curveProp2.DirectionY);

                                        //TaskDialog.Show("sdfsd", $"{arc.Center}\n{arc.Radius}\n{arc.XDirection}\n{arc.YDirection}");
                                        //TaskDialog.Show("sadas", arc.ToString());

                                        //doc.Create.NewDetailCurve(doc.ActiveView, arc);


                                        gridInA2.SetCurveInView(DatumExtentType.ViewSpecific, iterationViewA2, arc);
                                    }
                                }






                            }
                            ////toGrid.SetCurveInView(DatumExtentType.ViewSpecific, iterationView, arc);


                            //foreach(var v in curveProp)
                            //{
                            //    TaskDialog.Show("fjkdsjf", $"{v.Name}\n{v.Center}\n{v.StartAngle}\n{v.EndAngle}\n{v.DirectionX}\n{v.DirectionY}");
                            //}



                            //var radius = viewArc.Radius;

                            ////var startAngle = 1.27985002500652;
                            ////var endAngle = 1.33833141499587;
                            //var startAngle = viewArc.GetEndParameter(0);
                            //var endAngle = viewArc.GetEndParameter(1);

                            //var center = new XYZ(viewArc.Center.X, viewArc.Center.Y, viewArc.Center.Z);
                            ////var center = new XYZ(487948.00, -1118.942, 0.0);
                            ////var xAxis = new XYZ(0.9917, 0.1281, 0.0);
                            ////var yAxis = new XYZ(0.1281, 0.9917, 0.0);
                            ////var xAxis = new XYZ(1.0, 0.0, 0.0);
                            ////var yAxis = new XYZ(0.0, 1.0, 0.0);

                            //// X and Y directions of the arc (use snooping tool to see)
                            //var xAxis = new XYZ(viewArc.XDirection.X, viewArc.XDirection.Y, viewArc.XDirection.Z);
                            //var yAxis = new XYZ(viewArc.YDirection.X, viewArc.YDirection.Y, viewArc.YDirection.Z);

                            //TaskDialog.Show("asda", $"Radius:{viewArc.Radius}\ncenter:{viewArc.Center}\nXd:{viewArc.XDirection.Normalize()}\nYd:{viewArc.YDirection.Normalize()}\nStartAngle:{startAngle}\nEndAngle:{endAngle}");





                            //// create a test line to see if params work
                            //var arc = Arc.Create(center, radius, startAngle, endAngle, xAxis, yAxis);

                            ////doc.Create.NewDetailCurve(doc.ActiveView, arc);



                            ////toGrid.SetCurveInView(DatumExtentType.ViewSpecific, iterationView, arc);

                        }
                    }




                }

                

                t.Commit();

            }


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

public class CurveProperties
{
    public string Name { get; set; }
    public double Radius { get; set; }
    public double StartAngle { get; set; }
    public double EndAngle { get; set; }
    public XYZ Center { get; set; }
    public XYZ DirectionX { get; set; }
    public XYZ DirectionY { get; set; }

}
