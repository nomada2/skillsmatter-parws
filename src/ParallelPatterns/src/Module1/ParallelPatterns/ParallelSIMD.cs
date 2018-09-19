using System;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

namespace CSharp.Parallelx.ParallelSIMD
{
    public static class ParallelSIMD
    {
        public static async Task ExecuteInParallel<T>(this IEnumerable<T> collection,
            Func<T, Task> processor,
            int degreeOfParallelism)
        {
            var queue = new ConcurrentQueue<T>(collection);
            var tasks = Enumerable.Range(0, degreeOfParallelism).Select(async _ =>
            {
                T item;
                while (queue.TryDequeue(out item))
                {
                    await processor(item);
                }
            });

            await Task.WhenAll(tasks);
        }
        
        public static async Task<R[]> ExecuteInParallel<T, R>(this IEnumerable<T> collection,
            Func<T, Task<R>> processor,
            int degreeOfParallelism)
        {
            var queue = new ConcurrentQueue<T>(collection);
            var tasks = Enumerable.Range(0, degreeOfParallelism).Select(async _ =>
            { 
                List<R> localResults = new List<R>();
                T item;
                while (queue.TryDequeue(out item))
                {
                   var result = await processor(item);
                    localResults.Add(result);
                }

                return localResults;
            });

            var results = await Task.WhenAll(tasks.ToList());
            return results.SelectMany(i => i).ToArray();
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(dop)
                select Task.Run(async delegate
                {
                    using (partition)
                        while (partition.MoveNext())
                            await body(partition.Current);
                }));
        }

        public static Task ForEachAsyncConcurrent<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            var partitions = Partitioner.Create(source).GetPartitions(dop);
            var tasks = partitions.Select(async partition =>
            {
                using (partition)
                    while (partition.MoveNext())
                        await body(partition.Current);
            });

            return Task.WhenAll(tasks);
        }

        public static async Task ExecuteInParallelWithDegreeOfParallelism<T>(this IEnumerable<T> collection,
            Func<T, Task> processor,
            int degreeOfParallelism)
        {
            var queue = new ConcurrentQueue<T>(collection);
            var tasks = Enumerable.Range(0, degreeOfParallelism).Select(async _ =>
            {
                T item;
                while (queue.TryDequeue(out item))
                    await processor(item);
            });

            await Task.WhenAll(tasks);
        }

        private static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> toAdd)
        {
            foreach (var element in toAdd)
                @this.Add(element);
        }

        public static async Task<IEnumerable<R>> ProjectInParallelWithDegreeOfParallelism<T, R>(
            this IEnumerable<T> collection,
            Func<T, Task<R>> processor,
            int degreeOfParallelism)
        {
            var queue = new ConcurrentQueue<T>(collection);
            var results = new ConcurrentBag<R>();
            var tasks = Enumerable.Range(0, degreeOfParallelism).Select(async _ =>
            {
                List<R> localResults = new List<R>();
                T item;
                while (queue.TryDequeue(out item))
                {
                    var result = await processor(item);
                    localResults.Add(result);
                }

                results.AddRange(localResults);
            });

            await Task.WhenAll(tasks);
            return results;
        }
    }
}