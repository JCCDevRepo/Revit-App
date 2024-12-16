using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using static System.Windows.Forms.AxHost;
using Grid = Autodesk.Revit.DB.Grid;
using View = Autodesk.Revit.DB.View;

namespace VDC_App
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ModifyGridExtents : IExternalCommand
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
                .Where(e => e.Name.ToLower().Contains("sheet"));








            var listViewIds = new List<ElementId>();
            foreach (var e in viewPlanCollector)
            {
                var viewPlans = e as ViewPlan;
                if(viewPlans.IsTemplate)
                {
                    continue;
                }

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
                t.Start("Modify Grid Extents");
                try
                {

                    //var listPoints = new List<XYZ>();
                    #region Main Curve instructions

                    // this is for displaying grids/views that were not processed.
                    var listCurvedGrids = new List<string>();
                    var missingSCBox = new List<string>();
                    var viewGridMissing = new List<string>();




                    foreach (var id in listViewIds)
                    {
                        var iterationView = doc.GetElement(id) as View;
                        var iterationElement = doc.GetElement(id);

                        var gridCurrentView = new FilteredElementCollector(doc, id)
                            .OfClass(typeof(Grid))
                            .ToList();

                        // error detection: if revit grids do not exist
                        if (gridCurrentView.Count <= 0)
                        {
                            //TaskDialog.Show("Grid Error", $"No Revit grids detected\n{iterationView.Name}");
                            //return Result.Cancelled;
                            viewGridMissing.Add(iterationView.Name);

                        }

                        foreach (var g in gridCurrentView)
                        {

                            var elemToGrid = g as Grid;
                            var gridCurve = elemToGrid.GetCurvesInView(DatumExtentType.ViewSpecific, iterationView);




                            foreach (var c in gridCurve)
                            {
                                // error checking
                                // cropped view checking
                                if (iterationView.CropBoxActive == false)
                                {
                                    //TaskDialog.Show("ScopeBox Error", $"View Does not Have Scopebox Applied\nView Name: {iterationView.Name}");
                                    //return Result.Cancelled;
                                    
                                    missingSCBox.Add(iterationView.Name);
                                    continue;

                                }

                                if (iterationView.Name == "LEVEL x##### - Sheet_Risers" | iterationView.Name == "LEVEL x##### - Sheet_Sketches")
                                {
                                    continue;
                                }

                                // no algorithm for curved grid written.
                                // adds to a list for notify user and cont with iteration
                                if (elemToGrid.IsCurved == true)
                                {
                                    if(!listCurvedGrids.Contains(elemToGrid.Name))
                                    {
                                        listCurvedGrids.Add(elemToGrid.Name);

                                    }
                                    continue;
                                }


                                // gridZ is needed for Z coordinates of every grid. (grid exists on associated level)
                                // im using the Z elevation from the bounding box of the current iteration view
                                var gridZ = elemToGrid.get_BoundingBox(iterationView).Max.Z;
                                //TaskDialog.Show("errer", gridZ.ToString());




                                XYZ start = c.GetEndPoint(0);
                                XYZ end = c.GetEndPoint(1);

                                var getLine = c as Line;

                                var vector = getLine.Direction;

                                //TaskDialog.Show("asdas", $"vector:{vector}\nVector1:{vector1}");

                                double tValueStart = 0.0;
                                double tValueEnd = 0.0;
                                double yCoordStart = 0.0;
                                double xCoordStart = 0.0;
                                double yCoordEnd = 0.0;
                                double xCoordEnd = 0.0;

                                // offset constant is needed because the bounding box of the grids are set at the end points of the grid.
                                // Therefore the grid bubble will not be accounted for.
                                var offsetConstant = 2.5;

                                // these are for diagonal lines
                                var offsetConstantStart = 3.5;
                                var offsetConstantEnd = 3.5;


                                var viewsBoundingBox = iterationElement.get_BoundingBox(iterationView);



                                double startAxisXt = 0.0;
                                double startAxisYt = 0.0;
                                double endAxisXt = 0.0;
                                double endAxisYt = 0.0;

                                //       Max.Y
                                //       -----
                                // Min.X |   | Max.X
                                //       _____
                                //       Min.Y
                                // this is the orientation of bounding boxes in Revit and the respective axis

                                // anylising the direction of start and end points (need this because direction of grid depends on how it is drawn)
                                // Look up tool reports the direction wrong as well (print out statment if needed)
                                // this is if the X axis starts from left to right. (Positive direction)
                                // the Y axis starts from the bottom Up (positive direction)
                                if (vector.X == 1.0 | vector.Y == 1.0)
                                {
                                    //TaskDialog.Show("Direction", vector.ToString());

                                    if (start.X < viewsBoundingBox.Min.X)
                                    {

                                        // this equation is to obtain the t value for the parametric equation below.
                                        // the difference of the grid start point and the scopebox is equals to the t value.
                                        // added the offset constant because the start of grids do not include the grid bubbles.

                                        // to shift the line, you need to account for the direction. 
                                        // in this case, a positive value will shift the line to the right and vice versa.
                                        tValueStart = (viewsBoundingBox.Min.X - start.X) + offsetConstant;

                                        //TaskDialog.Show("start", viewsBoundingBox.Max.ToString());
                                        //TaskDialog.Show("startX", tValueStart.ToString());
                                    }

                                    else if (start.Y < viewsBoundingBox.Min.Y)
                                    {
                                        // Y axis: shifting it up requires a positive a positive value.
                                        tValueStart = (viewsBoundingBox.Min.Y - start.Y) + offsetConstant;
                                        //TaskDialog.Show("startY", $"tvalue{tValueStart}");

                                    }



                                    if (end.X > viewsBoundingBox.Max.X)
                                    {
                                        // shifting line back towards 0 (left) requires a negative value
                                        tValueEnd = (viewsBoundingBox.Max.X - end.X) - offsetConstant;
                                        //TaskDialog.Show("EndX", tValueEnd.ToString());

                                    }


                                    else if (end.Y > viewsBoundingBox.Max.Y)
                                    {
                                        // shifting down towards 0 = negative value
                                        tValueEnd = (viewsBoundingBox.Max.Y - end.Y) - offsetConstant;
                                        //TaskDialog.Show("EndY", tValueEnd.ToString());

                                    }

                                    // this is a parametric equation that calculates the X/Y coordinates.
                                    // x(t) = X1 + t *  a
                                    // y(t) = Y1 + t *  a
                                    // t is a paramter that varies and represent points on the line.
                                    // so, you can utilize t to shift the X/Y coords along the same vector. Affectively moving the line without changing the vector
                                    startAxisXt = start.X + tValueStart * vector.X;
                                    startAxisYt = start.Y + tValueStart * vector.Y;
                                    endAxisXt = end.X + tValueEnd * vector.X;
                                    endAxisYt = end.Y + tValueEnd * vector.Y;
                                }




                                // anylising the direction of start and end points (need this because direction of grid depends on how it is drawn)
                                // Look up tool reports the direction wrong as well (print out statment if needed)
                                // this is if the start point starts from right to left. (negative direction)
                                // the Y axis starts from the top down (negative direction)
                                if (vector.X == -1.0 | vector.Y == -1.0)
                                {

                                    if (start.X > viewsBoundingBox.Max.X)
                                    {
                                        //TaskDialog.Show("Direction", $"Direction: {vector}\n{g.Name}");

                                        // because direction changed, to shift line back towards 0 requires a positive value
                                        tValueStart = (start.X - viewsBoundingBox.Max.X) + offsetConstant;

                                        //TaskDialog.Show("start", viewsBoundingBox.Max.ToString());
                                        //TaskDialog.Show("EndX", tValueEnd.ToString());
                                    }

                                    else if (start.Y > viewsBoundingBox.Max.Y)
                                    {
                                        // same for shifting down towards 0, requires a positive value
                                        tValueStart = (start.Y - viewsBoundingBox.Max.Y) + offsetConstant;
                                        //TaskDialog.Show("start", $"tvalue{tValueStart}");

                                    }



                                    if (end.X < viewsBoundingBox.Min.X)
                                    {
                                        // negative value for shifting away from 0
                                        tValueEnd = (end.X - viewsBoundingBox.Min.X) - offsetConstant;

                                    }


                                    else if (end.Y < viewsBoundingBox.Min.Y)
                                    {
                                        // negative value for shifting up towards 0
                                        tValueEnd = (end.Y - viewsBoundingBox.Min.Y) - offsetConstant;
                                        //TaskDialog.Show("EndY", tValueEnd.ToString());

                                    }

                                    // same equation as above
                                    startAxisXt = start.X + tValueStart * vector.X;
                                    startAxisYt = start.Y + tValueStart * vector.Y;
                                    endAxisXt = end.X + tValueEnd * vector.X;
                                    endAxisYt = end.Y + tValueEnd * vector.Y;
                                }




                                // this is for diagonal lines from left/bottom to top/right 
                                // view diagram in OneNote: ex.1
                                // condition set to X > 0.0001 because conds above will also be processed if x > 0.0 is set.
                                // cond X < 0 but > 0.0001 is for diagonal vectors between 0.0 - 1.0
                                if (vector.X < 1.0 & vector.X > 0.000000001 & vector.Y < 1.0 & vector.Y > 0.000000001)
                                {
                                    //Diagram below shows the location of the start/end of point with *
                                    //         Max.Y
                                    //         -----
                                    // * Min.X |   | Max.X
                                    //         _____
                                    //         Min.Y
                                    if (start.X < viewsBoundingBox.Min.X)
                                    {
                                        // this is utilizing the y = mx + b equation
                                        // determines the X or Y intercepts to obtain the coordinates
                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);

                                        xCoordStart = viewsBoundingBox.Min.X;

                                        yCoordStart = slope * (xCoordStart - x1) + y1;



                                    }
                                    //       Max.Y
                                    //       -----
                                    // Min.X |   | Max.X
                                    //       _____
                                    //       Min.Y
                                    //        *
                                    else if (start.Y < viewsBoundingBox.Min.Y)
                                    {

                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);


                                        yCoordStart = viewsBoundingBox.Min.Y;

                                        xCoordStart = ((yCoordStart - y1) / slope) + x1;
                                        //TaskDialog.Show("StartX", $"xIntercept:{xCoordStart}");



                                    }
                                    // if the start of the diagonal lines are within the scopebox, keep the starting coords and reset offsetconst to 0.0
                                    else
                                    {
                                        xCoordStart = start.X;
                                        yCoordStart = start.Y;
                                        offsetConstantStart = 0.0;
                                    }

                                    //       Max.Y
                                    //       -----
                                    // Min.X |   | Max.X *
                                    //       _____
                                    //       Min.Y
                                    if (end.X > viewsBoundingBox.Max.X)
                                    {
                                        // this is utilizing the y = mx + b equation
                                        // determines the X or Y intercepts to obtain the coordinates
                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);

                                        xCoordEnd = viewsBoundingBox.Max.X;

                                        yCoordEnd = slope * (xCoordEnd - x1) + y1;




                                    }
                                    //         *
                                    //       Max.Y
                                    //       -----
                                    // Min.X |   | Max.X
                                    //       _____
                                    //       Min.Y
                                    else if (end.Y > viewsBoundingBox.Max.Y)
                                    {

                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);


                                        yCoordEnd = viewsBoundingBox.Max.Y;

                                        xCoordEnd = ((yCoordEnd - y1) / slope) + x1;
                                        //TaskDialog.Show("StartX", $"xIntercept:{xCoordEnd}");





                                    }
                                    else
                                    {
                                        xCoordEnd = end.X;
                                        yCoordEnd = end.Y;
                                        offsetConstantEnd = 0.0;
                                    }



                                    // I had to modify the equation because tvalue is difficult to obtain for diagonal lines
                                    startAxisXt = xCoordStart + offsetConstantStart * vector.X;
                                    startAxisYt = yCoordStart + offsetConstantStart * vector.Y;
                                    endAxisXt = xCoordEnd - offsetConstantEnd * vector.X;
                                    endAxisYt = yCoordEnd - offsetConstantEnd * vector.Y;

                                }


                                // this is for diagonal lines from start: right/top down to left/bottom 
                                // view diagram in OneNote: ex.2
                                if (vector.X > -1.0 & vector.X < -0.000000001 & vector.Y > -1.0 & vector.Y < -0.000000001)
                                {
                                    //TaskDialog.Show("sdfsd", $"Test");


                                    //       Max.Y
                                    //       -----
                                    // Min.X |   | Max.X *
                                    //       _____
                                    //       Min.Y
                                    if (start.X > viewsBoundingBox.Max.X)
                                    {
                                        // this is utilizing the y = mx + b equation
                                        // determines the X or Y intercepts to obtain the coordinates
                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);

                                        xCoordStart = viewsBoundingBox.Max.X;

                                        yCoordStart = slope * (xCoordStart - x1) + y1;




                                    }
                                    //        *
                                    //       Max.Y
                                    //       -----
                                    // Min.X |   | Max.X
                                    //       _____
                                    //       Min.Y
                                    else if (start.Y > viewsBoundingBox.Max.Y)
                                    {

                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);


                                        yCoordStart = viewsBoundingBox.Max.Y;

                                        xCoordStart = ((yCoordStart - y1) / slope) + x1;



                                    }
                                    // if the start of the diagonal lines are within the scopebox, keep the starting coords and reset offsetconst to 0.0
                                    else
                                    {
                                        xCoordStart = start.X;
                                        yCoordStart = start.Y;
                                        offsetConstantStart = 0.0;
                                    }

                                    //        Max.Y
                                    //        -----
                                    // *Min.X |   | Max.X
                                    //        _____
                                    //        Min.Y
                                    if (end.X < viewsBoundingBox.Min.X)
                                    {
                                        // this is utilizing the y = mx + b equation
                                        // determines the X or Y intercepts to obtain the coordinates
                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);

                                        xCoordEnd = viewsBoundingBox.Min.X;

                                        yCoordEnd = slope * (xCoordEnd - x1) + y1;



                                    }

                                    //       Max.Y
                                    //       -----
                                    // Min.X |   | Max.X
                                    //       _____
                                    //       Min.Y 
                                    //        *
                                    else if (end.Y < viewsBoundingBox.Min.Y)
                                    {

                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);


                                        yCoordEnd = viewsBoundingBox.Min.Y;

                                        xCoordEnd = ((yCoordEnd - y1) / slope) + x1;


                                    }
                                    else
                                    {
                                        xCoordEnd = end.X;
                                        yCoordEnd = end.Y;
                                        offsetConstantEnd = 0.0;
                                    }



                                    // I had to modify the equation because tvalue is difficult to obtain for diagonal lines
                                    startAxisXt = xCoordStart + offsetConstantStart * vector.X;
                                    startAxisYt = yCoordStart + offsetConstantStart * vector.Y;
                                    endAxisXt = xCoordEnd - offsetConstantEnd * vector.X;
                                    endAxisYt = yCoordEnd - offsetConstantEnd * vector.Y;


                                }

                                // this is for diagonal lines from start: left/top down to right/bottom 
                                // cond X < 0 but > 0.0001 is for diagonal vectors between 0.0 - 1.0
                                if (vector.X < 1.0 & vector.X > 0.000000001 & vector.Y > -1.0 & vector.Y < -0.000000001)
                                {
                                    //TaskDialog.Show("sdfsd", $"start{start}\nend{end}");


                                    //        Max.Y
                                    //        -----
                                    // *Min.X |   | Max.X
                                    //        _____
                                    //        Min.Y
                                    if (start.X < viewsBoundingBox.Min.X)
                                    {
                                        // this is utilizing the y = mx + b equation
                                        // determines the X or Y intercepts to obtain the coordinates
                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);

                                        xCoordStart = viewsBoundingBox.Min.X;

                                        yCoordStart = slope * (xCoordStart - x1) + y1;




                                    }
                                    //        *
                                    //       Max.Y
                                    //       -----
                                    // Min.X |   | Max.X
                                    //       _____
                                    //       Min.Y
                                    else if (start.Y > viewsBoundingBox.Max.Y)
                                    {

                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);


                                        yCoordStart = viewsBoundingBox.Max.Y;

                                        xCoordStart = ((yCoordStart - y1) / slope) + x1;



                                    }
                                    // if the start of the diagonal lines are within the scopebox, keep the starting coords and reset offsetconst to 0.0
                                    else
                                    {
                                        xCoordStart = start.X;
                                        yCoordStart = start.Y;
                                        offsetConstantStart = 0.0;
                                    }

                                    //        Max.Y
                                    //        -----
                                    //  Min.X |   | Max.X *
                                    //        _____
                                    //        Min.Y
                                    if (end.X > viewsBoundingBox.Max.X)
                                    {
                                        // this is utilizing the y = mx + b equation
                                        // determines the X or Y intercepts to obtain the coordinates
                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);

                                        xCoordEnd = viewsBoundingBox.Max.X;

                                        yCoordEnd = slope * (xCoordEnd - x1) + y1;



                                    }

                                    //       Max.Y
                                    //       -----
                                    // Min.X |   | Max.X
                                    //       _____
                                    //       Min.Y 
                                    //        *
                                    else if (end.Y < viewsBoundingBox.Min.Y)
                                    {

                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);


                                        yCoordEnd = viewsBoundingBox.Min.Y;

                                        xCoordEnd = ((yCoordEnd - y1) / slope) + x1;


                                    }
                                    else
                                    {
                                        xCoordEnd = end.X;
                                        yCoordEnd = end.Y;
                                        offsetConstantEnd = 0.0;
                                    }



                                    // I had to modify the equation because tvalue is difficult to obtain for diagonal lines
                                    startAxisXt = xCoordStart + offsetConstantStart * vector.X;
                                    startAxisYt = yCoordStart + offsetConstantStart * vector.Y;
                                    endAxisXt = xCoordEnd - offsetConstantEnd * vector.X;
                                    endAxisYt = yCoordEnd - offsetConstantEnd * vector.Y;


                                }

                                // this is for diagonal lines from start: right/bottom up to left/top 
                                // view diagram in OneNote: ex.2
                                if (vector.X > -1.0 & vector.X < -0.000000001 & vector.Y < 1.0 & vector.Y > 0.000000001)
                                {


                                    //       Max.Y
                                    //       -----
                                    // Min.X |   | Max.X *
                                    //       _____
                                    //       Min.Y
                                    if (start.X > viewsBoundingBox.Max.X)
                                    {
                                        // this is utilizing the y = mx + b equation
                                        // determines the X or Y intercepts to obtain the coordinates
                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);

                                        xCoordStart = viewsBoundingBox.Max.X;

                                        yCoordStart = slope * (xCoordStart - x1) + y1;




                                    }
                                    //        
                                    //       Max.Y
                                    //       -----
                                    // Min.X |   | Max.X
                                    //       _____
                                    //       Min.Y
                                    //         *
                                    else if (start.Y < viewsBoundingBox.Min.Y)
                                    {

                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);


                                        yCoordStart = viewsBoundingBox.Min.Y;

                                        xCoordStart = ((yCoordStart - y1) / slope) + x1;



                                    }
                                    // if the start of the diagonal lines are within the scopebox, keep the starting coords and reset offsetconst to 0.0
                                    else
                                    {
                                        xCoordStart = start.X;
                                        yCoordStart = start.Y;
                                        offsetConstantStart = 0.0;
                                    }

                                    //        Max.Y
                                    //        -----
                                    // *Min.X |   | Max.X
                                    //        _____
                                    //        Min.Y
                                    if (end.X < viewsBoundingBox.Min.X)
                                    {
                                        // this is utilizing the y = mx + b equation
                                        // determines the X or Y intercepts to obtain the coordinates
                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);

                                        xCoordEnd = viewsBoundingBox.Min.X;

                                        yCoordEnd = slope * (xCoordEnd - x1) + y1;



                                    }

                                    //         *
                                    //       Max.Y
                                    //       -----
                                    // Min.X |   | Max.X
                                    //       _____
                                    //       Min.Y 
                                    else if (end.Y > viewsBoundingBox.Max.Y)
                                    {

                                        double x1 = start.X, y1 = start.Y;
                                        double x2 = end.X, y2 = end.Y;

                                        var slope = (y2 - y1) / (x2 - x1);


                                        yCoordEnd = viewsBoundingBox.Max.Y;

                                        xCoordEnd = ((yCoordEnd - y1) / slope) + x1;


                                    }
                                    else
                                    {
                                        xCoordEnd = end.X;
                                        yCoordEnd = end.Y;
                                        offsetConstantEnd = 0.0;
                                    }



                                    // I had to modify the equation because tvalue is difficult to obtain for diagonal lines
                                    startAxisXt = xCoordStart + offsetConstantStart * vector.X;
                                    startAxisYt = yCoordStart + offsetConstantStart * vector.Y;
                                    endAxisXt = xCoordEnd - offsetConstantEnd * vector.X;
                                    endAxisYt = yCoordEnd - offsetConstantEnd * vector.Y;


                                }


                                XYZ newStart = new XYZ(startAxisXt, startAxisYt, gridZ);
                                XYZ newEnd = new XYZ(endAxisXt, endAxisYt, gridZ);


                                var newLine = Line.CreateBound(newStart, newEnd);

                                

                                //TaskDialog.Show("dfds", $"start:{start}\nend:{end}\nnewStart:{newStart}\nnewEnd{newEnd}\nlineStart:{newLine.GetEndPoint(0)}\nlineStart:{newLine.GetEndPoint(1)}\nvector:{vector}");


                                //var testStart = new XYZ(start.X, start.Y, gridZ);
                                //var testEnd = new XYZ(end.X, end.Y, gridZ);


                                //var testLine = Line.CreateBound(testStart, testEnd);
                                //doc.Create.NewDetailCurve(doc.ActiveView, testLine);


                                //Line newLine = Line.CreateBound(newStart, newEnd);





                                elemToGrid.SetCurveInView(DatumExtentType.ViewSpecific, iterationView, newLine);


                            }



                        }

                    }

                    // displays the name of the curved grids to the user
                    if(listCurvedGrids.Any() | missingSCBox.Any() | viewGridMissing.Any())
                    {
                        var mSCBoxTemp = missingSCBox.Distinct().ToList();
                        var vGMTemp = viewGridMissing.Distinct().ToList();
                        listCurvedGrids.Insert(0, "Curved Grid Lines:");
                        listCurvedGrids.Add("Views With Missing Scope Boxes:");
                        listCurvedGrids.AddRange(mSCBoxTemp);
                        listCurvedGrids.Add("Views with Missing Grids:");
                        listCurvedGrids.AddRange(vGMTemp);

                        TaskDialog.Show("Issues Found", $"Curved grids, Missing Scope Boxes/Grids were detected and not processed. Please review");

                        var curvedGrids = new SimpleForm(listCurvedGrids);
                        curvedGrids.Show();


                    }
                    //if (missingSCBox.Count > 0)
                    //{

                    //}
                    //if (missingSCBox.Any())
                    //{
                    //    TaskDialog.Show("Issues Found", $"Scope Boxes are missing");
                    //    var scb = new SimpleForm(missingSCBox);
                    //    scb.Show();

                    //}

                    #endregion


                }

                catch (Exception ex)
                {
                    message = ex.Message;
                    var errorMessage = "Please Double Check Grids";

                    TaskDialog.Show("Error", errorMessage);
                    //t.RollBack();
                    return Result.Cancelled;
                }
                
              
                t.Commit();
            }

            return Result.Succeeded;
        }




    }


}
