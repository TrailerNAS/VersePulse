using System.Windows;
using System.Windows.Controls;

namespace VersePulse.App.Controls
{
    public partial class MetricCard : UserControl
    {
        public MetricCard()
        {
            InitializeComponent();
        }

        //====================================================
        // Title
        //====================================================

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(MetricCard),
                new PropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        //====================================================
        // Value
        //====================================================

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(string),
                typeof(MetricCard),
                new PropertyMetadata("--"));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        //====================================================
        // Subtitle
        //====================================================

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(
                nameof(Subtitle),
                typeof(string),
                typeof(MetricCard),
                new PropertyMetadata(string.Empty));

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }
    }
}