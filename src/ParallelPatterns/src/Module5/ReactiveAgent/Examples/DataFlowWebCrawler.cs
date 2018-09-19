﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Reactive;
using System.Reactive.Disposables;

namespace DataFlowAgent
{
    public class DataFlowWebCrawler
    {
        private const string LINK_REGEX_HREF = "\\shref=('|\\\")?(?<LINK>http\\://.*?(?=\\1)).*>";
        private static readonly Regex _linkRegexHRef = new Regex(LINK_REGEX_HREF);

        private const string IMG_REGEX = "<\\s*img [^\\>]*src=('|\")?(?<IMG>http\\://.*?(?=\\1)).*>\\s*([^<]+|.*?)?\\s*</a>";
        private static readonly Regex _imgRegex = new Regex(IMG_REGEX);

        public static void Start(List<string> urls)
        {
            StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;
            Predicate<string> linkFilter = link =>
                link.IndexOf(".aspx", comparison) != -1 ||
                link.IndexOf(".php", comparison) != -1 ||
                link.IndexOf(".htm", comparison) != -1 ||
                link.IndexOf(".html", comparison) != -1 ||
                link.EndsWith(".com", comparison) ||
                link.EndsWith(".net", comparison);
            Predicate<string> imgFilter = url =>
                url.EndsWith(".jpg", comparison) ||
                url.EndsWith(".png", comparison) ||
                url.EndsWith(".gif", comparison);

            var downloader = new TransformBlock<string, string>(
                async (url) =>
                {
                    // using IOCP the thread pool worker thread does return to the pool
                    var client = new HttpClient();
                    string result = await client.GetStringAsync(url);
                    return result;
                }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 2 });

            var contentBroadcaster = new BroadcastBlock<string>(s => s);

            var linkParser = new TransformManyBlock<string, string>(
                (html) =>
                {
                    var output = new List<string>();
                    var links = _linkRegexHRef.Matches(html);
                    foreach (Match item in links)
                    {
                        var value = item.Groups["LINK"].Value;
                        output.Add(value);
                    }
                    return output;
                });


            var imgParser = new TransformManyBlock<string, string>((html) =>
        {
            var output = new List<string>();
            var images = _imgRegex.Matches(html);
            foreach (Match item in images)
            {
                var value = item.Groups["IMG"].Value;
                output.Add(value);
            }
            return output;
        });

            var writer = new ActionBlock<string>(async url =>
            {
                var client = new HttpClient();
                // using IOCP the thread pool worker thread does return to the pool
                byte[] buffer = await client.GetByteArrayAsync(url);
                string fileName = Path.GetFileName(url);

                var destination = "../../../../../Data/WebCrawler";
                if (!Directory.Exists(destination))
                    Directory.CreateDirectory(destination);

                string nameImage = Path.Combine(destination, fileName);

                using (Stream srm = File.OpenWrite(nameImage))
                {
                    Console.WriteLine($"Persisting file {Path.GetFileNameWithoutExtension(fileName)}");
                    await srm.WriteAsync(buffer, 0, buffer.Length);
                }
            });

            var linkBroadcaster = new BroadcastBlock<string>(s => s);

            IDisposable disposeAll = new CompositeDisposable(
                // from [downloader] to [contentBroadcaster]
                downloader.LinkTo(contentBroadcaster),
                // from [contentBroadcaster] to [imgParser]
                contentBroadcaster.LinkTo(imgParser),
                // from [contentBroadcaster] to [linkParserHRef]
                contentBroadcaster.LinkTo(linkParser),
                // from [linkParser] to [linkBroadcaster]
                linkParser.LinkTo(linkBroadcaster),
                // conditional link to from [linkBroadcaster] to [downloader]
                linkBroadcaster.LinkTo(downloader, linkFilter),
                // from [linkBroadcaster] to [writer]
                linkBroadcaster.LinkTo(writer, imgFilter),
                // from [imgParser] to [writer]
                imgParser.LinkTo(writer));

            foreach (var url in urls)
                downloader.Post(url);


            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
            downloader.Complete();
            disposeAll.Dispose();

        }
    }
}
