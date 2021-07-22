using Multitool.NTInterop;
using Multitool.NTInterop.Power;

using MultitoolWPF.UserControls;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace MultitoolWPF.Windows.ControlPanels
{
    /// <summary>
    /// Interaction logic for PowerPlansPanel.xaml
    /// </summary>
    public partial class PowerPlansPanel : UserControl
    {
        private bool _loaded;
        PowerOptions pwrOptions = new PowerOptions();

        public PowerPlansPanel()
        {
            InitializeComponent();
            pwrOptions.PowerPlanChanged += PwrOptions_PowerPlanChanged;
        }

        private void PwrOptions_PowerPlanChanged(PowerPlan plan)
        {
            //throw new System.NotImplementedException();
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            if (_loaded)
            {
                return;
            }

            pwrOptions.Register(new WindowInteropHelper(Application.Current.MainWindow).Handle, PowerNotifications.PowerSchemePersonality);

            InitializeControl();
            _loaded = true;
        }

        private void InitializeControl()
        {
            PowerPlan[] powerPlans = pwrOptions.EnumeratePowerPlans();
            int rows, column;
            column = powerPlans.Length > 1 ? 2 : 1;
            rows = (powerPlans.Length / 2) + (powerPlans.Length % 2);

            for (int i = 0; i < column; i++)
            {
                PowerPlans_Grid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(410)
                });
            }
            for (int i = 0; i < rows; i++)
            {
                PowerPlans_Grid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(250)
                });
            }

            for (int i = 0; i < powerPlans.Length; i++)
            {
                PowerPlanView view = new PowerPlanView(powerPlans[i])
                {
                    Margin = new Thickness(0, 0, 0, 30),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                view.MouseDown += PowerViewControl_MouseDown;
                /*view.Height = view.MinHeight;
                view.Width = view.MinWidth;*/

                Grid.SetRow(view, i / 2);
                Grid.SetColumn(view, i % 2 == 0 ? 0 : 1);
                PowerPlans_Grid.Children.Add(view);
            }
        }

        private void PowerViewControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is PowerPlanView view)
            {
                pwrOptions.SetActivePowerPlan(view.PowerPlan);
            }
        }
    }
}
