using Multitool.NTInterop.Power;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Multitool.NTInterop
{
    public abstract class WndProcService : IDisposable
    {
        private DummyWindow window = new DummyWindow();
        private IntPtr hwnd;
        private List<IntPtr> registerHandles = new List<IntPtr>();

        public WndProcService()
        {
            window.WndProcCalled += OnWndProcCalled;
            hwnd = window.Handle;
        }

        public void Dispose()
        {
            for (int i = 0; i < registerHandles.Count; i++)
            {
                UnregisterForNotifications(registerHandles[i]);
            }
            window.ReleaseHandle();
        }

        public void Register(IntPtr hwnd, PowerNotifications notification)
        {
            Guid powerSettingGuid;
            switch (notification)
            {
                case PowerNotifications.BatteryPercentageRemaining:
                    powerSettingGuid = PowerSettings.GUID_BATTERY_PERCENTAGE_REMAINING;
                    break;
                case PowerNotifications.MonitorPowerOn:
                    powerSettingGuid = PowerSettings.GUID_MONITOR_POWER_ON;
                    break;
                case PowerNotifications.ACDCPowerSource:
                    powerSettingGuid = PowerSettings.GUID_ACDC_POWER_SOURCE;
                    break;
                case PowerNotifications.PowerSchemePersonality:
                    powerSettingGuid = PowerSettings.GUID_POWERSCHEME_PERSONALITY;
                    break;
                case PowerNotifications.MaxPowerSavings:
                    powerSettingGuid = PowerSettings.GUID_MAX_POWER_SAVINGS;
                    break;
                case PowerNotifications.MinPowerSavings:
                    powerSettingGuid = PowerSettings.GUID_MIN_POWER_SAVINGS;
                    break;
                case PowerNotifications.TypicalPowerSavings:
                    powerSettingGuid = PowerSettings.GUID_TYPICAL_POWER_SAVINGS;
                    break;
                default:
                    throw new ArgumentException("Notification not recognized", nameof(notification));
            }

            IntPtr registerHandle = RegisterPowerSettingsNotification(hwnd, ref powerSettingGuid, 0);
            if (registerHandle != IntPtr.Zero)
            {
                registerHandles.Add(registerHandle);
            }
            else
            {
                throw InteropHelper.GetLastError("RegisterPowerSettingsNotification failed");
            }
        }

        protected abstract void OnWndProcCalled(object sender, System.Windows.Forms.Message e);

        protected void RegisterForNotifications(PowerNotifications notifications)
        {
            Guid powerSettingGuid;
            switch (notifications)
            {
                case PowerNotifications.BatteryPercentageRemaining:
                    powerSettingGuid = PowerSettings.GUID_BATTERY_PERCENTAGE_REMAINING;
                    break;
                case PowerNotifications.MonitorPowerOn:
                    powerSettingGuid = PowerSettings.GUID_MONITOR_POWER_ON;
                    break;
                case PowerNotifications.ACDCPowerSource:
                    powerSettingGuid = PowerSettings.GUID_ACDC_POWER_SOURCE;
                    break;
                case PowerNotifications.PowerSchemePersonality:
                    powerSettingGuid = PowerSettings.GUID_POWERSCHEME_PERSONALITY;
                    break;
                case PowerNotifications.MaxPowerSavings:
                    powerSettingGuid = PowerSettings.GUID_MAX_POWER_SAVINGS;
                    break;
                case PowerNotifications.MinPowerSavings:
                    powerSettingGuid = PowerSettings.GUID_MIN_POWER_SAVINGS;
                    break;
                case PowerNotifications.TypicalPowerSavings:
                    powerSettingGuid = PowerSettings.GUID_TYPICAL_POWER_SAVINGS;
                    break;
                default:
                    throw new ArgumentException("Notification not recognized", nameof(notifications));
            }

            IntPtr registerHandle = RegisterPowerSettingsNotification(hwnd, ref powerSettingGuid, 0);
            if (registerHandle != IntPtr.Zero)
            {
                registerHandles.Add(registerHandle);
            }
            else
            {
                throw InteropHelper.GetLastError("RegisterPowerSettingsNotification failed");
            }
        }


        protected void UnregisterForNotifications(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                UnregisterPowerSettingsNotification(handle);
            }
            else
            {
                throw new ArgumentNullException("Handle is null", nameof(handle));
            }
        }

        #region DllImport
        [DllImport(@"User32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall, EntryPoint = "RegisterPowerSettingNotification")]
        static extern IntPtr RegisterPowerSettingsNotification(
            IntPtr hRecipient,
            ref Guid PowerSettingGuid,
            Int32 Flags
        );

        [DllImport(@"User32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall, EntryPoint = "UnregisterPowerSettingsNotification")]
        static extern bool UnregisterPowerSettingsNotification(
            IntPtr handle
        );
        #endregion
    }
}
