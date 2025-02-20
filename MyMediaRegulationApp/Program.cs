namespace MyMediaRegulationApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm mainForm = new MainForm();
            mainForm.Hide(); // Ховаємо перед запуском циклу повідомлень

            Application.Run(); // Запускаємо додаток без головного вікна

        }
    }
}