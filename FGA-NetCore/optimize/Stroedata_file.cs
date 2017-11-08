using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGA_NetCore.optimize
{
    class Stroedata_file
    {
        public static string[,] LoadCsv()
        {
            string whole_file = System.IO.File.ReadAllText(@"/Users/wissanu/PIMA_BaggingLOF_VER1.csv");
            whole_file = whole_file.Replace('\n', '\r');
            string[] lines = whole_file.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // See how many rows and columns there are.
            int num_rows = lines.Length;
            int num_cols = lines[0].Split(',').Length;

            // Allocate the data array.
            string[,] values_train = new string[num_rows, num_cols];

            // Load the array.
            for (int r = 0; r < num_rows; r++)
            {
                string[] line_r = lines[r].Split(',');
                for (int c = 0; c < num_cols; c++)
                {
                    values_train[r, c] = line_r[c];
                }
            }
            return values_train;
        }
    }
}
