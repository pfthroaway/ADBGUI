using ADBGUI.Classes.Devices;
using Extensions;
using Extensions.DatabaseHelp;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ADBGUI.Classes.Database
{
    internal class SQLiteDatabaseInteraction : IDatabaseInteraction
    {
        private readonly string _con = $"Data Source = {_DATABASENAME};Version=3";

        // ReSharper disable once InconsistentNaming
        private const string _DATABASENAME = "ADBGUI.sqlite";

        /// <summary>Verifies that the requested database exists and that its file size is greater than zero. If not, it extracts the embedded database file to the local output folder.</summary>
        public void VerifyDatabaseIntegrity()
        {
            Functions.VerifyFileIntegrity(
                Assembly.GetExecutingAssembly().GetManifestResourceStream($"ADBGUI.{_DATABASENAME}"), _DATABASENAME);
        }

        public async Task<List<SupportedDevice>> LoadSupportedDevices()
        {
            List<SupportedDevice> allDevices = new List<SupportedDevice>();
            DataSet ds = await SQLite.FillDataSet("SELECT * FROM Devices", _con);

            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    string code = ds.Tables[0].Rows[i]["Codenames"].ToString();
                    List<string> codenames = code.Split(',').Select(p => p.Trim()).ToList();

                    SupportedDevice newDevice = new SupportedDevice
                    (
                        modelName: ds.Tables[0].Rows[i]["Name"].ToString(),
                        codenames: codenames,
                        modelNumber: ds.Tables[0].Rows[i]["ModelNumber"].ToString(),
                        manufacturerName: ds.Tables[0].Rows[i]["Manufacturer"].ToString(),
                        splashBlock: ds.Tables[0].Rows[i]["SplashBlock"].ToString(),
                        resolution: ds.Tables[0].Rows[i]["Resolution"].ToString(),
                        ram: ds.Tables[0].Rows[i]["RAM"].ToString(),
                        imageLocation: ds.Tables[0].Rows[i]["ImageLocation"].ToString()
                    );
                    allDevices.Add(newDevice);
                }
            }
            return allDevices;
        }
    }
}