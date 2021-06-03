﻿using Multitool.Reflection.ObjectFlatteners;

using MultitoolWPF.ViewModels;

using System.Collections.ObjectModel;

namespace MultitoolWPF.Windows
{
    public class SpreadsheetWindowData : DefaultWindowData
    {
        [ListFlattener(nameof(Items), typeof(CommonXmlObjectFlattener))]
        public ObservableCollection<SpreadsheetVM> Items { get; set; }

        public SpreadsheetWindowData()
        {
            Items = new ObservableCollection<SpreadsheetVM>();
        }
    }
}