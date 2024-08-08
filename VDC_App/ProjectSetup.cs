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

            // get list of levels in project
            var levels = new collector(doc).GetLevelsList();

            // create the WPF object
            var ui = new ProjectSetupUI();

            var tempOrdered = levels.OrderBy(e => e.Elevation).ToList();
            var tempLevElev = new List<LevelsElevations>();

            for (int i = 0; i < tempOrdered.Count; i++)
            {
                var test = 0.0;
                if (i == tempOrdered.Count - 1)
                {
                    test = 50.0;
                    
                }
                else
                {
                    test = tempOrdered[i + 1].Elevation - tempOrdered[i].Elevation;

                }
                tempLevElev.Add(new LevelsElevations(tempOrdered[i].Name, tempOrdered[i].Elevation, test, tempOrdered[i].Id));
            }

            //foreach (var e in tempOrdered)
            //{
            //    //TaskDialog.Show("sdsa", e.Elevation.ToString());
            //    tempLevElev.Add(new LevelsElevations(e.Name, e.Elevation));


            //}


            //var groupByElevation = tempLevElev
            //    .OrderBy(e => e.Elevation)
            //    .ToList();

            //var viewRangeCal = groupByElevation
            //    .Select(e => e.Elevation)
            //    .ToList();


            //var sd = new List<LevelsElevations>();

            //for (var i = 0; i < viewRangeCal.Count; i++)
            //{
            //    if (i == viewRangeCal.Count - 1)
            //    {
            //        continue;
            //    }

            //    var test = viewRangeCal[i + 1] - viewRangeCal[i];

            //    sd.Add(new LevelsElevations(test));
            //    //sd.Add(test);

            //}



            foreach (var e in tempLevElev)
            {

                ui.Levels.Items.Add(new { levelUi = e.Level, elevationUi = e.Elevation, viewRangeUi = e.ViewRange });
            }


            var form = new SimpleForm(tempLevElev);
            form.ShowDialog();
            ui.Show();

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



            return Result.Succeeded;
        }
    }
}

public class LevelsElevations
{
    public string Level { get; set; }
    public double Elevation { get; set; }
    public double ViewRange { get; set; }
    public ElementId Id { get; set; }


    public LevelsElevations(string level, double elevation, double viewRange, ElementId id)
    {
        Level = level;
        Elevation = elevation;
        ViewRange = viewRange;
        Id = id;
    }
}

public class collector
{
    public Document Doc { get; set; }
    public string ViewName { get; set; }

    public collector(Document doc, string viewName)
    {
        Doc = doc;
        ViewName = viewName;
    }

    public collector(Document doc)
    {
        Doc = doc;
    }

    public List<View> GetViewsList()
    {
        var colList = new FilteredElementCollector(Doc)
            .OfClass(typeof(View))
            .Cast<View>()
            .Where(e => e.Name.ToLower().Contains(ViewName))
            .ToList();
            
        return colList;
    }

    public List<Level> GetLevelsList()
    {
        var levelsCol = new FilteredElementCollector(Doc)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .Where(e => e.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).AsValueString().ToLower().Contains("jcc-levels"))
            .SkipWhile(e => e.Name.ToLower().Contains("insertion level"))
            .ToList();
        return levelsCol;
    }
}

