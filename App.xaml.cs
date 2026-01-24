using System;
using System.Windows;
using Ecoinv.Common; // Kell a Loggerhez

namespace Ecoinv
{
    public partial class App : Application
    {
        public App()
        {
            // Feliratkozunk a váratlan hibákra (még az ablakok indulása előtt)
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        // Ez a WPF felület (UI szál) hibáit kapja el
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Elmentjük a hibát a logba
            Logger.LogError(e.Exception, "KRITIKUS NEM KEZELT HIBA (UI)!");

            // Szólunk a felhasználónak
            MessageBox.Show("Egy váratlan hiba történt. A részleteket a naplófájlban találja.\n\nHiba: " + e.Exception.Message,
                            "Váratlan Hiba", MessageBoxButton.OK, MessageBoxImage.Error);

            // Megpróbáljuk nem bezárni a programot, ha nem muszáj
            e.Handled = true;
        }

        // Ez a mélyebb, rendszer szintű (háttérszál) hibákat kapja el
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Logger.LogError(ex, "KRITIKUS RENDSZER HIBA (Non-UI)!");
                MessageBox.Show("Kritikus rendszerhiba! A program leáll.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}