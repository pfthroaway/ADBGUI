using ADBGUI.Classes.Devices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ADBGUI.Classes.Database
{
    internal interface IDatabaseInteraction
    {
        void VerifyDatabaseIntegrity();

        Task<List<SupportedDevice>> LoadSupportedDevices();
    }
}