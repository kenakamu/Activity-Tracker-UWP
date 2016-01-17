using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ActivityTrackerUWP.Converters
{
    /// <summary>
    /// Convert bool value to Visibility
    /// </summary>
    public class ObjectToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Default to Collapsed
            Visibility visibility = Visibility.Collapsed;
            // If value is bool type, then check it.
            if (value is bool && (bool)value)
            {
                visibility = Visibility.Visible;
            }
            // For other type, if value is not null, then return Visible
            if(value != null)
            {
                visibility = Visibility.Visible;
            }    
        

            bool invertResult;
            if (parameter != null && bool.TryParse(parameter.ToString(), out invertResult))
            {
                if (invertResult)
                {
                    if (visibility == Visibility.Visible)
                    {
                        visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        visibility = Visibility.Visible;
                    }
                }
            }

            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility && (Visibility)value == Visibility.Visible)
                return true;
            else
                return false;
        }
    }
}
