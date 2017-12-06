using RegawMOD.Android;
using System.ComponentModel;

namespace ADBGUI.Classes.Devices
{
    internal class ConnectedDevice : INotifyPropertyChanged
    {
        private string _serialNumber;
        private DeviceState _connectionStatus;
        private bool _canFlashRecovery, _hasRoot;
        private SupportedDevice _deviceInfo = new SupportedDevice();

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Properties

        public string SerialNumber
        {
            get => _serialNumber;
            set { _serialNumber = value; OnPropertyChanged("SerialNumber"); }
        }

        public DeviceState ConnectionStatus
        {
            get => _connectionStatus;
            set { _connectionStatus = value; OnPropertyChanged("ConnectionStatus"); }
        }

        public bool HasRoot
        {
            get => _hasRoot;
            set { _hasRoot = value; OnPropertyChanged("HasRoot"); }
        }

        public bool CanFlashRecovery
        {
            get => _canFlashRecovery;
            set { _canFlashRecovery = value; OnPropertyChanged("CanFlashRecovery"); }
        }

        internal SupportedDevice DeviceInfo
        {
            get => _deviceInfo;
            set { _deviceInfo = value; OnPropertyChanged("DeviceInfo"); }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a default instance of ConnectedDevice.
        /// </summary>
        public ConnectedDevice()
        {
            SerialNumber = "";
            ConnectionStatus = DeviceState.UNKNOWN;
            CanFlashRecovery = false;
            HasRoot = false;
            DeviceInfo = new SupportedDevice();
        }

        /// <summary>
        /// This method creates a new instance of the ConnectedDevice class with all parameters filled.
        /// </summary>
        /// <param name="serialNumber">Serial Number</param>
        /// <param name="connectionStatus">Connection Status</param>
        /// <remarks></remarks>
        public ConnectedDevice(string serialNumber, DeviceState connectionStatus)
        {
            SerialNumber = serialNumber;
            ConnectionStatus = connectionStatus;
        }

        /// <summary>
        /// This method creates a new instance of the ConnectedDevice class with all parameters filled.
        /// </summary>
        /// <param name="serialNumber">Serial Number</param>
        /// <param name="connectionStatus">Connection Status</param>
        /// <param name="hasRoot">Root Status</param>
        /// <param name="canFlashRecovery">Can Flash Recovery</param>
        /// <param name="deviceInfo">Device Information</param>
        /// <remarks></remarks>
        public ConnectedDevice(string serialNumber, DeviceState connectionStatus, bool hasRoot, bool canFlashRecovery, SupportedDevice deviceInfo)
        {
            SerialNumber = serialNumber;
            ConnectionStatus = connectionStatus;
            CanFlashRecovery = canFlashRecovery;
            HasRoot = hasRoot;
            DeviceInfo = deviceInfo;
        }

        #endregion Constructors
    }
}