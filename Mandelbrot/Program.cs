using System;
using System.Linq;
using System.Windows.Forms;

namespace MandelBrot
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MandelbrotForm());
        }

        public static int Max(params int[] values)
        {
            return values.Max();
        }

        public static System.Drawing.Size SquareSize(int min)
        {
            return new System.Drawing.Size(min, min);
        }
    }
}
