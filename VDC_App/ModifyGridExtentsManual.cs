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




            var viewPlanCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewPlan))
                //.Where(e => e.Name.Contains("LEVEL 08 - Sheet_Hangers - 1"));
                //.Where(e => e.Name.Contains("SHEET"));
                .Where(e => e.Name.ToLower().Contains("test"));




            var listViewIds = new List<ElementId>();
            foreach (var e in viewPlanCollector)
            {
                //var viewPlans = e as View;

                listViewIds.Add(e.Id);

                //if (viewPlans.Name.Contains("Test")/* & !viewPlans.Name.Contains("- 0")*/)
                //{
                //    listViewIds.Add((e as View).Id);

                //}
            }
            //var SimpleForm = new SimpleForm(list);
            //SimpleForm.Show();

            

            using (Transaction t = new Transaction(doc))
            {
                t.Start("test");





                foreach (var id in listViewIds)
                {
                    var iterationView = doc.GetElement(id) as View;
                    var gridCurrentView = new FilteredElementCollector(doc, id)
                    .OfClass(typeof(Grid))
                    .ToElements();

                    foreach(var e in gridCurrentView)
                    {
                        var curveInView = (e as Grid).GetCurvesInView(DatumExtentType.ViewSpecific, iterationView);
                        var toGrid = e as Grid;


                        //foreach (var curve in curveInView)
                        //{
                        //    TaskDialog.Show("sdfs", $"0:{curve.GetEndPoint(0)}\n1:{curve.GetEndPoint(1)}");



                        //}

                        foreach (var c in curveInView)
                        {
                            var viewArc = c as Arc;


                            var radius = viewArc.Radius;

                            //var startAngle = 1.27985002500652;
                            //var endAngle = 1.33833141499587;
                            var startAngle = viewArc.GetEndParameter(0);
                            var endAngle = viewArc.GetEndParameter(1);

                            var center = new XYZ(viewArc.Center.X, viewArc.Center.Y, viewArc.Center.Z);
                            //var center = new XYZ(487948.00, -1118.942, 0.0);
                            //var xAxis = new XYZ(0.9917, 0.1281, 0.0);
                            //var yAxis = new XYZ(0.1281, 0.9917, 0.0);
                            //var xAxis = new XYZ(1.0, 0.0, 0.0);
                            //var yAxis = new XYZ(0.0, 1.0, 0.0);

                            // X and Y directions of the arc (use snooping tool to see)
                            var xAxis = new XYZ(viewArc.XDirection.X, viewArc.XDirection.Y, viewArc.XDirection.Z);
                            var yAxis = new XYZ(viewArc.YDirection.X, viewArc.YDirection.Y, viewArc.YDirection.Z);

                            TaskDialog.Show("asda", $"Radius:{viewArc.Radius}\ncenter:{viewArc.Center}\nXd:{viewArc.XDirection.Normalize()}\nYd:{viewArc.YDirection.Normalize()}\nStartAngle:{startAngle}\nEndAngle:{endAngle}");





                            // create a test line to see if params work
                            var arc = Arc.Create(center, radius, startAngle, endAngle, xAxis, yAxis);

                            //doc.Create.NewDetailCurve(doc.ActiveView, arc);



                            toGrid.SetCurveInView(DatumExtentType.ViewSpecific, iterationView, arc);

                        }
                    }





                }

                t.Commit();

            }


            return Result.Succeeded;
        }


    }
}

public class Gridlines
{
    public string Name { get; set; }

}
