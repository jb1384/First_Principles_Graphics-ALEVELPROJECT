namespace First_Principles_Graphics
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Screen screen = new Screen();
            Application.Run(screen);
        }
    }
}