using RegawMOD.Android;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace ADBGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AndroidController android;
        private Device device;
        private List<ConnectedDevice> connectedDevices = new List<ConnectedDevice>();
        private ConnectedDevice selectedDevice = new ConnectedDevice();
        private string nl = Environment.NewLine;

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Display Manipulation

        /// <summary>
        /// This method unselects the selected item in the lstDevices Listbox.
        /// </summary>
        private void ClearSelection()
        {
            lstDevices.UnselectAll();
            imgDevice.Source = AppState.AllSupportedDevices.Find(device => device.ModelName == "Unknown").Image;
            selectedDevice = new ConnectedDevice();
        }

        private void RemoveSelected()
        {
            connectedDevices.RemoveAt(lstDevices.SelectedIndex);
            lstDevices.Items.RemoveAt(lstDevices.SelectedIndex);
            DisableADB();
            DisableFastboot();
            ClearSelection();
        }

        /// <summary>
        /// Adds text to the txtOutput TextBox.
        /// </summary>
        /// <param name="newText">Text to be added.</param>
        private void AddTextTT(string newText)
        {
            if (newText.StartsWith("Android Debug Bridge"))
                txtOutput.Text += nl + nl + "Invalid command.";
            else
                txtOutput.Text += nl + nl + newText;
            txtOutput.Focus();
            txtOutput.CaretIndex = txtOutput.Text.Length;
            txtOutput.ScrollToEnd();
        }

        /// <summary>
        /// Updates the list of devices and displays it in the lstDevices ListBox.
        /// </summary>
        private void UpdateDeviceList()
        {
            string deviceList, adbDevices;
            deviceList = Adb.ExecuteAdbCommand(Adb.FormAdbCommand("devices"));
            connectedDevices.Clear();
            lstDevices.Items.Clear();
            DisableADB();
            DisableFastboot();

            if (deviceList.Contains("device") || deviceList.Contains("recovery") || deviceList.Contains("bootloader"))
            {
                while (deviceList.Contains(" & "))
                    deviceList.Remove(0, deviceList.IndexOf(" & ") + 1);
                StringReader s = new StringReader(deviceList);
                string line;

                while (s.Peek() != -1)
                {
                    line = s.ReadLine();
                    if (line.StartsWith("List") || line.StartsWith(Environment.NewLine) || line.Trim() == "")
                    {
                        //do nothing
                    }
                    else
                    {
                        adbDevices = line.Substring(0, line.IndexOf("	"));
                        line = line.Substring(line.IndexOf("	") + 1);

                        DeviceState currState;

                        switch (line.ToUpper())

                        {
                            case "DEVICE":
                            case "ONLINE":
                                currState = DeviceState.ONLINE;
                                break;

                            case "RECOVERY":
                                currState = DeviceState.RECOVERY;
                                break;

                            case "OFFLINE":
                                currState = DeviceState.OFFLINE;
                                break;

                            case "SIDELOAD":
                                currState = DeviceState.SIDELOAD;
                                break;

                            case "UNAUTHORIZED":
                                currState = DeviceState.UNAUTHORIZED;
                                break;

                            case "FASTBOOT":
                                currState = DeviceState.FASTBOOT;
                                break;

                            default:
                                currState = DeviceState.UNKNOWN;
                                break;
                        }

                        ConnectedDevice newDevice = new ConnectedDevice(adbDevices, currState);
                        connectedDevices.Add(newDevice);
                    }
                }
            }

            deviceList = Fastboot.ExecuteFastbootCommand(Fastboot.FormFastbootCommand("devices"));
            string fastbootDevices;
            if (deviceList.Length > 0)
            {
                StringReader s = new StringReader(deviceList);
                string line;

                while (s.Peek() != -1)
                {
                    line = s.ReadLine();
                    if (line.StartsWith("List") || line.StartsWith("	") || line.Trim() == "")
                    {
                        //do nothing
                    }
                    else
                    {
                        if (line.IndexOf("	") != -1)
                        {
                            line = line.Substring(0, line.IndexOf("	"));
                            fastbootDevices = line;
                            ConnectedDevice newDevice = new ConnectedDevice(fastbootDevices, DeviceState.FASTBOOT);
                            connectedDevices.Add(newDevice);
                        }
                    }
                }
            }

            for (int i = 0; i < connectedDevices.Count; i++)
            {
                lstDevices.Items.Add(connectedDevices[i].SerialNumber + " - " + connectedDevices[i].ConnectionStatus);
            }
        }

        /// <summary>
        /// Displays the device info next to its picture.
        /// </summary>
        /// <param name="modelNum">Device Model Number</param>
        private void displayDeviceInfo(string modelNum)
        {
            selectedDevice = new ConnectedDevice(device.SerialNumber, device.State, device.HasRoot, device.HasRoot, AppState.AllSupportedDevices.Find(device => device.Codenames.Contains(modelNum)));
            DataContext = selectedDevice.DeviceInfo;
        }

        #endregion Display Manipulation

        #region ADB Button-Click Methods

        private void btnADBRebootSystem_Click(object sender, RoutedEventArgs e)
        {
            if (IsDeviceConnected(selectedDevice.SerialNumber))
            {
                device.Reboot();
                AddTextTT("Rebooting " + selectedDevice.SerialNumber + " to system...");
                RemoveSelected();
            }
            else
                System.Windows.MessageBox.Show("This device appears to no longer be connected. Please ensure this device's connection, then click 'Refresh Devices'.", "ADB GUI", MessageBoxButton.OK);
        }

        private void btnADBRebootBootloader_Click(object sender, RoutedEventArgs e)
        {
            if (IsDeviceConnected(selectedDevice.SerialNumber))
            {
                device.RebootBootloader();
                AddTextTT("Rebooting " + selectedDevice.SerialNumber + " to bootloader...");
                RemoveSelected();
            }
            else
                System.Windows.MessageBox.Show("This device appears to no longer be connected. Please ensure this device's connection, then click 'Refresh Devices'.", "ADB GUI", MessageBoxButton.OK);
        }

        private void btnADBRebootRecovery_Click(object sender, RoutedEventArgs e)
        {
            if (IsDeviceConnected(selectedDevice.SerialNumber))
            {
                device.RebootRecovery();
                AddTextTT("Rebooting " + selectedDevice.SerialNumber + " to recovery...");
                RemoveSelected();
            }
            else
                System.Windows.MessageBox.Show("This device appears to no longer be connected. Please ensure this device's connection, then click 'Refresh Devices'.", "ADB GUI", MessageBoxButton.OK);
        }

        private async void btnADBCommand_Click(object sender, RoutedEventArgs e)
        {
            Task<String> myTask = RunADBCommand();
            string runCmd = await myTask;
            AddTextTT(runCmd);
        }

        private void btnADBShellCommand_Click(object sender, RoutedEventArgs e)
        {
            string adbCmd = txtADBShellCommand.Text;
            string parameters = "";
            bool su = chkSU.IsChecked.Value;

            if (adbCmd.Contains(" "))
            {
                parameters = (char)34 + adbCmd.Substring(adbCmd.IndexOf(" ") + 1) + (char)34;
                adbCmd = adbCmd.Substring(0, adbCmd.IndexOf(" "));
            }

            try
            {
                AddTextTT(Adb.ExecuteAdbCommand(Adb.FormAdbShellCommand(device, su, adbCmd, parameters)));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "ADB GUI", MessageBoxButton.OK);
            }
        }

        private void btnADBRemount_Click(object sender, RoutedEventArgs e)
        {
            AddTextTT(Adb.ExecuteAdbCommand(Adb.FormAdbCommand(device, "remount", "")));
        }

        #endregion ADB Button-Click Methods

        #region Window Button-Click Methods

        private void btnRefreshDevices_Click(object sender, RoutedEventArgs e)
        {
            ClearSelection();
            UpdateDeviceList();
        }

        private void btnClearOutput_Click(object sender, RoutedEventArgs e)
        {
            txtOutput.Text = "";
        }

        #endregion Window Button-Click Methods

        #region Custom Commands

        /// <summary>
        /// Runs a custom Fastboot command.
        /// </summary>
        /// <returns></returns>
        private async Task<string> RunFastbootCommand()
        {
            string fbCmd = txtFastbootCommand.Text;
            string parameters = "";

            if (fbCmd.Contains(" "))
            {
                parameters = (char)34 + fbCmd.Substring(fbCmd.IndexOf(" ") + 1) + (char)34;
                fbCmd = fbCmd.Substring(0, fbCmd.IndexOf(" "));
            }
            Task<string> cmd = Task.Factory.StartNew(() => { return Fastboot.ExecuteFastbootCommand(Fastboot.FormFastbootCommand(device, fbCmd, parameters)); });

            string result = await cmd;
            return result;
        }

        /// <summary>
        /// Runs a custom ADB command.
        /// </summary>
        /// <returns>Returns text result of command.</returns>
        private async Task<string> RunADBCommand()
        {
            string adbCmd = txtADBCommand.Text;
            string parameters = "";

            if (adbCmd.Contains(" "))
            {
                parameters = (char)34 + adbCmd.Substring(adbCmd.IndexOf(" ") + 1) + (char)34;
                adbCmd = adbCmd.Substring(0, adbCmd.IndexOf(" "));
                if (adbCmd == "push")
                {
                    parameters = parameters.Insert(parameters.IndexOf(" /"), ((char)34).ToString());
                    parameters = parameters.Insert(parameters.IndexOf("/"), ((char)34).ToString());
                }
                else if (adbCmd == "logcat")
                {
                    parameters = parameters.Substring(1);
                    parameters = parameters.Substring(0, parameters.Length - 1);
                }
            }

            AddTextTT(nl + "adb " + adbCmd + " " + parameters + nl);
            Task<string> cmd = Task.Factory.StartNew(() => { return Adb.ExecuteAdbCommand(Adb.FormAdbCommand(device, adbCmd, parameters)); });
            string result = await cmd;
            return result;
        }

        /// <summary>
        /// Reboots to the RUU for custom zip flashes.
        /// </summary>
        private void RebootRUU()
        {
            FastbootCommand fbCmd = Fastboot.FormFastbootCommand("-s " + selectedDevice.SerialNumber + " oem rebootRUU");
            AddTextTT(Fastboot.ExecuteFastbootCommand(fbCmd));
            System.Windows.MessageBox.Show("Your phone will now load the Rom Update Utility which, among other things, allows flashing of splash images and custom zip files. When the RUU has loaded on your device, press OK.", "ADB GUI", MessageBoxButton.OK);
        }

        #endregion Custom Commands

        #region Fastboot Button-Click Methods

        private void btnFastbootRebootBootloader_Click(object sender, RoutedEventArgs e)
        {
            if (IsDeviceConnected(selectedDevice.SerialNumber))
            {
                if (lstDevices.SelectedIndex >= 0)
                {
                    FastbootCommand fbCmd = Fastboot.FormFastbootCommand("-s " + selectedDevice.SerialNumber + " reboot-bootloader");
                    AddTextTT(Fastboot.ExecuteFastbootCommand(fbCmd));
                    AddTextTT("Rebooting " + selectedDevice.SerialNumber + " to bootloader...");
                    RemoveSelected();
                }
                else
                    System.Windows.MessageBox.Show("Please select a device to reboot.", "ADB GUI", MessageBoxButton.OK);
            }
            else
                System.Windows.MessageBox.Show("This device appears to no longer be connected. Please ensure this device's connection, then click 'Refresh Devices'.", "ADB GUI", MessageBoxButton.OK);
        }

        private void btnFastbootRebootSystem_Click(object sender, RoutedEventArgs e)
        {
            if (IsDeviceConnected(selectedDevice.SerialNumber))
            {
                device.FastbootReboot();
                AddTextTT("Rebooting " + selectedDevice.SerialNumber + " to system...");
                RemoveSelected();
            }
            else
                System.Windows.MessageBox.Show("This device appears to no longer be connected. Please ensure this device's connection, then click 'Refresh Devices'.", "ADB GUI", MessageBoxButton.OK);
        }

        private async void btnFastbootCommand_Click(object sender, RoutedEventArgs e)
        {
            Task<String> myTask = RunFastbootCommand();
            string runCmd = await myTask;
            AddTextTT(runCmd);
        }

        private void btnBrowseFlashRecovery_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog recoveryImage = new OpenFileDialog();
            recoveryImage.Title = "Please select the recovery image...";
            recoveryImage.Multiselect = false;
            recoveryImage.InitialDirectory = Directory.GetCurrentDirectory();
            recoveryImage.Filter = "IMG files (*.img)|*.img|All files (*.*)|*.*";
            recoveryImage.ShowDialog();
            if (DialogResult.HasValue)
                txtFlashRecovery.Text = recoveryImage.FileName;
        }

        private void btnFlashRecovery_Click(object sender, RoutedEventArgs e)
        {
            if (IsDeviceConnected(selectedDevice.SerialNumber))
            {
                if (File.Exists(txtFlashRecovery.Text))
                {
                    if (lstDevices.SelectedIndex >= 0)
                    {
                        FastbootCommand fbCmd = Fastboot.FormFastbootCommand("-s " + selectedDevice.SerialNumber + " flash recovery " + (char)34 + txtFlashRecovery.Text + (char)34);
                        AddTextTT(Fastboot.ExecuteFastbootCommand(fbCmd) + nl);
                        AddTextTT(Fastboot.ExecuteFastbootCommand(fbCmd) + nl);
                        AddTextTT(Fastboot.ExecuteFastbootCommand(fbCmd) + nl);
                    }
                }
                else
                    System.Windows.MessageBox.Show("The file at " + txtFlashRecovery.Text + " does not appear to exist.", "ADB GUI", MessageBoxButton.OK);
            }
            else
                System.Windows.MessageBox.Show("This device appears to no longer be connected. Please ensure this device's connection, then click 'Refresh Devices'.", "ADB GUI", MessageBoxButton.OK);
        }

        private void btnBrowseCustomZip_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog customZip = new OpenFileDialog();
            customZip.Title = "Please select the recovery image...";
            customZip.SupportMultiDottedExtensions = true;
            customZip.Multiselect = false;
            customZip.InitialDirectory = Directory.GetCurrentDirectory();
            customZip.Filter = "Zip files (*.zip)|*.zip|All files (*.*)|*.*";
            customZip.ShowDialog();
            if (DialogResult.HasValue)
                txtFlashCustomZip.Text = customZip.FileName;
        }

        private void btnFlashCustomZip_Click(object sender, RoutedEventArgs e)
        {
            string zipLoc = txtFlashCustomZip.Text;

            if (IsDeviceConnected(selectedDevice.SerialNumber))
            {
                RebootRUU();
                if (File.Exists(zipLoc))
                {
                    if (lstDevices.SelectedIndex >= 0)
                    {
                        FastbootCommand fbCmd = Fastboot.FormFastbootCommand("-s " + selectedDevice.SerialNumber + " flash zip " + zipLoc);
                        AddTextTT(Fastboot.ExecuteFastbootCommand(fbCmd) + nl);
                        AddTextTT(Fastboot.ExecuteFastbootCommand(fbCmd) + nl);
                        AddTextTT(Fastboot.ExecuteFastbootCommand(fbCmd) + nl);
                    }
                }
                else
                    System.Windows.MessageBox.Show("The file at " + zipLoc + " does not appear to exist.", "ADB GUI", MessageBoxButton.OK);
            }
            else
                System.Windows.MessageBox.Show("This device appears to no longer be connected. Please ensure this device's connection, then click 'Refresh Devices'.", "ADB GUI", MessageBoxButton.OK);
        }

        #endregion Fastboot Button-Click Methods

        #region Push Button-Click Methods

        private void btnBrowsePushFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog pushFile = new OpenFileDialog();
            pushFile.Title = "Please select the file to push to your device...";
            pushFile.SupportMultiDottedExtensions = true;
            pushFile.Multiselect = false;
            pushFile.InitialDirectory = Directory.GetCurrentDirectory();
            pushFile.ShowDialog();
            if (DialogResult.HasValue)
                txtPushFileSource.Text = pushFile.FileName;
        }

        private void btnPushFile_Click(object sender, RoutedEventArgs e)
        {
            if (device.PushFile(txtPushFileSource.Text, txtPushFileDestination.Text))
                AddTextTT("Successfully pushed file.");
            else
                AddTextTT("Failed to push file.");
        }

        private void btnBrowsePushDirectory_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog pushDirectory = new FolderBrowserDialog();
            pushDirectory.SelectedPath = Directory.GetCurrentDirectory();
            pushDirectory.Description = "Please select the folder to push to your device...";
            pushDirectory.ShowNewFolderButton = true;
            pushDirectory.ShowDialog();
            if (DialogResult.HasValue)
                txtPushDirectorySource.Text = pushDirectory.SelectedPath;
        }

        private void btnPushDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (device.PushFile(txtPushDirectorySource.Text, txtPushDirectoryDestination.Text))
                AddTextTT("Successfully pushed directory.");
            else
                AddTextTT("Failed to push directory.");
        }

        #endregion Push Button-Click Methods

        #region Pull Button-Click Methods

        private void btnBrowsePullFile_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog pullFileDestination = new FolderBrowserDialog();
            pullFileDestination.SelectedPath = Directory.GetCurrentDirectory();
            pullFileDestination.Description = "Please select the destination for the file you are attempting to pull from your device...";
            pullFileDestination.ShowNewFolderButton = true;
            pullFileDestination.ShowDialog();
            if (DialogResult.HasValue)
                txtPullFileDestination.Text = pullFileDestination.SelectedPath;
        }

        private void btnPullFile_Click(object sender, RoutedEventArgs e)
        {
            if (device.PullFile(txtPullFileSource.Text, txtPullFileDestination.Text))
                AddTextTT("Successfully pulled file.");
            else
                AddTextTT("Failed to pull file.");
        }

        private void btnBrowsePullDirectory_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog pullDirectory = new FolderBrowserDialog();
            pullDirectory.Description = "Please select the destination for the directory you are attempting to pull from your device...";
            pullDirectory.SelectedPath = Directory.GetCurrentDirectory();
            pullDirectory.ShowNewFolderButton = true;
            pullDirectory.ShowDialog();
            if (DialogResult.HasValue)
                txtPullDirectoryDestination.Text = pullDirectory.SelectedPath;
        }

        private void btnPullDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (device.PullDirectory(txtPullDirectorySource.Text, txtPullDirectoryDestination.Text))
                AddTextTT("Successfully pulled directory.");
            else
                AddTextTT("Failed to pull directory.");
        }

        #endregion Pull Button-Click Methods

        #region Check ADB Commands

        private void CheckADBCommand()
        {
            if (txtADBCommand.Text.Length > 0 && selectedDevice.ConnectionStatus != DeviceState.FASTBOOT)
                btnADBCommand.IsEnabled = true;
            else
                btnADBCommand.IsEnabled = false;
        }

        private void CheckADBShellCommand()
        {
            if (txtADBShellCommand.Text.Length > 0 && selectedDevice.ConnectionStatus != DeviceState.FASTBOOT)
                btnADBShellCommand.IsEnabled = true;
            else
                btnADBShellCommand.IsEnabled = false;
        }

        private void CheckPushFile()
        {
            if (txtPushFileSource.Text.Length > 0 && txtPushFileDestination.Text.Length > 0 && selectedDevice.ConnectionStatus != DeviceState.FASTBOOT)
                btnPushFile.IsEnabled = true;
            else
                btnPushFile.IsEnabled = false;
        }

        private void CheckPushDirectory()
        {
            if (txtPushDirectorySource.Text.Length > 0 && txtPushDirectoryDestination.Text.Length > 0 && selectedDevice.ConnectionStatus != DeviceState.FASTBOOT)
                btnPushDirectory.IsEnabled = true;
            else
                btnPushDirectory.IsEnabled = false;
        }

        private void CheckPullFile()
        {
            if (txtPullFileSource.Text.Length > 0 && txtPullFileDestination.Text.Length > 0 && selectedDevice.ConnectionStatus != DeviceState.FASTBOOT)
                btnPullFile.IsEnabled = true;
            else
                btnPullFile.IsEnabled = false;
        }

        private void CheckPullDirectory()
        {
            if (txtPullDirectorySource.Text.Length > 0 && txtPullDirectoryDestination.Text.Length > 0 && selectedDevice.ConnectionStatus != DeviceState.FASTBOOT)
                btnPullDirectory.IsEnabled = true;
            else
                btnPullDirectory.IsEnabled = false;
        }

        private void CheckRemount()
        {
            if (device.HasRoot && selectedDevice.ConnectionStatus != DeviceState.FASTBOOT)
                btnADBRemount.IsEnabled = true;
            else
                btnADBRemount.IsEnabled = false;
        }

        private void CheckSU()
        {
            if (selectedDevice.HasRoot)
                chkSU.IsEnabled = true;
            else
                chkSU.IsEnabled = false;
        }

        #endregion Check ADB Commands

        #region Check Fastboot Commands

        private void txtFlashRecovery_TextChanged(object sender, EventArgs e)
        {
            CheckFlashRecovery();
        }

        private void CheckFlashRecovery()
        {
            if (txtFlashRecovery.Text.Length > 0 && selectedDevice.ConnectionStatus == DeviceState.FASTBOOT && selectedDevice.CanFlashRecovery) { btnFlashRecovery.IsEnabled = true; }
            else { btnFlashRecovery.IsEnabled = false; }
        }

        private void txtFlashCustomZip_TextChanged(object sender, EventArgs e)
        {
            CheckFlashCustomZip();
        }

        private void CheckFlashCustomZip()
        {
            if (txtFlashCustomZip.Text.Length > 0 && selectedDevice.ConnectionStatus == DeviceState.FASTBOOT && selectedDevice.CanFlashRecovery) { btnFlashCustomZip.IsEnabled = true; }
            else { btnFlashCustomZip.IsEnabled = false; }
        }

        #endregion Check Fastboot Commands

        #region Enable/Disable Buttons

        /// <summary>
        /// Disables all ADB buttons.
        /// </summary>
        private void DisableADB()
        {
            btnADBRebootSystem.IsEnabled = false;
            btnADBRebootBootloader.IsEnabled = false;
            btnADBRebootRecovery.IsEnabled = false;
            btnADBCommand.IsEnabled = false;
            btnADBShellCommand.IsEnabled = false;
            btnADBRemount.IsEnabled = false;
            btnPushFile.IsEnabled = false;
            btnPushDirectory.IsEnabled = false;
            btnPullDirectory.IsEnabled = false;
            btnPullFile.IsEnabled = false;
            chkSU.IsChecked = false;
        }

        /// <summary>
        /// Disable Fastboot buttons.
        /// </summary>
        private void DisableFastboot()
        {
            btnFastbootRebootSystem.IsEnabled = false;
            btnFlashRecovery.IsEnabled = false;
            btnFastbootRebootBootloader.IsEnabled = false;
            btnFlashCustomZip.IsEnabled = false;
        }

        /// <summary>
        /// Enable ADB buttons.
        /// </summary>
        private void EnableADB()
        {
            btnADBRebootSystem.IsEnabled = true;
            btnADBRebootBootloader.IsEnabled = true;
            btnADBRebootRecovery.IsEnabled = true;

            CheckADBCommand();
            CheckADBShellCommand();
            CheckPushFile();
            CheckPushDirectory();
            CheckPullFile();
            CheckPullDirectory();
            CheckRemount();
            CheckSU();
        }

        /// <summary>
        /// Enable Fastboot buttons.
        /// </summary>
        private void EnableFastboot()
        {
            btnFastbootRebootSystem.IsEnabled = true;
            btnFastbootRebootBootloader.IsEnabled = true;
            CheckCustomFastboot();
            CheckFlashCustomZip();
            CheckFlashRecovery();
        }

        /// <summary>
        /// Checks whether Custom Fastboot commands should be enabled.
        /// </summary>
        private void CheckCustomFastboot()
        {
            if (CheckSelectedDevice() && selectedDevice.ConnectionStatus == DeviceState.FASTBOOT)
            {
                if (txtFastbootCommand.Text.Length > 0)
                    btnFastbootCommand.IsEnabled = true;
                else
                    btnFastbootCommand.IsEnabled = false;
            }
        }

        /// <summary>
        /// Checks whether the selected item in the lstDevices is valid.
        /// </summary>
        /// <returns>True if valid selection</returns>
        private bool CheckSelectedDevice()
        {
            if (lstDevices.SelectedIndex >= 0)
                return true;

            return false;
        }

        #endregion Enable/Disable Buttons

        #region Device Information Methods

        /// <summary>
        /// Gets the model number for a device.
        /// </summary>
        /// <param name="serial">Serial number of device</param>
        /// <returns>Model number</returns>
        private bool GetModelNum(string serial)
        {
            string modelNum = "";
            bool success = false;
            try
            {
                device = android.GetConnectedDevice(serial); if (device.State != DeviceState.FASTBOOT)
                {
                    AdbCommand shellCmd = Adb.FormAdbShellCommand(device, false, "cat", "/system/build.prop | grep product.model");
                    string line = Adb.ExecuteAdbCommand(shellCmd);
                    line = line.Substring(line.IndexOf("=") + 1).Trim();
                    StringReader s = new StringReader(line);
                    modelNum = s.ReadLine();
                    success = true;
                }
                else
                {
                    FastbootCommand fbCmd = Fastboot.FormFastbootCommand("-s " + device.SerialNumber + " getvar all");
                    modelNum = GetFastbootModelNum(Fastboot.ExecuteFastbootCommand(fbCmd));
                    modelNum = modelNum.Substring(modelNum.IndexOf(": ") + 2).Trim();
                }
            }
            catch (Exception)
            { }

            displayDeviceInfo(modelNum);
            return success;
        }

        /// <summary>
        /// Gets the model number for a device.
        /// </summary>
        /// <param name="parseString">String to be parsed</param>
        /// <returns>Model number</returns>
        private string GetFastbootModelNum(string parseString)
        {
            StringReader s = new StringReader(parseString);
            string line;

            while (s.Peek() != -1)
            {
                line = s.ReadLine();

                if (line.Contains("product"))
                    return line;
            }

            return "Could not determine device.";
        }

        /// <summary>
        /// Determines whether a device is connected.
        /// </summary>
        /// <param name="serial">Serial number of device</param>
        /// <returns>True if device is connected</returns>
        private bool IsDeviceConnected(string serial)
        {
            return android.IsDeviceConnected(serial);
        }

        #endregion Device Information Methods

        #region Window-Manipulation Methods

        private async Task<Boolean> loadTabbed()
        {
            await Task.Factory.StartNew(() => android = AndroidController.Instance);
            UpdateDeviceList();
            return true;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void lstDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstDevices.SelectedIndex >= 0)
            {
                if (android.IsDeviceConnected(connectedDevices[lstDevices.SelectedIndex].SerialNumber))
                {
                    string selection = lstDevices.SelectedItem.ToString();
                    selection = selection.Substring(0, selection.IndexOf(" "));
                    GetModelNum(selection);

                    if (lstDevices.SelectedItem.ToString().Contains("FASTBOOT")) //disable ADB
                    {
                        DisableADB();
                        EnableFastboot();
                    }
                    else    //disable Fastboot
                    {
                        DisableFastboot();
                        EnableADB();
                    }
                }
                else
                {
                    DisableADB();
                    DisableFastboot();
                }
            }
            else
            {
                selectedDevice = new ConnectedDevice();
                DataContext = selectedDevice.DeviceInfo;
            }

            imgDevice.Source = selectedDevice.DeviceInfo.Image;
        }

        private async void windowMain_Loaded(object sender, RoutedEventArgs e)
        {
            await loadTabbed();
            AppState.LoadAll();
            DataContext = selectedDevice.DeviceInfo;
        }

        #endregion Window-Manipulation Methods
    }
}