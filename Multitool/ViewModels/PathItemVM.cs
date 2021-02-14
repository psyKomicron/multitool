using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MultiTool.ViewModels
{
    public class PathItemVM : INotifyPropertyChanged, IComparable, IComparable<PathItemVM>
    {
        private string _path;
        private Brush _color;
        private long _size;
        private string _name;
        private string _displaySizeUnit;

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                NotifyPropertyChanged();
            }
        }

        public Brush Color
        {
            get => _color;
            set
            {
                _color = value;
                NotifyPropertyChanged();
            }
        }

        public long Size
        {
            get => _size;
            set
            {
                _size = value;
                NotifyPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

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
            if (other.Size > Size)
            {
                return 1;
            }
            else if (other.Size < Size)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
