using System;

namespace FunAndGamesWithSlimDX
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            //BoxDemo demo = new BoxDemo();
            //SimpleTriangleDemo demo = new SimpleTriangleDemo();
            TerrainDemo demo = new TerrainDemo();

            demo.Run();

            demo.Dispose();
        }
    }
}