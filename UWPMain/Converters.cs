using System;
using Windows.UI.Xaml.Data;

namespace UWPMain.Converters
{
    public class ValueToStringConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString();
            if (value.ToString() == "0")
                return "0";
            if (value.ToString().Length == 1)
                return value?.ToString() + ".0";
            if (value.ToString().Length > 3)
                return value?.ToString().Remove(3);
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToInt32(value as string);
        }
    }
}