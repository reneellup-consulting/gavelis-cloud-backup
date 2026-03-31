//using System.Timers;

namespace ConsoleApp1
{
    internal class Program
    {
        private static System.Timers.Timer aTimer;

        static void Main(string[] args)
        {
            aTimer = new System.Timers.Timer();

            // Schedule the first backup to run one hour from now
            DateTime nowPlusOneHour = DateTime.Now.AddHours(1);
            var timespanToNextHour = (nowPlusOneHour - DateTime.Now).TotalMilliseconds;

            aTimer.Interval = timespanToNextHour;

            aTimer.Elapsed += ATimer_Elapsed;

            // Schedule subsequent backups to run every hour
            aTimer.AutoReset = true;
            aTimer.Interval = TimeSpan.FromHours(1).TotalMilliseconds;
            aTimer.Start();
        }

        private static void ATimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}