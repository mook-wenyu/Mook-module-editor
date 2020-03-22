using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MookEditor.Utils
{
    public class OperationInterface
    {
        public static MainWindow mainWindow;
        public OperationInterface(MainWindow mainWindow)
        {
            OperationInterface.mainWindow = mainWindow;
        }

        public static void SetStatusText(string text, StatusText statusText = StatusText.Normal)
        {
            switch (statusText)
            {
                case StatusText.Warning:
                    mainWindow.statusText.Foreground = Brushes.Red;
                    break;
                default:
                    mainWindow.statusText.Foreground = (Brush)new BrushConverter().ConvertFrom("#FFDFDFDF");
                    break;
            }
            mainWindow.statusText.Text = text;
            
        }

        public enum StatusText
        {
            Normal,
            Warning
        }
    }
}
