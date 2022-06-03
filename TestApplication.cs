using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTestApp.SelectParameter;

namespace RevitTestApp
{
    public class TestApplication : IExternalApplication
    {
        private readonly ICollection<BuiltInCategory> targetCategories = new[]
        {
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_StructuralFraming,
            BuiltInCategory.OST_StructuralFoundation,
        };
        private CopyIdToParameterUpdater updater;
        private PushButton pbToggle;

        /// <summary>
        /// Provide access to singleton class instance.
        /// </summary>
        internal static TestApplication Instance { get; private set; }

        internal Settings Settings { get; private set; }

        internal ICollection<BuiltInCategory> TargetCategories => targetCategories;

        public Result OnStartup(UIControlledApplication application)
        {
            Instance = this;
            Settings = new Settings();

            // Register updater to react to element creations.
            updater = new CopyIdToParameterUpdater(application.ActiveAddInId, Settings);
            UpdaterRegistry.RegisterUpdater(updater);
            UpdaterRegistry.AddTrigger(
                updater.GetUpdaterId(), updater.GetUpdaterFilter(TargetCategories), Element.GetChangeTypeElementAddition());

            // Create own tab with tools.
            AddRibbonTab(application);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());

            return Result.Succeeded;
        }

        void AddRibbonTab(UIControlledApplication application)
        {
            // Create a custom ribbon tab.
            string tabName = "Awesome Addin";
            application.CreateRibbonTab(tabName);

            // Add a new ribbon panels.
            RibbonPanel ribbonPanelSettings = application.CreateRibbonPanel(tabName, "Fabulous Commands");

            // Get paths.
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            // create push button for Select Parameter
            var selectorButtonData = new PushButtonData(
                "selectorData",
                " Select " + Environment.NewLine +
                " Parameter ",
                thisAssemblyPath,
                typeof(SelectParameterECommand).FullName);

            var pbSelector = ribbonPanelSettings.AddItem(selectorButtonData) as PushButton;
            pbSelector.ToolTip = "Select parameter to copy element ID.";
            pbSelector.LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitTestApp;component/Resources/settings-32.png"));

            // create push button for Indicator
            var toggleButtonData = new PushButtonData(
                "toggleData",
                " ON/OFF ",
                thisAssemblyPath,
                typeof(ToggleECommand).FullName);

            pbToggle = ribbonPanelSettings.AddItem(toggleButtonData) as PushButton;
            pbToggle.ToolTip = "Enable/disable ID copying";
            pbToggle.LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitTestApp;component/Resources/settings-32.png"));
            UpdateToggleBotton();

        }

        internal void UpdateToggleBotton()
        {
            pbToggle.ItemText = $" It's {Environment.NewLine} {(Settings.IsActive ? "ON" : "OFF")} ";
        }
    }

    public class Settings
    {
        public bool IsActive { get; set; }

        public string ParameterName { get; set; } = "";
    }
}
