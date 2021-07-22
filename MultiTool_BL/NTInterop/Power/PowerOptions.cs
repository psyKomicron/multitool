using Microsoft.Win32;

using Multitool.NTInterop.Codes;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Multitool.NTInterop.Power
{
    public class PowerOptions : WndProcService
    {
        private const int balanced = 34;
        private const int performance = 218;

        public PowerOptions() : base()
        {
            RegisterForNotifications(PowerNotifications.PowerSchemePersonality);
        }

        public event PowerPlanChangedEventHandler PowerPlanChanged;
        internal static event PowerPlanChangedEventHandler ActiveChanged;

        /// <summary>
        /// Get this computer current power plan.
        /// </summary>
        public PowerPlan GetActivePowerPlan()
        {
            IntPtr guid = IntPtr.Zero;
            uint returnCode = PowerGetActiveScheme(IntPtr.Zero, ref guid);
            if (returnCode != (uint)SystemCodes.ERROR_SUCCESS)
            {
                throw InteropHelper.GetLastError("PowerGetActiveScheme call failed", returnCode);
            }
            string name = ReadFriendlyName(guid);
            if (name == string.Empty)
            {
                throw InteropHelper.GetLastError("PowerGetActiveScheme call failed. Name buffer was empty.", returnCode);
            }
            else
            {
                return new PowerPlan(Marshal.PtrToStructure<Guid>(guid), name, true);
            }
        }

        /// <summary>
        /// Get this computer available power plans.
        /// </summary>
        /// <returns><see cref="List{PowerPlan}"/> of power plans names.</returns>
        public PowerPlan[] EnumeratePowerPlans()
        {
            List<Guid> guids = ListPowerPlans();
            PowerPlan[] powerPlans = new PowerPlan[guids.Count];
            PowerPlan current = GetActivePowerPlan();
            Guid guid;
            for (int i = 0; i < guids.Count; i++)
            {
                guid = guids[i];
                PowerPlan plan = new PowerPlan(guid, ReadFriendlyName(ref guid), guid == current.Guid);
                powerPlans[i] = plan;
            }
            return powerPlans;
        }

        public void SetActivePowerPlan(PowerPlan powerPlan)
        {
            Guid guid = powerPlan.Guid;
            uint retCode = PowerSetActiveScheme(IntPtr.Zero, ref guid);

            ActiveChanged?.Invoke(powerPlan);
            if (retCode != (uint)SystemCodes.ERROR_SUCCESS)
            {
                throw InteropHelper.GetLastError("PowerSetActiveScheme call failed", retCode);
            }
            else
            {
                PowerPlanChanged?.BeginInvoke(powerPlan, null, null);
            }
        }

        #region private
        private static List<Guid> ListPowerPlans()
        {
            List<Guid> guids = new List<Guid>(3);
            IntPtr buffer;
            uint index = 0;
            uint returnCode = 0;
            uint bufferSize = 16;

            while (returnCode == 0)
            {
                buffer = Marshal.AllocHGlobal((int)bufferSize);
                try
                {
                    returnCode = PowerEnumerate(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, (uint)AccessFlags.ACCESS_SCHEME,
                                                index, buffer, ref bufferSize);
                    if (returnCode == 259)
                    {
                        break;
                    }
                    else if (returnCode != 0)
                    {
                        throw InteropHelper.GetLastError("Error while listing power schemes.", returnCode);
                    }
                    else
                    {
                        Guid guid = Marshal.PtrToStructure<Guid>(buffer);
                        if (guid != null)
                        {
                            guids.Add(guid);
                        }
                    }
                    index++;
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }

            return guids;
        }

        private static string ReadFriendlyName(IntPtr schemeGuid)
        {
            uint bufferSize = 0;
            uint returnCode = PowerReadFriendlyName(IntPtr.Zero, schemeGuid, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref bufferSize);
            if (returnCode == 0)
            {
                if (bufferSize == 0)
                {
                    return string.Empty;
                }

                IntPtr namePtr = Marshal.AllocHGlobal((int)bufferSize);
                try
                {
                    returnCode = PowerReadFriendlyName(IntPtr.Zero, schemeGuid, IntPtr.Zero, IntPtr.Zero, namePtr, ref bufferSize);
                    if (returnCode == 0)
                    {
                        string name = Marshal.PtrToStringUni(namePtr);
                        return name;
                    }
                    else
                    {
                        throw InteropHelper.GetLastError("Error getting power scheme friendly name.", returnCode);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(namePtr);
                }
            }
            else
            {
                throw InteropHelper.GetLastError("Error getting name buffer size", returnCode);
            }
        }

        private static string ReadFriendlyName(ref Guid guid)
        {
            uint bufferSize = 0;
            uint returnCode = PowerReadFriendlyName(IntPtr.Zero, ref guid, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref bufferSize);

            if (returnCode == 0)
            {
                if (bufferSize == 0)
                {
                    return string.Empty;
                }

                IntPtr namePtr = Marshal.AllocHGlobal((int)bufferSize);
                try
                {
                    returnCode = PowerReadFriendlyName(IntPtr.Zero, ref guid, IntPtr.Zero, IntPtr.Zero, namePtr, ref bufferSize);

                    if (returnCode == 0)
                    {
                        string name = Marshal.PtrToStringUni(namePtr);
                        return name;
                    }
                    else
                    {
                        throw InteropHelper.GetLastError("Error getting power scheme friendly name.", returnCode);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(namePtr);
                }
            }
            else
            {
                throw InteropHelper.GetLastError("Error getting name buffer size", returnCode);
            }
        }
        #endregion

        #region events
        protected override void OnWndProcCalled(object sender, Message e)
        {
#if DEBUG
            Console.WriteLine("msg -> " + e.Msg);
            Console.WriteLine("wParam -> " + e.WParam.ToInt32());
#endif
            if (e.Msg == PowerSettings.WM_POWERBROADCAST && e.WParam.ToInt32() == (int)PowerBroadcastEvent.PowerSettingChange)
            {
                POWERBROADCAST_SETTING pwrSetting = Marshal.PtrToStructure<POWERBROADCAST_SETTING>(e.LParam);
                //IntPtr pData = (IntPtr)(e.LParam.ToInt64() + Marshal.SizeOf(pwrSetting));

                if (pwrSetting.PowerSetting == PowerSettings.GUID_POWERSCHEME_PERSONALITY && pwrSetting.DataLength == Marshal.SizeOf(typeof(Guid)))
                {
#if DEBUG
                    switch (pwrSetting.Data)
                    {
                        case balanced:
                            Console.WriteLine("Power mode changed to balanced");
                            break;
                        case performance:
                            Console.WriteLine("Power mode changed to performance/high performance");
                            break;
                        default:
                            break;
                    }
#endif
                    //PowerPlanChanged?.BeginInvoke();
                }
            }
        }
        #endregion

        #region dllimports
        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerEnumerate(
            IntPtr RootPowerKey,
            IntPtr SchemeGuid,
            IntPtr SubGroupOfPowerSettingsGuid,
            uint AccessFlags,
            uint Index,
            IntPtr Buffer,
            ref uint BufferSize
        );

        [DllImport("powrprof.dll")]
        private static extern uint PowerReadFriendlyName(
            IntPtr RootPowerKey,
            IntPtr SchemeGuid,
            IntPtr SubGroupOfPowerSettingGuid,
            IntPtr PowerSettingGuid,
            IntPtr Buffer,
            ref uint BufferSize
        );

        [DllImport("powrprof.dll")]
        private static extern uint PowerReadFriendlyName(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            IntPtr SubGroupOfPowerSettingGuid,
            IntPtr PowerSettingGuid,
            IntPtr Buffer,
            ref uint BufferSize
        );

        [DllImport("powrprof.dll")]
        private static extern uint PowerGetActiveScheme(
            IntPtr UserRootPowerKey,
            ref IntPtr ActivePolicyGuid
        );

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerSetActiveScheme(
            IntPtr UserRootPowerKey,
            ref Guid SchemeGuid
        );

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(
            IntPtr hwnd,
            int message,
            IntPtr wParam, 
            IntPtr lParam
        );
        #endregion
    }

    // This structure is sent when the PBT_POWERSETTINGSCHANGE message is sent.
    // It describes the power setting that has changed and contains data about the change
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct POWERBROADCAST_SETTING
    {
        public Guid PowerSetting;
        public uint DataLength;
        public byte Data;
    }
}
