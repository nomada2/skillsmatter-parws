using Reactive;
using System;

namespace ReactiveEx
{
    class Program
    {
        static void Main(string[] args)
        {
            var ping = new Ping();
            var pong = new Pong();

            Console.WriteLine("Press any key to stop ...");

            // TODO 
            // Uncoment this code and copmlete the TODOs
            // in the "PingPongSubject" file
            //var pongSubscription = ping.Subscribe(pong);
            //var pingSubscription = pong.Subscribe(ping);

            Console.ReadKey();

            //pongSubscription.Dispose();
            //pingSubscription.Dispose();

            Console.WriteLine("Ping Pong has completed.");
        }
    }
}
