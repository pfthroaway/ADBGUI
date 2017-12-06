using ADBGUI.Classes.Database;
using ADBGUI.Classes.Devices;
using Extensions;
using Extensions.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace ADBGUI.Classes
{
    internal static class AppState
    {
        internal static List<SupportedDevice> AllSupportedDevices = new List<SupportedDevice>();
        internal static SQLiteDatabaseInteraction DatabaseInteraction = new SQLiteDatabaseInteraction();

        /// <summary>Loads most everything required for the application to function properly.</summary>
        /// <returns>Task</returns>
        internal static async Task LoadAll()
        {
            DatabaseInteraction.VerifyDatabaseIntegrity();
            AllSupportedDevices = await DatabaseInteraction.LoadSupportedDevices();
        }

        #region Notification Management

        /// <summary>Displays a new Notification in a thread-safe way.</summary>
        /// <param name="message">Message to be displayed</param>
        /// <param name="title">Title of the Notification Window</param>
        internal static void DisplayNotification(string message, string title)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                new Notification(message, title, NotificationButtons.OK).ShowDialog();
            });
        }

        /// <summary>Displays a new Notification in a thread-safe way.</summary>
        /// <param name="message">Message to be displayed</param>
        /// <param name="title">Title of the Notification Window</param>
        /// <param name="window">Window being referenced</param>
        internal static void DisplayNotification(string message, string title, Window window)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                new Notification(message, title, NotificationButtons.OK, window).ShowDialog();
            });
        }

        /// <summary>Displays a new Notification in a thread-safe way and retrieves a boolean result upon its closing.</summary>
        /// <param name="message">Message to be displayed</param>
        /// <param name="title">Title of the Notification Window</param>
        /// <param name="window">Window being referenced</param>
        /// <returns>Returns value of clicked button on Notification.</returns>
        internal static bool YesNoNotification(string message, string title, Window window)
        {
            bool result = false;
            Application.Current.Dispatcher.Invoke(delegate
            {
                if (new Notification(message, title, NotificationButtons.YesNo, window).ShowDialog() == true)
                    result = true;
            });
            return result;
        }

        #endregion Notification Management
    }
}