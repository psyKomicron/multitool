using System.Runtime.InteropServices;

namespace Multitool.NTInterop
{
    struct SYSTEM_POWER_CAPABILITIES
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool PowerButtonPresent;

        [MarshalAs(UnmanagedType.U1)]
        public bool SleepButtonPresent;

        [MarshalAs(UnmanagedType.U1)]
        public bool LidPresent;

        [MarshalAs(UnmanagedType.U1)]
        public bool SystemS1;

        [MarshalAs(UnmanagedType.U1)]
        public bool SystemS2;

        [MarshalAs(UnmanagedType.U1)]
        public bool SystemS3;

        [MarshalAs(UnmanagedType.U1)]
        public bool SystemS4;

        [MarshalAs(UnmanagedType.U1)]
        public bool SystemS5;

        [MarshalAs(UnmanagedType.U1)]
        public bool HiberFilePresent;

        [MarshalAs(UnmanagedType.U1)]
        public bool FullWake;

        [MarshalAs(UnmanagedType.U1)]
        public bool VideoDimPresent;

        [MarshalAs(UnmanagedType.U1)]
        public bool ApmPresent;

        [MarshalAs(UnmanagedType.U1)]
        public bool UpsPresent;

        [MarshalAs(UnmanagedType.U1)]
        public bool ThermalControl;

        [MarshalAs(UnmanagedType.U1)]
        public bool ProcessorThrottle;

        public byte ProcessorMinThrottle;

        /// <summary>
        /// Also known as ProcessorThrottleScale before Windows XP.
        /// </summary>
        public byte ProcessorMaxThrottle;

        [MarshalAs(UnmanagedType.U1)]
        public bool DiskSpinDown;

        /// <summary>
        /// Ignore if earlier than Windows 10 (10.0.10240.0).
        /// </summary>
        public byte HiberFileType;

        /// <summary>
        /// Ignore if earlier than Windows 10 (10.0.10240.0).
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool AoAcConnectivitySupported;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        private readonly byte[] spare3;

        [MarshalAs(UnmanagedType.U1)]
        public bool SystemBatteriesPresent;

        [MarshalAs(UnmanagedType.U1)]
        public bool BatteriesAreShortTerm;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public BATTERY_REPORTING_SCALE[] BatteryScale;

        public SYSTEM_POWER_STATE AcOnLineWake;

        public SYSTEM_POWER_STATE SoftLidWake;

        public SYSTEM_POWER_STATE RtcWake;

        public SYSTEM_POWER_STATE MinDeviceWakeState;

        public SYSTEM_POWER_STATE DefaultLowLatencyWake;
    }
}
