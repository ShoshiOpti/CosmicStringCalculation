using System;
using System.Globalization;
using System.IO;

namespace CosmicStringSuperposition
{
    class Program
    {
        // Global switching duration T (will be set by user input).
        static double T = 1.0;

        // Threshold for treating a sine value as zero.
        const double threshold = 1e-12;

        /// <summary>
        /// Theta(x) = 1 if x > 0, else 0.
        /// </summary>
        static double Theta(double x)
        {
            return (x > 0.0) ? 1.0 : 0.0;
        }

        /// <summary>
        /// Compute P_D(Omega, r) as defined:
        /// P_D = (T^2/16)*exp(-|Omega|T)*SUM[d=0..D-1]{ 1/(4r^2 sin^2(pi*d/D) + T^2) }
        ///       + Theta(Omega)*(T^3/8)*SUM[d=0..D-1]{ sin(2*Omega*T*sin(pi*d/D))/(2r*sin(pi*d/D)*(4r^2 sin^2(pi*d/D) + T^2)) }.
        /// For cases where sin(pi*d/D) is nearly zero, we compute the limit.
        /// </summary>
        static double ComputeP(int D, double Omega, double r)
        {
            double sum1 = 0.0;
            for (int d = 0; d < D; d++)
            {
                double angle = Math.PI * d / D;
                double sAngle = Math.Sin(angle);
                double denom = 4.0 * r * r * sAngle * sAngle + T * T;
                if (Math.Abs(denom) < threshold)
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
                    if (Math.Abs(denom) < threshold)
                        throw new Exception($"ComputeP: Denom=0 in second sum for d={d}, r={r}, T={T}");

                    double termVal = 0.0;
                    if (Math.Abs(sAngle) < threshold)
                    {
                        // Use the limit as sAngle -> 0: sin(2*Omega*T*sAngle) ~ 2*Omega*T*sAngle.
                        termVal = (2.0 * Omega * T) / (2.0 * r * T * T); // simplifies to Omega/(r*T)
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
        /// L_{NM} = (T^2/16)*exp(-|Omega|T)*SUM[n=0..N-1]SUM[m=0..M-1]{ 1/(4r^2 sin^2(pi(n/N - m/M)) + T^2) }
        ///          + Theta(Omega)*(T^3/8)*SUM[n=0..N-1]SUM[m=0..M-1]{ sin(2*r*Omega*sin(pi(n/N - m/M)))/(2r*sin(pi(n/N - m/M))*(4r^2 sin^2(pi(n/N - m/M)) + T^2)) }.
        /// For cases where sin(pi(n/N - m/M)) is nearly zero, we compute the limit.
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
                    if (Math.Abs(denom) < threshold)
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
                        if (Math.Abs(denom) < threshold)
                            throw new Exception($"ComputeL: Denom=0 in second sum for n={n}, m={m}, r={r}, T={T}");

                        double termVal = 0.0;
                        if (Math.Abs(sAngle) < threshold)
                        {
                            // Use the limit as sAngle -> 0: sin(2*r*Omega*sAngle) ~ 2*r*Omega*sAngle.
                            termVal = (2.0 * r * Omega) / (2.0 * r * T * T); // simplifies to Omega/(T^2)
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
            string fileName = "results.csv";
            string filePath = Path.Combine(desktopPath, fileName);
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
            // 5. Open a StreamWriter to output data into a CSV file on the desktop.
            //-------------------------------------------------------------------------
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write CSV header
                writer.WriteLine("Omega,r,PQ,PC");

                //-------------------------------------------------------------------------
                // 6. Constant parameter: lambda^4 (can be modified or input by user)
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
            // 8. Inform the user and wait for key press before exiting.
            //-------------------------------------------------------------------------
            Console.WriteLine("CSV file generated successfully on the Desktop.");
            Console.WriteLine("Data generation complete. Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}
