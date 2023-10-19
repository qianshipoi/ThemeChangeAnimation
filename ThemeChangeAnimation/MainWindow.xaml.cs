using MaterialDesignThemes.Wpf;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ThemeChangeAnimation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double _radius;
        private double _oldRadius;

        bool isDark = false;
        private Theme _theme;
        public MainWindow()
        {
            InitializeComponent();
            _theme = GetResourceDictionary().GetTheme();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            if (_oldRadius == _radius)
            {
                return;
            }
            _oldRadius = _radius;


            var hostWidth = ActualWidth;
            var hostHeight = ActualHeight;
            var maskEllipseCenterX = hostWidth / 2;
            var maskEllipseCenterY = hostHeight / 2;
            var maskRadiusX = _radius;
            var maskRadiusY = _radius;
            var maskRectangle = new RectangleGeometry(new Rect(new Size(hostWidth, hostHeight)));
            var maskEllipse = new EllipseGeometry(
                new Point(maskEllipseCenterX, maskEllipseCenterY),
                maskRadiusX,
                maskRadiusY);
            var combinedGeometry = Geometry.Combine(maskRectangle, maskEllipse, GeometryCombineMode.Exclude, null);
            var drawingBrush = new DrawingBrush(new GeometryDrawing(Brushes.Black, null, combinedGeometry));
            MaskImage.OpacityMask = drawingBrush;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // change theme 
            if (isDark)
            {
                _theme.SetLightTheme();
            }
            else
            {
                _theme.SetDarkTheme();
            }
            isDark = !isDark;

            var dic = Application.Current.Resources.MergedDictionaries.FirstOrDefault(x => x is IMaterialDesignThemeDictionary) ??
              Application.Current.Resources;


            RenderTargetBitmap RenderBmp = new((int)grid.ActualWidth, (int)grid.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            RenderBmp.Render(grid);

            MaskImage.Source = RenderBmp;
            MaskImage.Visibility = Visibility.Visible;
            _ = Task.Run(async () => {
                _radius = 0;
                var max = Math.Sqrt(ActualWidth * ActualWidth + ActualHeight * ActualHeight);
                while (_radius < max)
                {
                    _radius += 5;
                    await Task.Delay(10);
                }
                App.Current.Dispatcher.Invoke(() => {
                    MaskImage.Visibility = Visibility.Collapsed;
                });
            });

            dic.SetTheme(_theme);
            RecreateThemeDictionaries();
        }
        private static ResourceDictionary GetResourceDictionary()
        => Application.Current.Resources.MergedDictionaries.FirstOrDefault(x => x is IMaterialDesignThemeDictionary) ??
            Application.Current.Resources;

        private void RecreateThemeDictionaries()
        {
            ResourceDictionary root = Application.Current.Resources;
            for (int i = 0; i < root.MergedDictionaries.Count; i++)
            {
                ResourceDictionary dictionary = root.MergedDictionaries[i];
                if (dictionary["MaterialDesign.Resources.RecreateOnThemeChange"] is bool recreateOnThemeChange && recreateOnThemeChange)
                {
                    root.MergedDictionaries.RemoveAt(i);
                    root.MergedDictionaries.Insert(i, new ResourceDictionary()
                    {
                        Source = dictionary.Source
                    });
                }
            }
        }
    }
}