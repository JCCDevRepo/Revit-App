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
