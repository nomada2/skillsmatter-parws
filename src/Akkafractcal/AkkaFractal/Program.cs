﻿using System;
using Akka.Actor;
using Akka.Routing;
using Akka;
using Akka.Configuration;
using SixLabors.ImageSharp;
using AkkaFractalShared;
using SixLabors.ImageSharp.PixelFormats;

namespace AkkaFractalSource
{
    class Program
    {
        static void Main(string[] args)
        {

            var config = ConfigurationFactory.ParseString(@"
akka {
    log - config - on - start = on
    stdout - loglevel = DEBUG
    loglevel = DEBUG
    actor {
        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
        debug {
          receive = on
          autoreceive = on
          lifecycle = on
          event-stream = on
          unhandled = on
        }
        deployment {
            /localactor {
                router = round-robin-pool
                nr-of-instances = 5
            }
            /remoteactor {
                router = round-robin-pool
                nr-of-instances = 5
                remote = ""akka.tcp://RemoteSystem@127.0.0.1:8080""
            }
        }
    }
    remote {
        dot-netty.tcp {
		    port = 8090
		    hostname = 127.0.0.1
        }
    }
}
");

            Console.Title = "Akka Fractal";

            ActorSystem system = ActorSystem.Create("fractal", config);

            string destination = @"c:\Temp\done.jpg";
            var w = 8000;
            var h = 8000;

            var img = new Image<Rgba32>(w, h);

            var split = 80;
            var ys = h / split;
            var xs = w / split;

            Action<RenderedTile> renderer = tile =>
            {
                if (tile.IsLastTile)
                    img.Save(destination);
                else
                {
                    var tileImage = tile.Bytes.ToBitmap();
                    var xt = 0;
                    for (int x = 0; x < xs; x++)
                    {
                        int yt = 0;
                        for (int y = 0; y < ys; y++)
                        {
                            img[x + tile.X, y + tile.Y] = tileImage[x, y];
                            yt++;
                        }
                        xt++;
                    }
                }
            };

            // TODO
            // Complete the "displayTile" actor that uses the "render" lambda 
            // to generate e persiste the image
            var displayTile = Akka.Actor.Nobody.Instance; 

            // TODO
            // increase the parallelism of the Actor "TileRenderActor"
            var actor = system.ActorOf(Props.Create<TileRenderActor>(), "render");

            for (int y = 0; y < split; y++)
            {
                var yy = ys * y;
                for (int x = 0; x < split; x++)
                {
                    var xx = xs * x;
                    actor.Tell(new RenderTile(yy, xx, xs, ys), displayTile);
                }
            }
            actor.Tell(new RenderTile(true), displayTile);

            Console.WriteLine("Tile render completed");
            Console.ReadLine();
        }
    }
}