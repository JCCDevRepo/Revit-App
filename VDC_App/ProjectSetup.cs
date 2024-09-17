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
            var temp = Document.GetElement(e);

            if (temp.Category == null)
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
    public void ApplyViewRange()
    {

    }

}


