using Ducky.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ducky.Helpers
{
    public class LogService 
    {

        public static List<LogMessageModel> actionlist { get; set; } = new List<LogMessageModel>();
        TwitchUserDataHelper dataHelper = new TwitchUserDataHelper();

        public void Add(string message, MessageType.Type type, Type from)
        {
            actionlist.Add(new LogMessageModel
            {
                date = dataHelper.GetTimeNow(),
                type = type.ToString(),
                message = message
            });
            Debug.WriteLine(message);
        }
        public void Add(Exception e, MessageType.Type type, Type from)
        {

            actionlist.Add(new LogMessageModel
            {
                date = dataHelper.GetTimeNow(),
                type = type.ToString(),
                message = e.Message + " Error in" + from.Name
            });
            Debug.WriteLine(e.Message + "Error in " + from.Name);
        }

        public List<LogMessageModel> GetLogList()
        {
            return actionlist;
        }

        public JsonResult GetLogJSON()
        {
           

            JArray array = new JArray();

            foreach (LogMessageModel action in actionlist)
                array.Add(action.message);

            JObject ob = new JObject();
            ob["Action log"] = array;

            JObject json = JObject.FromObject(JsonConvert.DeserializeObject(ob.ToString()));

            return new JsonResult(json);
        }
    }
}
