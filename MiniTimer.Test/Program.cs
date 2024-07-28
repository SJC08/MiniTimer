using Asjc.MiniTimer;
using System.Diagnostics;

int interval = 10;
List<double> errors = [];
Stopwatch? stopwatch = new();
MiniTimer timer = new(interval, true, _ =>
{
    if (stopwatch.IsRunning)
    {
        var error = stopwatch.Elapsed.TotalMilliseconds - interval;
        errors.Add(error);
        Console.WriteLine($"{error:0.00} ms, average: {errors.Average():0.00} ms");
    }
    stopwatch.Restart();
});
