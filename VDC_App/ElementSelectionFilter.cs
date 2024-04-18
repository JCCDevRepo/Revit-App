using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDC_App
{
    public class ElementSelectionFilter
    {

        public abstract class BaseSelectionFilter : ISelectionFilter
        {
            protected readonly Func<Element, bool> ValidateElement;

            protected BaseSelectionFilter(Func<Element, bool> validateElement)
            {
                ValidateElement = validateElement;
            }
            public abstract bool AllowElement(Element elem);



            public abstract bool AllowReference(Reference reference, XYZ position);


        }


        public class LinkableSelectionFilter : BaseSelectionFilter
        {
            private readonly Autodesk.Revit.DB.Document _doc;
            public LinkableSelectionFilter(
                Autodesk.Revit.DB.Document doc,
                Func<Element, bool> validateELement)
                : base(validateELement)
            {
                _doc = doc;
            }

            public override bool AllowElement(Element elem)
            {
                return true;
            }

            public override bool AllowReference(Reference reference, XYZ postion)
            {
                if (_doc.GetElement(reference.ElementId) is RevitLinkInstance linkInstance)
                {
                    var element = linkInstance.GetLinkDocument().GetElement(reference.LinkedElementId);

                    return ValidateElement(element);
                }
                return ValidateElement(_doc.GetElement(reference.ElementId));
                //return _validateReference?.Invoke(reference) ?? true;
            }



        }

        public class HostSelectionFilter : BaseSelectionFilter
        {
            private readonly Func<Reference, bool> _validateReference;

            public HostSelectionFilter(Func<Element, bool> validateElement)
                : base(validateElement)
            {

            }

            public HostSelectionFilter(Func<Element, bool> validateElement, Func<Reference, bool> validateReference)
                : base(validateElement)
            {
                _validateReference = validateReference;
            }


            public override bool AllowElement(Element elem)
            {
                return ValidateElement(elem);
            }

            public override bool AllowReference(Reference reference, XYZ position)
            {
                return _validateReference?.Invoke(reference) ?? true;
            }
        }


    }
}