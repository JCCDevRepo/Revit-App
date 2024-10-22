using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VDC_App;
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
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;

            #region apply template testing
            /*
            // get the view template
            //var view = new FilteredElementCollector(doc)
            //    .OfClass(typeof(View))
            //    .Cast<View>()
            //    .Where(x => x.Name.Equals("VDC_ViewRange - LEVEL x0001"));
            //var template = view.FirstOrDefault() as ViewPlan;

            var viewCol = new collector(doc, "vdc_viewrange").GetViewsList();
            //var form = new SimpleForm(viewCol);
            //form.ShowDialog();


            // get the view you want to apply the template to
            var viewTest = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(x => x.Name.Equals("LEVEL x0001"));

            */
            #endregion

            #region generate levels for UI input
            //// get list of levels in project
            //var levels = new collector(doc).GetLevelsList();

            //// create the WPF object


            //var orderedLevels = levels.OrderBy(e => e.Elevation).ToList();
            //var levelParams = new List<LevelsElevations>();

            //for (int i = 0; i < orderedLevels.Count; i++)
            //{
            //    var viewR = 0.0;
            //    // The highest level in the iteration is set to 50ft
            //    if (i == orderedLevels.Count - 1)
            //    {
            //        viewR = 50.0;
                    
            //    }
            //    else
            //    {
            //        // take the difference between level above and current level to get the view range
            //        viewR = orderedLevels[i + 1].Elevation - orderedLevels[i].Elevation;

            //    }
            //    levelParams.Add(new LevelsElevations(orderedLevels[i].Name, orderedLevels[i].Elevation, viewR, orderedLevels[i].Id));
            //}



            //// bind parameters to the UI 
            //foreach (var e in levelParams)
            //{

            //    ui.Levels.Items.Add(new { levelUi = e.Level, elevationUi = e.Elevation, viewRangeUi = e.ViewRange, viewIdUi = e.Id });
            //}


            //var form = new SimpleForm(levelParams);
            //form.ShowDialog();

            #endregion

            #region View template toggles
            /*
            var viewCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(e => e.Name.Equals("_View Templates"))
                .FirstOrDefault();

            var viewTemplate = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(e => e.Name.Equals("test"))
                .FirstOrDefault();
            //TaskDialog.Show("asdas", viewCollector.Name);

            // ids for View category, subcategory and assign coordinator name (do not want to be included in template) 
            var viewCatParams = viewCollector.GetNonControlledTemplateParameterIds();
            var viewParams = new List<ElementId>();
            var tasdas = new ElementId(BuiltInParameter.PLAN_VIEW_RANGE);
            
            foreach (var param in viewTemplate.GetTemplateParameterIds()) 
            {
                if (param == tasdas)
                    continue;
                //var p = param as Parameter;
                viewParams.Add(param);
                //TaskDialog.Show("asdas", param.IntegerValue.ToString());

            }
            */
            #endregion 

            //using (Transaction t = new Transaction(doc))
            //{
            //    t.Start("Set View Range");
            //    #region apply template testing
            //    //var vr = template.GetViewRange();
            //    //vr.SetOffset(PlanViewPlane.TopClipPlane, 30.0);
            //    //template.SetViewRange(vr);


            //    //vr.SetOffset(PlanViewPlane.BottomClipPlane, 1.0);
            //    //template.SetViewRange(vr);


            //    //viewTest.FirstOrDefault().ApplyViewTemplateParameters(template);
            //    #endregion

            //    //viewCollector.SetNonControlledTemplateParameterIds(viewCatParams);
            //    //viewCollector.SetNonControlledTemplateParameterIds();

            //    //viewCollector.AreModelCategoriesHidden = true;
            //    //viewCollector.CreateViewTemplate();

            //    //viewTemplate.SetNonControlledTemplateParameterIds(viewParams);

            //    t.Commit();
            //}
            
            try
            {
                var ui = new ProjectSetupUI(doc, app);
                ui.ShowDialog();
            }

            catch 
            {
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

// class for populating datagrid view type objects
public class ViewTypes
{
    public string ViewType { get; set; }

    //public ViewTypes(string viewType)
    //{
    //    ViewType = viewType;
    //}


}

public class collector
{
    public Document Doc { get; set; }
    public string ViewName { get; set; }


    public collector(Document doc)
    {
        Doc = doc;
    }

    public collector(Document doc, string viewName)
    {
        Doc = doc;
        ViewName = viewName;
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

    public List<ViewPlan> GetViewType()
    {
        var viewsCol = new FilteredElementCollector(Doc)
            .OfClass(typeof(ViewPlan))
            .Cast<ViewPlan>()
            .Where(e => e.LookupParameter("View SubCategory").AsString() == ViewName)
            .ToList();
        return viewsCol;
    }

    public List<View> GetTemplatesList()
    {
        var templateList = new FilteredElementCollector(Doc)
            .OfClass(typeof(View))
            .Cast<View>()
            .Where(e => e.Name.ToLower().Contains(ViewName))
            .Where(e => e.IsTemplate == true)
            .ToList();

        return templateList;
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

public class EditViewTemplate
{
    private Document Doc { get; set; }
    public View View { get; set; }
    public List<ElementId> ExcludedIds { get; set; }

    public EditViewTemplate(Document doc, List<ElementId> exludeIds)
    {
        Doc = doc;
        ExcludedIds = exludeIds;
    }


    public List<ElementId> TemplateCreationInfo()
    {
        // get the view/template reference to copy
        var viewTP = new FilteredElementCollector(Doc)
            .OfClass(typeof(View))
            .Cast<View>()
            .Where(e => e.Name.Equals("_View Templates"))
            .FirstOrDefault();
        if (viewTP == null)
        {
            MessageBox.Show("Missing \"_View Templates\" View", "View Template Reference Error");

        }


        // ids for View category, subcategory and assign coordinator name (do not want to be included in template) 
        //var excludedList = ExcludedIds;

        var viewParams = new List<ElementId>();

        foreach (var parameter in viewTP.GetTemplateParameterIds())
        {
            // skip the viewrangeid (how it works is: id exluded = leave the checkbox turned on)
            if (ExcludedIds.Contains(parameter))
            {
                continue;

            }

            viewParams.Add(parameter);

        }

        View = viewTP;
        return viewParams;

    }



}

public class UserSelectedItems
{
    public DataGrid DataGrid { get; set; }

    public UserSelectedItems(DataGrid dataGrid)
    {
        DataGrid = dataGrid;
    }
    public List<LevelsElevations> SelectedLevels()
    {
        var levelsInfo = new List<LevelsElevations>();
        foreach (var i in DataGrid.SelectedItems)
        {
            var selectedLevInfo = i as LevelsElevations;

            //MessageBox.Show(selectedLevInfo.Id.ToString(), "test", MessageBoxButton.OK);

            levelsInfo.Add(selectedLevInfo);

        }
        return levelsInfo;
    }

    public List<ViewTypes> SelectedViewTypes()
    {
        var viewTypes = new List<ViewTypes>();
        foreach (var t in DataGrid.SelectedItems)
        {
            viewTypes.Add(t as ViewTypes);
        }
        return viewTypes;
    }

    public List<Element> SelectedScopeBoxes()
    {
        var scopeBoxes = new List<Element>();
        foreach (var e in DataGrid.SelectedItems)
        {
            scopeBoxes.Add(e as Element);
        }
        return scopeBoxes;
    }
}

public class ViewsFromIds
{
    private List<ElementId> ViewIds { get; set; }
    private Document Document { get; set; }
    public ViewsFromIds(Document doc , List<ElementId> viewIds)
    {
        Document = doc;
        ViewIds = viewIds;
    }

    public List<View> GetViews()
    {
        // get elements from new ids, if category = a view add to list
        var viewCatId = new ElementId(BuiltInCategory.OST_Views);
        var viewplanElems = new List<View>();

        foreach (var e in ViewIds)
        {
            // ids returned from newly created views can include uneeded elements
            // these if statements filtr them out and only return ids that are of category "Views"
            var temp = Document.GetElement(e);
            var isTemplate = temp as ViewPlan;

            if (temp.Category == null || isTemplate.IsTemplate == true)
            {
                continue;
            }


            if (temp.Category.Id == viewCatId)
            {
                viewplanElems.Add(temp as View);
                //newViewIds.Add(e);
            }
        }
        return viewplanElems;
    }
}

public class ApplyViewTemplates
{
    private Document Document { get; set; }
    private string ViewType { get; set; }
    private List<View> ViewTemplates { get; set; }

    public ApplyViewTemplates(Document doc, string viewType, List<View> viewTemplates)
    {
        Document= doc; 
        ViewType = viewType;
        ViewTemplates = viewTemplates;
    }

    public void ApplyTemplates()
    {
        var getViewByCat = new List<ViewPlan>();
        // need to be specific for working views because view name does not equal category name
        if (ViewType == "Working")
        {
            getViewByCat = new collector(Document, "Coordination").GetViewType().ToList();

        }
        else if (ViewType == "RCP")
        {
            // RCP needs to separated because is it not a sheet and does not contain dependents
            getViewByCat = new collector(Document, ViewType).GetViewType().ToList();

        }
        else
        {
            // Select only the parent views of sheets by " - 0"
            // the children views inherit the template from the parent.
            // children do not get process = more efficient
            getViewByCat = new collector(Document, ViewType).GetViewType()
                .Where(e => e.Name.Contains("- 0"))
                .ToList();
            
        }

        var getViewTemplate = ViewTemplates.Where(i => i.Name.Contains(ViewType)).FirstOrDefault();

        if (getViewByCat.Count < 1 || getViewTemplate == null)
        {
            MessageBox.Show($"Missing Views Or View Templates\nAffected View: {ViewType}");
            return;
        }

        foreach (var v in getViewByCat)
        {

            v.ApplyViewTemplateParameters(getViewTemplate);
        }
    }
}

public class SheetsName
{
    private List<ViewPlan> ViewPlans { get; set; }

    private ViewPlan View {  get; set; }

    private string Trade {  get; set; }


    public SheetsName (ViewPlan view, string trade)
    {
        //Document = doc;
        View = view;
        Trade = trade;
    }

    // this tupples allow the return of two variables
    public (string sheetName, string sheetNumber) RenameSheets()
    {
        string sheetNumber = null;
        string sheetName = null;

        // regx to return name by matching level(any character group)-
        // ex: LEVEL x0001
        var levelName = Regex.Match(View.Name, @"(level(.*?))-", RegexOptions.IgnoreCase).Groups[1].Value;

        // this returns only the "level"
        var level = Regex.Match(View.Name, @"(level\s(.*?)\s)-", RegexOptions.IgnoreCase).Groups[2].Value;
        
        // match - # or -## to determine area from name
        // ex: 1  returns only the numerial value  
        var areaNum = Regex.Match(View.Name, @"- (\d{1,2})").Groups[1].Value;

        

        var getViewType = View.LookupParameter("View SubCategory").AsString();
        switch (getViewType)
        {

            case "AccessPanels":
                {
                    // ex: HP.LXX.AREA.AP
                    sheetNumber = Trade + $".L{level}." + areaNum + ".AP";
                    sheetName = "ACCESS PANELS - " + levelName + "- AREA " + areaNum;
                    break;
                }

            case "BaseSupports":
                {
                    // ex: HP.S.LXX.Area.BS
                    sheetNumber = Trade + $".S.L{level}." + areaNum + ".BS";
                    sheetName = "BASE SUPPORTS - " + levelName + "- AREA " + areaNum;
                    break;
                }

            case "BeamPens":
                {
                    if(Trade == "FP")
                    {
                        sheetNumber = Trade + "2" + level + "." + areaNum + ".B";
                        sheetName = levelName + "- Area " + areaNum + " - Beam Penetration Drawing";
                    }
                    // ex: HP.P.LXX.AREA.BP
                    sheetNumber = Trade + $".P.L{level}." + areaNum + ".BP";
                    sheetName = "BEAM PENETRATIONS - " + levelName + " - AREA " + areaNum;
                    break;

                }

            case "Engineering":
                {
                    // ex: HP.XX.AREA.ENG
                    sheetNumber = Trade + $".{level}." + areaNum + ".ENG";
                    sheetName = "Engineering - " + levelName + "- AREA " + areaNum;
                    break;
                }

            case "FieldDrawings":
                {
                    if(Trade == "FP")
                    {
                        sheetNumber = Trade + "5" + level + "." + areaNum;
                        sheetName = levelName + "- Area " + areaNum + " - Field Drawing";
                        break;
                    }
                    sheetNumber = Trade + $".{level}." + areaNum + ".FIELD";
                    sheetName = "Field Drawing - " + levelName + "- AREA " + areaNum;
                    break;
                }


            case "Hangers":
                {
                    if (Trade == "FP")
                    {
                        sheetNumber = Trade + "4" + level + "." + areaNum;
                        sheetName = levelName + "- Area " + areaNum + " - Hanger Drawing";
                        break;
                    }
                    // ex: HP.LXX.AREA.AP
                    sheetNumber = Trade + $".S.L{level}." + areaNum + ".SUPTS";
                    sheetName = "SUPPORTS - " + levelName + "- AREA " + areaNum;
                    break;
                }

            case "HeadCutBack":
                {

                    sheetNumber = Trade + "6" + level + "." + areaNum;
                    sheetName = levelName + "- Area " + areaNum + " - Head Cutback Drawing";
                    break;
                }

            case "PadDrawings":
                {
                    // ex: HP.LXX.AREA.PADS
                    sheetNumber = Trade + $".L{level}." + areaNum + "PADS";
                    sheetName = levelName + "- PLAN - AREA " + areaNum;
                    break;

                }

            case "PointLoads":
                {
                    // ex: HP.LXX.AREA.LOADS
                    sheetNumber = Trade + $".L{level}." + areaNum + ".LOADS";
                    sheetName = "Point Loads - " + levelName + "- AREA " + areaNum;
                    break;
                }

            case "Risers":
                {
                    // ex: HP.Riser-XX.Area
                    sheetNumber = Trade + $".Riser.{level}." + areaNum;
                    sheetName = "RISERS - " + levelName + "- AREA " + areaNum;
                    break;
                }

            case "ServiceInstall":
                {
                    // ex: HP.LXX.Area.Service Abr
                    sheetNumber = Trade + $".L{level}." + areaNum + ".SVCS";
                    sheetName = "SERVICE INSTALL - " + levelName + "- AREA " + areaNum;
                    break;
                }

            case "Sketches":
                {
                    // ex: HP.SK.X.Area
                    sheetNumber = Trade + $".SK.{level}." + areaNum;
                    sheetName = "SKETCHES - " + levelName + "- AREA " + areaNum;
                    break;
                }

            case "ShopDrawing":
                {
                    if(Trade == "FP")
                    {
                        sheetNumber = Trade + "1" + level + "." + areaNum;
                        sheetName = levelName + "- Area " + areaNum + " - Shop Drawing";
                        break;
                    }
                    // ex: HP.LXX.Area
                    sheetNumber = Trade + $".L{level}." + areaNum;
                    sheetName = levelName + "- PLAN - AREA " + areaNum;
                    break;

                }

            case "Sleeving":
                {
                    if(Trade == "FP")
                    {
                        sheetNumber = Trade + "2" + level + "." + areaNum + ".S";
                        sheetName = levelName + "- Area " + areaNum + " - Slab Penetration";
                    }
                    // ex: HP.P.LXX.Area.SLV
                    sheetNumber = Trade + $".P.L{level}." + areaNum + ".SLV";
                    sheetName = "SLEEVING - " + levelName + " - AREA " + areaNum;
                    break;

                }

            case "SupplementalSteel":
                {
                    // ex: HP.S.LXX.Area.SS
                    sheetNumber = Trade + $"S.L{level}." + areaNum + ".SS";
                    sheetName = "SUPPLEMENTAL STEEL - " + levelName + "- AREA " + areaNum;
                    break;
                }

            case "WallPens":
                {
                    if (Trade == "FP")
                    {
                        sheetNumber = Trade + "2" + level + "." + areaNum + ".W";
                        sheetName = levelName + "- Area " + areaNum + " - Wall Penetration";
                        break;
                    }
                    sheetNumber = Trade + $".P.L{level}." + areaNum + ".W";
                    sheetName = "Wall Penetration - " + levelName + " - AREA " + areaNum;
                    break;
                }
        }
        return (sheetName, sheetNumber);


        //return sheetName;
    }

}


