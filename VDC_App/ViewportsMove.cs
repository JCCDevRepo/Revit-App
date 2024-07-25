using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace VDC_App
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ViewportsMove : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;

            var viewPlanCollector = new FilteredElementCollector(doc)
            .OfClass(typeof(ViewSheet))
            //.Where(e => e.Name.ToLower().Contains("sheet"))
            .ToList();




            var linksCol = new FilteredElementCollector(doc)
            .OfClass(typeof(RevitLinkInstance))
            .Select(e => e.Id)
            .ToList();
            var gridCategoryId = new ElementId(Convert.ToInt32(BuiltInCategory.OST_Grids));



            //TaskDialog.Show("safsad", viewport.ToString());

            //var SimpleForm = new SimpleForm(viewPlanCollector);
            //SimpleForm.Show();

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Move Viewport");

                foreach (var vp in viewPlanCollector)
                {
                    var viewsheet = vp as ViewSheet;
                    var viewportId = viewsheet.GetAllViewports().Select(e => e).FirstOrDefault();
                    if (viewportId == null)
                    {
                        continue;
                    }
                    var viewport = doc.GetElement(viewportId) as Viewport;

                    var vpId = viewport.ViewId;

                    var getVpElem = doc.GetElement(vpId) as View;
                    //TaskDialog.Show("ScopeBox Error", getVpElem.ToString());



                    if (getVpElem.CropBoxActive == false)
                    {
                        
                        //return Result.Cancelled;


                        continue;

                    }




                    //var constX = -7.071586620;
                    //var constY = -0.246025528;
                    //var maxX = -3.232489345;
                    //var maxY = 2.965760834;

                    var constX = 0;
                    var constY = 0;

                    var start = new XYZ(constX, constY, 0.0);
                    //var start = new XYZ(constX, constY, 0.0);
                    //var end = new XYZ(maxX, maxY, 0.0);
                    //var xyz2 = new XYZ(viewport.GetBoxOutline().MinimumPoint.X + 1.65625, viewport.GetBoxOutline().MinimumPoint.Y + 1.62890625, 0.0);

                    //var testLine = Line.CreateBound(start, end);
                    //doc.Create.NewDetailCurve(doc.ActiveView, testLine);

                    getVpElem.HideElements(linksCol);
                    getVpElem.SetCategoryHidden(gridCategoryId, true);
                    viewport.SetBoxCenter(start);
                    getVpElem.UnhideElements(linksCol);
                    getVpElem.SetCategoryHidden(gridCategoryId, false);
                    //TaskDialog.Show("asdasd", $"min:{outlineMin.ToString()}\nmax:{outlineMax.ToString()}");

                }




                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}
