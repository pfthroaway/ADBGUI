using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace ADBGUI.Classes.Devices
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
            get => _modelName;
            set { _modelName = value; OnPropertyChanged("ModelName"); }
        }

        public List<string> Codenames
        {
            get => _codenames;
            set { _codenames = value; OnPropertyChanged("Codenames"); }
        }

        public string ModelNumber
        {
            get => _modelNumber;
            set { _modelNumber = value; OnPropertyChanged("ModelNumber"); }
        }

        public string ManufacturerName
        {
            get => _manufacturerName;
            set { _manufacturerName = value; OnPropertyChanged("ManufacturerName"); }
        }

        public string Resolution
        {
            get => _resolution;
            set { _resolution = value; OnPropertyChanged("Resolution"); }
        }

        public string SplashBlock
        {
            get => _splashBlock;
            set { _splashBlock = value; OnPropertyChanged("SplashBlock"); }
        }

        public string RAM
        {
            get => _ram;
            set { _ram = value; OnPropertyChanged("RAM"); }
        }

        public string ImageLocation
        {
            get => _imageLocation;
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
            get => _image;
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
        /// <param name="ram">RAM</param>
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