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
using Autodesk.Revit.DB.Events;
using System.Drawing;
using System.IO;
using Application = Autodesk.Revit.ApplicationServices.Application;
using MessageBox = System.Windows.MessageBox;
using View = Autodesk.Revit.DB.View;
using System.Text.RegularExpressions;



namespace VDC_App
{
    /// <summary>
    /// Interaction logic for ProjectSetup.xaml
    /// </summary>
    public partial class ProjectSetupUI : Window
    {
        private Document Doc;
        private Application App;

        private List<LevelsElevations> UserSelected;

        private List<ElementId> NewViewIds;
        //private List<ElementId> CreatedViewIds;
        private List<ElementId> NewViewRangeIds;
        private List<ElementId> NewViewTemplateIds;

        private List<ViewTypes> ViewTypes;
        private List<ViewTypes> SelectedViewTypes;
        private List<ViewTypes> SelectedTemplateType;


        public ProjectSetupUI(Document doc, Application app)
        {
            Doc = doc;
            App = app;
            InitializeComponent();
            PopulateLevels();
            PopulateViewTypes();
            PopulateScopeboxes();


        }
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
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
                SheetLevels.Items.Add(i);
            }



        }

        private void PopulateViewTypes()
        {
            // display view type objects
            var viewsList = new List<ViewTypes>
            {
                new ViewTypes { ViewType = "Working"},
                new ViewTypes { ViewType = "RCP"},
                new ViewTypes { ViewType = "ShopDrawing"},
                new ViewTypes { ViewType = "Sleeving"},
                new ViewTypes { ViewType = "AccessPanels"},
                new ViewTypes { ViewType = "BeamPens"},
                new ViewTypes { ViewType = "Hangers"},
                new ViewTypes { ViewType = "PadDrawings"},
                new ViewTypes { ViewType = "PointLoads"},
                new ViewTypes { ViewType = "Risers"},
                new ViewTypes { ViewType = "Sketches"},
                new ViewTypes { ViewType = "WallPens"},
                new ViewTypes { ViewType = "SupplementalSteel"},
                new ViewTypes { ViewType = "ServiceInstall"},
                new ViewTypes { ViewType = "Engineering"},
                new ViewTypes { ViewType = "BaseSupports"},

                //new ViewTypes { ViewType = "Custom"},

            };

            foreach (var v in viewsList)
            {
                //ViewTypeSetup.Items.Add(new { ViewTypes = v});
                ViewTypeSetup.Items.Add(v);
                ApplyTemplateType.Items.Add(v);
                CreateSheetType.Items.Add(v);
            }

            ViewTypes = viewsList;
        }


        private void PopulateScopeboxes()
        {
            // get all scopeboxes with the vdc naming convention
            var scopeBoxCol = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_VolumeOfInterest)
                .Where(e => e.Name.Contains("ScopeBox_"))
                .ToList();
            // bind it to the UI datagrid
            ScopeBoxes.ItemsSource = scopeBoxCol;

        }






        #region Create Views

        private void viewsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //return the user selected items from the UI
                UserSelected = new UserSelectedItems(CreatViewsSetup).SelectedLevels();
                if (UserSelected.Count == 0)
                {
                    MessageBox.Show("Please Select A Level", "Selection Error", MessageBoxButton.OK);
                    return;
                }

                // viewtype selection check
                var vTypeCheck = new UserSelectedItems(ViewTypeSetup).SelectedViewTypes();
                if (vTypeCheck.Count == 0)
                {
                    MessageBox.Show("Please Select A View Type", "Selection Error", MessageBoxButton.OK);
                    return;
                }

                // Scopebox Checking
                var scopeBoxCheck = new UserSelectedItems(ScopeBoxes).SelectedScopeBoxes();
                if (scopeBoxCheck.Count == 0)
                {
                    MessageBox.Show("Please Select A Scope Box");
                    return;
                }

                CreateViews();
                CreateDependentViews();

            }


            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }

        private void CreateViews()
        {
            // get the view family type of "-working"
            var viewFamilyCol = new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewFamilyType))
                .Where(f => f.Name.Equals("-Working")).FirstOrDefault();

            // get the view family type of "-RCPs"
            var viewFamilyRcpCol = new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewFamilyType))
                .Where(f => f.Name.Equals("Ceiling Plan")).FirstOrDefault();

            if (viewFamilyCol == null | viewFamilyRcpCol == null)
            {
                MessageBox.Show("View Family Type Is Missing For AF or RCP");
                return;
            }

            
            SelectedViewTypes = new UserSelectedItems(ViewTypeSetup).SelectedViewTypes();

            // checks for the view categories 
            // Need _View Template View as a reference in central
            var categoryCheck = new collector(Doc, "_view templates").GetViewsList();
            if (categoryCheck.Count == 0)
            {
                MessageBox.Show("Missing _View Templates View");
                //MessageBox.Show(categoryCheck.Count.ToString());

                return;
            }

            // check for view category/sub category parameter
            foreach (var e in categoryCheck)
            {
                if (e.LookupParameter("View Category") == null | e.LookupParameter("View SubCategory") == null)
                {
                    MessageBox.Show("View Category/Sub Category Parameters are missing.\nPlease Add Them To Views/Sheets", "View Category Error");
                    return;
                }
            }

            var vDuplicateCheck = new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewPlan))
                .Select(e => e.Name)
                .ToList();


            NewViewIds = new List<ElementId>();
            // event that returns the newly created element ids
            App.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);

            using (Transaction t = new Transaction(Doc))
            {

                t.Start("Create Views");

                // foreach loop for the user selected levels (do this for each of the user selected levels).
                foreach (var l in UserSelected)
                {
                    // while loop is for selected view types (for each selected levels, create all views of each type)
                    var i = 0;
                    while (i < SelectedViewTypes.Count)
                    {
                        // variable to simplify the statements. this just returns the view type at the current index
                        var viewTypeI = SelectedViewTypes[i].ViewType;

                        // switch for delineating view types and renaming
                        switch(viewTypeI)
                        {
                            case "Working":
                                {
                                    // if statement for checking if the view already exists
                                    if (!vDuplicateCheck.Contains(l.Level))
                                    {
                                        var createView = ViewPlan.Create(Doc, viewFamilyCol.Id, l.Id);
                                        createView.Name = l.Level;
                                        createView.LookupParameter("View Category").Set("_WORKING");
                                        createView.LookupParameter("View SubCategory").Set("Coordination");
                                    }

                                    break;
                                }

                            case "RCP":
                                {
                                    if (!vDuplicateCheck.Contains(l.Level + "_RCP"))
                                    {
                                        var createView = ViewPlan.Create(Doc, viewFamilyRcpCol.Id, l.Id);
                                        createView.Name = l.Level + "_RCP";
                                        createView.LookupParameter("View Category").Set("_WORKING");
                                        createView.LookupParameter("View SubCategory").Set("RCP");

                                    }
                                    break;
                                }

                            default:
                                {
                                    if(!vDuplicateCheck.Contains(l.Level + " - Sheet_" + viewTypeI + " - 0"))
                                    {
                                        var createView = ViewPlan.Create(Doc, viewFamilyCol.Id, l.Id);
                                        createView.Name = l.Level + " - Sheet_" + viewTypeI + " - 0";
                                        createView.LookupParameter("View Category").Set("ANNOTATION");
                                        createView.LookupParameter("View SubCategory").Set(viewTypeI);
                                    }

                                    break;
                                }

                            
                        }

                        i++;
                    }

                }

                t.Commit();
            }

            App.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);



        }

        private void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            //MessageBox.Show(e.ToString());
            NewViewIds.AddRange(e.GetAddedElementIds());
        }

      
        private void CreateDependentViews()
        {
            // get all vdc scope boxes in project
            var scopeBoxCol = new UserSelectedItems(ScopeBoxes).SelectedScopeBoxes();

            //var scopeBoxAsStr = new List<string>();
            //foreach (var e in scopeBoxCol) 
            //{
            //    var sbSuffix = Regex.Match(e.Name, @"_0*(\d+$)").Groups[1].Value;
            //    scopeBoxAsStr.Add(sbSuffix);
            //}
            

            var sheetCol = new FilteredElementCollector(Doc)
                .OfClass(typeof(View))
                .Where(e => e.Name.ToLower().Contains("sheet_"))
                .ToList();



            // this method filters out other views that are not necessary
            var sheetViews = new ViewsFromIds(Doc, sheetCol.Select(e => e.Id).ToList()).GetViews();

            var primaryViews = new List<View>();

            var dependV = new List<View>();

            foreach (var e in sheetViews)
            {
                var levStr = Regex.Match(e.Name, @"(level(.*?)) -", RegexOptions.IgnoreCase).Groups[1].Value;

                // returns 3 in ex: LEVEL x0001 - Sheet_ShopDrawing - 3
                var scopeboxStr = Regex.Match(e.Name, @" - (\d+$)").Groups[1].Value;

                // if primary value is -1 (view is parent view) and if view has no dependents (getdependviewids returns  < 1)
                if (e.GetPrimaryViewId().IntegerValue == -1 & e.GetDependentViewIds().Count() < 1)
                {
                    primaryViews.Add(e);
                    //MessageBox.Show(e.Name);

                }
                //else if (e.GetPrimaryViewId().IntegerValue == -1 & e.GetDependentViewIds().Count() > 1)
                //else if (UserSelected.Select(i => i.Level.Contains(levStr)).Any() & e.GetPrimaryViewId().IntegerValue != -1)
                //{
                //    var www = scopeBoxAsStr.Where(i => i.Equals(scopeboxStr));
                //    //var www = scopeBoxCol.Select(i => i.Name.  );
                //    //var www = scopeBoxCol.Where(i => i.Name.Contains(scopeboxStr));

                //    //MessageBox.Show(scopeboxStr);

                //    dependV.Add(www);


                //}
            }


            //var form = new SimpleForm(dependV.Select(e => e.Name));
            //form.ShowDialog();

            using (Transaction tran = new Transaction(Doc))
            {
                tran.Start("Create Dependent Views");

                // list for storing the created dependent views
                var dViewsList = new List<ElementId>();

                foreach (var e in primaryViews)
                {
                    var ogName = e.Name;
                    
                    var i = 0;
                    while (i < scopeBoxCol.Count)
                    {
                        // temporary renames the overall to include the selected scopebox
                        e.Name = ogName + scopeBoxCol[i].Name;

                        // create dependent views
                        var dependentView = e.Duplicate(ViewDuplicateOption.AsDependent);


                        dViewsList.Add(dependentView);

                        i++;
                    }

                    // reset overall view to original name
                    e.Name = ogName;
                }


                // This loop is needed to correctly rename the dependent views
                foreach (var e in dViewsList)
                {
                    var initialName = Doc.GetElement(e).Name;

                    // regex to modify ex: LEVEL x0001 - Sheet_ShopDrawing - 0ScopeBox_003 - Dependent 1
                    // returns LEVEL x0001 - Sheet_ShopDrawing - 03
                    // "$1" refers to the value of group1 case group (\d{2})
                    var result = Regex.Replace(initialName, @"0scopebox_0*(\d+) - dependent \d+", "$1", RegexOptions.IgnoreCase);

                    Doc.GetElement(e).Name = result;

                    //MessageBox.Show(result);
                }

                tran.Commit();
            }


        }


        #endregion



        #region Create View Range Templates

        private void VRangeButton_Click(object sender, RoutedEventArgs e)
        {

            UserSelected = new UserSelectedItems(ViewRangeSetup).SelectedLevels();

            if (UserSelected.Count == 0)
            {
                MessageBox.Show("Please Select A Level", "Selection Error", MessageBoxButton.OK);
            }

            //var simpF = new SimpleForm(test);
            //simpF.Show();





            CreateViewRangeTemplates();


            //this.Close();
        }

        private void CreateViewRangeTemplates()
        {
            try
            {
                NewViewRangeIds = new List<ElementId>();
                App.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnViewRangeTemplateChanged);

                using (Transaction t = new Transaction(Doc))
                {

                    t.Start("Create View Template");


                    var viewRangeCatId = new List<ElementId>
                    {
                        new ElementId(BuiltInParameter.PLAN_VIEW_RANGE)

                    };

                    var paramList = new EditViewTemplate(Doc, viewRangeCatId);
                    var turnOffList = paramList.TemplateCreationInfo();
                    var refView = paramList.View;
                    var vToViewPlan = refView as ViewPlan;
                    var vr = vToViewPlan.GetViewRange();



                    foreach (var e in UserSelected)
                    {
                        var levElement = Doc.GetElement(e.Id);
                        var levName = levElement.Name;

                        var templatesChecking = new collector(Doc, "vdc_viewrange").GetTemplatesList()
                            .Where(i => i.Name.Contains(levName)).FirstOrDefault();
                        //MessageBox.Show(templatesList.ToString());


                        if ( templatesChecking != null )
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

                        // .083 is approximately 1 inch
                        vr.SetOffset(PlanViewPlane.CutPlane, e.ViewRange - 0.083333333);
                        vr.SetOffset(PlanViewPlane.TopClipPlane, e.ViewRange - 0.083333333);

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

                App.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(OnViewRangeTemplateChanged);


                //var simp = new SimpleForm(NewViewRangeIds);
                //simp.ShowDialog();

            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error Detected", ex.ToString());

            }

        }

        private void OnViewRangeTemplateChanged(object sender, DocumentChangedEventArgs e)
        {
            
            NewViewRangeIds.AddRange(e.GetAddedElementIds());

        }


        private void RenameViewTemplates()
        {
            // this method is needed because when templates are created, a "copy" is added to the name
            try
            {
                // collector depends on w
                var vRangetemplatesList = new collector(Doc, "vdc_viewrange").GetTemplatesList()
                    .Where(e => e.Name.Contains("Copy"))
                    .ToList();
                var viewTemplatesList = new collector(Doc, "vdc_visibilityoverride").GetTemplatesList()
                    .Where(e => e.Name.Contains("Copy"))
                    .ToList();


                if (vRangetemplatesList.Count > 0)
                {
                    foreach (var e in vRangetemplatesList)
                    {
                        var originalName = e.Name;

                        var indexOfCopy = originalName.IndexOf("Copy");

                        e.Name = originalName.Remove(indexOfCopy - 1);
                        //MessageBox.Show(e.Name);

                    }

                }
                else if( viewTemplatesList.Count > 0)
                {

                    foreach (var e in viewTemplatesList)
                    {
                        var originalName = e.Name;

                        var indexOfCopy = originalName.IndexOf("Copy");

                        e.Name = originalName.Remove(indexOfCopy - 1);
                        //MessageBox.Show(e.Name);

                    }
                }
                //else
                //{
                //    MessageBox.Show("Template Error", "No View Range Templates Found");

                //}

            }
            catch 
            {
                MessageBox.Show("template already exists");
                

            }
            // get the viewrange templates list
            // select only the ones with "copy" in the name


        }


        #endregion


        #region Create View Templates

        private void VTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            CreateViewTemplates();
        }

        private void CreateViewTemplates()
        {

            NewViewTemplateIds = new List<ElementId>();
            // event that returns the newly created element ids
            App.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnViewTemplateChanged);

            // Transaction goes here




            using (Transaction t = new Transaction(Doc))
            {
                t.Start("Create View Templates");


                //var simpform = new SimpleForm(ViewTypes);
                //simpform.ShowDialog();

                var excludeIdsList = new List<ElementId>
                {
                    new ElementId(BuiltInParameter.VIS_GRAPHICS_MODEL),
                    new ElementId(BuiltInParameter.VIS_GRAPHICS_ANNOTATION),
                    new ElementId(BuiltInParameter.VIS_GRAPHICS_ANALYTICAL_MODEL),
                    new ElementId(BuiltInParameter.VIS_GRAPHICS_IMPORT),
                    //new ElementId(BuiltInParameter.VIS_GRAPHICS_FILTERS),
                    new ElementId(BuiltInParameter.VIS_GRAPHICS_WORKSETS),
                    new ElementId(BuiltInParameter.VIS_GRAPHICS_RVT_LINKS),
                    new ElementId(BuiltInParameter.VIEW_SCALE_PULLDOWN_IMPERIAL)

                };


                foreach (var e in ViewTypes)
                {
                    // duplicate template checking
                    var templatesChecking = new collector(Doc, "vdc_template").GetTemplatesList()
                        .Where(i => i.Name.Contains(e.ViewType)).FirstOrDefault();

                    if (templatesChecking != null)
                    {
                        //MessageBox.Show($"Duplicate Template detected: {e.ViewType}\nTemplate Creation Skipped");
                        continue;
                    }

                    // include the view range param if view type is sleeving.
                    // else remove needed otherwise it will be included for certain types (depends on index of sleeving)
                    var vRangeId = new ElementId(BuiltInParameter.PLAN_VIEW_RANGE);
                    if (e.ViewType == "Sleeving")
                    {
                        excludeIdsList.Add(vRangeId);
                    }
                    else
                    {
                        excludeIdsList.Remove(vRangeId);
                    }

                    // class that returns the list of ids to be excluded. also returns the reference view
                    var templateInfo = new EditViewTemplate(Doc, excludeIdsList);
                    var returnedExcludeIds = templateInfo.TemplateCreationInfo();
                    var refView = templateInfo.View;


                    var ogName = refView.Name;

                    if (e.ViewType == "Working" | e.ViewType == "RCP")
                    {
                        refView.Name = "VDC_VisibilityOverride_" + e.ViewType;

                    }
                    else
                    {
                        refView.Name = "VDC_VisibilityOverride_Sheet_" + e.ViewType;

                    }

                    // this sets the view range for sleeving templates as it is different from the rest
                    if (e.ViewType == "Sleeving")
                    {
                        var vToViewPlan = refView as ViewPlan;
                        var vr = vToViewPlan.GetViewRange();
                        vr.SetOffset(PlanViewPlane.TopClipPlane, 2.0);
                        vr.SetOffset(PlanViewPlane.CutPlane, 2.0);
                        vr.SetOffset(PlanViewPlane.BottomClipPlane, -1.0);
                        vr.SetOffset(PlanViewPlane.ViewDepthPlane, -1.0);
                        vToViewPlan.SetViewRange(vr);

                        refView.SetNonControlledTemplateParameterIds(returnedExcludeIds);
                        refView.CreateViewTemplate();

                        // resets the reference view's ranges
                        Doc.Regenerate();
                        vr.SetOffset(PlanViewPlane.BottomClipPlane, 0.0);
                        vr.SetOffset(PlanViewPlane.ViewDepthPlane, 0.0);
                        vToViewPlan.SetViewRange(vr);
                        refView.Name = ogName;
                    }
                    else
                    {
                        refView.SetNonControlledTemplateParameterIds(returnedExcludeIds);
                        refView.CreateViewTemplate();

                        Doc.Regenerate();
                        refView.Name = ogName;
                    }

                }

                RenameViewTemplates();

                t.Commit();
                App.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(OnViewTemplateChanged);

            }

        }
        private void OnViewTemplateChanged(object sender, DocumentChangedEventArgs e)
        {
            //MessageBox.Show(i.ToString());
            NewViewTemplateIds.AddRange(e.GetAddedElementIds());
        }
        #endregion

        #region Apply View Templates
        private void vTemplateApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedTemplateType = new UserSelectedItems(ApplyTemplateType).SelectedViewTypes();
            if (SelectedTemplateType.Count == 0)
            {
                MessageBox.Show("Please Select A View Template Type", "Selection Error", MessageBoxButton.OK);
                return;
            }

            ApplyViewTemplate();
        }

        private void ApplyViewTemplate()
        {
            var templateCol = new collector(Doc, "vdc_visibilityoverride").GetTemplatesList();

            //var getAllViews = new collector(Doc, "")

            try
            {
                //var simpForm = new SimpleForm(temp);
                //simpForm.ShowDialog();

                using (Transaction t = new Transaction(Doc))
                {
                    t.Start("Apply View Templates");
                    foreach (var e in SelectedTemplateType)
                    {
                        new ApplyViewTemplates(Doc, e.ViewType, templateCol).ApplyTemplates();

                    }

                    t.Commit();
                }
            }
            catch( Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            
        }

        private void vRangeApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedTemplateType = new UserSelectedItems(ApplyTemplateType).SelectedViewTypes();
            if (SelectedTemplateType.Count == 0)
            {
                MessageBox.Show("Please Select A View Template Type", "Selection Error", MessageBoxButton.OK);
                return;
            }

            ApplyViewRangeTemplates();
            
        }

        private void ApplyViewRangeTemplates()
        {
            try
            {
                using (Transaction t = new Transaction(Doc))
                {
                    t.Start("Apply View Range");

                    var templateCol = new collector(Doc, "vdc_viewrange").GetTemplatesList();

                    var viewsCol = new FilteredElementCollector(Doc)
                        .OfClass(typeof(ViewPlan))
                        .Where(e => e.LookupParameter("View SubCategory").HasValue)
                        .Where(e => e.LookupParameter("View Category").AsString() != "VDC")
                        .Cast<ViewPlan>()
                        .ToList();

                    //var path = "C:\\Users\\tdang\\Desktop\\test.txt";
                    //File.WriteAllLines(path, temp);

                    // null check 
                    if (templateCol.Count == 0)
                    {
                        MessageBox.Show("Please check if View Range Templates Exist");
                        return;
                    }

                    if (viewsCol.Count == 0)
                    {
                        MessageBox.Show("Please check if Views exist or View's Categories are Named Correctly.");
                        return;
                    }

                    foreach (var e in SelectedTemplateType)
                    {
                        var i = 0;
                        while (i < viewsCol.Count)
                        {
                            // matches the view sub cat with the name of the selected view type.
                            // also checks if the sheet names contain - 0 (child views are not need because they inherit)
                            if (viewsCol[i].LookupParameter("View SubCategory").AsString() == e.ViewType & viewsCol[i].Name.Contains("- 0"))
                            {

                                //MessageBox.Show("worked");
                                var vrTemplate = templateCol.Where(v => v.Name.Contains(viewsCol[i].GenLevel.Name)).First();
                                MessageBox.Show(viewsCol[i].Name + vrTemplate.Name);

                                viewsCol[i].ApplyViewTemplateParameters(vrTemplate);
                            }
                            else if (viewsCol[i].LookupParameter("View SubCategory").AsString() == "Coordination")
                            {
                                var vrTemplate = templateCol.Where(v => v.Name.Contains(viewsCol[i].GenLevel.Name)).First();
                                viewsCol[i].ApplyViewTemplateParameters(vrTemplate);

                            }
                            else if (e.ViewType == "RCP")
                            {
                                MessageBox.Show("RCPs Do Not Need View Ranges To Be Applied");
                                return;
                            }
                            //else
                            //{
                            //    MessageBox.Show($"View Subcategory did not matched the view type\n Please double check View Category Name or View Name\n" +
                            //        $"{viewsCol[i].Name}\n{e.ViewType}");
                            //    return;
                            //}

                            i++;
                        }
                    }
                    t.Commit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            
        }

        #endregion

        #region Create Sheets
        private void CreateSheetsButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedViewTypes = new UserSelectedItems(CreateSheetType).SelectedViewTypes();
            if (SelectedViewTypes.Count == 0)
            {
                MessageBox.Show("Please Select A Sheet Type", "Selection Error", MessageBoxButton.OK);
                return;
            }

            //return the user selected items from the UI
            UserSelected = new UserSelectedItems(SheetLevels).SelectedLevels();

            if (UserSelected.Count == 0)
            {
                MessageBox.Show("Please Select A Level", "Selection Error", MessageBoxButton.OK);
                return;
            }

            // check to see if a trade is selected
            if (FP.IsChecked == false & HD.IsChecked == false & HP.IsChecked == false & PL.IsChecked == false)
            {
                MessageBox.Show("Please Select A Trade", "Selection Error", MessageBoxButton.OK);
                return;

            }

            //MessageBox.Show(SelectedTemplateType.Count.ToString());

            CreateSheets();
        }

        private void CreateSheets()
        {
            try
            {

                var titleBlock48 = new FilteredElementCollector(Doc)
                    .OfClass(typeof(FamilySymbol))
                    .Where(tb => tb.Name.Contains("36x48"))
                    .Cast<FamilySymbol>()
                    .FirstOrDefault();

                string trade = null;

                if (FP.IsChecked == true)
                {
                    trade = "FP";
                }
                else if (HD.IsChecked == true)
                {
                    trade = "HD";
                }
                else if (HP.IsChecked == true)
                {
                    trade = "HP";
                }
                else if (PL.IsChecked == true)
                {
                    trade = "PL";
                }

                var selectedViews = new List<ViewPlan>();

                // returns the views based on user's selected levels and type
                foreach (var e in UserSelected)
                {
                    // using view's name strings to select the view types and levels
                    foreach (var vt in SelectedViewTypes)
                    {

                        var viewsCol = new FilteredElementCollector(Doc)
                            .OfClass(typeof(ViewPlan))
                            .Where(v => v.Name.ToLower().Contains(vt.ViewType.ToLower()))
                            //.Where(v => v.LookupParameter("View SubCategory"))
                            .Where(v => v.Name.ToLower().Contains(e.Level.ToLower()))
                            .Where(v => v.LookupParameter("View SubCategory").AsString().Contains(vt.ViewType))
                            .Cast<ViewPlan>()
                            .ToList();

                        selectedViews.AddRange(viewsCol);
                    }


                }

                //var simpleform = new SimpleForm(selectedViews.Select(v => v.Name));
                //simpleform.Show();

                if (selectedViews.Count == 0)
                {
                    MessageBox.Show("Sheet Views Were Not Detected.\nPlease Check Views Or Naming Convention");
                    return;
                }

                using (Transaction t = new Transaction(Doc))
                {
                    t.Start("Create Sheets");



                    foreach (var e in selectedViews)
                    {
                        // logic to rename sheet type and trade
                        var sheetRename = new SheetsName(e, trade).RenameSheets();

                        ViewSheet sheet = null;

                        sheet = ViewSheet.Create(Doc, titleBlock48.Id);

                        sheet.SheetNumber = sheetRename.sheetNumber;
                        sheet.Name = sheetRename.sheetName;
                        
                        // set the sheet category to the Level name (this case I used the Generated level)
                        // Sub cat set to the values of the Sheet View
                        sheet.LookupParameter("View Category").Set(e.GenLevel.Name);
                        sheet.LookupParameter("View SubCategory").Set(e.LookupParameter("View SubCategory").AsString());


                        Viewport.Create(Doc, sheet.Id, e.Id, XYZ.Zero);

                        //MessageBox.Show($"{sheetName.sheetName}\n{sheetName.sheetNumber}");


                    }




                    t.Commit();
                }


                var test = new List<string>();
                foreach (var e in selectedViews)
                {
                    test.Add(e.Name);
                }

                //var simpform = new SimpleForm(test);
                //simpform.Show();



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion


    }


}
