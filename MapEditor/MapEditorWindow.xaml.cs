using System;
using System.Windows;
using log4net;

namespace MapEditor
{
    /// <summary>
    /// Interaction logic for MapEditorWindow.xaml
    /// </summary>
    public partial class MapEditorWindow : Window
    {
        private ILog logger = LogManager.GetLogger("application-logger");

        public MapEditorWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}