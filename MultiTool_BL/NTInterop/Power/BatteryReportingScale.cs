namespace Multitool.NTInterop
{
    public class BatteryReportingScale
    {
        public BatteryReportingScale(uint granularity, uint capacity)
        {
            Granularity = granularity;
            Capacity = capacity;
        }

        public uint Granularity { get; private set; }
        public uint Capacity { get; private set; }

        public override string ToString()
        {
            return nameof(Granularity) + " " + Granularity + ", " + nameof(Capacity) + " " + Capacity;
        }
    }
}
