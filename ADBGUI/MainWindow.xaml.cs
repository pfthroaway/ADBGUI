using ADBGUI.Classes;
using ADBGUI.Classes.Devices;
using Extensions;
using RegawMOD.Android;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace ADBGUI
{
    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow
    {
        private AndroidController _android;
        private Device _device;
        private readonly List<ConnectedDevice> _connectedDevices = new List<ConnectedDevice>();
        private ConnectedDevice _selectedDevice = new ConnectedDevice();

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
            LstDevices.UnselectAll();
            ImgDevice.Source = AppState.AllSupportedDevices.Find(connectedDevice => connectedDevice.ModelName == "Unknown").Image;
            _selectedDevice = new ConnectedDevice();
        }

        private void RemoveSelected()
        {
            _connectedDevices.RemoveAt(LstDevices.SelectedIndex);
            LstDevices.Items.RemoveAt(LstDevices.SelectedIndex);
            DisableAdb();
            DisableFastboot();
            ClearSelection();
        }

        /// <summary>
        /// Adds text to the txtOutput TextBox.
        /// </summary>
        /// <param name="newText">Text to be added.</param>
        private void AddTextTt(string newText)
        {
            Functions.AddTextToTextBox(TxtOutput, newText.StartsWith("Android Debug Bridge") ? "Invalid command." : newText);
        }

        /// <summary>
        /// Updates the list of devices and displays it in the lstDevices ListBox.
        /// </summary>
        private void UpdateDeviceList()
        {
            string deviceList = Adb.ExecuteAdbCommand(Adb.FormAdbCommand("devices"));
            _connectedDevices.Clear();
            LstDevices.Items.Clear();
            DisableAdb();
            DisableFastboot();

            if (deviceList.Contains("device") || deviceList.Contains("recovery") || deviceList.Contains("bootloader"))
            {
                while (deviceList.Contains(" & "))
                    deviceList.Remove(0, deviceList.IndexOf(" & ") + 1);
                StringReader s = new StringReader(deviceList);

                while (s.Peek() != -1)
                {
                    string line = s.ReadLine();
                    if (line != null && (line.StartsWith("List") || line.StartsWith(Environment.NewLine) || line.Trim() == ""))
                    {
                        //do nothing
                    }
                    else
                    {
                        if (line != null)
                        {
                            string adbDevices = line.Substring(0, line.IndexOf("	"));
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
                            _connectedDevices.Add(newDevice);
                        }
                    }
                }
            }

            deviceList = Fastboot.ExecuteFastbootCommand(Fastboot.FormFastbootCommand("devices"));
            if (deviceList.Length > 0)
            {
                StringReader s = new StringReader(deviceList);

                while (s.Peek() != -1)
                {
                    string line = s.ReadLine();
                    if (line == null || (!line.StartsWith("List") && !line.StartsWith("	") && line.Trim() != ""))
                    {
                        if (line != null && line.IndexOf("	") != -1)
                        {
                            line = line.Substring(0, line.IndexOf("	"));
                            string fastbootDevices = line;
                            ConnectedDevice newDevice = new ConnectedDevice(fastbootDevices, DeviceState.FASTBOOT);
                            _connectedDevices.Add(newDevice);
                        }
                    }
                    else
                    {
                        //do nothing
                    }
                }
            }

            foreach (ConnectedDevice connectedDevice in _connectedDevices)
                LstDevices.Items.Add($"{connectedDevice.SerialNumber} - {connectedDevice.ConnectionStatus}");
        }

        /// <summary>
        /// Displays the device info next to its picture.
        /// </summary>
        /// <param name="modelNum">Device Model Number</param>
        private void DisplayDeviceInfo(string modelNum)
        {
            List<SupportedDevice> matchingDevices = AppState.AllSupportedDevices.Where(connDevice => connDevice.Codenames.Contains(modelNum)).ToList();

            _selectedDevice = matchingDevices.Count == 1
                ? new ConnectedDevice(_device.SerialNumber, _device.State, _device.HasRoot, _device.HasRoot, AppState.AllSupportedDevices.Find(connectedDevice => connectedDevice.Codenames.Contains(modelNum)))
                : new ConnectedDevice(_device.SerialNumber, _device.State, _device.HasRoot, _device.HasRoot, AppState.AllSupportedDevices.Find(connectedDevice => connectedDevice.ModelName == "Unknown"));
            DataContext = _selectedDevice.DeviceInfo;
        }

        #endregion Display Manipulation

        #region ADB Button-Click Methods

        private void btnADBRebootSystem_Click(object sender, RoutedEventArgs e)
        {
            if (IsDeviceConnected(_selectedDevice.SerialNumber))
            {
                _device.Reboot();
                AddTextTt($"Rebooting {_selectedDevice.SerialNumber} to system...");
                RemoveSelected();
            }
            else
                AppState.DisplayNotification("This device appears to no longer be connected. Please ensure this device's connection, then click 'Refresh Devices'.", "ADB GUI", this);
        }

        private void btnADBRebootBootloader_Click(object sender, RoutedEventArgs e)
        {
            if (IsDeviceConnected(_selectedDevice.SerialNumber))
            {
                _device.RebootBootloader();
                AddTextTt($"Rebooting {_selectedDevice.SerialNumber} to bootloader...");
                RemoveSelected();
            }
            else
                AppState.DisplayNotification("This device appears to no longer be connected. Please ensure this device's connection, then click 'Refresh Devices'.", "ADB GUI", this);
        }

        private void btnADBRebootRecovery_Click(object sender, RoutedEventArgs e)
        {
            if (IsDeviceConnected(_selectedDevice.SerialNumber))
            {
                _device.RebootRecovery();
                AddTextTt($"Rebooting {_selectedDevice.SerialNumber} to recovery...");
                RemoveSelected();
            }
            else
                AppState.DisplayNotification("This device appears to no longer be connected. Please ensure this device's connection, then click 'Refresh Devices'.", "ADB GUI", this);
        }

        private async void btnADBCommand_Click(object sender, RoutedEventArgs e)
        {
            Task<String> myTask = RunAdbCommand();
            string runCmd = await myTask;
            AddTextTt(runCmd);
            if (runCmd == "Please use the ADB shell commands textbox.")
                TxtAdbShellCommand.Focus();
        }

        private void btnADBShellCommand_Click(object sender, RoutedEventArgs e)
        {
            string adbCmd = TxtAdbShellCommand.Text;
            string parameters = "";
            bool su = ChkSu.IsChecked != null && ChkSu.IsChecked.Value;

            if (adbCmd.Contains(" "))
            {
                parameters = $"\"{adbCmd.Substring(adbCmd.IndexOf(" ") + 1)}\"";
                adbCmd = adbCmd.Substring(0, adbCmd.IndexOf(" "));
            }

            try
            {
                AddTextTt(Adb.ExecuteAdbCommand(Adb.FormAdbShellCommand(_device, su, adbCmd, parameters)));
            }
            catch (Exception ex)
            {
                AppState.DisplayNotification(ex.Message, "ADB GUI", this);
            }
        }

        private void btnADBRemount_Click(object sender, RoutedEventArgs e)
        {
            AddTextTt(Adb.ExecuteAdbCommand(Adb.FormAdbCommand(_device, "remount", "")));
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
            TxtOutput.Text = "";
        }

        #endregion Window Button-Click Methods

        #region Custom Commands

        /// <summary>
        /// Runs a custom Fastboot command.
        /// </summary>
        /// <returns></returns>
        private async Task<string> RunFastbootCommand()
        {
            string fbCmd = TxtFastbootCommand.Text;
            string parameters = "";

            if (fbCmd.Contains(" "))
            {
                parameters = $"\"{fbCmd.Substring(fbCmd.IndexOf(" ") + 1)}\"";
                fbCmd = fbCmd.Substring(0, fbCmd.IndexOf(" "));
            }
            Task<string> cmd = Task.Factory.StartNew(() => Fastboot.ExecuteFastbootCommand(Fastboot.FormFastbootCommand(_device, fbCmd, parameters)));

            string result = await cmd;
            return result;
        }

        /// <summary>
        /// Runs a custom ADB command.
        /// </summary>
        /// <returns>Returns text result of command.</returns>
        private async Task<string> RunAdbCommand()
        {
            string adbCmd = TxtAdbCommand.Text;
            string parameters = "";

            if (adbCmd != "shell")
            {
                if (adbCmd.Contains(" "))
                {
                    parameters = $"\"{adbCmd.Substring(adbCmd.IndexOf(" ", StringComparison.Ordinal) + 1)}\"";
                    adbCmd = adbCmd.Substring(0, adbCmd.IndexOf(" ", StringComparison.Ordinal));
                    if (adbCmd == "push")
                    {
                        parameters = parameters.Insert(parameters.IndexOf(" /", StringComparison.Ordinal), "\"");
                        parameters = parameters.Insert(parameters.IndexOf("/", StringComparison.Ordinal), "\"");
                    }
                    else if (adbCmd == "logcat")
                    {
                        parameters = parameters.Substring(1);
                        parameters = parameters.Substring(0, parameters.Length - 1);
                    }
                }

                AddTextTt($"\nadb {adbCmd} {parameters}\n");
                Task<string> cmd = Task.Factory.StartNew(() => Adb.ExecuteAdbCommand(Adb.FormAdbCommand(_device, adbCmd, parameters)));
                string result = await cmd;
                return result;
            }
            return "Please use the ADB shell commands textbox.";
        }

        /// <summary>
        /// Reboots to the RUU for custom zip flashes.
        /// </summary>
        private void RebootRuu()
        {
            FastbootCommand fbCmd = Fastboot.FormFastbootCommand($"-s {_selectedDevice.SerialNumber} oem rebootRUU");
            AddTextTt(Fastboot.ExecuteFastbootCommand(fbCmd));
            AppState.DisplayNotification("Your phone will now load the Rom Update Utility which, among other things, allows flashing of splash images and custom zip files. When the RUU has loaded on your device, press OK.", "ADB GUI", this);
        }

        #endregion Custom Commands

        #region Fastboot Button-Click Methods

        private void btnFastbootRebootBootloader_Click(object sender, RoutedEventArgs e)
        {
            if (IsDeviceConnected(_selectedDevice.SerialNumber))
            {
                if (LstDevices.SelectedIndex >= 0)
                {
                    FastbootCommand fbCmd = Fastboot.FormFastbootCommand($"-s {_selectedDevice.SerialNumber} reboot-bootloader");
                    AddTextTt(Fastboot.ExecuteFastbootCommand(fbCmd));
                    AddTextTt($"Rebooting {_selectedDevice.SerialNumber} to bootloader...");
                    RemoveSelected();
                }
                else
                    AppState.DisplayNotification("Please select a device to reboot.", "ADB GUI", this);
            }
            else
                AppState.DisplayNotification("This device appears to no longer be connected. Please ensure this device's connection, then click 'Refresh Devices'.", "ADB GUI", this);
        }

        private void btnFastbootRebootSystem_Click(object sender, RoutedEventArgs e)
        {
            if (IsDeviceConnected(_selectedDevice.SerialNumber))
            {
                _device.FastbootReboot();
                AddTextTt($"Rebooting {_selectedDevice.SerialNumber} to system...");
                RemoveSelected();
            }
            else
                AppState.DisplayNotification("This device appears to no longer be connected. Please ensure this device's connection, then click 'Refresh Devices'.", "ADB GUI", this);
        }

        private async void btnFastbootCommand_Click(object sender, RoutedEventArgs e)
        {
            Task<String> myTask = RunFastbootCommand();
            string runCmd = await myTask;
            AddTextTt(runCmd);
        }

        private void btnBrowseFlashRecovery_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog recoveryImage = new OpenFileDialog
            {
                Title = @"Please select the recovery image...",
                Multiselect = false,
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = @"IMG files (*.img)|*.img|All files (*.*)|*.*"
            };
            recoveryImage.ShowDialog();
            if (DialogResult.HasValue)
                TxtFlashRecovery.Text = recoveryImage.FileName;
        }

        private void btnFlashRecovery_Click(object sender, RoutedEventArgs e)
        {
            if (IsDeviceConnected(_selectedDevice.SerialNumber))
            {
                if (File.Exists(TxtFlashRecovery.Text))
                {
                    if (LstDevices.SelectedIndex >= 0)
                    {
                        FastbootCommand fbCmd = Fastboot.FormFastbootCommand($"-s {_selectedDevice.SerialNumber} flash recovery \"{TxtFlashRecovery.Text}\"");
                        AddTextTt($"Fastboot.ExecuteFastbootCommand({fbCmd})\n");
                        AddTextTt($"Fastboot.ExecuteFastbootCommand({fbCmd})\n");
                        AddTextTt($"Fastboot.ExecuteFastbootCommand({fbCmd})\n");
                    }
                }
                else
                    AppState.DisplayNotification($"The file at {TxtFlashRecovery.Text} does not appear to exist.", "ADB GUI", this);
            }
            else
                AppState.DisplayNotification("This device appears to no longer be connected. Please ensure this device's connection, then click 'Refresh Devices'.", "ADB GUI", this);
        }

        private void btnBrowseCustomZip_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog customZip = new OpenFileDialog
            {
                Title = @"Please select the recovery image...",
                SupportMultiDottedExtensions = true,
                Multiselect = false,
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = @"Zip files (*.zip)|*.zip|All files (*.*)|*.*"
            };
            customZip.ShowDialog();
            if (DialogResult.HasValue)
                TxtFlashCustomZip.Text = customZip.FileName;
        }

        private void btnFlashCustomZip_Click(object sender, RoutedEventArgs e)
        {
            string zipLoc = TxtFlashCustomZip.Text;

            if (IsDeviceConnected(_selectedDevice.SerialNumber))
            {
                RebootRuu();
                if (File.Exists(zipLoc))
                {
                    if (LstDevices.SelectedIndex >= 0)
                    {
                        FastbootCommand fbCmd = Fastboot.FormFastbootCommand($"-s {_selectedDevice.SerialNumber} flash zip {zipLoc}");
                        AddTextTt($"Fastboot.ExecuteFastbootCommand({fbCmd})\n");
                        AddTextTt($"Fastboot.ExecuteFastbootCommand({fbCmd})\n");
                        AddTextTt($"Fastboot.ExecuteFastbootCommand({fbCmd})\n");
                    }
                }
                else
                    AppState.DisplayNotification($"The file at {zipLoc} does not appear to exist.", "ADB GUI", this);
            }
            else
                AppState.DisplayNotification("This device appears to no longer be connected. Please ensure this device's connection, then click 'Refresh Devices'.", "ADB GUI", this);
        }

        #endregion Fastboot Button-Click Methods

        #region Push Button-Click Methods

        private void btnBrowsePushFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog pushFile = new OpenFileDialog
            {
                Title = @"Please select the file to push to your device...",
                SupportMultiDottedExtensions = true,
                Multiselect = false,
                InitialDirectory = Directory.GetCurrentDirectory()
            };
            pushFile.ShowDialog();
            if (DialogResult.HasValue)
                TxtPushFileSource.Text = pushFile.FileName;
        }

        private void btnPushFile_Click(object sender, RoutedEventArgs e)
        {
            AddTextTt(_device.PushFile(TxtPushFileSource.Text, TxtPushFileDestination.Text)
                ? "Successfully pushed file."
                : "Failed to push file.");
        }

        private void btnBrowsePushDirectory_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog pushDirectory = new FolderBrowserDialog
            {
                SelectedPath = Directory.GetCurrentDirectory(),
                Description = @"Please select the folder to push to your device...",
                ShowNewFolderButton = true
            };
            pushDirectory.ShowDialog();
            if (DialogResult.HasValue)
                TxtPushDirectorySource.Text = pushDirectory.SelectedPath;
        }

        private void btnPushDirectory_Click(object sender, RoutedEventArgs e)
        {
            AddTextTt(_device.PushFile(TxtPushDirectorySource.Text, TxtPushDirectoryDestination.Text)
                ? "Successfully pushed directory."
                : "Failed to push directory.");
        }

        #endregion Push Button-Click Methods

        #region Pull Button-Click Methods

        private void btnBrowsePullFile_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog pullFileDestination = new FolderBrowserDialog
            {
                SelectedPath = Directory.GetCurrentDirectory(),
                Description =
                    "Please select the destination for the file you are attempting to pull from your device...",
                ShowNewFolderButton = true
            };
            pullFileDestination.ShowDialog();
            if (DialogResult.HasValue)
                TxtPullFileDestination.Text = pullFileDestination.SelectedPath;
        }

        private void btnPullFile_Click(object sender, RoutedEventArgs e)
        {
            AddTextTt(_device.PullFile(TxtPullFileSource.Text, TxtPullFileDestination.Text)
                ? "Successfully pulled file."
                : "Failed to pull file.");
        }

        private void btnBrowsePullDirectory_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog pullDirectory = new FolderBrowserDialog
            {
                Description =
                    "Please select the destination for the directory you are attempting to pull from your device...",
                SelectedPath = Directory.GetCurrentDirectory(),
                ShowNewFolderButton = true
            };
            pullDirectory.ShowDialog();
            if (DialogResult.HasValue)
                TxtPullDirectoryDestination.Text = pullDirectory.SelectedPath;
        }

        private void btnPullDirectory_Click(object sender, RoutedEventArgs e)
        {
            AddTextTt(_device.PullDirectory(TxtPullDirectorySource.Text, TxtPullDirectoryDestination.Text)
                ? "Successfully pulled directory."
                : "Failed to pull directory.");
        }

        #endregion Pull Button-Click Methods

        #region Check ADB Commands

        private void CheckAdbCommand()
        {
            if (TxtAdbCommand.Text.Length > 0 && _selectedDevice.ConnectionStatus != DeviceState.FASTBOOT)
                BtnAdbCommand.IsEnabled = true;
            else
                BtnAdbCommand.IsEnabled = false;
        }

        private void CheckAdbShellCommand()
        {
            if (TxtAdbShellCommand.Text.Length > 0 && _selectedDevice.ConnectionStatus != DeviceState.FASTBOOT)
                BtnAdbShellCommand.IsEnabled = true;
            else
                BtnAdbShellCommand.IsEnabled = false;
        }

        private void CheckPushFile()
        {
            if (TxtPushFileSource.Text.Length > 0 && TxtPushFileDestination.Text.Length > 0 && _selectedDevice.ConnectionStatus != DeviceState.FASTBOOT)
                BtnPushFile.IsEnabled = true;
            else
                BtnPushFile.IsEnabled = false;
        }

        private void CheckPushDirectory()
        {
            if (TxtPushDirectorySource.Text.Length > 0 && TxtPushDirectoryDestination.Text.Length > 0 && _selectedDevice.ConnectionStatus != DeviceState.FASTBOOT)
                BtnPushDirectory.IsEnabled = true;
            else
                BtnPushDirectory.IsEnabled = false;
        }

        private void CheckPullFile()
        {
            if (TxtPullFileSource.Text.Length > 0 && TxtPullFileDestination.Text.Length > 0 && _selectedDevice.ConnectionStatus != DeviceState.FASTBOOT)
                BtnPullFile.IsEnabled = true;
            else
                BtnPullFile.IsEnabled = false;
        }

        private void CheckPullDirectory()
        {
            if (TxtPullDirectorySource.Text.Length > 0 && TxtPullDirectoryDestination.Text.Length > 0 && _selectedDevice.ConnectionStatus != DeviceState.FASTBOOT)
                BtnPullDirectory.IsEnabled = true;
            else
                BtnPullDirectory.IsEnabled = false;
        }

        private void CheckRemount()
        {
            if (_device.HasRoot && _selectedDevice.ConnectionStatus != DeviceState.FASTBOOT)
                BtnAdbRemount.IsEnabled = true;
            else
                BtnAdbRemount.IsEnabled = false;
        }

        private void CheckSu()
        {
            ChkSu.IsEnabled = _selectedDevice.HasRoot;
        }

        #endregion Check ADB Commands

        #region Check Fastboot Commands

        private void txtFlashRecovery_TextChanged(object sender, EventArgs e)
        {
            CheckFlashRecovery();
        }

        private void CheckFlashRecovery()
        {
            if (TxtFlashRecovery.Text.Length > 0 && _selectedDevice.ConnectionStatus == DeviceState.FASTBOOT && _selectedDevice.CanFlashRecovery) { BtnFlashRecovery.IsEnabled = true; }
            else { BtnFlashRecovery.IsEnabled = false; }
        }

        private void txtFlashCustomZip_TextChanged(object sender, EventArgs e)
        {
            CheckFlashCustomZip();
        }

        private void CheckFlashCustomZip()
        {
            if (TxtFlashCustomZip.Text.Length > 0 && _selectedDevice.ConnectionStatus == DeviceState.FASTBOOT && _selectedDevice.CanFlashRecovery) { BtnFlashCustomZip.IsEnabled = true; }
            else { BtnFlashCustomZip.IsEnabled = false; }
        }

        #endregion Check Fastboot Commands

        #region Enable/Disable Buttons

        /// <summary>
        /// Disables all ADB buttons.
        /// </summary>
        private void DisableAdb()
        {
            BtnAdbRebootSystem.IsEnabled = false;
            BtnAdbRebootBootloader.IsEnabled = false;
            BtnAdbRebootRecovery.IsEnabled = false;
            BtnAdbCommand.IsEnabled = false;
            BtnAdbShellCommand.IsEnabled = false;
            BtnAdbRemount.IsEnabled = false;
            BtnPushFile.IsEnabled = false;
            BtnPushDirectory.IsEnabled = false;
            BtnPullDirectory.IsEnabled = false;
            BtnPullFile.IsEnabled = false;
            ChkSu.IsChecked = false;
        }

        /// <summary>
        /// Disable Fastboot buttons.
        /// </summary>
        private void DisableFastboot()
        {
            BtnFastbootRebootSystem.IsEnabled = false;
            BtnFlashRecovery.IsEnabled = false;
            BtnFastbootRebootBootloader.IsEnabled = false;
            BtnFlashCustomZip.IsEnabled = false;
        }

        /// <summary>
        /// Enable ADB buttons.
        /// </summary>
        private void EnableAdb()
        {
            BtnAdbRebootSystem.IsEnabled = true;
            BtnAdbRebootBootloader.IsEnabled = true;
            BtnAdbRebootRecovery.IsEnabled = true;

            CheckAdbCommand();
            CheckAdbShellCommand();
            CheckPushFile();
            CheckPushDirectory();
            CheckPullFile();
            CheckPullDirectory();
            CheckRemount();
            CheckSu();
        }

        /// <summary>
        /// Enable Fastboot buttons.
        /// </summary>
        private void EnableFastboot()
        {
            BtnFastbootRebootSystem.IsEnabled = true;
            BtnFastbootRebootBootloader.IsEnabled = true;
            CheckCustomFastboot();
            CheckFlashCustomZip();
            CheckFlashRecovery();
        }

        /// <summary>
        /// Checks whether Custom Fastboot commands should be enabled.
        /// </summary>
        private void CheckCustomFastboot()
        {
            if (CheckSelectedDevice() && _selectedDevice.ConnectionStatus == DeviceState.FASTBOOT)
            {
                BtnFastbootCommand.IsEnabled = TxtFastbootCommand.Text.Length > 0;
            }
        }

        /// <summary>
        /// Checks whether the selected item in the lstDevices is valid.
        /// </summary>
        /// <returns>True if valid selection</returns>
        private bool CheckSelectedDevice()
        {
            if (LstDevices.SelectedIndex >= 0)
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
                _device = _android.GetConnectedDevice(serial);
                if (_device.State != DeviceState.FASTBOOT)
                {
                    AdbCommand shellCmd = Adb.FormAdbShellCommand(_device, false, "cat", "/system/build.prop | grep product.model");
                    string line = Adb.ExecuteAdbCommand(shellCmd);
                    line = line.Substring(line.IndexOf("=", StringComparison.Ordinal) + 1).Trim();
                    StringReader s = new StringReader(line);
                    modelNum = s.ReadLine();
                    success = true;
                }
                else
                {
                    FastbootCommand fbCmd = Fastboot.FormFastbootCommand($"-s {_device.SerialNumber} getvar all");
                    modelNum = GetFastbootModelNum(Fastboot.ExecuteFastbootCommand(fbCmd));
                    modelNum = modelNum.Substring(modelNum.IndexOf(": ") + 2).Trim();
                    success = true;
                }
            }
            catch (Exception ex)
            {
                AppState.DisplayNotification(ex.Message, "ADB GUI", this);
            }

            DisplayDeviceInfo(modelNum);
            return success;
        }

        /// <summary>
        /// Gets the model number for a device.
        /// </summary>
        /// <param name="parseString">String to be parsed</param>
        /// <returns>Model number</returns>
        private static string GetFastbootModelNum(string parseString)
        {
            StringReader s = new StringReader(parseString);

            while (s.Peek() != -1)
            {
                string line = s.ReadLine();

                if (line != null && line.Contains("product"))
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
            return _android.IsDeviceConnected(serial);
        }

        #endregion Device Information Methods

        #region Window-Manipulation Methods

        private async Task<Boolean> LoadTabbed()
        {
            await Task.Factory.StartNew(() => _android = AndroidController.Instance);
            UpdateDeviceList();
            return true;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void lstDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstDevices.SelectedIndex >= 0)
            {
                if (_android.IsDeviceConnected(_connectedDevices[LstDevices.SelectedIndex].SerialNumber))
                {
                    string selection = LstDevices.SelectedItem.ToString();
                    selection = selection.Substring(0, selection.IndexOf(" "));
                    if (GetModelNum(selection))
                    {
                        if (LstDevices.SelectedItem.ToString().Contains("FASTBOOT")) //disable ADB
                        {
                            DisableAdb();
                            EnableFastboot();
                        }
                        else //disable Fastboot
                        {
                            DisableFastboot();
                            EnableAdb();
                        }
                    }
                }
                else
                {
                    DisableAdb();
                    DisableFastboot();
                }
            }
            else
            {
                _selectedDevice = new ConnectedDevice();

                DataContext = _selectedDevice.DeviceInfo;
            }

            ImgDevice.Source = _selectedDevice.DeviceInfo.Image;
        }

        private void txtADBShellCommand_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtAdbShellCommand.SelectAll();
        }

        private async void windowMain_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTabbed();
            await AppState.LoadAll();
            DataContext = _selectedDevice.DeviceInfo;
        }

        #endregion Window-Manipulation Methods
    }
}