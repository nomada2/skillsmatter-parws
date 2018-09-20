using ReactiveAgent.Agents;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using ReactiveAgent.Agents.Dataflow;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using DataFlowAgent;
using ParallelPatterns;

namespace ReactiveAgent.CS
{
    public class Program
    {
        private static async Task ReadDataAsync(BufferBlock<int> bufferingBlock)
        {
            // Receive the messages back .
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(await bufferingBlock.ReceiveAsync());
            }
        }

        private static async Task WriteDataAsync(BufferBlock<int> bufferingBlock)
        {
            // Post some messages to the block.
            for (int i = 0; i < 10; i++)
            {
                await bufferingBlock.SendAsync(i * i);
            }
        }


        public static void Main(string[] args)
        {
            //PingPongAgents.Start();
            //Console.ReadLine();

            var urls = new List<string> { "https://edition.cnn.com", "http://www.bbc.com", "https://www.microsoft.com" };
            DataFlowWebCrawler.Start(urls);
            Console.ReadLine();
            
            WordCountAgentsExample.Run().Wait();

            Console.ReadLine();


            DataflowPipeline.DataflowPipeline.Start();

            Console.ReadLine();

            // Create a BufferBlock object.
            var bufferingBlock = new BufferBlock<int>();

            WriteDataAsync(bufferingBlock).Wait();
            ReadDataAsync(bufferingBlock).Wait();

            Console.WriteLine("Finished. Press any key to exit.");
            Console.ReadLine();

        }

        static async Task Play()
        {
            (new DataflowTransformActionBlocks()).Run();

            AgentAggregate.Run();

            await WordCountAgentsExample.Run();
        }
    }
}
