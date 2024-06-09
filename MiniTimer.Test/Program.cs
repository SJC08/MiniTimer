using Asjc.MiniTimer;

MiniTimer timer = new(500, true, _ => Console.WriteLine(DateTime.Now));
