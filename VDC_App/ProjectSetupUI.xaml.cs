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
using System.IO;

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
        private List<ViewTypes> SelectedViewTypes;
        private List<ViewTypes> SelectedTemplateType;

        private List<ElementId> NewViewIds;
        private List<ElementId> CreatedViewIds;
        private List<ElementId> NewViewRangeIds;
        private List<ElementId> NewViewTemplateIds;

        private List<ViewTypes> ViewTypes;


        public ProjectSetupUI(Document doc, Application app)
        {
            Doc = doc;
            App = app;
            InitializeComponent();
            PopulateLevels();
            PopulateViewTypes();


        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
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
            }



        }

        private void PopulateViewTypes()
        {
            // Method to display view type objects
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
                //new ViewTypes { ViewType = "Custom"},

            };

            foreach (var v in viewsList)
            {
                //ViewTypeSetup.Items.Add(new { ViewTypes = v});
                ViewTypeSetup.Items.Add(v);
                ApplyTemplateType.Items.Add(v);
            }

            ViewTypes = viewsList;
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

                CreateViews();
                RenameViews();
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

            var viewFamilyRcpCol = new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewFamilyType))
                .Where(f => f.Name.Equals("Ceiling Plan")).FirstOrDefault();

            if (viewFamilyCol == null | viewFamilyRcpCol == null)
            {
                MessageBox.Show("View Family Type Is Missing For AF or RCP");
                return;
            }

            
            SelectedViewTypes = new UserSelectedItems(ViewTypeSetup).SelectedViewTypes();
            if (SelectedViewTypes.Count == 0)
            {
                MessageBox.Show("Please Select A View Type", "Selection Error", MessageBoxButton.OK);
                return;
            }

            //var viewDuplicateCheck = new FilteredElementCollector(Doc)
            //    .Where(e => e.Name.Contains("_Sheet"))
            //    .Where(e => e.LookupParameter("View Category").Equals("ANNOTATION"))
            //    .ToList();

            //foreach (var e in viewDuplicateCheck)
            //{
            //    if (e.Name.Contains())
            //}



            // checks for the view categories 
            // Need _View Template View as a reference in central
            var categoryCheck = new collector(Doc, "_view templates").GetViewsList();
            if (categoryCheck.Count == 0)
            {
                MessageBox.Show("Missing _View Templates View");
                MessageBox.Show(categoryCheck.Count.ToString());

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


            NewViewIds = new List<ElementId>();
            // event that returns the newly created element ids
            App.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);

            using (Transaction t = new Transaction(Doc))
            {

                t.Start("Create Views");
                //var simpform = new SimpleForm(viewTypeCol);
                //simpform.Show();
                foreach (var l in UserSelected)
                {
                    // if the user selects an RCP (different plan type), use the rcp view family
                    var i = 0;
                    while (i < SelectedViewTypes.Count)
                    {
                        if (SelectedViewTypes[i].ViewType.Contains("RCP"))
                        {
                            ViewPlan.Create(Doc, viewFamilyRcpCol.Id, l.Id);

                        }
                        else
                        {
                            ViewPlan.Create(Doc, viewFamilyCol.Id, l.Id);

                        }

                        i++;
                    }

                }

                t.Commit();
            }

            App.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);



            //var simpF = new SimpleForm(NewViewIds);
            //simpF.Show();
        }

        private void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            //MessageBox.Show(i.ToString());
            NewViewIds.AddRange(e.GetAddedElementIds());
        }


        private void RenameViews()
        {
            //CreatedViewIds = new List<ElementId>();
            //var viewCatId = new ElementId(BuiltInCategory.OST_Views);

            if (NewViewIds == null)
            {
                MessageBox.Show("The Ids of The New Views Were not Found", "Returned Ids Error");
                return;
            }

            var viewplanElems = new List<View>();

            var getNewView = new ViewsFromIds(Doc, NewViewIds);

            viewplanElems = getNewView.GetViews();

            CreatedViewIds = getNewView.GetViews()
                .Select(e => e.Id).ToList();

            // this reorder is needed as the iteration is dependant on consecutive items
            var sortedVpElems = viewplanElems.OrderBy(e => e.Name).ToList();

            using (Transaction tran = new Transaction(Doc))
            {
                tran.Start("Renamed Views");

                for (var i = 0; i < sortedVpElems.Count; i++)
                {
                    var elem = sortedVpElems[i];
                    // Genlevel is the generated level that the view was created from. used this to rename
                    var levName = elem.GenLevel.Name;

                    // using the modulo operator to reset collection count once the max item as been reached.
                    // This must be paired with the ordered list element names as it is processed consecutively.
                    var index2 = i % SelectedViewTypes.Count;

                    var viewType = SelectedViewTypes[index2];

                    // control to isolate non sheet views
                    if (viewType.ViewType.Equals("Working"))
                    {
                        //elem.Name = levName + " - " + viewType.ViewType;
                        elem.Name = levName;

                    }
                    else if (viewType.ViewType.Equals("RCP"))
                    {
                        elem.Name = levName + "_" + "RCP";
                    }
                    else
                    {
                        elem.Name = levName + " - " + "Sheet_" + viewType.ViewType + " - 0";
                    }



                }
                tran.Commit();
            }




            //MessageBox.Show(viewTypes.ViewType)
            //var simpF = new SimpleForm(viewTypes);
            //simpF.Show();

        }

        private void CreateDependentViews()
        {
            // get all vdc scope boxes in project
            var scopeBoxCol = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_VolumeOfInterest)
                .Where(e => e.Name.Contains("ScopeBox_"))
                .ToList();

            // null checking
            if (scopeBoxCol.Count == 0)
            {
                MessageBox.Show("No Scopeboxes Found\nPlease add Scopeboxes or\nPlease Follow Naming Convention: ScopeBox_##");
                return;
            }

            // this returns all the newly created views with "sheet" in the name
            var sheetViews = new List<View>();
            var workingView = new List<View>();
            foreach (var e in CreatedViewIds)
            {
                var temp = Doc.GetElement(e);
                if (temp.Name.ToLower().Contains("sheet_"))
                {
                    sheetViews.Add(temp as View);
                }
                else
                {
                    workingView.Add(temp as View);
                }

            }



            using (Transaction tran = new Transaction(Doc))
            {
                tran.Start("Create Dependent Views - Apply Cat");
                foreach (var e in sheetViews)
                {
                    // add the Annotation value to v cat parameter
                    e.LookupParameter("View Category").Set("ANNOTATION");
                    
                    // string manipulation to return the annotation view type
                    // used this name as the subcategory
                    var vName = e.Name;
                    var sbView = new StringBuilder(vName);
                    var indexEnd = vName.LastIndexOf("_") + 1;
                    var sheetType = sbView.Remove(0, indexEnd).Replace("- 0", "").ToString();
                    //var test = sheetType.Replace("- 0", "");
                    // set sub cat to name of view type
                    e.LookupParameter("View SubCategory").Set(sheetType);

                }

                // add categories to working views
                foreach (var e in  workingView)
                {
                    e.LookupParameter("View Category").Set("_WORKING");

                    if (e.Name.Contains("RCP"))
                    {
                        e.LookupParameter("View SubCategory").Set("RCP");

                    }
                    else
                    {
                        e.LookupParameter("View SubCategory").Set("Coordination");

                    }


                }

                foreach (var e in sheetViews)
                {
                    var i = 0;
                    while (i < scopeBoxCol.Count)
                    {
                        e.Duplicate(ViewDuplicateOption.AsDependent);

                        i++;
                    }
                }

                var dependView = new collector(Doc, "- dependent").GetViewsList();

                foreach (var e in dependView)
                {
                    var sbView = new StringBuilder(e.Name);
                    sbView.Replace("- 0 - Dependent", "-");
                    //MessageBox.Show(sbView.ToString());

                    e.Name = sbView.ToString();

                }

                //var simpF = new SimpleForm(dependView);
                //simpF.Show();



                tran.Commit();
            }



            // View Category Parameters checking

            //foreach (var s in scopeBoxCol)
            //{
            //    MessageBox.Show(s.Name);
            //}
            //var simpF = new SimpleForm(sheetIds);
            //simpF.Show();


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
            var templateCol = new collector(Doc, "vdc_visibilityoverride").GetTemplatesList().ToList();

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

                        switch (e.ViewType)
                        {
                            case "Working":
                                {
                                    //var viewsCol = new FilteredElementCollector(Doc)
                                    //    .OfClass(typeof(ViewPlan))
                                    //    //.Cast<View>()
                                    //    .Where(i => i.LookupParameter("View Category").AsString() == "_WORKING")
                                    //    .Where(i => i.LookupParameter("View SubCategory").AsString() == "Coordination")
                                    //    .ToList();

                                    //var getViewByCat = new collector(Doc, "Coordination").GetViewType().ToList();

                                    //var workingTemplate = templateCol.Where(i => i.Name.Contains(e.ViewType)).First();

                                    //foreach (var v in getViewByCat)
                                    //{
                                    //    //MessageBox.Show(v.Name);

                                    //    v.ApplyViewTemplateParameters(workingTemplate);
                                    //}

                                    var tset = new ApplyViewTemplates(Doc, e.ViewType, templateCol);
                                    tset.ApplyTemplates();

                                    break;

                                }
                            case "RCP":
                                {
                                    //var workingCol = new collector(Doc,
                                    MessageBox.Show(e.ViewType);
                                    break;

                                }
                            case "ShopDrawing":
                                {
                                    //var workingCol = new collector(Doc,
                                    MessageBox.Show(e.ViewType);
                                    break;

                                }
                            case "Sleeving":
                                {
                                    //var workingCol = new collector(Doc,
                                    MessageBox.Show(e.ViewType);
                                    break;

                                }
                            case "AccessPanels":
                                {
                                    //var workingCol = new collector(Doc,
                                    MessageBox.Show(e.ViewType);
                                    break;

                                }
                            case "BeamPens":
                                {
                                    //var workingCol = new collector(Doc,
                                    MessageBox.Show(e.ViewType);
                                    break;

                                }
                            case "Hangers":
                                {
                                    //var workingCol = new collector(Doc,
                                    MessageBox.Show(e.ViewType);
                                    break;

                                }
                            case "PadDrawings":
                                {
                                    //var workingCol = new collector(Doc,
                                    MessageBox.Show(e.ViewType);
                                    break;

                                }
                            case "PointLoads":
                                {
                                    //var workingCol = new collector(Doc,
                                    MessageBox.Show(e.ViewType);
                                    break;

                                }
                            case "Risers":
                                {
                                    //var workingCol = new collector(Doc,
                                    MessageBox.Show(e.ViewType);
                                    break;

                                }
                            case "Sketches":
                                {
                                    //var workingCol = new collector(Doc,
                                    MessageBox.Show(e.ViewType);
                                    break;

                                }
                            case "WallPens":
                                {
                                    //var workingCol = new collector(Doc,
                                    MessageBox.Show(e.ViewType);
                                    break;

                                }
                        }
                    }

                    t.Commit();
                }
            }
            catch( Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            
        }
    }


}
