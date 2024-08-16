using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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

namespace VDC_App
{
    /// <summary>
    /// Interaction logic for ProjectSetup.xaml
    /// </summary>
    public partial class ProjectSetupUI : Window
    {
        private Document Doc;

        private List<LevelsElevations> SelectedElemList;

        public ProjectSetupUI(Document doc)
        {
            Doc = doc;

            InitializeComponent();
            PopulateLevels();



        }
        private void PopulateLevels()
        {
            // get list of levels in project
            var levels = new collector(Doc).GetLevelsList();


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
                LevelsInfo.Items.Add(i);
            }



        }

        private void vrButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedElemList = new List<LevelsElevations>();
            if (LevelsInfo.SelectedItems.Count > 0)
            {
                foreach (var i in LevelsInfo.SelectedItems)
                {
                    var selectedLevInfo = i as LevelsElevations;

                    //MessageBox.Show(selectedLevInfo.Id.ToString(), "test", MessageBoxButton.OK);

                    SelectedElemList.Add(selectedLevInfo);

                }

            }
            else
            {
                MessageBox.Show("Please Select A Level", "Selection Error", MessageBoxButton.OK);
            }


            //var simpF = new SimpleForm(test);
            //simpF.Show();




            //MessageBox.Show(paramList.View.Name, "Selection Error", MessageBoxButton.OK);

            CreateViewTemplates();
            //this.Close();
        }

        private void CreateViewTemplates()
        {

            var paramList = new EditViewTemplate(Doc);
            var turnOffList = paramList.SetViewRange();
            var view = paramList.View;
            try
            {
                using (Transaction t = new Transaction(Doc))
                {

                    t.Start("Create View Template");

                    foreach (var e in SelectedElemList)
                    {
                        var levElement = Doc.GetElement(e.Id);
                        var levName = levElement.Name;
                        var vrInput = e.ViewRange;
                        var vToViewPlan = view as ViewPlan;

                        var vr = vToViewPlan.GetViewRange();

                        // need to figure out what the parameter that is used for associated level in view ranges
                        var assoLev = new ElementId(BuiltInParameter.ASSOCIATED_LEVEL_OFFSET);

                        vr.SetLevelId(PlanViewPlane.TopClipPlane, assoLev);

                        //vr.SetOffset(PlanViewPlane.CutPlane, 30.0);
                        //vr.SetOffset(PlanViewPlane.TopClipPlane, 30.0);

                        vToViewPlan.SetViewRange(vr);


                        //vr.SetOffset(PlanViewPlane.BottomClipPlane, 1.0);
                        //vToViewPlan.SetViewRange(vr);



                        view.Name = "VDC_ViewRange - " + levName;
                        view.SetNonControlledTemplateParameterIds(turnOffList);
                        view.CreateViewTemplate();
                        
                    }

                    // reset template reference name
                    view.Name = "_View Templates";

                    t.Commit();
                }

            }
            catch (Exception ex)
            {
                TaskDialog.Show("asdas", ex.ToString());


            }



            //private void Test()
            //{
            //    if (ElemIdList != null)
            //    {
            //        var simpF = new SimpleForm(ElemIdList);
            //        simpF.Show();
            //    }
            //}
        }
    }
}
