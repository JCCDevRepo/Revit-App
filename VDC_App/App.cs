using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using System.IO;
using System.Windows.Forms;

namespace VDC_App
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {   // get the path of where the assembly is saved (for the IMG saved location)
            String assembName = Assembly.GetExecutingAssembly().Location;
            String path = System.IO.Path.GetDirectoryName(assembName);
            // Create a Tab
            String tabName = "VDC";
            application.CreateRibbonTab(tabName);

            

            // Create a panel
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "VDC 1.1");
            // ver 1.1 (linkdwgBySelection) added automatic detection of cloud vs Q drive central and extracts project path automatically. Also added a checkbox for worksets
            


            // Create PushButton data

            //PushButtonData linkAllDwg = new PushButtonData("VDC_App","DWG Link", assembName, "VDC_App.LinkDwgAll");
            //PushButtonData linkSelectedDwg = new PushButtonData("test2", "Btn2", assembName, "VDC_App.LinkDwgAll");
            //PushButtonData create3D = new PushButtonData("test3", "Btn3", assembName, "VDC_App.LinkDwgAll");
            //PushButtonData viewportsUnpin = new PushButtonData("test4", "Btn4", assembName, "VDC_App.LinkDwgAll");
            PushButtonData linkAllDwg = new PushButtonData("DWG Link - All", "Link All DWG", assembName, "VDC_App.LinkDwgAll");
            PushButtonData linkSelectedDwg = new PushButtonData("DWG Link - Selected", "Select DWG", assembName, "VDC_App.LinkDwgBySelection");

            PushButtonData create3D = new PushButtonData("Create 3D views", "Create 3Ds", assembName, "VDC_App.Create3DViews");


            PushButtonData viewportsUnpin = new PushButtonData("Unpin Viewports", "Unpin Viewports", assembName, "VDC_App.ViewportsUnpin");
            
            PushButtonData viewportsPin = new PushButtonData("Pin Viewports", "Pin Viewports", assembName, "VDC_App.ViewportsPin");



            // hover discription for the buttons
            linkAllDwg.ToolTip = "Link DWG all files";
            linkSelectedDwg.ToolTip = "Link Selected DWGs";
            create3D.ToolTip = "Create 3D Views for Each Level In Project";
            viewportsUnpin.ToolTip = "Unpins all Viewports on Sheets";
            viewportsPin.ToolTip = "Pins all viewports on Sheets";



            // Apply a bit map image to the buttons
            //linkAllDwg.LargeImage = new BitmapImage(new Uri(path + @"\link_add.png"));
            //linkSelectedDwg.LargeImage = new BitmapImage(new Uri(path + @"\link_add.png"));
            linkAllDwg.LargeImage = new BitmapImage(new Uri(path + @"\DwgImage.png"));
            linkSelectedDwg.LargeImage = new BitmapImage(new Uri(path + @"\DwgImage.png"));
            create3D.LargeImage = new BitmapImage(new Uri(path + @"\Create3DImage.png"));
            viewportsUnpin.LargeImage = new BitmapImage(new Uri(path + @"\ViewportsUnpin.png"));
            viewportsPin.LargeImage = new BitmapImage(new Uri(path + @"\ViewportsPin.png"));


            //create3D.Image = new BitmapImage(new Uri(path + @"\page_add_16.png"));
            //viewportsUnpin.Image = new BitmapImage(new Uri(path + @"\page_add_16.png"));

            // add the buttons to the panel and format
            //panel.AddItem(linkAllDwg);
            //panel.AddItem(linkSelectedDwg);
            //panel.AddSeparator();
            //panel.AddStackedItems(create3D , viewportsUnpin);
            //panel.AddSeparator();

            // this is for the split buttons 
            SplitButtonData sb1 = new SplitButtonData("SplitButton1", "Split");
            SplitButton sb = panel.AddItem(sb1) as SplitButton;

            sb.AddPushButton(linkAllDwg);
            sb.AddPushButton(linkSelectedDwg);
            panel.AddSeparator();


            panel.AddItem(create3D);
            panel.AddSeparator();

            //panel.AddStackedItems(viewportsUnpin, viewportsPin);
            //panel.AddSeparator();

            var sb2 = new SplitButtonData("SplitbuttonViewport", "Split");
            var sbViewport = panel.AddItem(sb2) as SplitButton;
            sbViewport.AddPushButton(viewportsUnpin);
            sbViewport.AddPushButton(viewportsPin);
            panel.AddSeparator();



            return Result.Succeeded;
        }



        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Failed;
        }

    }
}
