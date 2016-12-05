using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace ADBGUI
{
    internal static class AppState
    {
        private const string _DBPROVIDERANDSOURCE = "Data Source = ADBGUI.sqlite;Version=3";

        internal static List<SupportedDevice> AllSupportedDevices = new List<SupportedDevice>();

        internal static bool LoadAll()
        {
            bool success = false;

            SQLiteConnection con = new SQLiteConnection();
            SQLiteDataAdapter da = new SQLiteDataAdapter("SELECT * FROM Devices", con);
            DataSet ds = new DataSet();
            con.ConnectionString = _DBPROVIDERANDSOURCE;

            try
            {
                da.Fill(ds, "Devices");

                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        List<string> codenames = new List<string>();
                        string code = ds.Tables[0].Rows[i]["Codenames"].ToString();
                        codenames = code.Split(',').Select(p => p.Trim()).ToList();

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

                        AllSupportedDevices.Add(newDevice);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally { con.Close(); }

            return success;
        }
    }
}