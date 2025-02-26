using System;
using System.Globalization;
using System.IO;
using CosmicStringCSharp;
using HelixToolkit.Wpf;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using MathNet.Numerics;  // Added MathNet.Numerics namespace

using System.Windows;
using HelixToolkit.Wpf;

namespace CosmicStringSuperposition
{
    class Program
    {
        // Global switching duration T (will be set by user input).
        static double T = 1.0;

        // Increased threshold for treating a value as zero (now 1e-8)
        const double threshold = 1e-8;

        /// <summary>
        /// Theta(x) = 1 if x > 0, else 0.
        /// </summary>
        static double Theta(double x)
        {
            return (x > 0.0) ? 1.0 : 0.0;
        }

        /// <summary>
        /// Returns a unique file path on the desktop by checking if baseFileName + extension exists.
        /// If it does, appends numbers until an available file name is found.
        /// </summary>
        static string GetUniqueFilePath(string desktopPath, string baseFileName, string extension)
        {
            string filePath = Path.Combine(desktopPath, baseFileName + extension);
            int counter = 1;
            while (File.Exists(filePath))
            {
                filePath = Path.Combine(desktopPath, baseFileName + counter.ToString() + extension);
                counter++;
            }
            return filePath;
        }

        /// <summary>
        /// Compute P_D(Omega, r) as defined:
        /// P_D = (T^2/16)*exp(-|Omega|T)*Σ[d=0..D-1]{ 1/(4*r^2*sin^2(pi*d/D) + T^2) }
        ///       + Theta(Omega)*(T^3/8)*Σ[d=0..D-1]{ sin(2*Omega*T*sin(pi*d/D))/(2*r*sin(pi*d/D)*(4*r^2*sin^2(pi*d/D) + T^2)) }.
        /// If sin(pi*d/D) is nearly zero, the limit is used.
        /// </summary>
        static double ComputeP(int D, double Omega, double r)
        {
            double sum1 = 0.0;
            for (int d = 0; d < D; d++)
            {
                double angle = Math.PI * d / D;
                double sAngle = Math.Sin(angle);
                double denom = 4.0 * r * r * sAngle * sAngle + T * T;
                if (Precision.AlmostEqual(denom, 0.0, threshold))
                    throw new Exception($"ComputeP: Denom=0 in first sum for d={d}, r={r}, T={T}");
                sum1 += 1.0 / denom;
            }
            double term1 = (T * T / 16.0) * Math.Exp(-Math.Abs(Omega) * T) * sum1;

            double sum2 = 0.0;
            if (Omega > 0.0)
            {
                for (int d = 0; d < D; d++)
                {
                    double angle = Math.PI * d / D;
                    double sAngle = Math.Sin(angle);
                    double denom = 4.0 * r * r * sAngle * sAngle + T * T;
                    if (Precision.AlmostEqual(denom, 0.0, threshold))
                        throw new Exception($"ComputeP: Denom=0 in second sum for d={d}, r={r}, T={T}");

                    double termVal = 0.0;
                    if (Precision.AlmostEqual(sAngle, 0.0, threshold))
                    {
                        // As sAngle -> 0, use the limit:
                        // sin(2*Omega*T*sAngle) ~ 2*Omega*T*sAngle,
                        // so term becomes: (2*Omega*T*sAngle)/(2*r*T^2*sAngle) = Omega/(r*T)
                        termVal = Omega / (r * T);
                    }
                    else
                    {
                        double numerator = Math.Sin(2.0 * Omega * T * sAngle);
                        termVal = numerator / (2.0 * r * sAngle * denom);
                    }
                    sum2 += termVal;
                }
            }
            double term2 = (T * T * T / 8.0) * sum2;
            return term1 + term2;
        }

        /// <summary>
        /// Compute L_{NM}(Omega, r) as defined:
        /// L_{NM} = (T^2/16)*exp(-|Omega|T)*Σ[n=0..N-1]Σ[m=0..M-1]{ 1/(4*r^2*sin^2(pi(n/N - m/M)) + T^2) }
        ///          + Theta(Omega)*(T^3/8)*Σ[n=0..N-1]Σ[m=0..M-1]{ sin(2*r*Omega*sin(pi(n/N - m/M)))/(2*r*sin(pi(n/N - m/M))*(4*r^2*sin^2(pi(n/N - m/M)) + T^2)) }.
        /// If sin(pi(n/N - m/M)) is nearly zero, the limit is used.
        /// </summary>
        static double ComputeL(int N, int M, double Omega, double r)
        {
            double sum1 = 0.0;
            for (int n = 0; n < N; n++)
            {
                for (int m = 0; m < M; m++)
                {
                    double angle = Math.PI * ((double)n / N - (double)m / M);
                    double sAngle = Math.Sin(angle);
                    double denom = 4.0 * r * r * sAngle * sAngle + T * T;
                    if (Precision.AlmostEqual(denom, 0.0, threshold))
                        throw new Exception($"ComputeL: Denom=0 in first sum for n={n}, m={m}, r={r}, T={T}");
                    sum1 += 1.0 / denom;
                }
            }
            double term1 = (T * T / 16.0) * Math.Exp(-Math.Abs(Omega) * T) * sum1;

            double sum2 = 0.0;
            if (Omega > 0.0)
            {
                for (int n = 0; n < N; n++)
                {
                    for (int m = 0; m < M; m++)
                    {
                        double angle = Math.PI * ((double)n / N - (double)m / M);
                        double sAngle = Math.Sin(angle);
                        double denom = 4.0 * r * r * sAngle * sAngle + T * T;
                        if (Precision.AlmostEqual(denom, 0.0, threshold))
                            throw new Exception($"ComputeL: Denom=0 in second sum for n={n}, m={m}, r={r}, T={T}");

                        double termVal = 0.0;
                        if (Precision.AlmostEqual(sAngle, 0.0, threshold))
                        {
                            // As sAngle -> 0, use the limit:
                            // sin(2*r*Omega*sAngle) ~ 2*r*Omega*sAngle,
                            // so term becomes: (2*r*Omega*sAngle)/(2*r*T^2*sAngle) = Omega/(T^2)
                            termVal = Omega / (T * T);
                        }
                        else
                        {
                            double numerator = Math.Sin(2.0 * r * Omega * sAngle);
                            termVal = numerator / (2.0 * r * sAngle * denom);
                        }
                        sum2 += termVal;
                    }
                }
            }
            double term2 = (T * T * T / 8.0) * sum2;
            return term1 + term2;
        }

        [STAThread]
        static void Main(string[] args)
        {
            //-------------------------------------------------------------------------
            // 1. Prompt user for switching time T.
            //-------------------------------------------------------------------------
            Console.Write("Enter switching time T (positive number): ");
            T = double.Parse(Console.ReadLine() ?? "1.0", CultureInfo.InvariantCulture);

            //-------------------------------------------------------------------------
            // 2. Prompt user for topological charges N and M (each <= 3)
            //-------------------------------------------------------------------------
            Console.Write("Enter N (integer <= 3): ");
            int N = int.Parse(Console.ReadLine() ?? "1");

            Console.Write("Enter M (integer <= 3): ");
            int M = int.Parse(Console.ReadLine() ?? "1");

            //-------------------------------------------------------------------------
            // 3. Define the output CSV file path to the desktop.
            //-------------------------------------------------------------------------
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = GetUniqueFilePath(desktopPath, "results", ".csv");
            Console.WriteLine($"CSV file will be saved to: {filePath}");

            //-------------------------------------------------------------------------
            // 4. Define the range and step for dimensionless energy gap (Omega) and radius (r).
            //-------------------------------------------------------------------------
            double OmegaStart = 0.1;
            double OmegaEnd = 5.0;
            double OmegaStep = 0.1;

            double rStart = 0.1;
            double rEnd = 5.0;
            double rStep = 0.1;

            //-------------------------------------------------------------------------
            // 5. Open a StreamWriter to output data into the CSV file.
            //-------------------------------------------------------------------------
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write CSV header.
                writer.WriteLine("Omega,r,PQ,PC");

                //-------------------------------------------------------------------------
                // 6. Constant parameter: lambda^4 (can be modified or input by user).
                //-------------------------------------------------------------------------
                double lambda4 = 1.0;

                //-------------------------------------------------------------------------
                // 7. Loop over the ranges of Omega and r to compute transition probabilities.
                //    If an error is encountered, the error message is written.
                //-------------------------------------------------------------------------
                for (double Omega = OmegaStart; Omega <= OmegaEnd + 1e-9; Omega += OmegaStep)
                {
                    for (double r = rStart; r <= rEnd + 1e-9; r += rStep)
                    {
                        string PQ_str, PC_str;
                        try
                        {
                            double P_A = ComputeP(N, Omega, r);  // for cosmic string with charge N
                            double P_B = ComputeP(M, Omega, r);  // for cosmic string with charge M
                            double L_AB = ComputeL(N, M, Omega, r);

                            double P_C = 0.5 * lambda4 * (P_A + P_B);
                            double P_Q = 0.5 * lambda4 * (P_A + P_B + 2.0 * L_AB);

                            PQ_str = P_Q.ToString("E6", CultureInfo.InvariantCulture);
                            PC_str = P_C.ToString("E6", CultureInfo.InvariantCulture);
                        }
                        catch (Exception ex)
                        {
                            // Write the error message if an error occurs.
                            PQ_str = "Error: " + ex.Message;
                            PC_str = "Error: " + ex.Message;
                        }

                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture,
                                      "{0:F4},{1:F4},{2},{3}",
                                      Omega, r, PQ_str, PC_str));
                    }
                }
            }

            //-------------------------------------------------------------------------
            // 8. Inform the user that CSV generation is complete.
            //-------------------------------------------------------------------------
            Console.WriteLine("CSV file generated successfully on the Desktop.");

            //-------------------------------------------------------------------------
            // 9. Ask the user if they want to visualize the data.
            //-------------------------------------------------------------------------
            Console.Write("Do you want to visualize the data in 3D? (yes/no): ");
            string answer = Console.ReadLine()?.Trim().ToLower();
            if (answer == "yes" || answer == "y")
            {
                Console.WriteLine("Launching 3D visualization...");
                // Call the HelixToolkit visualizer (make sure this class exists and is referenced)
                HelixQuantumClassicalPlotter.Show3DPlot(filePath);
            }
            else
            {
                Console.WriteLine("Visualization skipped.");
            }

            Console.WriteLine("Data generation complete. Press ENTER to exit.");
            Console.ReadLine();
        }
    }
    public class HelixQuantumClassicalPlotter
    {
        [STAThread]
        public static void Show3DPlot(string csvPath)
        {
            // Check if file exists
            if (!File.Exists(csvPath))
            {
                Console.WriteLine($"Error: CSV file '{csvPath}' not found.");
                return;
            }

            // Parse CSV data (skip header)
            var lines = File.ReadAllLines(csvPath).Skip(1);
            var dataRows = lines.Select(line => line.Split(',')).ToList();

            var dataPoints = dataRows.Select(parts => new
            {
                Omega = double.Parse(parts[0]),
                R = double.Parse(parts[1]),
                PQ = double.Parse(parts[2]),
                PC = double.Parse(parts[3])
            }).ToList();

            // Prepare point collections for Quantum (PQ) and Classical (PC)
            var quantumPoints = new Point3DCollection(
                dataPoints.Select(dp => new Point3D(dp.Omega, dp.R, dp.PQ))
            );
            var classicalPoints = new Point3DCollection(
                dataPoints.Select(dp => new Point3D(dp.Omega, dp.R, dp.PC))
            );

            // Create a HelixViewport3D and configure it.
            var viewport = new HelixViewport3D { Background = Brushes.White };

            // Set up a PerspectiveCamera.
            viewport.Camera = new PerspectiveCamera
            {
                Position = new Point3D(0, -20, 20),
                LookDirection = new Vector3D(0, 20, -20),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 45
            };

            // (Optional) Add custom axes (using a helper class or built-in coordinate system).
            // Here we add a custom AxesHelper3D which you need to implement.
            var axes = new AxesHelper3D
            {
                XLabel = "Omega (Ω)",
                YLabel = "r",
                ZLabel = "P",
                AxisLength = 10.0
            };
            viewport.Children.Add(axes);

            // Add the Quantum data points (blue).
            var quantumVisual = new PointsVisual3D
            {
                Color = Colors.Blue,
                Size = 2.0,
                Points = quantumPoints
            };
            viewport.Children.Add(quantumVisual);

            // Add the Classical data points (red).
            var classicalVisual = new PointsVisual3D
            {
                Color = Colors.Red,
                Size = 2.0,
                Points = classicalPoints
            };
            viewport.Children.Add(classicalVisual);

            // Create a WPF Window to host the HelixViewport3D.
            var window = new System.Windows.Window
            {
                Title = "3D Visualization (Quantum vs. Classical)",
                Content = viewport,
                Width = 1200,
                Height = 800
            };

            // Start the WPF application and run the window.
            var app = new Application();
            app.Run(window);
        }
    }
}
