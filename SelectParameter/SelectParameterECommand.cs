using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitTestApp.SelectParameter
{
    [Transaction(TransactionMode.Manual)]
    internal class SelectParameterECommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            ExternalCommandData cd = commandData ?? throw new ArgumentNullException(nameof(commandData));
            Document document = cd.Application.ActiveUIDocument.Document;
            if (document.IsFamilyDocument)
            {
                message = "The document must be a project document.";
                return Result.Failed;
            }

            TestApplication addinInstance = TestApplication.Instance;

            try
            {
                // Show dialog window allowing the user to choose a parameter name.
                var viewModel = new SelectParameterVM(addinInstance.TargetParameterName);
                var dialogWindow = new SelectParameterView(viewModel);
                if (dialogWindow.ShowDialog() != true)
                {
                    return Result.Cancelled;
                }

                // Update parameter name.
                addinInstance.TargetParameterName = viewModel.ParameterName;
            }
            catch (Exception ex)
            {
                message = $"{ex.Message}";
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}
