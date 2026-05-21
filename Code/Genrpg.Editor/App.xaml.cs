using Microsoft.UI.Xaml;
using OxDb.ServerCore.Setup;
using OxDb.SharedCore.Environments.Constants;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Genrpg.Editor
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private List<IInjectable> _initialServices = null;
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            _initialServices = DotNetServiceConfiguration.SetupServiceInstances(null, GameComponentNames.Editor);
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainMenuWindow(_initialServices);
            m_window.Activate();
        }

        private Window m_window;
    }
}



