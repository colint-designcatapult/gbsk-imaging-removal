using Heracles.Application.Enums;
using Heracles.Robot.Models;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Heracles.Robot.Converters
{
    public class RobotModelStateToBrushConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RobotModelState? state = value as RobotModelState? ?? throw new ArgumentNullException(nameof(value), "RobotModelState: value cannot be null.");

            var colorResources = new ResourceDictionary
            {
                Source = new Uri("/Xcc.Application;component/UI/Resources/ColorResources.xaml", UriKind.RelativeOrAbsolute)
            };

            return state.Value switch
            {
                RobotModelState.ImagingHeadGrabInProgress or RobotModelState.TreatmentHeadGrabInProgress or RobotModelState.TreatmentHeadQcInProgress 
                => colorResources["StopButtonBackgroundBrush"],
                _ 
                => colorResources["ButtonBackgroundColor"],
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

}
