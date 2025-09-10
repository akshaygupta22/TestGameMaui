using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace TestGameMaui
{
    public class HealthToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int health = (int)value;
            int heartIndex = int.Parse(parameter.ToString()!);
            return health >= heartIndex ? 1.0 : 0.2;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class SelectedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SelectionState state)
            {
                return state switch
                {
                    SelectionState.Correct => Color.FromArgb("#C8E6C9"), // light green
                    SelectionState.Wrong => Color.FromArgb("#FFCDD2"), // light red
                    SelectionState.Selected => Color.FromArgb("#1976D2"),
                    _ => Color.FromArgb("#BBDEFB"),
                };
            }

            if (value is bool isSelected)
            {
                return isSelected ? Color.FromArgb("#1976D2") : Color.FromArgb("#BBDEFB");
            }

            return Color.FromArgb("#BBDEFB");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool enabled)
            {
                // Use blue when sound is on, gray when muted
                return enabled ? Color.FromArgb("#1976D2") : Color.FromArgb("#9E9E9E");
            }
            return Color.FromArgb("#9E9E9E");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
