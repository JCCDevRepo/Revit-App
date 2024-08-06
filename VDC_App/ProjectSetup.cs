using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace VDC_App
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ProjectSetup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;


            // get the view template
            var view = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(x => x.Name.Equals("VDC_ViewRange - LEVEL x0001"));
            var template = view.FirstOrDefault() as ViewPlan;

            // get the view you want to apply the template to
            var viewTest = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(x => x.Name.Equals("LEVEL x0001"));


            //var test = Level.GetNearestLevelId(doc, 19.5);
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Set View Range");

                //var vr = template.GetViewRange();
                //vr.SetOffset(PlanViewPlane.TopClipPlane, 30.0);
                //template.SetViewRange(vr);


                //vr.SetOffset(PlanViewPlane.BottomClipPlane, 1.0);
                //template.SetViewRange(vr);


                //viewTest.FirstOrDefault().ApplyViewTemplateParameters(template);



                t.Commit();
            }
            var ui = new ProjectSetupUI();

            foreach (var e in viewTest)
            {
                ui.Levels.Items.Add(new { Level = e.Name });
            }


            //ui.Show();

            return Result.Succeeded;
        }
    }
}
