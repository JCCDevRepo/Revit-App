using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using static System.Windows.Forms.LinkLabel;

namespace VDC_App
{
    /// <summary>
    /// Interaction logic for ListLevelsWindow.xaml
    /// </summary>
    public partial class LinkDwgBySelectionUI : Window
    {
        public UIApplication uiapp { get; }
        public Document doc { get; }
      

        public LinkDwgBySelectionUI(Document Doc)
        {
            //uiapp = UiApp;
            doc = Doc;

            InitializeComponent();

            
        }

        private void CbAll_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CbAf_Checked(object sender, RoutedEventArgs e)
        {
            if (CbAf.IsChecked == true)
            {
                CbAll.IsChecked = false;
            }
        }

        private void CbAr_Checked(object sender, RoutedEventArgs e)
        {
            if (CbAr.IsChecked == true)
            {
                CbAll.IsChecked = false;
            }
        }

        private void CbStc_Checked(object sender, RoutedEventArgs e)
        {
            if (CbStc.IsChecked == true)
            {
                CbAll.IsChecked = false;
            }
        }

        private void CbSte_Checked(object sender, RoutedEventArgs e)
        {
            if (CbSte.IsChecked == true)
            {
                CbAll.IsChecked = false;
            }
        }

        private void CbEl_Checked(object sender, RoutedEventArgs e)
        {
            if (CbEl.IsChecked == true)
            {
                CbAll.IsChecked = false;
            }
        }

        private void CbFp_Checked(object sender, RoutedEventArgs e)
        {
            if (CbFp.IsChecked == true)
            {
                CbAll.IsChecked = false;
            }
        }

        private void CbHd_Checked(object sender, RoutedEventArgs e)
        {
            if (CbHd.IsChecked == true)
            {
                CbAll.IsChecked = false;
            }
        }

        private void CbHp_Checked(object sender, RoutedEventArgs e)
        {
            if (CbHp.IsChecked == true)
            {
                CbAll.IsChecked = false;
            }
        }

        private void CbPl_Checked(object sender, RoutedEventArgs e)
        {
            if (CbPl.IsChecked == true)
            {
                CbAll.IsChecked = false;
            }
        }

        private void CbWorkset_Checked(object sender, RoutedEventArgs e)
        {

        }

        //public string Levels
        //{ get; private set; }

        //public List<string> LevelList { get; private set; }


        //public event EventHandler BtnClicked;




        //public List<string> ListLevels
        //{
        //    get { return listOfLevels; }


        //}
        public void BtnLink_Click(object sender, RoutedEventArgs e)
        {
            

            var tempSelectedList = new List<string>();




            //this flow control is for returning the items in its order
            // without this, the ListView returns the last selected item first idk
            foreach (var item in lvLevels.Items)
            {
                if (lvLevels.SelectedItems.Contains(item))
                {
                    //var SimpleLevels = new SimpleForm(item.ToString());
                    //SimpleLevels.Show();

                    tempSelectedList.Add(item.ToString());


                }
            }

            // same flow control above, this one returns the unselected elevations (for steel)
            var allLevelsInList = new List<string>();
            foreach (var item in lvLevels.Items)
            {
                if (lvLevels.Items.Contains(item))
                {
                    //var SimpleLevels = new SimpleForm(item.ToString());
                    //SimpleLevels.Show();

                    allLevelsInList.Add(item.ToString());


                }
            }


            // returns the selected levels
            var selectedLevels = new List<string>();
            foreach (var item in tempSelectedList) 
            {

                // string trying to manipulate: { Level = LEVEL 11, Elevation = 165 } and get the "Level 11"
                // remove from 0 index to index 10 of the above string
                var newStr = item.Remove(0, 10);
                
                //get index of the comma and remove everyting after it
                int index = newStr.IndexOf(",");
                var str2 = newStr.Remove(index);

                // this is for removing the "vel " to return "Lev11" and tolower the "EV" part of lEV
                var removeLevelStr = new StringBuilder(str2).Remove(3, 3).ToString();
                var strFormatLev = removeLevelStr.Substring(0, 1) + removeLevelStr.Substring(1, 2).ToLower() + removeLevelStr.Substring(3);


                selectedLevels.Add(strFormatLev);

                //TaskDialog.Show("dsa", str2);
            }




            var selectedElevations = new List<double>();
            foreach (var item in tempSelectedList)
            {
                // string trying to manipulate: { Level = LEVEL 11, Elevation = 165 }
                // get the index of the last equals sign plus 1 index to the right
                int indexOfEquals = item.LastIndexOf("=")+1;

                // remove everything before the = sign
                var tempStr = item.Remove(0,indexOfEquals);

                // get the index of the last white space to remove the space and }
                int indexOfBracket = tempStr.LastIndexOf(" ");

                var elevationStr = tempStr.Remove(indexOfBracket);

                //selectedElevations.Add(elevationStr);

                
                selectedElevations.Add(Convert.ToDouble(elevationStr));


                //TaskDialog.Show("dsa", str3);
            }



            // This returns all of the Elevations from the ListView (not just the selected ones)
            var populatedListOfLevels = new List<string>();
            foreach (var item in allLevelsInList)
            {
                // string trying to manipulate: { Level = LEVEL 11, Elevation = 165 } and get the "Level 11"
                // remove from 0 index to index 10 of the above string
                var newStr = item.Remove(0, 10);

                //get index of the comma and remove everyting after it
                int index = newStr.IndexOf(",");
                var str2 = newStr.Remove(index);

                // this is for removing the "vel " to return "Lev11" and tolower the "EV" part of lEV
                var removeLevelStr = new StringBuilder(str2).Remove(3, 3).ToString();
                var strFormatLev = removeLevelStr.Substring(0, 1) + removeLevelStr.Substring(1, 2).ToLower() + removeLevelStr.Substring(3);


                populatedListOfLevels.Add(strFormatLev);


                //TaskDialog.Show("test", strFormatLev);
            }


            //var SimpleLevels = new SimpleForm(populatedListOfElev);
            //SimpleLevels.Show();


            //OnClicked();

            //Get central model path
            //(ex: "\\cannistraro.local\storage\PROJECTS\##### - TEST PRJ\01 Field Management\04 Coordination\BIM\Revit\Drawing1_JCC.Cadsystems.rvt")
            var rvtCen = doc.GetWorksharingCentralModelPath();
            var rvtCenPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(rvtCen);

            //if project path found than adjust for Xrefs

            string rvtDir = rvtCenPath.Substring(0, 37);
            string prjCordDir = "\\01 Field Management\\04 Coordination\\";
            string prjFolder = null;
            string prjNumber = null;
            string coordFolder = null;


            if (rvtDir == "\\\\cannistraro.local\\storage\\PROJECTS\\" & !rvtCenPath.Contains("Autodesk Docs"))
            {
                //Get project Folder
                if (rvtCenPath.Contains(rvtDir) && rvtCenPath.Contains(prjCordDir))
                {
                    int Start, End;
                    Start = rvtCenPath.IndexOf(rvtDir, 0) + rvtDir.Length;
                    End = rvtCenPath.IndexOf(prjCordDir, Start);
                    prjFolder = rvtCenPath.Substring(Start, End - Start);
                    //TaskDialog.Show("Project Folder", prjFolder);
                    prjNumber = prjFolder.Substring(0, 6);
                    //TaskDialog.Show("Project Folder", prjNumber);
                    coordFolder = rvtDir + prjFolder + prjCordDir;
                    //TaskDialog.Show("asdas", coordFolder);

                }
                //else
                //{
                //    if (TboxInput.Text != "###### - PRJ Name")
                //    {
                //        prjFolder = TboxInput.Text;
                //    }
                //    else
                //        TaskDialog.Show("TROUBLESHOOT", "project folder not found or No Project Path Given");


                //}
            }
            else if (rvtCenPath.Contains("Autodesk Docs"))
            {

                var projectNumber = doc.ProjectInformation.Number;

                var di = new DirectoryInfo("\\\\cannistraro.local\\storage\\PROJECTS\\");
                var currentPrjName = di.GetDirectories().Where(p => p.Name.Contains(projectNumber)).ToList();


                foreach (var prj in currentPrjName)
                {

                    coordFolder = $"\\\\cannistraro.local\\storage\\PROJECTS\\{prj}{prjCordDir}";

                }

                prjNumber = projectNumber;

                //var SimpleLevels = new SimpleForm(test);
                //SimpleLevels.Show();
            }
            else
            {
                TaskDialog.Show("TROUBLESHOOT", "project directory not found or Please fill out Project #");
            }
            //else
            //{
            //    if (TboxInput.Text != "###### - PRJ Name")
            //    {
            //        coordFolder = "\\\\cannistraro.local\\storage\\PROJECTS\\" + TboxInput.Text + prjCordDir;
            //        prjNumber = TboxInput.Text.Remove(6);
            //        //TaskDialog.Show("dfsd", prjNumber);


            //    }
            //    else
            //    {
            //        TaskDialog.Show("TROUBLESHOOT", "project directory not found or Q Path Not Given");

            //    }
            //}



            //string coordFolder = rvtDir + prjFolder + prjCordDir;
            //TaskDialog.Show("asdas", prjNumber);



            //this is for checking for duplicate dwgs that already exists in the model
            //otherwise the program will link multiple instances each time it is ran
            var getImportCadInfo = new FilteredElementCollector(doc)
                .OfClass(typeof(ImportInstance))
                .Where(s => s.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME).AsString().Contains("-Lev"))
                .ToList();



            var listOfExistingCads = new List<string>();

            if(getImportCadInfo != null)
            {
                foreach (var item in getImportCadInfo)
                {
                    listOfExistingCads.Add(item.ToString());

                }
            }


            //var SimpleLevels = new SimpleForm(listOfExistingCads);
            //SimpleLevels.Show();

            //Create a default import option




            // get all worksets in project to apply to the linked cads
            var worksets = new FilteredWorksetCollector(doc)
                .OfKind(WorksetKind.UserWorkset);


            var wsAfId = 0;
            var wsArId = 0;
            var wsElId = 0;
            var wsFpId = 0;
            var wsHdId = 0;
            var wsHpId = 0;
            var wsPlId = 0;
            var wsStcId = 0;
            var wsSteId = 0;

            // get workset Ids for applying the change
            foreach (var w in worksets)
            {
                var cadType = w.Name;

                switch (cadType)
                {
                    case "MISC-Link_AF":
                        wsAfId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_AR":
                        wsArId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_EL":
                        wsElId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_FP":
                        wsFpId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_HD":
                        wsHdId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_HP":
                        wsHpId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_PL":
                        wsPlId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_STC":
                        wsStcId = w.Id.IntegerValue;
                        break;

                    case "MISC-Link_STE":
                        wsSteId = w.Id.IntegerValue;
                        break;
                }

                //TaskDialog.Show("sfs", cadType);
            }




            DWGImportOptions dwgOpt = new DWGImportOptions();
            dwgOpt.Unit = ImportUnit.Default;
            dwgOpt.Placement = ImportPlacement.Origin;
            dwgOpt.AutoCorrectAlmostVHLines = true;
            dwgOpt.ThisViewOnly = false;
            dwgOpt.VisibleLayersOnly = true;

            ElementId linkId = ElementId.InvalidElementId;
            using (Transaction t = new Transaction(doc, "link CADs"))
            {
                t.Start();


                //itterate through each level and elevate it according to the index of the lists
                for (int i = 0; i < selectedLevels.Count; i++)
                {
                    string dwgAF = coordFolder + "Architecturals\\" + prjNumber + "_AF-" + selectedLevels[i] + ".dwg";
                    string dwgAR = coordFolder + "Architecturals\\" + prjNumber + "_AR-" + selectedLevels[i] + ".dwg";
                    string dwgHD = coordFolder + "HVAC-DUCT\\" + prjNumber + "_HD-" + selectedLevels[i] + ".dwg";
                    string dwgHP = coordFolder + "HVAC-PIPE\\" + prjNumber + "_HP-" + selectedLevels[i] + ".dwg";
                    string dwgPL = coordFolder + "Plumbing\\" + prjNumber + "_PL-" + selectedLevels[i] + ".dwg";
                    string dwgFP = coordFolder + "Fire Protection\\" + prjNumber + "_FP-" + selectedLevels[i] + ".dwg";
                    string dwgEL = coordFolder + "Electrical\\" + prjNumber + "_EL-" + selectedLevels[i] + ".dwg";
                    //TaskDialog.Show("tdsfds", dwgAF);


                    //set import option to pull elevations from the sorted list of ELV
                    var zElevationTemp = new XYZ(0, 0, selectedElevations[i]);
                    dwgOpt.ReferencePoint = zElevationTemp;


                    // this is var for the duplicate checking
                    var dwgAFCheck = prjNumber + "_AF-" + selectedLevels[i];
                    var dwgARCheck = prjNumber + "_AR-" + selectedLevels[i];
                    var dwgHDCheck = prjNumber + "_HD-" + selectedLevels[i];
                    var dwgHPCheck = prjNumber + "_HP-" + selectedLevels[i];
                    var dwgPLCheck = prjNumber + "_PL-" + selectedLevels[i];
                    var dwgFPCheck = prjNumber + "_FP-" + selectedLevels[i];
                    var dwgELCheck = prjNumber + "_EL-" + selectedLevels[i];


                    //this is the logic for the check boxes
                    // if all CB is checked then link all the disciplines
                    if (CbAll.IsChecked == true)
                    {
                        //duplicate checking
                        if (listOfExistingCads.Contains(dwgAFCheck) | listOfExistingCads.Contains(dwgARCheck) | listOfExistingCads.Contains(dwgHDCheck) | listOfExistingCads.Contains(dwgHPCheck) | listOfExistingCads.Contains(dwgPLCheck) | listOfExistingCads.Contains(dwgFPCheck) | listOfExistingCads.Contains(dwgELCheck))
                        {
                            TaskDialog.Show("error", "Duplicate DWGs");
                            t.RollBack();
                        }

                        doc.Link(dwgAF, dwgOpt, doc.ActiveView, out linkId);
                        doc.Link(dwgAR, dwgOpt, doc.ActiveView, out linkId);
                        doc.Link(dwgHD, dwgOpt, doc.ActiveView, out linkId);
                        doc.Link(dwgHP, dwgOpt, doc.ActiveView, out linkId);
                        doc.Link(dwgPL, dwgOpt, doc.ActiveView, out linkId);
                        doc.Link(dwgFP, dwgOpt, doc.ActiveView, out linkId);
                        doc.Link(dwgEL, dwgOpt, doc.ActiveView, out linkId);

                    }
                    // else link in the checked disciplines only
                    else
                    {
                        if(CbAf.IsChecked == true | CbAr.IsChecked == true | CbEl.IsChecked == true | CbFp.IsChecked == true | CbHd.IsChecked == true| CbHp.IsChecked == true | CbPl.IsChecked == true)
                        {

                            if (CbAf.IsChecked == true)
                            {
                                if (listOfExistingCads.Contains(dwgAFCheck))
                                {
                                    TaskDialog.Show("error", "Duplicate DWGs");
                                    t.RollBack();
                                }

                                doc.Link(dwgAF, dwgOpt, doc.ActiveView, out linkId);

                            }

                            if (CbAr.IsChecked == true)
                            {
                                if (listOfExistingCads.Contains(dwgARCheck))
                                {
                                    TaskDialog.Show("error", "Duplicate DWGs");
                                    t.RollBack();
                                }

                                doc.Link(dwgAR, dwgOpt, doc.ActiveView, out linkId);

                            }

                            if (CbEl.IsChecked == true)
                            {
                                if (listOfExistingCads.Contains(dwgELCheck))
                                {
                                    TaskDialog.Show("error", "Duplicate DWGs");
                                    t.RollBack();
                                }
                                doc.Link(dwgEL, dwgOpt, doc.ActiveView, out linkId);
                            }

                            if (CbFp.IsChecked == true)
                            {
                                if (listOfExistingCads.Contains(dwgFPCheck))
                                {
                                    TaskDialog.Show("error", "Duplicate DWGs");
                                    t.RollBack();
                                }
                                doc.Link(dwgFP, dwgOpt, doc.ActiveView, out linkId);

                            }

                            if (CbHd.IsChecked == true)
                            {
                                if (listOfExistingCads.Contains(dwgHDCheck))
                                {
                                    TaskDialog.Show("error", "Duplicate DWGs");
                                    t.RollBack();
                                }
                                doc.Link(dwgHD, dwgOpt, doc.ActiveView, out linkId);

                            }

                            if (CbHp.IsChecked == true)
                            {
                                if (listOfExistingCads.Contains(dwgHPCheck))
                                {
                                    TaskDialog.Show("error", "Duplicate DWGs");
                                    t.RollBack();
                                }
                                doc.Link(dwgHP, dwgOpt, doc.ActiveView, out linkId);

                            }

                            if (CbPl.IsChecked == true)
                            {
                                if (listOfExistingCads.Contains(dwgPLCheck))
                                {
                                    TaskDialog.Show("error", "Duplicate DWGs");
                                    t.RollBack();
                                }
                                doc.Link(dwgPL, dwgOpt, doc.ActiveView, out linkId);

                            }
                        }
                    }



                }

                for (int i = 0; i < selectedLevels.Count; i++)
                {

                    // linq to find the index of the selected Levels
                    // compare it to the full list of levels for determine the index of the level above
                    // offset it by +1 because we want the index of the level above
                    var indexOfSelectedLevel = populatedListOfLevels.FindIndex(a => a.Contains(selectedLevels[i])) + 1;


                    if (indexOfSelectedLevel >= populatedListOfLevels.Count)
                    {
                        break;

                    }

                    else
                    {

                        string dwgSTC = coordFolder + "Structurals\\" + prjNumber + "_STC-" + populatedListOfLevels[indexOfSelectedLevel] + ".dwg";
                        string dwgSTE = coordFolder + "Structurals\\" + prjNumber + "_STE-" + populatedListOfLevels[indexOfSelectedLevel] + ".dwg";
                        //TaskDialog.Show("sdfds", dwgSTE);

                        //var SimpleLevels = new SimpleForm(dwgSTE);
                        //SimpleLevels.Show();


                        var zElevationTemp = new XYZ(0, 0, Convert.ToDouble(selectedElevations[i]));
                        dwgOpt.ReferencePoint = zElevationTemp;

                        //TaskDialog.Show("sdfds", indexOfSelectedElv.ToString());

                        //duplicate ST checking
                        var dwgSTCCheck = prjNumber + "_STC-" + populatedListOfLevels[indexOfSelectedLevel];
                        var dwgSTECheck = prjNumber + "_STE-" + populatedListOfLevels[indexOfSelectedLevel];

                        if (CbAll.IsChecked == true)
                        {
                            //duplicate checking

                            if (listOfExistingCads.Contains(dwgSTCCheck) | listOfExistingCads.Contains(dwgSTECheck))
                            {
                                TaskDialog.Show("error", "Duplicate STC/STE DWGs");
                                t.RollBack();
                            }

                            doc.Link(dwgSTC, dwgOpt, doc.ActiveView, out linkId);
                            doc.Link(dwgSTE, dwgOpt, doc.ActiveView, out linkId);


                        }
                        else
                        {
                            if (CbStc.IsChecked == true | CbSte.IsChecked == true) 
                            {
                                if (listOfExistingCads.Contains(dwgSTCCheck) | listOfExistingCads.Contains(dwgSTECheck))
                                {
                                    TaskDialog.Show("error", "Duplicate STC/STE DWGs");
                                    t.RollBack();
                                }

                                if (CbStc.IsChecked == true)
                                {
                                    doc.Link(dwgSTC, dwgOpt, doc.ActiveView, out linkId);

                                }

                                if (CbSte.IsChecked == true)
                                {
                                    doc.Link(dwgSTE, dwgOpt, doc.ActiveView, out linkId);

                                }

                            }
                        }



                    }


                }

                if (CbWorkset.IsChecked == true)
                {
                    #region get each type of linked cads

                    var getImportAF = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME).AsString().Contains("AF-Lev"))
                        .ToList();
                    //var SimpleLevels = new SimpleForm(getImportAF);
                    //SimpleLevels.Show();

                    var getImportAR = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME).AsString().Contains("AR-Lev"))
                        .ToList();

                    var getImportEL = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME).AsString().Contains("EL-Lev"))
                        .ToList();

                    var getImportFP = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME).AsString().Contains("FP-Lev"))
                        .ToList();

                    var getImportHD = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME).AsString().Contains("HD-Lev"))
                        .ToList();

                    var getImportHP = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME).AsString().Contains("HP-Lev"))
                        .ToList();

                    var getImportPL = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME).AsString().Contains("PL-Lev"))
                        .ToList();

                    var getImportSTC = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME).AsString().Contains("STC-Lev"))
                        .ToList();

                    var getImportSTE = new FilteredElementCollector(doc)
                        .OfClass(typeof(ImportInstance))
                        .Where(s => s.get_Parameter(BuiltInParameter.IMPORT_SYMBOL_NAME).AsString().Contains("STE-Lev"))
                        .ToList();
                    #endregion

                    #region set the CAD elements to the respective workset via Ids

                    foreach (var i in getImportAF)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsAfId);

                    }

                    foreach (var i in getImportAR)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsArId);

                    }

                    foreach (var i in getImportEL)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsElId);

                    }

                    foreach (var i in getImportFP)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsFpId);

                    }

                    foreach (var i in getImportHD)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsHdId);

                    }

                    foreach (var i in getImportHP)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsHpId);

                    }

                    foreach (var i in getImportPL)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsPlId);

                    }

                    foreach (var i in getImportSTC)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsStcId);

                    }

                    foreach (var i in getImportSTE)
                    {
                        i.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsSteId);

                    }
                    #endregion
                }



                t.Commit();
            }


        }






        //public void OnClicked()
        //{

        //    BtnClicked?.Invoke(this, EventArgs.Empty);

        //}

        //var indexOfSelectedElv = populatedListOfElev.FindIndex(a => a.Contains(selectedElevations[i].ToString())) - 1;
        //        // need this flow control for the lowest level.
        //// lowest level does not have a framing level below so the Elevation doesnt exist.
        //// if the index is less than 0 break
        //if (indexOfSelectedElv < 0)
        //{
        //    //string lowestLev = "this is the lowest level";
        //    //TaskDialog.Show("sdfds", lowestLev);

        //    break;

        //}

        //else
        //{
        //    var zElevationTemp = new XYZ(0, 0, Convert.ToDouble(populatedListOfElev[indexOfSelectedElv]));
        //    dwgOpt.ReferencePoint = zElevationTemp;

        //    //TaskDialog.Show("sdfds", indexOfSelectedElv.ToString());

        //    doc.Link(dwgSTC, dwgOpt, doc.ActiveView, out linkId);
        //    doc.Link(dwgSTE, dwgOpt, doc.ActiveView, out linkId);
        //}



    }
}
