using CsvHelper;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Axure_Scraper
{

    public class Module
    {
        public string Id { get; set; }
        public string ModuleId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    class Program
    {
        async static Task Instagram()
        {
            var insta = new Instagram(new HttpClient());
            var query = new InstagramQuery
            {
                id = 10609417706,
                first = 50
            };
            /*var posts = insta.GetAllPosts(query);
            File.WriteAllText("./posts.json", posts.ToString());*/

            var rawPosts = File.ReadAllText("./posts.json");
            JArray parsedPosts = JArray.Parse(rawPosts) as JArray;
            Console.WriteLine($"done {parsedPosts.Count()}");

            await insta.GetAllPhotosAsync(parsedPosts);

            /*posts = insta.GetPosts(query);
            File.AppendAllText("./posts.json", posts.ToString());
            Console.WriteLine($"ready {posts}");*/

        }

        async static Task LeaveWizard()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

            using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false
            }))
            using (var page = await browser.NewPageAsync())
            {
                /*var contents = await page.GoToAsync("https://app.leavewizard.com/");

                await page.WaitForSelectorAsync("input[name=username]");
                await page.TypeAsync("input[name='username']", "jordan.lane@steinias.com");
                await page.TypeAsync("input[name='password']", "Branches32!");
                await page.ClickAsync("#btnLogin");

                await page.WaitForFunctionAsync("() => window.__PRELOADED_STATE__ && typeof window.__PRELOADED_STATE__.accessToken === 'string'");
                var accessToken = await page.EvaluateExpressionAsync<string>("window.__PRELOADED_STATE__.accessToken");
                Console.WriteLine($"ready {accessToken}");*/

                var accessToken = "ae910066d1f83814499a7d82460b282c";

                await page.SetRequestInterceptionAsync(true);
                page.Request += async (sender, e) =>
                {
                    await e.Request.ContinueAsync(new Payload
                    {
                        Headers = new Dictionary<string, string>()
                        {
                            { "Authorization", $"Bearer {accessToken}" },
                            { "Content-Type", "application/json" }
                        }
                    });
                };

                var result = await page.GoToAsync("https://api.leavewizard.com/event/whosonleave?groupKey=Workgroup-0&fromDate=2020-02-01T00%3A00%3A00Z&toDate=2020-09-29T23%3A59%3A59Z", 600000);

                Console.WriteLine(page.GetContentAsync().Result);
            }
        }

        async static Task Main(string[] args)
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

            var modules = new List<Module>();
            using(var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            }))
            using (var page = await browser.NewPageAsync())
            {
                var contents = await page.GoToAsync("https://sqf8iu.axshare.com/#id=ufxrjr&p=3-1-001_a___module_block&g=1");
                var sitemap = await page.EvaluateExpressionAsync<dynamic>("$axure.document.sitemap");

                ParseNode(sitemap.rootNodes[0], modules);
                Console.WriteLine("Sitemap parsed.");
            }

            using (var writer = new StreamWriter(Directory.GetCurrentDirectory() + "\\sitemap.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(modules);
                Console.WriteLine("CSV updated.");
            }

            Console.WriteLine("Press ESC to stop");
            do
            {
                while (!Console.KeyAvailable)
                {
                    // Do something
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

        }

        static List<Module> ParseNode(dynamic node, List<Module> nodes)
        {
            if (node.type == "Folder")
            {
                foreach (var child in node.children)
                {
                    ParseNode(child, nodes);
                }
                return nodes;
            }

            var pageName = node.pageName.ToString();
            var separator = pageName.IndexOf('|');
            nodes.Add(new Module() {
                Id = node.id,
                ModuleId = separator != -1 ? pageName.Substring(0, separator) : "",
                Name = node.pageName,
                Url = $"https://sqf8iu.axshare.com/#id={node.id}&p={node.url}&g=1"
            });

            return nodes;
        }
    }
}
