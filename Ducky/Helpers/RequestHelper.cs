using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ducky.Helpers
{
    public class RequestHelper
    {
        private readonly string clientid = "vlf9a4486ky1mxdp7zeiedskxkxh8j";
        public async Task<string> SendGetRequest(string link)
        {
            try
            {
                HttpWebRequest webRequest = CreateGetRequest(link);
                var res = await GetResponseAsync(webRequest);
                return res;
            }
            catch (Exception e)
            {
                return null;
            }

        }
        private HttpWebRequest CreateGetRequest(string link)
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(link);
            webRequest.Method = "GET";
            webRequest.Accept = "application/vnd.twitchtv.v5+json";
            webRequest.Headers.Add("Client-ID", clientid);
            return webRequest;
        }
        private async Task<string> GetResponseAsync(WebRequest request)
        {
            try
            {
                HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
