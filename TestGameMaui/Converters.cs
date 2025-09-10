using System;
using System.Globalization;
using Microsoft.Maui.Controls;

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
            // support both bool and SelectionState
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
}
