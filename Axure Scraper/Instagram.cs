using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Axure_Scraper
{
    public class Instagram
    {
        private readonly HttpClient Client;
        private string Cursor;

        public Instagram(HttpClient client)
        {
            this.Client = client;
        }

        public dynamic GetPosts(InstagramQuery query)
        {
            if (Cursor != null)
            {
                query.after = Cursor;
            }

            var data = this.Client.GetAsync($"https://www.instagram.com/graphql/query/?query_hash=e769aa130647d2354c40ea6a439bfc08&variables={query.ToString()}").Result.Content.ReadAsStringAsync().Result;
            JObject jsonVal = JObject.Parse(data) as JObject;
            dynamic insta = jsonVal;
            
            return insta.data.user.edge_owner_to_timeline_media;
        }

        public dynamic GetAllPosts(InstagramQuery query)
        {
            var timeline = GetPosts(query);
            var posts = new JArray(timeline.edges);
            while (timeline.page_info.has_next_page == true)
            {
                Cursor = timeline.page_info.end_cursor;
                timeline = GetPosts(query);
                posts.Merge(timeline.edges);
            }

            return posts;
        }

        public async Task GetAllPhotosAsync(dynamic posts)
        {
            foreach (var post in posts)
            {
                var url = post.node.thumbnail_resources[0].src;
                Console.WriteLine(url);

                if (File.Exists($"./photos/{post.node.id}.jpg")) continue;
                using (var request = new HttpRequestMessage(HttpMethod.Get, url.ToString()))
                using (
                    Stream contentStream = await(await Client.SendAsync(request)).Content.ReadAsStreamAsync(),
                    stream = new FileStream($"./photos/{post.node.id}.jpg", FileMode.Create, FileAccess.Write, FileShare.None, 3145728, true))
                {
                    await contentStream.CopyToAsync(stream);
                }
            }
        }
    }
}
