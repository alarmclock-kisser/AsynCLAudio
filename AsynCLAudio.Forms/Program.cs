using AsynCLAudio.Core;
using AsynCLAudio.OpenCl;

namespace AsynCLAudio.Forms
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var audioCollection = new AudioCollection();
            var openClService = new OpenClService();

            ApplicationConfiguration.Initialize();
            Application.Run(new WindowMain(audioCollection, openClService));
        }
    }
}