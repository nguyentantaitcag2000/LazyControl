namespace LazyControl
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Ch?y ?ng d?ng ?n b?ng context
            Application.Run(new TrayAppContext());
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        //[STAThread]
        //static void Main()
        //{
        //    // To customize application configuration such as set high DPI settings or default font,
        //    // see https://aka.ms/applicationconfiguration.
        //    ApplicationConfiguration.Initialize();
        //    Application.Run(new Form1());
        //}
    }
}