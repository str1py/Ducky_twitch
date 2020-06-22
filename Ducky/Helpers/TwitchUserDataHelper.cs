using Ducky.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace Ducky.Helpers
{
    public class TwitchUserDataHelper
    {
        RequestHelper requestHelper = new RequestHelper();
        public TwitchUserModel GetUserFromMessage(OnMessageReceivedArgs data)
        {
            try
            {
                var badges = GetDataFromBadges(data.ChatMessage.Badges, data.ChatMessage.RawIrcMessage);
                bool vip;
                if (badges[2] == 1)
                    vip = true;
                else
                    vip = false;

                var user = (new TwitchUserModel
                {
                    userId = Int32.Parse(data.ChatMessage.UserId),
                    userName = (string)data.ChatMessage.DisplayName,
                    userType = (string)data.ChatMessage.UserType.ToString(),
                    subMounthsCount = badges[0],
                    SubPlan = (string)"1",
                    isSub = (bool)data.ChatMessage.IsSubscriber,
                    isVip = vip,
                    isModer = (bool)data.ChatMessage.IsModerator,
                    giftsCount = badges[1],
                });
                return user;
            }
            catch (Exception ex)
            {
                //logService.Add(ex, MessageType.Type.ERROR,
                //     MethodBase.GetCurrentMethod().DeclaringType);
                Console.WriteLine(ex.Message + "in " + MethodBase.GetCurrentMethod().DeclaringType);
                return null;
            }
        }
        public TwitchUserModel GetUserFromReSub(OnReSubscriberArgs data)
        {
            try
            {
                var badges = GetDataFromBadges(data.ReSubscriber.Badges, data.ReSubscriber.RawIrc);
                bool vip;
                if (badges[2] == 1)
                    vip = true;
                else
                    vip = false;
                var user = (new TwitchUserModel
                {
                    userId = Int32.Parse(data.ReSubscriber.UserId),
                    userName = data.ReSubscriber.DisplayName.ToString(),
                    SubPlan = GetSubTier(data.ReSubscriber.SubscriptionPlan.ToString()),
                    isSub = true,
                    isVip = vip,
                    isModer = data.ReSubscriber.IsModerator,
                    subMounthsCount = badges[0],
                    giftsCount = badges[1],
                    userType = data.ReSubscriber.UserType.ToString()
                });
                return user;
            }
            catch (Exception ex)
            {
                //logService.Add(ex, MessageType.Type.ERROR,
                //     MethodBase.GetCurrentMethod().DeclaringType);
                Console.WriteLine(ex.Message + "in " + MethodBase.GetCurrentMethod().DeclaringType);
                return null;
            }
        }
        public TwitchUserModel GetUserFromNewSub(OnNewSubscriberArgs data)
        {
            try
            {
                var badges = GetDataFromBadges(data.Subscriber.Badges, data.Subscriber.RawIrc);
                bool vip;
                if (badges[2] == 1)
                    vip = true;
                else
                    vip = false;
                var user = (new TwitchUserModel
                {
                    userId = Int32.Parse(data.Subscriber.UserId),
                    userName = data.Subscriber.DisplayName,
                    userType = data.Subscriber.UserType.ToString(),
                    isSub = data.Subscriber.IsSubscriber,
                    isVip = vip,
                    isModer = data.Subscriber.IsModerator,
                    giftsCount = badges[1],
                    subMounthsCount = 1,
                    SubPlan = GetSubTier(data.Subscriber.SubscriptionPlan.ToString()),
                });

                return user;
            }
            catch (Exception ex)
            {
                //logService.Add(ex, MessageType.Type.ERROR,
                //     MethodBase.GetCurrentMethod().DeclaringType);
                Console.WriteLine(ex.Message + "in " + MethodBase.GetCurrentMethod().DeclaringType);
                return null;
            }
        }
        public async Task<TwitchUserModel> GetUserFromGifted(OnGiftedSubscriptionArgs data)
        {
            try
            {
                var badges = GetDataFromBadges(data.GiftedSubscription.Badges);
                string vipfrombadges = badges.Where(x => x.Key == "vip").Select(x => x.Value).FirstOrDefault();
                bool vip;
                if (vipfrombadges == "1")
                    vip = true;
                else
                    vip = false;
                var user = (new TwitchUserModel
                {
                    userId = await GetUseridByName(data.GiftedSubscription.DisplayName),
                    userName = data.GiftedSubscription.DisplayName,
                    userType = data.GiftedSubscription.UserType.ToString(),
                    isSub = data.GiftedSubscription.IsSubscriber,
                    isVip = vip,
                    isModer = data.GiftedSubscription.IsModerator,
                    giftsCount = Int32.Parse(badges.Where(x => x.Key == "sub-gifter").Select(x => x.Value).FirstOrDefault()),
                    subMounthsCount = Int32.Parse(badges.Where(x => x.Key == "subscriber").Select(x => x.Value).FirstOrDefault()),
                    SubPlan = (string)"1",
                });

                return user;
            }
            catch (Exception ex)
            {
                //logService.Add(ex, MessageType.Type.ERROR,
                //     MethodBase.GetCurrentMethod().DeclaringType);
                Console.WriteLine(ex.Message + "in " + MethodBase.GetCurrentMethod().DeclaringType);
                return null;
            }


        }
        public async Task<TwitchUserModel> GetUserFromCommunitySubs(OnCommunitySubscriptionArgs data)
        {
            try
            {
                var badges = data.GiftedSubscription.Badges;
                bool vip;
                if (badges[2] == 1)
                    vip = true;
                else
                    vip = false;

                var user = (new TwitchUserModel
                {
                    userId = await GetUseridByName(data.GiftedSubscription.DisplayName),
                    userName = data.GiftedSubscription.DisplayName,
                    isSub = data.GiftedSubscription.IsSubscriber,
                    isVip = vip,
                    isModer = data.GiftedSubscription.IsModerator,
                    giftsCount = badges[1],
                    subMounthsCount = badges[0],
                    SubPlan = GetSubTier(data.GiftedSubscription.MsgParamSubPlan.ToString()),
                    userType = data.GiftedSubscription.UserType.ToString()
                });

                return user;
            }
            catch (Exception ex)
            {
                //logService.Add(ex, MessageType.Type.ERROR,
                //     MethodBase.GetCurrentMethod().DeclaringType);
                Console.WriteLine(ex.Message + "in " + MethodBase.GetCurrentMethod().DeclaringType);
                return null;
            }
        }

        public async Task<int> GetUseridByName(string usermame)
        {
            try
            {
                var jsonString = await requestHelper.SendGetRequest($"https://api.twitch.tv/helix/users?login={usermame}");
                var user = JObject.Parse(jsonString);
                string userid = (string)user["data"][0]["id"];
                return Int32.Parse(userid);
            }
            catch (Exception ex)
            {
                //logService.Add(ex, MessageType.Type.ERROR,
                //     MethodBase.GetCurrentMethod().DeclaringType);
                Console.WriteLine(ex.Message + "in " + MethodBase.GetCurrentMethod().DeclaringType);
                return 0;
            }
        }
        private string GetSubMonths(string badges)
        {
            try
            {
                string[] parsed = badges.Split(";");
                string[] parsed2 = parsed[0].Split("=");
                string[] parsed3;
                if (parsed2[1] != "")
                {
                    parsed3 = parsed2[1].Split("/");
                    return parsed3[1];
                }
                else
                    return "0";
            }
            catch (Exception ex)
            {
                //logService.Add(ex, MessageType.Type.ERROR,
                //     MethodBase.GetCurrentMethod().DeclaringType);
                Console.WriteLine(ex.Message + "in " + MethodBase.GetCurrentMethod().DeclaringType);
                return null;
            }
            // @badge-info=subscriber/21;
            // badges =broadcaster/1,subscriber/0;color=#8A2BE2;

        }
        public string GetSubTier(string subplan)
        {
            try
            {
                if (subplan.Contains("Tier"))
                    return subplan.Replace("Tier", "");
                else if (subplan.Contains("Prime"))
                    return "1";
                else if (subplan == "")
                    return "0";
                else if (subplan == "1")
                    return "1";
                else if (subplan == "2")
                    return "2";
                else if (subplan == "3")
                    return "3";
                else
                    return "0";
            }
            catch (Exception ex)
            {
                //logService.Add(ex, MessageType.Type.ERROR,
                //     MethodBase.GetCurrentMethod().DeclaringType);
                Console.WriteLine(ex.Message + "in " + MethodBase.GetCurrentMethod().DeclaringType);
                return "0";
            }
        }

        public async Task<Dictionary<string, string>> GetChannelInfo(string channelname)
        {
            try
            {
                var id = await GetUseridByName(Startup.streamerName);
                var jsonString = await requestHelper.SendGetRequest($"https://api.twitch.tv/kraken/streams/{id}");
                var o = JObject.Parse(jsonString);
                string streamStatusString = GetStreamStatus(jsonString);

                Dictionary<string, string> info = new Dictionary<string, string>();

                if (streamStatusString == "live")
                {
                    ViewersHelper helper = new ViewersHelper();
                    DateTime s = (DateTime)o["stream"]["created_at"]; //hz cho esli live tyt budet
                    info.Add("status", streamStatusString);
                    info.Add("viewers", await helper.GetViewersCount(Startup.streamerName));
                    info.Add("channelname", (string)o["stream"]["channel"]["display_name"]);
                    info.Add("userid", (string)o["stream"]["_id"]);
                    info.Add("streamlength", GetStreamLength(s));
                    info.Add("logo", (string)o["stream"]["channel"]["logo"]);
                }
                else if (streamStatusString == "rerun")
                {
                    info.Add("status", streamStatusString);
                    info.Add("viewers", (string)o["stream"]["viewers"]);
                    info.Add("channelname", (string)o["stream"]["channel"]["display_name"]);
                    info.Add("logo", (string)o["stream"]["channel"]["logo"]);
                    info.Add("userid", (string)o["stream"]["_id"]);
                }
                if (streamStatusString == "offline")
                {
                    info.Add("status", "offline");
                    info.Add("viewers", "0");
                    info.Add("channelname", Startup.streamerName);
                    info.Add("userid", "0");
                }
                return info;
            }
            catch (Exception e)
            {
                //logService.Add($"{e.Message}", MessageType.Type.ERROR,
                //  MethodBase.GetCurrentMethod().DeclaringType);
                Console.WriteLine(e.Message + "in " + MethodBase.GetCurrentMethod().DeclaringType);
                return null;
            }
        }
        private string GetStreamLength(DateTime date)
        {
            try
            {
                var s = DateTime.UtcNow - date;

                string hours;
                string legth;

                if (s.Hours < 10)
                    hours = "0" + s.Hours;

                if (s.Days != 0)
                    legth = $"{s.Days}d {s.Hours}h {s.Minutes}m";
                else if (s.Hours != 0)
                    legth = $"{s.Hours}h {s.Minutes}m";
                else
                    legth = $"{s.Minutes}m";

                return legth;
            }
            catch (Exception e)
            {
                //logService.Add($"{e.Message}", MessageType.Type.ERROR,
                //      MethodBase.GetCurrentMethod().DeclaringType);
                Console.WriteLine(e.Message + "in " + MethodBase.GetCurrentMethod().DeclaringType);
                return "0";
            }
        }
        private string GetStreamStatus(string json)
        {
            try
            {
                var o = JObject.Parse(json);
                string streamStatus = "";
                try { streamStatus = (string)o["stream"]["stream_type"]; }
                catch { }
                if (streamStatus == "" || String.IsNullOrEmpty(streamStatus))
                    streamStatus = "offline";

                return streamStatus;
            }
            catch (Exception e)
            {
                //logService.Add($"{e.Message}", MessageType.Type.ERROR,
                //      MethodBase.GetCurrentMethod().DeclaringType);
                Console.WriteLine(e.Message + "in " + MethodBase.GetCurrentMethod().DeclaringType);
                return "error";
            }
        }

        public async Task<bool> IsStreamerOnline()
        {
            try
            {
                var id = await GetUseridByName(Startup.streamerName);
                var jsonString = await requestHelper.SendGetRequest($"https://api.twitch.tv/kraken/streams/{id}");
                GetStreamStatus(jsonString);
                string streamStatusString = "";
                if (streamStatusString == "" || String.IsNullOrEmpty(streamStatusString))
                    streamStatusString = "offline";

                bool isLive;
                if (streamStatusString == "live")
                    isLive = true;
                else
                    isLive = false;

                return isLive;
            }
            catch (Exception e)
            {
                //logService.Add($"{e.Message}", MessageType.Type.ERROR,
                //MethodBase.GetCurrentMethod().DeclaringType);
                Console.WriteLine(e.Message + "in " + MethodBase.GetCurrentMethod().DeclaringType);
                return false;
            }
        }

        private List<int> GetDataFromBadges(List<KeyValuePair<string, string>> badges, string _badges)
        {
            try
            {
                string subcount = GetSubMonths(_badges);
                string subgifter = badges.Where(y => y.Key == "sub-gifter").Select(x => x.Value).FirstOrDefault();
                string vip = badges.Where(y => y.Key == "vip").Select(x => x.Value).FirstOrDefault();

                switch (subgifter)
                {
                    case null:
                        subgifter = "0";
                        break;
                }
                switch (vip)
                {
                    case null:
                        vip = "0";
                        break;
                }

                // 0-submonths 1-subgiftscount
                List<int> parsed = new List<int>();
                parsed.Add(Int32.Parse(subcount)); parsed.Add(Int32.Parse(subgifter)); parsed.Add(Int32.Parse(vip));
                return parsed;
            }
            catch (Exception ex)
            {
                //logService.Add(ex, MessageType.Type.ERROR,
                //     MethodBase.GetCurrentMethod().DeclaringType);
                Console.WriteLine(ex.Message + "in " + MethodBase.GetCurrentMethod().DeclaringType);
                return null;
            }
        }
        private Dictionary<string, string> GetDataFromBadges(string badges)
        {
            try
            {
                // 0-submonths 1-subgiftscount
                // moderator sub-gift-leader//"subscriber/6,sub-gifter/50"
                Dictionary<string, string> badgesDic = new Dictionary<string, string>();
                var parserd = badges.Split(",");

                foreach (string str in parserd)
                {
                    string[] par = str.Split("/");
                    badgesDic.Add(par[0], par[1]);
                }

                string moder = badgesDic.Where(x => x.Key == "moderator").Select(x => x.Value).FirstOrDefault();
                string submonths = badgesDic.Where(x => x.Key == "subscriber").Select(x => x.Value).FirstOrDefault();
                string subgifts = badgesDic.Where(x => x.Key == "sub-gifter").Select(x => x.Value).FirstOrDefault();
                string vip = badgesDic.Where(x => x.Key == "vip").Select(x => x.Value).FirstOrDefault();

                if (moder == null)
                    moder = "0";
                if (submonths == null)
                    submonths = "0";
                if (subgifts == null)
                    subgifts = "0";
                if (vip == null)
                    subgifts = "0";

                return badgesDic;
            }
            catch (Exception ex)
            {
                //logService.Add(ex, MessageType.Type.ERROR,
                //     MethodBase.GetCurrentMethod().DeclaringType);
                Console.WriteLine(ex.Message + "in " + MethodBase.GetCurrentMethod().DeclaringType);
                return null;
            }
        }
        public string GetTimeNow()
        {
            try
            {
                var date = DateTime.Now;
                string seconds;
                string minutes;
                if (date.Second < 10)
                    seconds = "0" + date.Second.ToString();
                else
                    seconds = date.Second.ToString();

                if (date.Minute < 10)
                    minutes = "0" + date.Minute.ToString();
                else
                    minutes = date.Minute.ToString();

                return $"{date.Hour}:{minutes}:{seconds}";
            }
            catch (Exception ex)
            {
                //logService.Add(ex, MessageType.Type.ERROR,
                //     MethodBase.GetCurrentMethod().DeclaringType);
                Console.WriteLine(ex.Message + "in " + MethodBase.GetCurrentMethod().DeclaringType);
                return null;
            }
        }
    }
}
