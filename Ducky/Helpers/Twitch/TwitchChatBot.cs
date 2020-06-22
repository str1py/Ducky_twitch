using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace Ducky.Helpers.Twitch
{
    public class TwitchChatBot
    {
        private Timer _timer;
        private static string _botName = "Anulyev";
        //private static string _twitchOAuth = "oauth:mu770dm6h0adoqeynbxa19x46hf2gy"; // get chat bot's oauth from www.twitchapps.com/tmi/
        // private static string _clientId = "vlf9a4486ky1mxdp7zeiedskxkxh8j";
        private static string accesstoken = "nnf0v2j5f2x0tl33mxedzfjt681mk7";
        private static string refreshtoken = "62gtduw7czvlwem5u2vmu9osiz7r1rcu2ynvch67cwm8s0630p";
        TwitchClient client;
        ConnectionCredentials credentials = new ConnectionCredentials(_botName, accesstoken);
        TwitchUserDataHelper dataHelper = new TwitchUserDataHelper();
        ExperienceEditor db = Startup.GetEditor();
        LogService logService = Startup.GetLog();
        static HttpClient clienthttp = new HttpClient();
        private int startscount = 0;
        ViewersHelper vh = new ViewersHelper();

        public TwitchChatBot()
        {
            Connect();
            StartAsync();
        }

        public void Connect()
        {
            client = new TwitchClient();
            client.Initialize(credentials, Startup.streamerName, '\0', '\0', true);

            client.Connect();
            var emotes = client.ChannelEmotes;
            var name = client.TwitchUsername;
            var c = client.JoinedChannels.Count();

            var b = client.JoinedChannels;
            if (client.IsConnected)
            {
                client.OnConnected += Client_OnConnected;
                client.OnMessageReceived += Client_OnMessageReceived;
                client.OnChannelStateChanged += Client_OnChannelStateChanged;

                client.OnConnectionError += Client_OnConnectionError;
                client.OnDisconnected += Client_OnDisconnected;
                client.OnIncorrectLogin += Client_OnIncorrectLogin;

                client.OnCommunitySubscription += Client_OnCommunitySubscription;
                client.OnNewSubscriber += Client_OnNewSubscriber;
                client.OnReSubscriber += Client_OnReSubscriber;
                client.OnGiftedSubscription += Client_OnGiftedSubscription;
                client.OnMessageSent += Client_OnMessageSent;

               //client.OnLog += Client_OnLog;
            }
        }

        public void Disconnect()
        {
            client.Disconnect();
        }

        public async Task RefreshToken()
        {
            string link = $"https://twitchtokengenerator.com/api/refresh/{refreshtoken}";
            string responsestring;
            HttpResponseMessage response = await clienthttp.GetAsync(link);
            if (response.IsSuccessStatusCode)
                responsestring = await response.Content.ReadAsStringAsync();
            else
                responsestring = "";

            var o = JObject.Parse(responsestring);
            accesstoken = (string)o["token"];

            logService.Add($"Token has been resreshed", MessageType.Type.EVENT,
                MethodBase.GetCurrentMethod().DeclaringType);
        }
        #region ON_Methods
        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            logService.Add($"{ e.AutoJoinChannel}", MessageType.Type.EVENT,
                MethodBase.GetCurrentMethod().DeclaringType);

            // client.SendMessage(Startup.streamerName, "olyashHailmedoed olyashHailmedoed olyashHailmedoed");
        }

        private void Client_OnMessageSent(object sender, OnMessageSentArgs e)
        {
            logService.Add($"{e.SentMessage.DisplayName}: {e.SentMessage.Message}", MessageType.Type.EVENT,
                MethodBase.GetCurrentMethod().DeclaringType);
        }

        private void Client_OnIncorrectLogin(object sender, OnIncorrectLoginArgs e)
        {
            logService.Add($"{e.Exception.Message} {e.Exception.Data}", MessageType.Type.ERROR,
                    MethodBase.GetCurrentMethod().DeclaringType);
        }

        private void Client_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            try
            {
                logService.Add($"Reconnecting......", MessageType.Type.EVENT,
                      MethodBase.GetCurrentMethod().DeclaringType);
                client.Reconnect();
                if (client.IsConnected == false)
                {
                    credentials = new ConnectionCredentials("Anulyev", accesstoken);
                    client.Initialize(credentials, Startup.streamerName, '\0', '\0', true);
                    client.Connect();
                    if (client.JoinedChannels.Count == 0)
                    {
                        Debug.WriteLine($"Is Connected : {client.IsConnected}. No connected channels . Lets try to connect");
                        client.JoinChannel(Startup.streamerName);
                    }

                    logService.Add($"Bot connection : {client.IsConnected}. Connected to {client.JoinedChannels[0]?.Channel}", MessageType.Type.DEBUGINFO,
                             MethodBase.GetCurrentMethod().DeclaringType);
                }
            }
            catch (Exception ex)
            {
                logService.Add($"{ex.Message}", MessageType.Type.EVENT,
                                    MethodBase.GetCurrentMethod().DeclaringType);
            }
        }

        private async void Client_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            try
            {
                await RefreshToken();
                credentials = new ConnectionCredentials("Anulyev", accesstoken);
                client.Initialize(credentials, Startup.streamerName, '\0', '\0', true);
                client.Reconnect();
            }
            catch (Exception ex)
            {
                logService.Add($"{ex.Message} | {e.Error.Message}", MessageType.Type.ERROR,
                MethodBase.GetCurrentMethod().DeclaringType);
                await RefreshToken();
                credentials = new ConnectionCredentials("Anulyev", accesstoken);
                client.Initialize(credentials, Startup.streamerName, '\0', '\0', true);
                client.Reconnect();
                logService.Add(ex, MessageType.Type.DEBUGINFO,
                            MethodBase.GetCurrentMethod().DeclaringType);

            }
        }

        private void Client_OnChannelStateChanged(object sender, OnChannelStateChangedArgs e)
        {
            logService.Add($"{e.ChannelState.Channel}", MessageType.Type.DEBUGINFO,
                              MethodBase.GetCurrentMethod().DeclaringType);
        }


        private async void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            try
            {
                var user = dataHelper.GetUserFromMessage(e);
                db.AddActiveChatUsers(e.ChatMessage.DisplayName);
                await db.EditExpViaMessage(user, e.ChatMessage.Message);

                if (e.ChatMessage.Message.Contains('!'))
                {
                    string answer = await db.RunCommnamd(user, e.ChatMessage.Message);
                    if (answer != null)
                        client.SendMessage(Startup.streamerName, $"#{e.ChatMessage.DisplayName} {answer}");
                }
            }
            catch (Exception ex)
            {
                logService.Add(ex, MessageType.Type.ERROR,
                       MethodBase.GetCurrentMethod().DeclaringType);
            }
        }

        private void Client_OnReSubscriber(object sender, OnReSubscriberArgs e)
        {
            try
            {
                var user = dataHelper.GetUserFromReSub(e);
                db.EditExpViaReSub(user);
                logService.Add($"RESUB {e.ReSubscriber.DisplayName}", MessageType.Type.RESUB,
                           MethodBase.GetCurrentMethod().DeclaringType);
                //  client.SendMessage(Startup.streamerName, "olyashFonarik olyashFonarik olyashFonarik olyashFonarik");
            }
            catch (Exception ex)
            {
                logService.Add(ex, MessageType.Type.ERROR,
                            MethodBase.GetCurrentMethod().DeclaringType);
            }
        }

        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            try
            {
                var user = dataHelper.GetUserFromNewSub(e);
                db.EditExpViaNewSub(user);
                logService.Add($"{e.Subscriber.DisplayName}", MessageType.Type.NEWSUB,
                   MethodBase.GetCurrentMethod().DeclaringType);
                //  client.SendMessage(Startup.streamerName, "olyashFonarik olyashFonarik olyashFonarik olyashFonarik");
            }
            catch (Exception ex)
            {
                logService.Add(ex, MessageType.Type.ERROR,
                     MethodBase.GetCurrentMethod().DeclaringType);
            }
        }

        private async void Client_OnCommunitySubscription(object sender, OnCommunitySubscriptionArgs e)
        {
            try
            {
                if (e.GiftedSubscription.DisplayName != "AnAnonymousGifter")
                {
                    var user = await dataHelper.GetUserFromCommunitySubs(e);
                    db.EditExpViaCommunitySubGifts(user, e.GiftedSubscription.MsgParamMassGiftCount);
                    logService.Add($":{e.GiftedSubscription.DisplayName} Gift count: {e.GiftedSubscription.MsgParamMassGiftCount}. Gift in Total:{e.GiftedSubscription.MsgParamSenderCount}", MessageType.Type.EVENT,
                                 MethodBase.GetCurrentMethod().DeclaringType);
                    // client.SendMessage(Startup.streamerName, "olyashFonarik olyashFonarik olyashFonarik olyashFonarik");
                }
                else
                {
                    logService.Add($" Anon sen GIFTS!!!!: {e.GiftedSubscription.MsgParamMassGiftCount}", MessageType.Type.ANONGIFT,
                                      MethodBase.GetCurrentMethod().DeclaringType);
                }
            }
            catch (Exception ex)
            {
                logService.Add(ex, MessageType.Type.ERROR,
                     MethodBase.GetCurrentMethod().DeclaringType);
            }
            //client.SendMessage(_broadcasterName, "olyashFonarik olyashFonarik olyashFonarik olyashFonarik");
        }

        private async void Client_OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            try
            {
                if (e.GiftedSubscription.DisplayName != "AnAnonymousGifter")
                {
                    var user = await dataHelper.GetUserFromGifted(e);
                    db.EditExpViaGiftedSub(user);
                    logService.Add($":{e.GiftedSubscription.DisplayName}", MessageType.Type.SUBGIFT,
                               MethodBase.GetCurrentMethod().DeclaringType);
                }
                else
                {
                    logService.Add($":{e.GiftedSubscription.DisplayName}", MessageType.Type.ANONGIFT,
                              MethodBase.GetCurrentMethod().DeclaringType);
                }
                //    client.SendMessage(Startup.streamerName, "olyashFonarik olyashFonarik olyashFonarik olyashFonarik");
            }
            catch (Exception ex)
            {
                logService.Add(ex, MessageType.Type.ERROR,
                     MethodBase.GetCurrentMethod().DeclaringType);
            }
        }
        #endregion

        public Task StartAsync()
        {
            logService.Add($"Timed Background Service is starting.", MessageType.Type.DEBUGINFO,
                        MethodBase.GetCurrentMethod().DeclaringType);

            _timer = new Timer(DoWork, null, 0, 600000);
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            try
            {
                #region DEBUGINFO
                string link = "https://duckyweb.azurewebsites.net/log";
                HttpResponseMessage response = await clienthttp.GetAsync(link);
                Debug.WriteLine($"Request to https://duckyweb.azurewebsites.net/log --- {response.StatusCode.ToString()}");

                Debug.WriteLine("--------------------");
                Debug.WriteLine(client.TwitchUsername);
                if (client.JoinedChannels.Count == 0)
                {
                    Debug.WriteLine($"Is Connected : {client.IsConnected}. No connected channels . Lets try to connect");
                    client.JoinChannel(Startup.streamerName);
                }
                else
                    Debug.WriteLine($"Is Connected : {client.IsConnected}. Connected to {client.JoinedChannels[0]?.Channel}");

                Debug.WriteLine($"Is Initialized : {client.IsInitialized}");
                Debug.WriteLine($"Version : {client.Version}");


                logService.Add($"Bot connection : {client.IsConnected}. Connected to {client.JoinedChannels[0]?.Channel}. Bot status - {response.StatusCode.ToString()}", MessageType.Type.DEBUGINFO,
                         MethodBase.GetCurrentMethod().DeclaringType);

                #endregion

                if (startscount > 0)
                {
                    var viewers = await vh.GetViewers($"{Startup.streamerName}");
                    List<string> viewersnow = new List<string>();
                    var viewersparse = viewers["Viewers"];
                    foreach (string name in viewersparse)
                    {
                        viewersnow.Add(name);
                    }

                    db.SetActivity();
                    //db.EditExpViaTime(viewersnow);
                }
                else startscount++;
            }
            catch (Exception ex)
            {
                logService.Add(ex, MessageType.Type.ERROR,
                     MethodBase.GetCurrentMethod().DeclaringType);
            }
        }
    }
}
