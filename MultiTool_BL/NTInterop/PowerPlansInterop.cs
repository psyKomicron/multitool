using MultiToolBusinessLayer.NTInterop.Codes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MultiToolBusinessLayer.NTInterop
{
    public class PowerPlansInterop
    {
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

        /// <summary>
        /// Get this computer current power plan.
        /// </summary>
        public string GetCurrentPowerPlan()
        {
            string name = ReadFriendlyName(GetCurrentPowerGuid());

            if (name == string.Empty)
            {
                return "Unable to get power plan name (return buffer size was 0)";
            }
            else
            {
                return name;
            }
        }

        /// <summary>
        /// Get this computer available power plans.
        /// </summary>
        /// <returns><see cref="List{string}"/> of power plans names.</returns>
        public List<string> GetPowerPlans()
        {
            List<string> guidsNames = new List<string>(3);
            List<Guid> guids = ListPowerPlans();

            for (int i = 0; i < guids.Count; i++)
            {
                try
                {
                    Guid guid = guids[i];
                    guidsNames.Add(ReadFriendlyName(ref guid));
                }
                catch (COMException e)
                {
                    guidsNames.Add(e.ToString());
                }
            }

            return guidsNames;
        }

        private IntPtr GetCurrentPowerGuid()
        {
            IntPtr guid = IntPtr.Zero;
            uint returnCode = PowerGetActiveScheme(IntPtr.Zero, ref guid);

            if (returnCode == (uint)SystemCodes.ERROR_SUCCESS)
            {
                return guid;
            }
            else
            {
                throw new COMException("PowerGetActiveScheme call failed", (int)returnCode);
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
                        throw new COMException("Error while listing power schemes.", (int)returnCode);
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
                        throw new COMException("Error getting power scheme friendly name.", (int)returnCode);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(namePtr);
                }
            }
            else
            {
                throw new COMException("Error getting name buffer size.", (int)returnCode);
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
                        throw new COMException("Error getting power scheme friendly name.", (int)returnCode);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(namePtr);
                }
            }
            else
            {
                throw new COMException("Error getting name buffer size.", (int)returnCode);
            }
        }
    }
}
