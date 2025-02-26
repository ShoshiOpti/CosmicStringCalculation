// See https://aka.ms/new-console-template for more information
using System;
using System.Globalization;
using System.IO;

namespace CosmicStringSuperposition
{
    class Program
    {
        /// <summary>
        /// Computes the transition probability for a single cosmic-string spacetime,
        /// P_D(tildeOmega, tildeR), for topological charge D.
        /// Replace the dummy expression below with your derived formula.
        /// </summary>
        static double ComputeP(int D, double Omega, double r)
        {
            // Dummy placeholder expression for P_D:
            double dummy = Math.Exp(-0.1 * D * (Omega + r)) * (1.0 + 0.01 * D);
            return dummy;
        }

        /// <summary>
        /// Computes the cross-correlation term L_{N,M}(tildeOmega, tildeR) for cosmic string superposition.
        /// Replace the dummy expression below with your derived formula.
        /// </summary>
        static double ComputeL(int N, int M, double Omega, double r)
        {
            // Dummy placeholder expression for L_{N,M}:
            double dummy = 0.5 * Math.Sin(0.2 * (N + M) * Omega) * Math.Exp(-0.1 * r);
            return dummy;
        }

        static void Main(string[] args)
        {
            //--------------------------------------------------------------------------
            // 1. Prompt user for topological charges N and M (each <= 3)
            //--------------------------------------------------------------------------
            Console.Write("Enter N (integer <= 3): ");
            int N = int.Parse(Console.ReadLine() ?? "1");

            Console.Write("Enter M (integer <= 3): ");
            int M = int.Parse(Console.ReadLine() ?? "1");

            //--------------------------------------------------------------------------
            // 2. Define the output CSV file path to the desktop.
            //--------------------------------------------------------------------------
            // Get the desktop folder path.
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName = "results.csv";
            string filePath = Path.Combine(desktopPath, fileName);

            // Inform the user of the file destination.
            Console.WriteLine($"CSV file will be saved to: {filePath}");

            //--------------------------------------------------------------------------
            // 3. Define the range and step for dimensionless energy gap (Omega) and radius (r).
            //    Adjust these values as needed.
            //--------------------------------------------------------------------------
            double OmegaStart = 0.1;
            double OmegaEnd = 5.0;
            double OmegaStep = 0.1;

            double rStart = 0.1;
            double rEnd = 5.0;
            double rStep = 0.1;

            //--------------------------------------------------------------------------
            // 4. Open a StreamWriter to output data into a CSV file on the desktop.
            //--------------------------------------------------------------------------
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write CSV header
                writer.WriteLine("Omega,r,PQ,PC");

                //--------------------------------------------------------------------------
                // 5. Constant parameter: lambda^4 (can be modified or input by user)
                //--------------------------------------------------------------------------
                double lambda4 = 1.0;

                //--------------------------------------------------------------------------
                // 6. Loop over the ranges of Omega and r to compute transition probabilities
                //--------------------------------------------------------------------------
                for (double Omega = OmegaStart; Omega <= OmegaEnd + 1e-9; Omega += OmegaStep)
                {
                    for (double r = rStart; r <= rEnd + 1e-9; r += rStep)
                    {
                        // Compute the single detector responses for each topological charge:
                        double P_A = ComputeP(N, Omega, r);  // for cosmic string with charge N
                        double P_B = ComputeP(M, Omega, r);  // for cosmic string with charge M

                        // Compute the cross-correlation term L_{AB} = L_{N,M}(Omega, r)
                        double L_AB = ComputeL(N, M, Omega, r);

                        //--------------------------------------------------------------------------
                        // 7. Compute "Classical" and "Quantum" transition probabilities:
                        //
                        //    Classical: P_{AB}^C = (lambda^4 / 2) * [P_A + P_B]
                        //    Quantum:   P_{AB}^Q = (lambda^4 / 2) * [P_A + P_B + 2*L_{AB}]
                        //--------------------------------------------------------------------------
                        double P_C = 0.5 * lambda4 * (P_A + P_B);
                        double P_Q = 0.5 * lambda4 * (P_A + P_B + 2.0 * L_AB);

                        //--------------------------------------------------------------------------
                        // 8. Write the computed values to the CSV file in a structured format.
                        //--------------------------------------------------------------------------
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture,
                                      "{0:F4},{1:F4},{2:E6},{3:E6}",
                                      Omega, r, P_Q, P_C));
                    }
                }
            }

            //--------------------------------------------------------------------------
            // 9. Inform the user and wait for key press before exiting.
            //--------------------------------------------------------------------------
            Console.WriteLine("CSV file generated successfully on the Desktop.");
            Console.WriteLine("Data generation complete. Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}