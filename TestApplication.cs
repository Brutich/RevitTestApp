using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTestApp.SelectParameter;
using RevitTestApp.Services;
using Settings = RevitTestApp.Services.Settings;

namespace RevitTestApp
{
    public class TestApplication : IExternalApplication
    {
        private ISettingsStorageService settingsStorageService;
        private CopyIdToParameterUpdater updater;
        private bool isUpdaterActive;
        private PushButton pbToggle;

        /// <summary>
        /// Provide access to singleton class instance.
        /// </summary>
        internal static TestApplication Instance { get; private set; }

        internal string TargetParameterName { get; set; }

        internal bool IsUpdaterActive
        {
            get => isUpdaterActive;
            set { isUpdaterActive = value; UpdateToggleBotton(); }
        }

        public Result OnStartup(UIControlledApplication application)
        {
            // Save the instance of this addin so you can access it from external commands.
            Instance = this;

            // Read the settings.
            settingsStorageService = new InFileStorageService();
            Settings settings = settingsStorageService.Read();
            TargetParameterName = settings.ParameterName;
            IsUpdaterActive = settings.IsActive;

            // Register updater to react to element creations.
            updater = new CopyIdToParameterUpdater(application.ActiveAddInId, Instance);
            UpdaterRegistry.RegisterUpdater(updater);
            UpdaterRegistry.AddTrigger(
                updater.GetUpdaterId(), updater.GetUpdaterFilter(), Element.GetChangeTypeElementAddition());

            // Create own tab with tools.
            AddRibbonTab(application);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // Unregister updater.
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());

            // Store the settings.
            var settings = new Settings { ParameterName = TargetParameterName, IsActive = IsUpdaterActive };
            settingsStorageService.Store(settings);

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
            pbToggle.LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitTestApp;component/Resources/on-32.png"));
            UpdateToggleBotton();

        }

        internal void UpdateToggleBotton()
        {
            if (!(pbToggle is PushButton toggle))
                return;

            toggle.ItemText = $" It's {Environment.NewLine} {(IsUpdaterActive ? "ON" : "OFF")} ";
            pbToggle.LargeImage = new BitmapImage(new Uri($"pack://application:,,,/RevitTestApp;component/Resources/{(IsUpdaterActive ? "on" : "off")}-32.png"));
        }
    }
}
