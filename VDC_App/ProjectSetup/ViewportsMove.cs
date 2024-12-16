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

            // get all sheets
            var viewPlanCollector = new FilteredElementCollector(doc)
            .OfClass(typeof(ViewSheet))
            //.Where(e => e.Name.ToLower().Contains("test"))
            .ToList();


            // this collector is for getting the ids of linked revit models (to turn on/off)
            var linksCol = new FilteredElementCollector(doc)
            .OfClass(typeof(RevitLinkInstance))
            .Select(e => e.Id)
            .ToList();

            // this is for getting the built in category id of grids. 
            var gridCategoryId = new ElementId(Convert.ToInt32(BuiltInCategory.OST_Grids));

            //var SimpleForm = new SimpleForm(viewPlanCollector);
            //SimpleForm.Show();

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Move Viewport");

                foreach (var vp in viewPlanCollector)
                {
                    var viewsheet = vp as ViewSheet;
                    var viewportId = viewsheet.GetAllViewports().Select(e => e).FirstOrDefault();

                    // skip sheet if viewport doesnt exist
                    if (viewportId == null)
                    {
                        continue;
                    }
                    var viewport = doc.GetElement(viewportId) as Viewport;

                    var vpId = viewport.ViewId;

                    var getVpElem = doc.GetElement(vpId) as View;

                    // skip if cropbox doesnt exist
                    if (getVpElem.CropBoxActive == false)
                    {
                        
                        //return Result.Cancelled;


                        continue;

                    }

                    // moving viewports to zero (title block's family must also match zero location)
                    var constX = 0;
                    var constY = 0;

                    var start = new XYZ(constX, constY, 0.0);

                    // because viewport's zero-zero is dependant on the elements are currently visible
                    // I am turning off grids and links so that zero point is consistent across all sheets.
                    // without this, some VPs will be placed off center.
                    getVpElem.HideElements(linksCol);
                    getVpElem.SetCategoryHidden(gridCategoryId, true);

                    // needed a doc regen because turning off the elements affects the new zero point.
                    doc.Regenerate();

                    // main instruction to move vps to zero
                    viewport.SetBoxCenter(start);

                    // links are turned back on.
                    getVpElem.UnhideElements(linksCol);
                    getVpElem.SetCategoryHidden(gridCategoryId, false);

                    //var testLine = Line.CreateBound(start, end);
                    //doc.Create.NewDetailCurve(doc.ActiveView, testLine);

                    //var epsilon = 1e-9;
                    //var boxCenter = viewport.GetBoxCenter();
                    //if (Math.Abs(boxCenter.X) < epsilon & Math.Abs(boxCenter.Y) < epsilon)
                    //{
                    //    getVpElem.UnhideElements(linksCol);
                    //    getVpElem.SetCategoryHidden(gridCategoryId, false);
                    //    continue;
                    //}
                }




                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}
