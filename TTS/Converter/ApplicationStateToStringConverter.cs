using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TTS.Converter
{
    public class ApplicationStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (ApplicationState)value;
            switch (state)
            {
                case ApplicationState.Read:
                    return "Pause";
                case ApplicationState.Pause:
                    return "Resume";
                case ApplicationState.Idle:
                    return "Play";
                default:
                    return "Play";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
