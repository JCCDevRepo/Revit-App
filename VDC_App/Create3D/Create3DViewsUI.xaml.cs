using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace VDC_App
{
    /// <summary>
    /// Interaction logic for Create3DViews.xaml
    /// </summary>
    public partial class Create3DViewsUI : Window
    {
        public Document doc { get; }
        public Create3DViewsUI(Document Doc)
        {
            doc = Doc;
            InitializeComponent();


        }


        private void CbIso_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CbJcc_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CbGc_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CbA3d_Checked(object sender, RoutedEventArgs e)
        {

        }

        public void Button_ClickCreate(object sender, RoutedEventArgs e)
        {


            
            IList<Level> levels = new FilteredElementCollector(doc)
            .OfClass(typeof(Level))
            .SkipWhile(s => s.Name == "INSERTION LEVEL")
            .Where(s => s.Name.Contains("LEV"))
            .Cast<Level>()
            .OrderBy(l => l.Elevation)
            .ToList();


            //SimpleForm form = new SimpleForm(levels);
            //form.Show();

            var prjInfo = doc.ProjectInformation;



            ViewFamilyType viewFamilyType = (from v in new FilteredElementCollector(doc).
                                 OfClass(typeof(ViewFamilyType)).
                                 Cast<ViewFamilyType>()
                                 where v.ViewFamily == ViewFamily.ThreeDimensional
                                 select v).First();







            try
            {
                using (Transaction t = new Transaction(doc, "Create Views"))
                {




                    var getImportGrid = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        // where method to access the import grid under Snooped caterogry > Name
                        .Where(s => s.Category.Name.EndsWith("GRID.dwg"))
                        .ToList();




                    //SimpleForm form = new SimpleForm(getImportGrid);
                    //form.Show();


                    //TaskDialog.Show("fsadf", getImportGrid.ToString());



                    // trying to isolate just one grid 
                    if (getImportGrid.Count > 1 | getImportGrid.Count <= 0)
                    {
                        TaskDialog.Show("Grid Error", "Please check if grid is imported or Multiple grids are imported");
                        t.RollBack();

                        //return Result.Cancelled;
                    }

                    var customViewName = TboxInput.Text;

                    //TaskDialog.Show("fdsf", TboxInput.Text);

                    int ctr = 0;
                    // loop through all levels
                    foreach (Level level in levels)
                    {
                        t.Start();

                        // LEVEL xxxxx
                        int indexOfSpace = level.Name.IndexOf(" ") + 1;

                        // remove everything but the level name
                        var editedLevelStr = level.Name.Remove(0, indexOfSpace);


                        // Set the name of the view
                        //viewForGc.Name = prjInfo.Number + level.Name + " Section Box";

                        foreach (var i in getImportGrid)
                        {
                            // Logic to isolate the X and Y coordinates: (82.2069046655, 405.257102627, 0.0000000)

                            // Max is front right upper limit of the section box
                            // the +- 100 is a buffer to extend the scope just incase grid bounding box is smaller than the RVT models
                            var max = i.get_BoundingBox(null).Max.ToString().Remove(0, 1);

                            // Min is bottom left & lower limit.
                            var min = i.get_BoundingBox(null).Min.ToString().Remove(0, 1);

                            var maxXValue = Convert.ToDouble(max.Remove(max.IndexOf("."))) + 100;

                            var getCommaIndex = max.IndexOf(",") + 1;
                            var maxYTemp = max.Remove(0, getCommaIndex);
                            var maxYValue = Convert.ToDouble(maxYTemp.Remove(maxYTemp.IndexOf("."))) + 100;


                            var minXValue = Convert.ToDouble(min.Remove(min.IndexOf("."))) - 100;

                            var getMinCommaIndex = min.IndexOf(",") + 1;
                            var minYTemp = min.Remove(0, getMinCommaIndex);
                            var minYValue = Convert.ToDouble(minYTemp.Remove(minYTemp.IndexOf("."))) - 100;



                            // Create a new BoundingBoxXYZ to define a 3D rectangular space
                            BoundingBoxXYZ boundingBoxXYZ = new BoundingBoxXYZ();

                            // Set the lower left bottom corner of the box
                            // Use the Z of the current level.
                            // X & Y values have been hardcoded based on this RVT geometry


                            if (ctr == 0)
                            {

                                boundingBoxXYZ.Min = new XYZ(minXValue, minYValue, level.Elevation - 50.0);
                                //TaskDialog.Show("asdf", boundingBoxXYZ.ToString());
                            }

                            else
                            {
                                boundingBoxXYZ.Min = new XYZ(minXValue, minYValue, level.Elevation);
                                //TaskDialog.Show("asdf", "it was skipped");

                            }

                            //TaskDialog.Show("min z", boundingBoxXYZ.Min.ToString());;

                            // Determine the height of the bounding box
                            double zOffset = 0;



                            // If there is another level above this one, use the elevation of that level
                            if (levels.Count > ctr + 1)
                                zOffset = levels.ElementAt(ctr + 1).Elevation;
                            // If this is the top level, use an offset of 50 feet
                            else
                                zOffset = level.Elevation + 50;
                            boundingBoxXYZ.Max = new XYZ(maxXValue, maxYValue, zOffset);


                            // this logic is for creating the views based on if the checkbox are checked or not
                            if (CbA3d.IsChecked == true | CbIso.IsChecked == true | CbGc.IsChecked == true | CbJcc.IsChecked == true | !string.IsNullOrEmpty(customViewName))
                            {
                                if (CbA3d.IsChecked == true)
                                {
                                    View3D view = View3D.CreateIsometric(doc, viewFamilyType.Id);


                                    view.Name = "3D-Lev" + editedLevelStr;

                                    view.SetSectionBox(boundingBoxXYZ);

                                }


                                if (CbIso.IsChecked == true)
                                {
                                    View3D viewIso = View3D.CreateIsometric(doc, viewFamilyType.Id);

                                    viewIso.Name = "ISO_TRADE-Lev" + editedLevelStr;


                                    viewIso.SetSectionBox(boundingBoxXYZ);


                                }


                                if (CbGc.IsChecked == true)
                                {
                                    View3D viewForGc = View3D.CreateIsometric(doc, viewFamilyType.Id);


                                    viewForGc.Name = "PRJ_TRADE_Lev" + editedLevelStr;


                                    viewForGc.SetSectionBox(boundingBoxXYZ);

                                }


                                if (CbJcc.IsChecked == true)
                                {
                                    View3D viewJcc = View3D.CreateIsometric(doc, viewFamilyType.Id);



                                    viewJcc.Name = prjInfo.Number + "_TRADE-Lev" + editedLevelStr;


                                    viewJcc.SetSectionBox(boundingBoxXYZ);

                                }

                                // if there are content in the textbox, the naming will be used to create the views
                                if (!string.IsNullOrEmpty(customViewName))
                                {
                                    View3D viewCustom = View3D.CreateIsometric(doc, viewFamilyType.Id);

                                    viewCustom.Name = customViewName + "_Lev" + editedLevelStr;

                                    viewCustom.SetSectionBox(boundingBoxXYZ);
                                }



                            }

                            else
                            {
                                break;
                            }




                        }







                        t.Commit();

 
                        ctr++;
                    }



                }
            }

            catch
            {

                var errorMessage = "Transaction cancelled\nOr\nDuplicate 3D views already exist";
                TaskDialog.Show("Error", errorMessage);

            }






        }


    }
}
