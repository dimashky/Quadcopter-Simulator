using System;
using System.Windows.Forms;

namespace TripleM.Quadcopter.Graphics
{
#if WINDOWS || XBOX
    public static class Program
    {
        public static bool start = false;
        public static Game1 game;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main(string[] args)
        {
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Quadcopter_Simulator.QuadcopterSimulator());
            if (start)
                using (game = new Game1())
                {
                    game.Run();
                }

        }
    }
#endif
}

