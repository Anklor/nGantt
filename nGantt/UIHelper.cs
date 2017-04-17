using System.Windows;
using System.Windows.Media;

namespace nGantt
{
    public static class UIHelper
    {
        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;

            T parent = parentObject as T;

            return parent != null
                ? parent
                : FindVisualParent<T>(parentObject);
        } 
    }
}
