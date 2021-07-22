using Multitool.NTInterop;

using MultitoolWPF.Tools;
using MultitoolWPF.ViewModels;

using System.Windows.Controls;

namespace MultitoolWPF.Windows
{
    /// <summary>
    /// Interaction logic for ControlPanelsWindow.xaml
    /// </summary>
    public partial class PowerCapabilitiesPanelControl : UserControl, ISerializableWindow
    {
        public PowerCapabilitiesPanelControl()
        {
            InitializeComponent();

            PowerCapabilities capabilities = new PowerCapabilities();

            // Buttons
            _ = PowerCapabilities_ListView.Items.Add("Power button present : " + (capabilities.PowerButtonPresent ? "Yes" : "No"));
            _ = PowerCapabilities_ListView.Items.Add("Sleep button present : " + (capabilities.SleepButtonPresent ? "Yes" : "No"));
            _ = PowerCapabilities_ListView.Items.Add("Lid present : " + (capabilities.LidPresent ? "Yes" : "No"));

            // CPU
            _ = Processor_ListView.Items.Add("Processor throttle : " + capabilities.ProcessorThrottle);
            _ = Processor_ListView.Items.Add("Processor minimum throttle : " + capabilities.ProcessorMinThrottle);
            _ = Processor_ListView.Items.Add("Processor maximum throttle : " + capabilities.ProcessorMaxThrottle);
            // States
            string[] states = capabilities.CpuPowerStates();
            for (int i = 0; i < states.Length; i++)
            {
                S_ListView.Items.Add(states[i]);
            }

            // Power
            _ = Batteries_ListView.Items.Add("Batteries present : " + (capabilities.SystemBatteriesPresent ? "Yes" : "No"));
            _ = Batteries_ListView.Items.Add("Batteries short term : " + (capabilities.BatteriesAreShortTerm ? "Yes" : "No"));
            _ = Batteries_ListView.Items.Add("Battery 1 : " + capabilities.BatterieScale1);
            _ = Batteries_ListView.Items.Add("Battery 2 : " + capabilities.BatterieScale2);
            _ = Batteries_ListView.Items.Add("Battery 3 : " + capabilities.BatterieScale3);

            // Wake states
            _ = Wake_ListView.Items.Add("AC line connected wake : " + capabilities.AcOnLineWake.Name);
            _ = Wake_ListView.Items.Add("Soft lid wake : " + capabilities.SoftLidWake.Name);
            _ = Wake_ListView.Items.Add("RTC wake : " + capabilities.RtcWake.Name);
            _ = Wake_ListView.Items.Add("Minimum wake : " + capabilities.MinDeviceWakeState.Name);
            _ = Wake_ListView.Items.Add("Low latency wake : " + capabilities.DefaultLowLatencyWake.Name);


            // Others
            _ = Others_ListView.Items.Add("Hibernation file present : " + (capabilities.HibernationFilePresent ? "Yes" : "No"));
            _ = Others_ListView.Items.Add("Full wake available : " + (capabilities.FullWake ? "Yes" : "No"));
            _ = Others_ListView.Items.Add("Video dim available : " + (capabilities.VideoDimPresent ? "Yes" : "No"));
            _ = Others_ListView.Items.Add("APM (Advanced power management) available : " + (capabilities.ApmPresent ? "Yes" : "No"));
            _ = Others_ListView.Items.Add("UPS (Uninterruptible Power Supply) present : " + (capabilities.UpsPresent ? "Yes" : "No"));
            _ = Others_ListView.Items.Add("Thermal control available : " + (capabilities.ThermalControl ? "Yes" : "No"));
            _ = Others_ListView.Items.Add("Disk spin down enabled : " + (capabilities.DiskSpinDown ? "Yes" : "No"));
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
