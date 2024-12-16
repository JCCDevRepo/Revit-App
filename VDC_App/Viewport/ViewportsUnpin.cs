using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace VDC_App
{

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ViewportsUnpin : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;

            var viewports = new FilteredElementCollector(doc)
                .OfClass(typeof(Viewport)).ToElements();

            //TaskDialog.Show("asdsa", viewports.Count.ToString());

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Unpinned Viewports");
                try
                {
                    foreach (var e in viewports)
                    {
                        e.Pinned = false;

                    }
                }

                catch (Exception ex)
                {
                    message = ex.Message;
                    var errorMessage = "No viewports on sheets found";
                    TaskDialog.Show("Error", errorMessage);
                    t.RollBack();
                    return Result.Failed;
                }

                t.Commit();

                return Result.Succeeded;
            }
        }
    }
}
