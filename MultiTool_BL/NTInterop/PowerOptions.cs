using Multitool.NTInterop.Codes;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Multitool.NTInterop
{
    public class PowerOptions
    {
        #region dllimports
        [DllImport("powrprof.dll", SetLastError = true)]
        public static extern uint PowerEnumerate(
            IntPtr RootPowerKey,
            IntPtr SchemeGuid,
            IntPtr SubGroupOfPowerSettingsGuid,
            uint AccessFlags,
            uint Index,
            IntPtr Buffer,
            ref uint BufferSize
        );

        [DllImport("PowrProf.dll")]
        public static extern uint PowerReadFriendlyName(
            IntPtr RootPowerKey,
            IntPtr SchemeGuid,
            IntPtr SubGroupOfPowerSettingGuid,
            IntPtr PowerSettingGuid,
            IntPtr Buffer,
            ref uint BufferSize
        );

        [DllImport("powrprof.dll")]
        public static extern uint PowerReadFriendlyName(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            IntPtr SubGroupOfPowerSettingGuid,
            IntPtr PowerSettingGuid,
            IntPtr Buffer,
            ref uint BufferSize
        );

        [DllImport("PowrProf.dll")]
        public static extern uint PowerGetActiveScheme(
            IntPtr UserRootPowerKey,
            ref IntPtr ActivePolicyGuid
        );

        [DllImport("powrprof.dll", SetLastError = true)]
        public static extern uint PowerSetActiveScheme(
            IntPtr UserRootPowerKey,
            ref Guid SchemeGuid
        );
        #endregion

        /// <summary>
        /// Get this computer current power plan.
        /// </summary>
        public PowerPlan GetActivePowerPlan()
        {
            IntPtr guid = IntPtr.Zero;
            uint returnCode = PowerGetActiveScheme(IntPtr.Zero, ref guid);

            if (returnCode != (uint)SystemCodes.ERROR_SUCCESS)
            {
                throw ExceptionThrower.GetLastError("PowerGetActiveScheme call failed", returnCode);
            }

            string name = ReadFriendlyName(guid);

            if (name == string.Empty)
            {
                throw ExceptionThrower.GetLastError("PowerGetActiveScheme call failed. Name buffer was empty.", returnCode);
            }
            else
            {

                return new PowerPlan(Marshal.PtrToStructure<Guid>(guid), name, true);
            }
        }

        /// <summary>
        /// Get this computer available power plans.
        /// </summary>
        /// <returns><see cref="List{string}"/> of power plans names.</returns>
        public List<PowerPlan> EnumeratePowerPlans()
        {
            List<PowerPlan> guidsNames = new List<PowerPlan>();
            List<Guid> guids = ListPowerPlans();
            PowerPlan current = GetActivePowerPlan();

            for (int i = 0; i < guids.Count; i++)
            {
                Guid guid = guids[i];
                guidsNames.Add(new PowerPlan(guid, ReadFriendlyName(ref guid), guid == current.Guid));
            }

            return guidsNames;
        }

        public void SwitchPowerPlan(PowerPlan plan)
        {
            Guid powerPlanGuid = plan.Guid;

            if (powerPlanGuid != Guid.Empty)
            {
                uint retCode = PowerSetActiveScheme(IntPtr.Zero, ref powerPlanGuid);
                if (retCode != (uint)SystemCodes.ERROR_SUCCESS)
                {
                    throw ExceptionThrower.GetLastError("PowerSetActiveScheme call failed", retCode);
                }
            }
        }

        private List<Guid> ListPowerPlans()
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
                        throw ExceptionThrower.GetLastError("Error while listing power schemes.", returnCode);
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

        private string ReadFriendlyName(IntPtr schemeGuid)
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
                        throw ExceptionThrower.GetLastError("Error getting power scheme friendly name.", returnCode);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(namePtr);
                }
            }
            else
            {
                throw ExceptionThrower.GetLastError("Error getting name buffer size", returnCode);
            }
        }

        private string ReadFriendlyName(ref Guid guid)
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
                        throw ExceptionThrower.GetLastError("Error getting power scheme friendly name.", returnCode);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(namePtr);
                }
            }
            else
            {
                throw ExceptionThrower.GetLastError("Error getting name buffer size", returnCode);
            }
        }
    }
}
