namespace Multitool.NTInterop
{
    public class SystemPowerState
    {
        internal SystemPowerState(SYSTEM_POWER_STATE state)
        {
            Level = (uint)state;
            switch (state)
            {
                case SYSTEM_POWER_STATE.PowerSystemUnspecified:
                    Name = "Unspecified";
                    break;
                case SYSTEM_POWER_STATE.PowerSystemWorking:
                    Name = "S0";
                    break;
                case SYSTEM_POWER_STATE.PowerSystemSleeping1:
                    Name = "S1";
                    break;
                case SYSTEM_POWER_STATE.PowerSystemSleeping2:
                    Name = "S2";
                    break;
                case SYSTEM_POWER_STATE.PowerSystemSleeping3:
                    Name = "S3";
                    break;
                case SYSTEM_POWER_STATE.PowerSystemHibernate:
                    Name = "S4";
                    break;
                case SYSTEM_POWER_STATE.PowerSystemShutdown:
                    Name = "S5";
                    break;
                case SYSTEM_POWER_STATE.PowerSystemMaximum:
                    Name = "S6 (Maximum)";
                    break;
            }
        }

        public uint Level { get; private set; }
        public string Name { get; private set; }
    }
}
