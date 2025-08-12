using AsynCLAudio.Core;
using AsynCLAudio.OpenCl;

namespace AsynCLAudio.Forms
{
    internal static class Program
    {
        // Enumeration of possible graph colors
        private static readonly Color[] GraphColors =
		[
			Color.LimeGreen,
			Color.BlueViolet,
            Color.DarkOrange,
            Color.Crimson,
            Color.DarkTurquoise
		];

		[STAThread]
        static void Main()
        {
			// Get count of instances of this application
            int instanceCount = System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location)).Length;
            Color graphColor = GraphColors[(instanceCount % GraphColors.Length)];

			var audioCollection = new AudioCollection(graphColor);
            var openClService = new OpenClService();

            ApplicationConfiguration.Initialize();
			Application.Run(new WindowMain(audioCollection, openClService));
        }
    }
}