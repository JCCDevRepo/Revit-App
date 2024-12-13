using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Application = Autodesk.Revit.ApplicationServices.Application;


namespace VDC_App
{
    public class ProjectSetupEvents
    {
        //private Application App;

        //public ProjectSetupEvents(Application app)
        //{
        //    App = app;
        //}

        //public void DocumentSubscribe()
        //{
        //    App.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
        //}

        //public void DocumentUnsubscribe()
        //{
        //    App.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
        //}

        //private void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        //{
        //    var getTransName = e.GetTransactionNames().FirstOrDefault();
        //    MessageBox.Show("test");
        //    if (getTransName != null && getTransName.Contains("Apply View"))
        //    {
        //        //MessageBox.Show("View Templates Applied", "View Templates Applied", MessageBoxButton.OK, MessageBoxImage.Information);

        //    }
        //}

    }
}
