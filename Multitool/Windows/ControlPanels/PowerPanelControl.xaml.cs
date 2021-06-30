using Multitool.NTInterop;

using MultitoolWPF.Tools;
using MultitoolWPF.ViewModels;

using System.Windows;
using System.Windows.Controls;

namespace MultitoolWPF.Windows
{
    /// <summary>
    /// Interaction logic for ControlPanelsWindow.xaml
    /// </summary>
    public partial class PowerPanelControl : UserControl, ISerializableWindow
    {
        public PowerPanelControl()
        {
            InitializeComponent();

            PowerCapabilities capabilities = new PowerCapabilities();

            // Buttons
            PowerCapabilities_ListView.Items.Add("Power button present : " + capabilities.PowerButtonPresent);
            PowerCapabilities_ListView.Items.Add("Sleep button present : " + capabilities.SleepButtonPresent);
            PowerCapabilities_ListView.Items.Add("Lid present : " + capabilities.LidPresent);

            // CPU
            Processor_ListView.Items.Add("Processor throttle : " + capabilities.ProcessorThrottle);
            Processor_ListView.Items.Add("Processor minimum throttle : " + capabilities.ProcessorMinThrottle);
            Processor_ListView.Items.Add("Processor maximum throttle : " + capabilities.ProcessorMaxThrottle);
            // States
            string[] states = capabilities.CpuPowerStates();
            for (int i = 0; i < states.Length; i++)
            {
                S_ListView.Items.Add(states[i]);
            }

            // Power
            Batteries_ListView.Items.Add("Batteries present : " + (capabilities.SystemBatteriesPresent ? "Yes" : "No"));
            Batteries_ListView.Items.Add("Batteries short term : " + (capabilities.BatteriesAreShortTerm ? "Yes" : "No"));
            Batteries_ListView.Items.Add("Battery 1 : " + capabilities.BatterieScale1);
            Batteries_ListView.Items.Add("Battery 2 : " + capabilities.BatterieScale2);
            Batteries_ListView.Items.Add("Battery 3 : " + capabilities.BatterieScale3);

            // Wake states
            Wake_ListView.Items.Add("AC line connected wake : " + capabilities.AcOnLineWake.Name);
            Wake_ListView.Items.Add("Soft lid wake : " + capabilities.SoftLidWake.Name);
            Wake_ListView.Items.Add("RTC wake : " + capabilities.RtcWake.Name);
            Wake_ListView.Items.Add("Minimum wake : " + capabilities.MinDeviceWakeState.Name);
            Wake_ListView.Items.Add("Low latency wake : " + capabilities.DefaultLowLatencyWake.Name);


            // Others
            Others_ListView.Items.Add("Hibernation file present : " + (capabilities.HibernationFilePresent ? "Yes" : "No"));
            Others_ListView.Items.Add("Full wake available : " + (capabilities.FullWake ? "Yes" : "No"));
            Others_ListView.Items.Add("Video dim available : " + (capabilities.VideoDimPresent ? "Yes" : "No"));
            Others_ListView.Items.Add("APM (Advanced power management) available : " + (capabilities.ApmPresent ? "Yes" : "No"));
            Others_ListView.Items.Add("UPS (Uninterruptible Power Supply) present : " + (capabilities.UpsPresent ? "Yes" : "No"));
            Others_ListView.Items.Add("Thermal control available : " + (capabilities.ThermalControl ? "Yes" : "No"));
            Others_ListView.Items.Add("Disk spin down enabled : " + (capabilities.DiskSpinDown ? "Yes" : "No"));
        }

        public DefaultWindowData Data { get; set; }

        public void Deserialize()
        {
            Data = WindowManager.PreferenceManager.GetWindowData<DefaultWindowData>(Name);
        }

        public void Serialize()
        {
            WindowManager.PreferenceManager.AddWindowData(Data, Name);
        }
    }
}
