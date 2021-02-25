using BusinessLayer.FileSystem;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace MultiTool.ViewModels
{
    public class PathItemVM : INotifyPropertyChanged, IComparable, IComparable<PathItemVM>
    {
        private readonly string greenCheckMark = "\u2705";
        private readonly PathItem pathItem;
        private Brush _color;
        private string _displaySizeUnit;

        public string Path
        {
            get => pathItem.Path;
            set => pathItem.Path = value;
        }

        public long Size
        {
            get => pathItem.Size;
            set => pathItem.Size = value;
        }

        public string Name
        {
            get => pathItem.Name;
            set => pathItem.Name = value;
        }

        public FileAttributes Attributes
        {
            get => pathItem.Attributes;
            set => pathItem.Attributes = value;
        }

        public string IsHidden => pathItem.IsHidden ? greenCheckMark : string.Empty;

        public string IsSystem => pathItem.IsSystem ? greenCheckMark : string.Empty;

        public string IsReadOnly => pathItem.IsReadOnly ? greenCheckMark : string.Empty;

        public string IsEncrypted => pathItem.IsEncrypted ? greenCheckMark : string.Empty;

        public string IsCompressed => pathItem.IsCompressed ? greenCheckMark : string.Empty;

        public string IsDevice => pathItem.IsDevice ? greenCheckMark : string.Empty;

        public Brush Color
        {
            get => _color;
            set
            {
                _color = value;
                NotifyPropertyChanged();
            }
        }

        public string DisplaySize
        {
            get
            {
                int unit = 0;
                decimal currentSize = Size;

                while (currentSize > 1024 && unit != 4)
                {
                    currentSize /= 1024;
                    unit++;
                }

                string[] u = new string[] { "b", "Kb", "Mb", "Gb", "Tb" };
                DisplaySizeUnit = u[unit];

                return currentSize.ToString("F2", CultureInfo.InvariantCulture);
            }
        }

        public string DisplaySizeUnit
        {
            get => _displaySizeUnit;
            set
            {
                _displaySizeUnit = value;
                NotifyPropertyChanged();
            }
        }

        protected PathItem PathItem { get => pathItem; }

        public event PropertyChangedEventHandler PropertyChanged;

        public PathItemVM(PathItem item)
        {
            pathItem = item;
        }

        public int CompareTo(object obj)
        {
            if (obj is PathItemVM that)
            {
                return CompareTo(that);
            }
            else
            {
                return 0;
            }
        }

        public int CompareTo(PathItemVM other)
        {
            return pathItem.CompareTo(other.PathItem);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(e.PropertyName);
        }
    }
}
