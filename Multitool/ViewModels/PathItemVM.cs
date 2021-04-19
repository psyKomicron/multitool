﻿using BusinessLayer.FileSystem;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace MultiTool.ViewModels
{
    public class PathItemVM : IFileSystemEntry
    {
        private readonly string greenCheckMark = "\u2705";
        private readonly IFileSystemEntry pathItem;
        private Brush _color;
        private string _displaySizeUnit;

        public event PropertyChangedEventHandler PropertyChanged;

        protected IFileSystemEntry PathItem { get => pathItem; }

        #region PathItem
        public string Path => pathItem.Path;
        public long Size => pathItem.Size;
        public string Name => pathItem.Name;
        public FileAttributes Attributes => pathItem.Attributes;
        public bool IsHidden => pathItem.IsHidden;
        public bool IsSystem => pathItem.IsSystem;
        public bool IsReadOnly => pathItem.IsReadOnly;
        public bool IsEncrypted => pathItem.IsEncrypted;
        public bool IsCompressed => pathItem.IsCompressed;
        public bool IsDevice => pathItem.IsDevice;
        public bool IsDirectory => pathItem.IsDirectory;
        #endregion

        #region decorator
        public string IsHiddenCM => pathItem.IsHidden ? greenCheckMark : string.Empty;
        public string IsSystemCM => pathItem.IsSystem ? greenCheckMark : string.Empty;
        public string IsReadOnlyCM => pathItem.IsReadOnly ? greenCheckMark : string.Empty;
        public string IsEncryptedCM => pathItem.IsEncrypted ? greenCheckMark : string.Empty;
        public string IsCompressedCM => pathItem.IsCompressed ? greenCheckMark : string.Empty;
        public string IsDeviceCM => pathItem.IsDevice ? greenCheckMark : string.Empty;

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
        #endregion

        public PathItemVM(IFileSystemEntry item)
        {
            pathItem = item;
            item.PropertyChanged += OnItemPropertyChanged;
        }

        public int CompareTo(IFileSystemEntry other)
        {
            return pathItem.CompareTo(other);
        }

        public int CompareTo(object obj)
        {
            return pathItem.CompareTo(obj);
        }

        public bool Equals(IFileSystemEntry other)
        {
            return pathItem.Equals(other);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(e.PropertyName);
        }
    }
}
