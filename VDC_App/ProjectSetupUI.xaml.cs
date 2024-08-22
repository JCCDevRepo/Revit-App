using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Application = Autodesk.Revit.ApplicationServices.Application;
using Autodesk.Revit.DB.Events;
using System.Drawing;

namespace VDC_App
{
    /// <summary>
    /// Interaction logic for ProjectSetup.xaml
    /// </summary>
    public partial class ProjectSetupUI : Window
    {
        private Document Doc;
        private Application App;
        //private DocumentChangedEventArgs DocumentChangedEventArgs;

        private List<LevelsElevations> UserSelected;
        private List<ElementId> NewIds;


        public ProjectSetupUI(Document doc, Application app)
        {
            Doc = doc;
            App = app;

            InitializeComponent();
            PopulateLevels();



        }
        private void PopulateLevels()
        {
            // get list of levels in project
            var levels = new collector(Doc).GetLevelsList();

            if (levels.Count < 1)
            {
                MessageBox.Show("No Levels Detected.\nPlease Make Sure Levels Exist And Placed On the Correct Workset\n", "Levels Error", MessageBoxButton.OK );
                this.Close();
            }

            var orderedLevels = levels.OrderBy(e => e.Elevation).ToList();
            var levelParams = new List<LevelsElevations>();

            for (int i = 0; i < orderedLevels.Count; i++)
            {
                var viewR = 0.0;
                // The highest level in the iteration is set to 50ft
                if (i == orderedLevels.Count - 1)
                {
                    viewR = 50.0;

                }
                else
                {
                    // take the difference between level above and current level to get the view range
                    viewR = orderedLevels[i + 1].Elevation - orderedLevels[i].Elevation;

                }
                levelParams.Add(new LevelsElevations(orderedLevels[i].Name, orderedLevels[i].Elevation, viewR, orderedLevels[i].Id));
            }

            foreach (var i in levelParams)
            {
                ViewRangeSetup.Items.Add(i);
                CreatViewsSetup.Items.Add(i);
            }



        }

        private void vtButton_Click(object sender, RoutedEventArgs e)
        {
            //UserSelected = new List<LevelsElevations>();
            //if (ViewRangeSetup.SelectedItems.Count > 0)
            //{
            //    foreach (var i in ViewRangeSetup.SelectedItems)
            //    {
            //        var selectedLevInfo = i as LevelsElevations;

            //        //MessageBox.Show(selectedLevInfo.Id.ToString(), "test", MessageBoxButton.OK);

            //        UserSelected.Add(selectedLevInfo);

            //    }

            //}
            //else
            //{
            //    MessageBox.Show("Please Select A Level", "Selection Error", MessageBoxButton.OK);
            //}

            UserSelected = new UserSelectedItems(ViewRangeSetup).ReturnSelectedItems();

            if(UserSelected.Count == 0)
            {
                MessageBox.Show("Please Select A Level", "Selection Error", MessageBoxButton.OK);
            }

            //var simpF = new SimpleForm(test);
            //simpF.Show();





            CreateViewTemplates();


            //this.Close();
        }

        private void viewsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //return the user selected items from the UI
                UserSelected = new UserSelectedItems(CreatViewsSetup).ReturnSelectedItems();

                if (UserSelected.Count == 0)
                {
                    MessageBox.Show("Please Select A Level", "Selection Error", MessageBoxButton.OK);
                    return;
                }

                CreateViews();
            }


            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }

        private void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            //MessageBox.Show(i.ToString());
            NewIds.AddRange(e.GetAddedElementIds());
        }


        #region Create Views
        private void CreateViews()
        {
            // get the view family type of "-working"
            var viewTypeCol = new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewFamilyType))
                .Where(f => f.Name.Equals("-Working")).FirstOrDefault();

            if (viewTypeCol == null)
            {
                MessageBox.Show("-Working View Family Type Is Missing");
            }

            NewIds = new List<ElementId>();
            // event that returns the newly created element ids
            App.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);

            using (Transaction t = new Transaction(Doc))
            {
                t.Start("Create Views");
                //var simpform = new SimpleForm(viewTypeCol);
                //simpform.Show();
                foreach (var l in UserSelected)
                {
                    var viewPlanElement = Doc.GetElement(l.Id) as ViewPlan;

                    var floorPlan = ViewPlan.Create(Doc, viewTypeCol.Id, l.Id);

                }
                t.Commit();
            }

            App.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);

            //var simpF = new SimpleForm(NewIds);
            //simpF.Show();
        }
    
        #endregion



        #region Create View Templates
        private void CreateViewTemplates()
        {
            try
            {
                using (Transaction t = new Transaction(Doc))
                {

                    t.Start("Create View Template");



                    var paramList = new EditViewTemplate(Doc);
                    var turnOffList = paramList.TemplateCreationInfo();
                    var refView = paramList.View;
                    var vToViewPlan = refView as ViewPlan;
                    var vr = vToViewPlan.GetViewRange();



                    foreach (var e in UserSelected)
                    {
                        var levElement = Doc.GetElement(e.Id);
                        var levName = levElement.Name;
                        var vrInput = e.ViewRange;

                        var templatesList = new collector(Doc, "vdc_viewrange").GetTemplatesList()
                            .Where(i => i.Name.Contains(levName)).FirstOrDefault();
                        //MessageBox.Show(templatesList.ToString());


                        if ( templatesList != null )
                        {
                            MessageBox.Show($"Duplicate Template detected: {levName}\nTemplate Creation Skipped");
                            continue;
                        }

                        // commentted code below can be used to set vr to the associated levels
                        // slight issue of offset defaulting to level elevation.

                        //vr.SetLevelId(PlanViewPlane.TopClipPlane, e.Id);
                        //vr.SetLevelId(PlanViewPlane.CutPlane, e.Id);
                        //vr.SetLevelId(PlanViewPlane.BottomClipPlane, e.Id);
                        //vr.SetLevelId(PlanViewPlane.ViewDepthPlane, e.Id);
                        //vToViewPlan.SetViewRange(vr);


                        vr.SetOffset(PlanViewPlane.CutPlane, e.ViewRange);
                        vr.SetOffset(PlanViewPlane.TopClipPlane, e.ViewRange);

                        vToViewPlan.SetViewRange(vr);


                        vr.SetOffset(PlanViewPlane.BottomClipPlane, 0.0);
                        vr.SetOffset(PlanViewPlane.ViewDepthPlane, 0.0);

                        vToViewPlan.SetViewRange(vr);



                        refView.Name = "VDC_ViewRange - " + levName;
                        refView.SetNonControlledTemplateParameterIds(turnOffList);
                        refView.CreateViewTemplate();

                    }

                    // reset template references
                    Doc.Regenerate();
                    refView.Name = "_View Templates";

                    vr.SetOffset(PlanViewPlane.CutPlane, 0.0);
                    vr.SetOffset(PlanViewPlane.TopClipPlane, 0.0);

                    vToViewPlan.SetViewRange(vr);


                    vr.SetOffset(PlanViewPlane.BottomClipPlane, 0.0);
                    vr.SetOffset(PlanViewPlane.ViewDepthPlane, 0.0);

                    vToViewPlan.SetViewRange(vr);

                    RenameViewTemplates();

                    t.Commit();


                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error Detected", ex.ToString());

            }
            
        }

        private void RenameViewTemplates()
        {

            try
            {
                var templatesList = new collector(Doc, "vdc_viewrange").GetTemplatesList()
                    .Where(e => e.Name.Contains("Copy"));

                if (templatesList != null)
                {
                    foreach (var e in templatesList)
                    {
                        var originalName = e.Name;

                        var indexOfCopy = originalName.IndexOf("Copy");

                        e.Name = originalName.Remove(indexOfCopy - 1);
                        //MessageBox.Show(e.Name);

                    }

                }
                else
                {
                    MessageBox.Show("Template Error", "No View Range Templates Found");

                }

            }
            catch 
            {
                MessageBox.Show("template already exists");
                

            }
            // get the viewrange templates list
            // select only the ones with "copy" in the name


        }
        #endregion


    }


}
