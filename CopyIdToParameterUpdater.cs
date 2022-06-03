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
        private readonly Settings settings;

        public CopyIdToParameterUpdater(AddInId appId, Settings settings)
        {
            updaterId = new UpdaterId(appId, new Guid("F3DC7620-72DC-4D70-9D7E-63CCF3530308"));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string GetAdditionalInformation() => "Copies the element ID to a predefined parameter when the element is created.";

        public ChangePriority GetChangePriority() => ChangePriority.FloorsRoofsStructuralWalls;

        public UpdaterId GetUpdaterId() => updaterId;

        public string GetUpdaterName() => updaterName;

        public void Execute(UpdaterData data)
        {
            if (!settings.IsActive)
                return;

            ICollection<ElementId> addedElementIds = data.GetAddedElementIds();
            if (!addedElementIds.Any())
                return;

            Document document = data.GetDocument();

            foreach (var elemId in addedElementIds)
            {
                Element element = document.GetElement(elemId)
                    ?? throw new InvalidOperationException($"Unexpectedly there is no element with this ID: {elemId.IntegerValue}");

                IList<Parameter> parameters = element.GetParameters(settings.ParameterName);

                if (!parameters.Any())
                {
                    TaskDialog.Show(nameof(CopyIdToParameterUpdater),
                        $"Failed to assign value to parameter: {settings.ParameterName}." + Environment.NewLine +
                        $"Updater will be stoped." + Environment.NewLine +
                        $"Check settings and try start again.");

                    settings.IsActive = false;
                    return;
                }

                Parameter parameter = parameters
                    .OrderByDescending(p => !p.IsReadOnly)
                    .OrderByDescending(p => p.Definition is InternalDefinition def && def.BuiltInParameter != BuiltInParameter.INVALID)
                    .First();

                parameter.Set($"{elemId.IntegerValue}");
            }
        }

        internal ElementFilter GetUpdaterFilter(ICollection<BuiltInCategory> targetCategories)
        {
            IList<ElementFilter> categoryFilters = targetCategories
                .Select(category => new ElementCategoryFilter(category))
                .Cast<ElementFilter>()
                .ToList();

            return new LogicalOrFilter(categoryFilters);
        }
    }
}
