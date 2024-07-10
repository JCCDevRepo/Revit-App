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


            //var SimpleForm = new SimpleForm(list);
            //SimpleForm.Show();

            // returns the CurveAreaViews that has a curve in it.
            // cancel the transaction if there are no curves or CurveAreaViews are missing
            var getCurvedGridViews = new ViewsElementId(doc, "curvegridarea").CheckSourceForCurve();
            if (getCurvedGridViews.Count < 1)
            {
                TaskDialog.Show("Error", "Please check if curves exist or CurveAreaView plans exist");
                return Result.Cancelled;
            }



            for (int i = 0; i < getCurvedGridViews.Count; i++)
            {
                switch (getCurvedGridViews[i])
                {
                    case "curvegridarea1":

                        var sourceArea1Id = new ViewsElementId(doc, "curvegridarea1").ElementList();
                        var area1ViewId = new ViewsElementId(doc, "- 1").ElementList();
                        if (area1ViewId.Count < 1)
                        {
                            TaskDialog.Show("Error", "Please check if destination view has the correct area indicator (ex: - 1 for area 1)");
                            return Result.Cancelled;
                        }
                        new ModifyArc(sourceArea1Id, area1ViewId, doc, "Modify Grid Area1 Plan").MoveGrid();
                        break;

                    case "curvegridarea2":

                        var sourceArea2Id = new ViewsElementId(doc, "curvegridarea2").ElementList();
                        var area2ViewId = new ViewsElementId(doc, "- 2").ElementList();
                        if (area2ViewId.Count < 1)
                        {
                            TaskDialog.Show("Error", "Please check if destination view has the correct area indicator (ex: - 1 for area 1)");
                            return Result.Cancelled;
                        }
                        new ModifyArc(sourceArea2Id, area2ViewId, doc, "Modify Grid Area2 Plan").MoveGrid();
                        break;

                    case "curvegridarea3":

                        var sourceArea3Id = new ViewsElementId(doc, "curvegridarea3").ElementList();
                        var area3ViewId = new ViewsElementId(doc, "- 3").ElementList();
                        if (area3ViewId.Count < 1)
                        {
                            TaskDialog.Show("Error", "Please check if destination view has the correct area indicator (ex: - 1 for area 1)");
                            return Result.Cancelled;
                        }
                        new ModifyArc(sourceArea3Id, area3ViewId, doc, "Modify Grid Area3 Plan").MoveGrid();
                        break;

                    case "curvegridarea4":

                        var sourceArea4Id = new ViewsElementId(doc, "curvegridarea4").ElementList();
                        var area4ViewId = new ViewsElementId(doc, "- 4").ElementList();
                        if (area4ViewId.Count < 1)
                        {
                            TaskDialog.Show("Error", "Please check if destination view has the correct area indicator (ex: - 1 for area 1)");
                            return Result.Cancelled;
                        }
                        new ModifyArc(sourceArea4Id, area4ViewId, doc, "Modify Grid Area4 Plan").MoveGrid();
                        break;

                    case "curvegridarea5":

                        var sourceArea5Id = new ViewsElementId(doc, "curvegridarea5").ElementList();
                        var area5ViewId = new ViewsElementId(doc, "- 5").ElementList();
                        if (area5ViewId.Count < 1)
                        {
                            TaskDialog.Show("Error", "Please check if destination view has the correct area indicator (ex: - 1 for area 1)");
                            return Result.Cancelled;
                        }
                        new ModifyArc(sourceArea5Id, area5ViewId, doc, "Modify Grid Area5 Plan").MoveGrid();
                        break;

                    case "curvegridarea6":

                        var sourceArea6Id = new ViewsElementId(doc, "curvegridarea6").ElementList();
                        var area6ViewId = new ViewsElementId(doc, "- 6").ElementList();
                        if (area6ViewId.Count < 1)
                        {
                            TaskDialog.Show("Error", "Please check if destination view has the correct area indicator (ex: - 1 for area 1)");
                            return Result.Cancelled;
                        }
                        new ModifyArc(sourceArea6Id, area6ViewId, doc, "Modify Grid Area6 Plan").MoveGrid();
                        break;

                    case "curvegridarea7":

                        var sourceArea7Id = new ViewsElementId(doc, "curvegridarea7").ElementList();
                        var area7ViewId = new ViewsElementId(doc, "- 7").ElementList();
                        if (area7ViewId.Count < 1)
                        {
                            TaskDialog.Show("Error", "Please check if destination view has the correct area indicator (ex: - 1 for area 1)");
                            return Result.Cancelled;
                        }
                        new ModifyArc(sourceArea7Id, area7ViewId, doc, "Modify Grid Area7 Plan").MoveGrid();
                        break;

                    case "curvegridarea8":

                        var sourceArea8Id = new ViewsElementId(doc, "curvegridarea8").ElementList();
                        var area8ViewId = new ViewsElementId(doc, "- 8").ElementList();
                        if (area8ViewId.Count < 1)
                        {
                            TaskDialog.Show("Error", "Please check if destination view has the correct area indicator (ex: - 1 for area 1)");
                            return Result.Cancelled;
                        }
                        new ModifyArc(sourceArea8Id, area8ViewId, doc, "Modify Grid Area8 Plan").MoveGrid();
                        break;

                    case "curvegridarea9":

                        var sourceArea9Id = new ViewsElementId(doc, "curvegridarea9").ElementList();
                        var area9ViewId = new ViewsElementId(doc, "- 9").ElementList();
                        if (area9ViewId.Count < 1)
                        {
                            TaskDialog.Show("Error", "Please check if destination view has the correct area indicator (ex: - 1 for area 1)");
                            return Result.Cancelled;
                        }
                        new ModifyArc(sourceArea9Id, area9ViewId, doc, "Modify Grid Area9 Plan").MoveGrid();
                        break;

                    case "curvegridarea10":

                        var sourceArea10Id = new ViewsElementId(doc, "curvegridarea10").ElementList();
                        var area10ViewId = new ViewsElementId(doc, "- 10").ElementList();
                        if (area10ViewId.Count < 1)
                        {
                            TaskDialog.Show("Error", "Please check if destination view has the correct area indicator (ex: - 1 for area 1)");
                            return Result.Cancelled;
                        }
                        new ModifyArc(sourceArea10Id, area10ViewId, doc, "Modify Grid Area10 Plan").MoveGrid();
                        break;

                    case "curvegridarea11":

                        var sourceArea11Id = new ViewsElementId(doc, "curvegridarea11").ElementList();
                        var area11ViewId = new ViewsElementId(doc, "- 11").ElementList();
                        if (area11ViewId.Count < 1)
                        {
                            TaskDialog.Show("Error", "Please check if destination view has the correct area indicator (ex: - 1 for area 1)");
                            return Result.Cancelled;
                        }
                        new ModifyArc(sourceArea11Id, area11ViewId, doc, "Modify Grid Area11 Plan").MoveGrid();
                        break;

                    case "curvegridarea12":

                        var sourceArea12Id = new ViewsElementId(doc, "curvegridarea12").ElementList();
                        var area12ViewId = new ViewsElementId(doc, "- 12").ElementList();
                        if (area12ViewId.Count < 1)
                        {
                            TaskDialog.Show("Error", "Please check if destination view has the correct area indicator (ex: - 1 for area 1)");
                            return Result.Cancelled;
                        }
                        new ModifyArc(sourceArea12Id, area12ViewId, doc, "Modify Grid Area12 Plan").MoveGrid();
                        break;
                        
                    default:
                        TaskDialog.Show("Error", "No Curved Grids found or missing CurveGridArea Views");
                        break;
                }
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
        .Where(e => e.Name.ToLower().Contains(ViewName));


        var listViewIds = new List<ElementId>();
        foreach (var e in viewPlanCollector)
        {
            
            listViewIds.Add(e.Id);

        }

        return listViewIds;
    }
    public List<string> CheckSourceForCurve()
    {
        var viewPlanCollector = new FilteredElementCollector(Doc)
        .OfClass(typeof(ViewPlan))
        .Where(e => e.Name.ToLower().Contains(ViewName));


        var viewIdsContainCurve = new List<string>();
        // this block is for checking if curved grids exist in any of the source views. 
        // if true, return the name of the views
        foreach (var e in viewPlanCollector)
        {
            var gridCurrentView = new FilteredElementCollector(Doc, e.Id)
                .OfClass(typeof(Grid))
                .ToElements();
            foreach (var g in gridCurrentView)
            {
                var sourceGrid = g as Grid;
                if (sourceGrid.IsCurved == true)
                {
                    
                    viewIdsContainCurve.Add(e.Name.ToLower());

                }
            }


        }

        // distinct method to remove duplicate elements.
        // as we are only interested in one entry of each view
        return viewIdsContainCurve.Distinct().ToList();
    }
}

public class ModifyArc
{
    public Document Doc { get; set; }
    public List<ElementId> ViewIds { get; set; }
    public List<ElementId> AreaIds { get; set; }
    // trans number is to identify which transaction for which area ran
    public string TransactionNumber { get; set; }

    public List<Arc> Arcs { get; set; }
    public ModifyArc(List<ElementId> viewIds, List<ElementId> areaIds, Document doc, string transNumber)
    {

        ViewIds = viewIds;
        AreaIds = areaIds;
        Doc = doc;
        TransactionNumber = transNumber;

    }
    public void MoveGrid()
    {
        using (Transaction t = new Transaction(Doc))
        {
            t.Start(TransactionNumber);

            // ids of source views used for extracting the source grids
            foreach (var id in ViewIds)
            {
                var iterationView = Doc.GetElement(id) as View;
                var gridCurrentView = new FilteredElementCollector(Doc, id)
                .OfClass(typeof(Grid))
                .ToElements();

                // get source grids and only select curved grids
                foreach (var e in gridCurrentView)
                {

                    var curveInView = (e as Grid).GetCurvesInView(DatumExtentType.ViewSpecific, iterationView);
                    var sourceGrid = e as Grid;

                    if (sourceGrid.IsCurved == false)
                    {
                        continue;
                    }

                    foreach (var c in curveInView)
                    {

                        var viewArc = c as Arc;

                        // this is for the grids that will be proccessed in each area view
                        // get the Ids with the respective area.
                        foreach (var i in AreaIds)
                        {
                            var iterationViewArea = Doc.GetElement(i) as View;
                            var gridCurrentViewArea = new FilteredElementCollector(Doc, i)
                            .OfClass(typeof(Grid))
                            .ToElements();



                            // views ids for area views
                            foreach (var g in gridCurrentViewArea)
                            {
                                var gridInArea = g as Grid;


                                if (gridInArea.Id == sourceGrid.Id)
                                {
                                    // needed to adjust the Z value of the Center because it changes based the level of the view.
                                    // the boundingbox max.z value contains the levels elevation value.
                                    var adjustedCenter = new XYZ(viewArc.Center.X, viewArc.Center.Y, gridInArea.get_BoundingBox(iterationViewArea).Min.Z);

                                    var arc = Arc.Create(adjustedCenter, viewArc.Radius, viewArc.GetEndParameter(0), viewArc.GetEndParameter(1), viewArc.XDirection, viewArc.YDirection);


                                    gridInArea.SetCurveInView(DatumExtentType.ViewSpecific, iterationViewArea, arc);
                                }
                            }
                        }
                    }
                }
            }
            t.Commit();
        }

    }




}