namespace CheckinLS.API.Misc
{
    public readonly struct Constants
    {
        public static int PretCurs { get; } = 17;
        public static int PretPregatire { get; } = 8;
        public static int PretRecuperare { get; } = 17;
        public static int PretOffice { get; } = 10;
    }

    public enum Month
    {
        Last = 0,
        Current = 1
    }
}
