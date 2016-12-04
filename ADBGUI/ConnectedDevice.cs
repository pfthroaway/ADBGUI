using RegawMOD.Android;
using System.Drawing;

namespace ADBGUI
{
    internal class ConnectedDevice
    {
        private string _serialNumber, _modelName, _modelNumber, _manufacturerName, _splashBlock, _resolution;
        private DeviceState _connectionStatus;
        private bool _canFlashRecovery, _hasRoot;
        private Bitmap _image;

        #region Properties

        public string SerialNumber
        {
            get { return _serialNumber; }
            set { _serialNumber = value; }
        }

        public DeviceState ConnectionStatus
        {
            get { return _connectionStatus; }
            set { _connectionStatus = value; }
        }

        public string ModelName
        {
            get { return _modelName; }
            set { _modelName = value; }
        }

        public string ModelNumber
        {
            get { return _modelNumber; }
            set { _modelNumber = value; }
        }

        public string ManufacturerName
        {
            get { return _manufacturerName; }
            set { _manufacturerName = value; }
        }

        public string Resolution
        {
            get { return _resolution; }
            set { _resolution = value; }
        }

        public string SplashBlock
        {
            get { return _splashBlock; }
            set { _splashBlock = value; }
        }

        public bool HasRoot
        {
            get { return _hasRoot; }
            set { _hasRoot = value; }
        }

        public bool CanFlashRecovery
        {
            get { return _canFlashRecovery; }
            set { _canFlashRecovery = value; }
        }

        public Bitmap Image
        {
            get { return _image; }
            set { _image = value; }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a default instance of ConnectedDevice.
        /// </summary>
        public ConnectedDevice()
        {
            _serialNumber = "";
            _connectionStatus = DeviceState.UNKNOWN;
            _modelName = "";
            _modelNumber = "";
            _manufacturerName = "";
            _splashBlock = "";
            _resolution = "";
            _resolution = "";
            _canFlashRecovery = false;
            _hasRoot = false;
            _image = null;
        }

        /// <summary>
        /// This method creates a new instance of the ConnectedDevice class with two parameters.
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
        /// <param name="modelName">Model Name</param>
        /// <param name="modelNum">Model Number</param>
        /// <param name="manufacturerName">Manufacturer Name</param>
        /// <param name="splashBlock">Splash Block</param>
        /// <param name="resolution">Resolution</param>
        /// <param name="hasRoot">Root Status</param>
        /// <param name="canFlashRecovery">Can Flash Recovery</param>
        /// <param name="image">Image</param>
        /// <remarks></remarks>
        public ConnectedDevice(string serialNumber, DeviceState connectionStatus, string modelName, string modelNum, string manufacturerName, string splashBlock, string resolution, bool hasRoot, bool canFlashRecovery, Bitmap image)
        {
            SerialNumber = serialNumber;
            ConnectionStatus = connectionStatus;
            ModelName = modelName;
            ModelNumber = modelNum;
            ManufacturerName = manufacturerName;
            SplashBlock = splashBlock;
            Resolution = resolution;
            CanFlashRecovery = canFlashRecovery;
            HasRoot = hasRoot;
            Image = image;
        }

        #endregion Constructors
    }
}