using System.Runtime.InteropServices;

namespace Multitool.NTInterop
{
    public class PowerCapabilities
    {
        private SYSTEM_POWER_CAPABILITIES sys = new SYSTEM_POWER_CAPABILITIES();

        #region DllImports
        [DllImport("powrprof.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        static extern bool GetPwrCapabilities(out SYSTEM_POWER_CAPABILITIES sysCaps);
        #endregion

        public PowerCapabilities()
        {
            GetPowerCapabilities();
        }

        #region properties
        public bool PowerButtonPresent { get; private set; }
        public bool SleepButtonPresent { get; private set; }
        public bool LidPresent { get; private set; }
        public bool S1 { get; private set; }
        public bool S2 { get; private set; }
        public bool S3 { get; private set; }
        public bool S4 { get; private set; }
        public bool S5 { get; private set; }
        public byte ProcessorMaxThrottle { get; private set; }
        public byte ProcessorMinThrottle { get; private set; }
        public bool ProcessorThrottle { get; private set; }
        public bool SystemBatteriesPresent { get; private set; }
        public bool BatteriesAreShortTerm { get; private set; }
        public BatteryReportingScale BatterieScale1 { get; private set; }
        public BatteryReportingScale BatterieScale2 { get; private set; }
        public BatteryReportingScale BatterieScale3 { get; private set; }
        public bool HibernationFilePresent { get; private set; }
        public bool FullWake { get; private set; }
        public bool VideoDimPresent { get; private set; }
        public bool ApmPresent { get; private set; }
        public bool UpsPresent { get; private set; }
        public bool ThermalControl { get; private set; }
        public bool DiskSpinDown { get; private set; }
        public SystemPowerState AcOnLineWake { get; private set; }
        public SystemPowerState SoftLidWake { get; private set; }
        public SystemPowerState RtcWake { get; private set; }
        public SystemPowerState MinDeviceWakeState { get; private set; }
        public SystemPowerState DefaultLowLatencyWake { get; private set; }
        #endregion

        public string[] CpuPowerStates()
        {
            string[] states = new string[5];
            if (S1)
            {
                states[0] = "S1 Supported";
            }
            if (S2)
            {
                states[1] = "S2 Supported";
            }
            if (S3)
            {
                states[2] = "S3 Supported";
            }
            if (S4)
            {
                states[3] = "S4 Supported";
            }
            if (S5)
            {
                states[4] = "S5 Supported";
            }
            return states;
        }

        private void GetPowerCapabilities()
        {
            if (GetPwrCapabilities(out sys))
            {
                // fill this
                PowerButtonPresent = sys.PowerButtonPresent;
                SleepButtonPresent = sys.SleepButtonPresent;
                LidPresent = sys.LidPresent;
                S1 = sys.SystemS1;
                S2 = sys.SystemS2;
                S3 = sys.SystemS3;
                S4 = sys.SystemS4;
                S5 = sys.SystemS5;
                ProcessorMaxThrottle = sys.ProcessorMaxThrottle;
                ProcessorMinThrottle = sys.ProcessorMinThrottle;
                ProcessorThrottle = sys.ProcessorThrottle;
                SystemBatteriesPresent = sys.SystemBatteriesPresent;
                BatteriesAreShortTerm = sys.BatteriesAreShortTerm;
                BatterieScale1 = new BatteryReportingScale(sys.BatteryScale[0].Granularity, sys.BatteryScale[0].Granularity);
                BatterieScale2 = new BatteryReportingScale(sys.BatteryScale[1].Granularity, sys.BatteryScale[1].Granularity);
                BatterieScale3 = new BatteryReportingScale(sys.BatteryScale[2].Granularity, sys.BatteryScale[2].Granularity);
                HibernationFilePresent = sys.HiberFilePresent;
                FullWake = sys.FullWake;
                VideoDimPresent = sys.VideoDimPresent;
                ApmPresent = sys.UpsPresent;
                UpsPresent = sys.UpsPresent;
                ThermalControl = sys.ThermalControl;
                DiskSpinDown = sys.DiskSpinDown;
                AcOnLineWake = new SystemPowerState(sys.AcOnLineWake);
                SoftLidWake = new SystemPowerState(sys.SoftLidWake);
                RtcWake = new SystemPowerState(sys.RtcWake);
                MinDeviceWakeState = new SystemPowerState(sys.MinDeviceWakeState);
                DefaultLowLatencyWake = new SystemPowerState(sys.DefaultLowLatencyWake);
            }
            else
            {
                throw InteropHelper.GetLastError(nameof(GetPwrCapabilities) + " failed", 0);
            }
        }
    }
}
