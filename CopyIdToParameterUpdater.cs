using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitTestApp
{
    public class CopyIdToParameterUpdater : IUpdater
    {
        private readonly UpdaterId updaterId;
        private readonly string updaterName = nameof(CopyIdToParameterUpdater);
        private readonly TestApplication addin;

        internal CopyIdToParameterUpdater(AddInId appId, TestApplication addin)
        {
            updaterId = new UpdaterId(appId, new Guid("F3DC7620-72DC-4D70-9D7E-63CCF3530308"));
            this.addin = addin ?? throw new ArgumentNullException(nameof(addin));
        }

        public string GetAdditionalInformation() => "Copies the element ID to a predefined parameter when the element is created.";

        public ChangePriority GetChangePriority() => ChangePriority.FloorsRoofsStructuralWalls;

        public UpdaterId GetUpdaterId() => updaterId;

        public string GetUpdaterName() => updaterName;

        public void Execute(UpdaterData data)
        {
            if (!addin.IsUpdaterActive)
                return;

            ICollection<ElementId> addedElementIds = data.GetAddedElementIds();
            if (!addedElementIds.Any())
                return;

            Document document = data.GetDocument();

            foreach (var elemId in addedElementIds)
            {
                Element element = document.GetElement(elemId)
                    ?? throw new InvalidOperationException($"Unexpectedly there is no element with this ID: {elemId.IntegerValue}");

                IList<Parameter> parameters = element.GetParameters(addin.TargetParameterName)
                    .Where(p => p.StorageType == StorageType.String)
                    .Where(p => !p.IsReadOnly)
                    .ToList();

                if (!parameters.Any())
                {
                    StopWithErrorMessage();
                    return;
                }

                Parameter parameter = parameters
                    .OrderByDescending(p => p.Definition is InternalDefinition def && def.BuiltInParameter != BuiltInParameter.INVALID)
                    .First();

                try
                {
                    if (parameter.Set($"{elemId.IntegerValue}"))
                        continue;

                    StopWithErrorMessage();
                    return;
                }
                catch
                {
                    StopWithErrorMessage();
                }
            }

            void StopWithErrorMessage()
            {
                TaskDialog.Show(nameof(CopyIdToParameterUpdater),
                    $"Failed to assign value to parameter: {addin.TargetParameterName}" + Environment.NewLine +
                    $"Updater will be stoped." + Environment.NewLine +
                    $"Check settings and try start again.");

                addin.IsUpdaterActive = false;
            }
        }

        internal ElementFilter GetUpdaterFilter()
        {
            ICollection<BuiltInCategory> targetCategories = new[]
            {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Floors,
                BuiltInCategory.OST_StructuralColumns,
                BuiltInCategory.OST_StructuralFraming,
                BuiltInCategory.OST_StructuralFoundation,
            };

            IList<ElementFilter> categoryFilters = targetCategories
                .Select(category => new ElementCategoryFilter(category))
                .Cast<ElementFilter>()
                .ToList();

            return new LogicalOrFilter(categoryFilters);
        }
    }
}
