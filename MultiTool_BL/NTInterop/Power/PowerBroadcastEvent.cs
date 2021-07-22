namespace Multitool.NTInterop.Power
{
    public enum PowerBroadcastEvent : int
    {
        ApmQuerySuspend = 0x0,
        ApmQueryStandby = 0x1,
        ApmQuerySuspendFailed = 0x2,
        ApmQueryStandbyFailed = 0x3,
        ApmSuspend = 0x4,
        ApmStandby = 0x5,
        ApmResumeCritical = 0x6,
        ApmResumeSuspend = 0x7,
        ApmResumeStandby = 0x8,
        ApmBatteryLow = 0x9,
        ApmPowerStatusChange = 0xA,
        ApmOemEvent = 0xb,
        ApmResumeAutomatic = 0x12,
        PowerSettingChange = 0x8013
    }
}
