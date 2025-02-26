using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace CosmicStringCSharp
{
    public class HelixQuantumClassicalPlotter
    {
        [STAThread]
        public static void Show3DPlot(string csvPath)
        {
            // 1) Parse CSV
            var lines = File.ReadAllLines(csvPath).Skip(1); // skip header
            var dataRows = lines.Select(line => line.Split(',')).ToList();

            var dataPoints = dataRows.Select(parts => new
            {
                Omega = double.Parse(parts[0]),
                R = double.Parse(parts[1]),
                PQ = double.Parse(parts[2]),
                PC = double.Parse(parts[3])
            }).ToList();

            var quantumPoints3D = new Point3DCollection(
                dataPoints.Select(dp => new Point3D(dp.Omega, dp.R, dp.PQ))
            );
            var classicalPoints3D = new Point3DCollection(
                dataPoints.Select(dp => new Point3D(dp.Omega, dp.R, dp.PC))
            );

            // 2) Create HelixViewport3D
            var viewport = new HelixViewport3D
            {
                Background = Brushes.White
            };

            // 3) Camera setup
            viewport.Camera = new PerspectiveCamera
            {
                Position = new Point3D(0, -20, 20),
                LookDirection = new Vector3D(0, 20, -20),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 45
            };

            // 4) Add our custom Axes (replaces old AxesVisual3D)
            var axes = new AxesHelper3D
            {
                XLabel = "Omega (Ω)",
                YLabel = "r",
                ZLabel = "P",
                AxisLength = 10.0
            };
            viewport.Children.Add(axes);

            // 5) Optional grid on the X-Y plane
            var grid = new GridLinesVisual3D
            {
                Center = new Point3D(0, 0, 0),
                Width = 50,
                Length = 50,
                MajorDistance = 5,
                MinorDistance = 1,
                Thickness = 0.2,
                Normal = new Vector3D(0, 0, 1) // grid lines on XY-plane
            };
            viewport.Children.Add(grid);

            // 6) Lights
            viewport.Children.Add(new DefaultLights());

            // 7) Points for Quantum data (blue)
            var quantumPointsVisual = new PointsVisual3D
            {
                Color = Colors.Blue,
                Size = 2.0,
                Points = quantumPoints3D
            };
            viewport.Children.Add(quantumPointsVisual);

            // 8) Points for Classical data (red)
            var classicalPointsVisual = new PointsVisual3D
            {
                Color = Colors.Red,
                Size = 2.0,
                Points = classicalPoints3D
            };
            viewport.Children.Add(classicalPointsVisual);

            // 9) Create and show WPF Window
            var window = new Window
            {
                Title = "HelixToolkit 3D Plot (Quantum vs. Classical)",
                Content = viewport,
                Width = 1200,
                Height = 800
            };

            var app = new Application();
            app.Run(window);
        }
    }

    /// <summary>
    /// Custom helper to visualize X/Y/Z axes with labeled ends.
    /// Uses LinesVisual3D instead of LineVisual3D.
    /// </summary>
    public class AxesHelper3D : ModelVisual3D
    {
        public string XLabel { get; set; } = "X";
        public string YLabel { get; set; } = "Y";
        public string ZLabel { get; set; } = "Z";

        public double AxisLength { get; set; } = 10.0;

        public Color XAxisColor { get; set; } = Colors.Red;
        public Color YAxisColor { get; set; } = Colors.Green;
        public Color ZAxisColor { get; set; } = Colors.Blue;

        public double LabelFontSize { get; set; } = 12.0;

        public AxesHelper3D()
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            // Clear any existing visuals
            Children.Clear();

            // Use LinesVisual3D to draw the three axes
            var axisLines = new LinesVisual3D
            {
                Color = Colors.Black,
                Thickness = 2
            };

            // X Axis: Red
            axisLines.Points.Add(new Point3D(0, 0, 0));
            axisLines.Points.Add(new Point3D(AxisLength, 0, 0));

            // Y Axis: Green
            axisLines.Points.Add(new Point3D(0, 0, 0));
            axisLines.Points.Add(new Point3D(0, AxisLength, 0));

            // Z Axis: Blue
            axisLines.Points.Add(new Point3D(0, 0, 0));
            axisLines.Points.Add(new Point3D(0, 0, AxisLength));

            Children.Add(axisLines);

            // Labels using BillboardTextVisual3D
            var xLabel3D = new BillboardTextVisual3D
            {
                Text = XLabel,
                Foreground = new SolidColorBrush(XAxisColor),
                FontSize = LabelFontSize,
                Position = new Point3D(AxisLength + 0.5, 0, 0)
            };
            Children.Add(xLabel3D);

            var yLabel3D = new BillboardTextVisual3D
            {
                Text = YLabel,
                Foreground = new SolidColorBrush(YAxisColor),
                FontSize = LabelFontSize,
                Position = new Point3D(0, AxisLength + 0.5, 0)
            };
            Children.Add(yLabel3D);

            var zLabel3D = new BillboardTextVisual3D
            {
                Text = ZLabel,
                Foreground = new SolidColorBrush(ZAxisColor),
                FontSize = LabelFontSize,
                Position = new Point3D(0, 0, AxisLength + 0.5)
            };
            Children.Add(zLabel3D);
        }
    }

}


