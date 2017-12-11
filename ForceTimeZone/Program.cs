using System;
using System.Diagnostics;
using System.Timers;
using Topshelf;

namespace ForceTimeZone
{
  public class Program
  {
    public const string TimeZoneId = "W. Europe Standard Time";

    public static void Main(string[] args)
    {
      HostFactory.Run(x =>
      {
        x.Service<Task>(s =>
        {
          s.ConstructUsing(name => new Task());
          s.WhenStarted(tc => tc.Start());
          s.WhenStopped(tc => tc.Stop());
        });

        x.StartAutomatically();
        x.RunAsLocalSystem();
      });
    }

    public class Task
    {
      private readonly Timer _timer;

      public Task()
      {
        _timer = new Timer(1000 * 60) {AutoReset = true};
        _timer.Elapsed += DoWork;
      }

      public void Start()
      {
        Console.WriteLine($"{DateTime.Now}\tRunning...");
        Console.WriteLine($"{DateTime.Now}\tForcing zone: {TimeZoneId}");
        _timer.Start();
      }

      public void Stop()
      {
        Console.WriteLine($"{DateTime.Now}\tStopping...");
        _timer.Stop();
      }

      public void DoWork(object sender, ElapsedEventArgs args)
      {
        var check = Process.Start(new ProcessStartInfo
        {
          FileName = "tzutil.exe",
          Arguments = "/g",
          UseShellExecute = false,
          CreateNoWindow = true,
          RedirectStandardOutput = true
        });

        if (check == null) return;
        if (check.StandardOutput.ReadToEnd() == TimeZoneId) return;

        var set = Process.Start(new ProcessStartInfo
        {
          FileName = "tzutil.exe",
          Arguments = "/s \"" + TimeZoneId + "\"",
          UseShellExecute = false,
          CreateNoWindow = true
        });

        if (set == null) return;

        check.WaitForExit();
        set.WaitForExit();

        TimeZoneInfo.ClearCachedData();
      }
    }
  }
}