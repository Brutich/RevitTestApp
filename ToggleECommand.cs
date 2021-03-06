using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitTestApp
{
    [Transaction(TransactionMode.ReadOnly)]
    internal class ToggleECommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TestApplication addinInstance = TestApplication.Instance;
            addinInstance.IsUpdaterActive = !addinInstance.IsUpdaterActive;

            return Result.Succeeded;
        }
    }
}
