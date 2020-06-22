using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ducky.Helpers
{
    public class ViewersHelper
    {
        static HttpClient client = new HttpClient();

        public async Task<JObject> GetViewers(string streamername)
        {
            string link = $"https://tmi.twitch.tv/group/user/{streamername}/chatters";
            string responsestring;
            HttpResponseMessage response = await client.GetAsync(link);
            if (response.IsSuccessStatusCode)
                responsestring = await response.Content.ReadAsStringAsync();
            else
                responsestring = "";

            var o = JObject.Parse(responsestring);
            List<string> viewers = new List<string>();

            var vips = o["chatters"]["vips"];
            var moderators = o["chatters"]["moderators"];
            var baseviewers = o["chatters"]["viewers"];

            foreach (string vip in vips)
                viewers.Add(vip);

            foreach (string moderator in moderators)
                viewers.Add(moderator);

            foreach (string baseviewer in baseviewers)
                viewers.Add(baseviewer);

            JArray array = new JArray();
            foreach (string viewer in viewers)
                array.Add(viewer);

            JObject ob = new JObject();
            ob["Viewers"] = array;

            return ob;
        }

        public async Task<string> GetViewersCount(string streamername)
        {
            string link = $"https://tmi.twitch.tv/group/user/{streamername}/chatters";
            string responsestring;
            HttpResponseMessage response = await client.GetAsync(link);
            if (response.IsSuccessStatusCode)
                responsestring = await response.Content.ReadAsStringAsync();
            else
                responsestring = "";

            var o = JObject.Parse(responsestring);
            List<string> viewers = new List<string>();

            var vips = o["chatters"]["vips"];
            var moderators = o["chatters"]["moderators"];
            var baseviewers = o["chatters"]["viewers"];

            foreach (string vip in vips)
                viewers.Add(vip);

            foreach (string moderator in moderators)
                viewers.Add(moderator);

            foreach (string baseviewer in baseviewers)
                viewers.Add(baseviewer);

            return viewers.Count.ToString();
        }
    }
}
