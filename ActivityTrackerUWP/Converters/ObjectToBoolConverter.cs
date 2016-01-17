using System;
using Windows.UI.Xaml.Data;

namespace ActivityTrackerUWP.Converters
{
    /// <summary>
    /// Convert object value to bool.
    /// </summary>
    public class ObjectToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // If no value set, then returns false
            if (string.IsNullOrEmpty(value.ToString()))
                return false;
            else
                return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
