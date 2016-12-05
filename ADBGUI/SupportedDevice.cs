using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace ADBGUI
{
    internal class SupportedDevice : INotifyPropertyChanged
    {
        private string _modelName, _modelNumber, _manufacturerName, _splashBlock, _resolution, _imageLocation, _ram;
        private BitmapImage _image;
        private List<string> _codenames = new List<string>();

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Properties

        public string ModelName
        {
            get { return _modelName; }
            set { _modelName = value; OnPropertyChanged("ModelName"); }
        }

        public List<string> Codenames
        {
            get { return _codenames; }
            set { _codenames = value; OnPropertyChanged("Codenames"); }
        }

        public string ModelNumber
        {
            get { return _modelNumber; }
            set { _modelNumber = value; OnPropertyChanged("ModelNumber"); }
        }

        public string ManufacturerName
        {
            get { return _manufacturerName; }
            set { _manufacturerName = value; OnPropertyChanged("ManufacturerName"); }
        }

        public string Resolution
        {
            get { return _resolution; }
            set { _resolution = value; OnPropertyChanged("Resolution"); }
        }

        public string SplashBlock
        {
            get { return _splashBlock; }
            set { _splashBlock = value; OnPropertyChanged("SplashBlock"); }
        }

        public string RAM
        {
            get { return _ram; }
            set { _ram = value; OnPropertyChanged("RAM"); }
        }

        public string ImageLocation
        {
            get { return _imageLocation; }
            set
            {
                _imageLocation = value;
                BitmapImage newImage = new BitmapImage();
                newImage.BeginInit();
                newImage.UriSource = new Uri(_imageLocation, UriKind.Relative);
                newImage.CacheOption = BitmapCacheOption.OnLoad;
                newImage.EndInit();

                Image = newImage;
                OnPropertyChanged("ImageLocation"); OnPropertyChanged("Image");
            }
        }

        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; OnPropertyChanged("Image"); }
        }

        #endregion Properties

        public override string ToString()
        {
            return ModelName;
        }

        #region Constructors

        /// <summary>
        /// Initializes a default instance of ConnectedDevice.
        /// </summary>
        public SupportedDevice()
        {
            _modelName = "";
            _modelNumber = "";
            _manufacturerName = "";
            _splashBlock = "";
            _resolution = "";
            _resolution = "";
            _imageLocation = "Resources\\clear.png";
        }

        /// <summary>
        /// This method creates a new instance of the ConnectedDevice class with all parameters filled.
        /// </summary>
        /// <param name="modelName">Model Name</param>
        /// <param name="codenames">Codenames</param>
        /// <param name="modelNumber">Model Number</param>
        /// <param name="manufacturerName">Manufacturer Name</param>
        /// <param name="splashBlock">Splash Block</param>
        /// <param name="resolution">Resolution</param>
        /// <param name="imageLocation">Image</param>
        /// <remarks></remarks>
        public SupportedDevice(string modelName, List<string> codenames, string modelNumber, string manufacturerName, string splashBlock, string resolution, string ram, string imageLocation)
        {
            ModelName = modelName;
            Codenames = codenames;
            ModelNumber = modelNumber;
            ManufacturerName = manufacturerName;
            SplashBlock = splashBlock;
            Resolution = resolution;
            RAM = ram;
            ImageLocation = imageLocation;
        }

        #endregion Constructors
    }
}