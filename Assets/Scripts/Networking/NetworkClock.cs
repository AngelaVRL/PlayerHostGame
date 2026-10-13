using System.Diagnostics;

namespace PlayerHostGame.Networking
{
    public static class NetworkClock
    {
        public static long NowMilliseconds
        {
            get { return Stopwatch.GetTimestamp() * 1000L / Stopwatch.Frequency; }
        }
    }
}
