using System.Windows.Controls;

namespace AutoserviceApp.Helpers
{
    public static class ScrollHelper
    {
        public static void HandleMouseWheel(/*this UserControl userControl, */object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                e.Handled = true;
            }
        }
    }
}
