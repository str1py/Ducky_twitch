using Ducky.Contexts;
using Ducky.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ducky.Helpers
{
    public class ExperienceEditor
    {
        List<string> viewersAgo = new List<string>();
        public static List<string> activechatusers = new List<string>();
        public List<string> explist = new List<string>();

        private readonly int messageexp = 10;
        private readonly int suborvipexp = 2;
        private readonly int subvipexp = 3;

        private readonly int timeexp = 20;
        private readonly int resubexp = 2000;
        private readonly int subgiftexp = 3500;

        LogService logService = Startup.GetLog();
        TwitchUserDataHelper dataHelper = new TwitchUserDataHelper();
        RequestHelper reqHelper = new RequestHelper();
        ViewersHelper vh = new ViewersHelper();

        private readonly int MaxDegreeOfParallelism = 6;


        public async Task EditExpViaMessage(TwitchUserModel user, string message)
        {
            try
            {
                using (DataBaseContext context = new DataBaseContext())
                {
                    try
                    {
                        var userfromdb = await context?.Users?.SingleOrDefaultAsync(x => x.userName == user.userName);
                        //Ecли пользователь есть в БД
                        if (userfromdb != null)
                        {
                            //Если в сообщение есть обращение к стримеру
                            if (message.Contains($"@{Startup.streamerName}") || message.Contains($"{Startup.streamerName}"))
                                userfromdb.messagestostreamer += 1;

                            //+1 к кол-ву сообщений 
                            userfromdb.messagescount += 1;
                            //кол-во месяцев подписки
                            userfromdb.subMounthsCount = user.subMounthsCount;
                            //опыт из сообщения
                            userfromdb.exp += CalculateMessageExp(userfromdb);
                            //Лвл из кол-ва опыта
                            userfromdb.userlevel = GetLevelFromExp(userfromdb.userName, userfromdb.exp, context, CalculateMessageExp(userfromdb));

                            await context.SaveChangesAsync();
                        }
                        //Если пользователя нет 
                        else
                            await AddNewUser(user);
                    }
                    catch (Exception e)
                    {
                        logService.Add($"{e.Message}", MessageType.Type.ERROR,
                       MethodBase.GetCurrentMethod().DeclaringType);
                    }
                }
            }
            catch (Exception e)
            {
                logService.Add($"{e.Message}", MessageType.Type.ERROR,
                       MethodBase.GetCurrentMethod().DeclaringType);
            }
        }
        public async void EditExpViaNewSub(TwitchUserModel user)
        {
            using (DataBaseContext context = new DataBaseContext())
            {
                try
                {
                    var userfromdb = context?.Users?.SingleOrDefault(x => x.userId == user.userId) ?? null;
                    //Если пользователя есть в ДБ
                    if (userfromdb != null)
                    {
                        //Добавить опыт за подписку ресаб умноженное на сабплан
                        userfromdb.exp += (resubexp * Int32.Parse(user.SubPlan));
                        userfromdb.userlevel = GetLevelFromExp(userfromdb.userName, userfromdb.exp, context, resubexp * Int32.Parse(user.SubPlan));
                        userfromdb.SubPlan = user.SubPlan;
                        await context.SaveChangesAsync();
                        logService.Add($"{ userfromdb.userName } +{resubexp}xp for NEWSUB", MessageType.Type.NEWSUB,
                            MethodBase.GetCurrentMethod().DeclaringType);


                    }
                    else
                    {
                        await AddNewUser(user);
                        var newuserfromdb = context?.Users?.SingleOrDefault(x => x.userId == user.userId) ?? null;
                        newuserfromdb.exp += (resubexp * Int32.Parse(user.SubPlan));
                        newuserfromdb.userlevel = GetLevelFromExp(newuserfromdb.userName, newuserfromdb.exp, context, resubexp * Int32.Parse(user.SubPlan));
                        newuserfromdb.SubPlan = user.SubPlan;
                        await context.SaveChangesAsync();

                        logService.Add($"{ newuserfromdb.userName } +{resubexp}xp for NEWSUB", MessageType.Type.NEWSUB,
                         MethodBase.GetCurrentMethod().DeclaringType);
                    }
                }
                catch (Exception ex)
                {
                    logService.Add($"{ex.Message}", MessageType.Type.ERROR,
                   MethodBase.GetCurrentMethod().DeclaringType);
                }
            }
        }
        public async void EditExpViaReSub(TwitchUserModel user)
        {
            try
            {
                using (DataBaseContext context = new DataBaseContext())
                {
                    try
                    {
                        var userfromdb = context?.Users?.SingleOrDefault(x => x.userId == user.userId) ?? null;
                        if (userfromdb != null)
                        {
                            userfromdb.exp += (resubexp * Int32.Parse(user.SubPlan));
                            userfromdb.userlevel = GetLevelFromExp(userfromdb.userName, userfromdb.exp, context, resubexp * Int32.Parse(user.SubPlan));
                            userfromdb.SubPlan = user.SubPlan;
                            await context.SaveChangesAsync();

                            logService.Add($"{ userfromdb.userName } +{resubexp}xp for RESUB", MessageType.Type.RESUB,
                                MethodBase.GetCurrentMethod().DeclaringType);
                        }
                        else
                        {
                            await AddNewUser(user);
                            var newuserfromdb = context?.Users?.SingleOrDefault(x => x.userId == user.userId) ?? null;
                            newuserfromdb.exp += (resubexp * newuserfromdb.subMounthsCount * Int32.Parse(newuserfromdb.SubPlan));
                            newuserfromdb.userlevel = GetLevelFromExp(newuserfromdb.userName, newuserfromdb.exp, context, 0);
                            await context.SaveChangesAsync();

                            logService.Add($"{newuserfromdb.userName } +{resubexp * newuserfromdb.subMounthsCount }xp for SUB({newuserfromdb.subMounthsCount})", MessageType.Type.RESUB,
                              MethodBase.GetCurrentMethod().DeclaringType);
                        }
                    }
                    catch (Exception e)
                    {
                        logService.Add($"{e.InnerException}", MessageType.Type.ERROR,
                         MethodBase.GetCurrentMethod().DeclaringType);
                    }
                }
            }
            catch (Exception e)
            {
                logService.Add($"{e.Message}", MessageType.Type.ERROR,
                        MethodBase.GetCurrentMethod().DeclaringType);
            }
        }
        public async void EditExpViaCommunitySubGifts(TwitchUserModel user, int giftscount)
        {
            try
            {
                using (DataBaseContext context = new DataBaseContext())
                {
                    try
                    {
                        var userfromdb = context?.Users?.SingleOrDefault(x => x.userName == user.userName);
                        if (userfromdb != null)
                        {
                            if (userfromdb.giftsCount == 0)
                            {
                                userfromdb.giftsCount = user.giftsCount;
                                userfromdb.exp += subgiftexp * userfromdb.giftsCount;

                                logService.Add($"{userfromdb.userName} +{subgiftexp * user.giftsCount}xp for {giftscount} Sub gift at all", MessageType.Type.SUBGIFT,
                                      MethodBase.GetCurrentMethod().DeclaringType);
                                await context.SaveChangesAsync();
                            }
                            else
                            {
                                userfromdb.exp += subgiftexp * giftscount;
                                userfromdb.SubPlan = user.SubPlan;
                                userfromdb.giftsCount += giftscount;
                                userfromdb.userlevel = GetLevelFromExp(userfromdb.userName, userfromdb.exp, context, subgiftexp * giftscount);

                                await context.SaveChangesAsync();

                                logService.Add($"{userfromdb.userName} +{subgiftexp * giftscount}xp for {giftscount} Sub gift", MessageType.Type.SUBGIFT,
                                 MethodBase.GetCurrentMethod().DeclaringType);
                            }
                        }
                        else
                        {
                            await AddNewUser(user);
                            var newuserfromdb = context?.Users?.SingleOrDefault(x => x.userName == user.userName);
                            newuserfromdb.exp += subgiftexp * newuserfromdb.giftsCount;
                            await context.SaveChangesAsync();

                            logService.Add($"{newuserfromdb.userName} +{subgiftexp * newuserfromdb.giftsCount}xp for {newuserfromdb.giftsCount} Sub gift", MessageType.Type.SUBGIFT,
                              MethodBase.GetCurrentMethod().DeclaringType);
                        }
                    }
                    catch (Exception e)
                    {
                        logService.Add($"{e.Message}", MessageType.Type.ERROR,
                          MethodBase.GetCurrentMethod().DeclaringType);
                    }
                }
            }
            catch (Exception e)
            {
                logService.Add($"{e.Message}", MessageType.Type.ERROR,
                     MethodBase.GetCurrentMethod().DeclaringType);
            }
        }
        public async void EditExpViaGiftedSub(TwitchUserModel user)
        {
            try
            {
                using (DataBaseContext context = new DataBaseContext())
                {
                    try
                    {
                        var userfromdb = context?.Users?.SingleOrDefault(x => x.userName == user.userName);
                        if (userfromdb != null)
                        {
                            if (userfromdb.giftsCount == 0)
                            {
                                userfromdb.giftsCount = user.giftsCount;
                                userfromdb.exp += subgiftexp * userfromdb.giftsCount;
                                await context.SaveChangesAsync();
                                logService.Add($"{userfromdb.userName} +{subgiftexp * userfromdb.giftsCount}xp for {userfromdb.giftsCount} SubGift  at all", MessageType.Type.SUBGIFT,
                                 MethodBase.GetCurrentMethod().DeclaringType);
                            }
                            else
                            {
                                userfromdb.exp += subgiftexp;
                                await context.SaveChangesAsync();
                                logService.Add($"{userfromdb.userName} +{subgiftexp}xp for SubGift", MessageType.Type.SUBGIFT,
                                  MethodBase.GetCurrentMethod().DeclaringType);
                            }
                        }
                        else
                        {
                            await AddNewUser(user);
                            var newuserfromdb = context?.Users?.SingleOrDefault(x => x.userName == user.userName);
                            newuserfromdb.exp += subgiftexp * newuserfromdb.giftsCount;
                            await context.SaveChangesAsync();
                            logService.Add($"{newuserfromdb.userName} +{subgiftexp * newuserfromdb.giftsCount}xp for {newuserfromdb.giftsCount} Sub gifts", MessageType.Type.SUBGIFT,
                              MethodBase.GetCurrentMethod().DeclaringType);
                        }
                    }
                    catch (Exception e)
                    {
                        logService.Add($" {e.Message}", MessageType.Type.ERROR,
                            MethodBase.GetCurrentMethod().DeclaringType);
                    }
                }
            }
            catch (Exception e)
            {
                logService.Add($" {e.Message}", MessageType.Type.ERROR,
                                          MethodBase.GetCurrentMethod().DeclaringType);
            }
        }
        public async void EditExpViaTime(List<string> viewers)
        {
            if (await dataHelper.IsStreamerOnline())
            {
                try
                {
                    using (DataBaseContext context = new DataBaseContext())
                    {
                        var countact = context.ActivityCount.FirstOrDefault(x => x.id == 1);
                        countact.count += 1;
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception e)
                {
                    logService.Add($" {e.Message}", MessageType.Type.ERROR,
                             MethodBase.GetCurrentMethod().DeclaringType);
                }
            }
            if (viewersAgo.Count == 0)
            {
                var nowviewers = await vh.GetViewers($"{Startup.streamerName}");
                var viewersparse = nowviewers["Viewers"];
                foreach (string name in viewersparse)
                    viewersAgo.Add(name);
            }
            else
            {
                foreach (string name in viewers)
                {
                    if (viewersAgo.Contains(name))
                        explist.Add(name);
                }
                try
                {
                    var options = new ParallelOptions() { MaxDegreeOfParallelism = MaxDegreeOfParallelism };
                    Parallel.ForEach<string>(explist, options, GiveWatchinExpirience);
                }
                catch (Exception e)
                {
                    logService.Add($" {e.Message}", MessageType.Type.ERROR,
                                               MethodBase.GetCurrentMethod().DeclaringType);
                }
                viewersAgo = viewers;
                explist.Clear();
            }
        }
        public void AddActiveChatUsers(string user)
        {
            if (!activechatusers.Contains(user))
                activechatusers.Add(user);
        }

        private int GetLevelFromExp(string name, double exp, DataBaseContext levelcontext, double earnedxp)
        {
            try
            {
                var level = levelcontext.Levels.Where(x => x.exp_total - exp <= 0);

                var levelnow = level.LastOrDefault().lvl;
                var nextlevelexp = levelcontext.Levels.Where(x => x.lvl > levelnow).FirstOrDefault().exp_total;

                logService.Add($"+{earnedxp}xp. {name} lvl {levelnow}. EXP - {exp}/{nextlevelexp}. Next lvl xp - {nextlevelexp - exp}xp", MessageType.Type.STATS,
                               MethodBase.GetCurrentMethod().DeclaringType);

                return levelnow;
            }
            catch (Exception e)
            {
                logService.Add($"{e.Message}", MessageType.Type.ERROR,
                      MethodBase.GetCurrentMethod().DeclaringType);
                return 0;
            }
        }
        private string GetLevelFromExp(TwitchUserModel user)
        {
            using (DataBaseContext levelcontext = new DataBaseContext())
            {
                var level = levelcontext.Levels.Where(x => x.exp_total - user.exp <= 0);

                var levelnow = level.LastOrDefault().lvl;
                var nextlevelexp = levelcontext.Levels.Where(x => x.lvl > levelnow).FirstOrDefault().exp_total;

                return $"{user.userName} lvl {levelnow}. EXP - {user.exp}/{nextlevelexp}. Next lvl xp - {nextlevelexp - user.exp}xp";
            }
        }

        private int CalculateMessageExp(TwitchUserModel user)
        {
            if (user.isSub == true && user.isVip == true)
                return messageexp * subvipexp;
            else if (user.isSub == true || user.isVip == true)
                return messageexp * suborvipexp;
            else return messageexp;
        }

        private async void GiveWatchinExpirience(string name)
        {
            try
            {
                using (DataBaseContext context = new DataBaseContext())
                {
                    var users = await context?.Users?.SingleOrDefaultAsync(x => x.userName == name) ?? null;
                    if (users != null)
                    {
                        users.exp += timeexp;
                        users.watchtime += 5;
                        if (activechatusers.Contains(users.userName))
                            users.userActivity += 1;
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception e)
            {
                logService.Add($"{e.Message}", MessageType.Type.ERROR,
                MethodBase.GetCurrentMethod().DeclaringType);
            }
        }
        public async void SetActivity()
        {
            try
            {
                if (await dataHelper.IsStreamerOnline())
                {
                    using (DataBaseContext context = new DataBaseContext())
                    {
                        var streaminfo = await dataHelper.GetChannelInfo(Startup.streamerName);
                        var viewerscount = streaminfo.Where(y => y.Key == "viewers").Select(x => x.Value).FirstOrDefault();
                        var activeviewersscount = ExperienceEditor.activechatusers.Count().ToString();
                        decimal activity;
                        if (Int32.Parse(activeviewersscount) != 0 && Int32.Parse(viewerscount) != 0)
                            activity = Math.Round(((decimal)Int32.Parse(activeviewersscount) / (decimal)Int32.Parse(viewerscount)) * 100, 2);
                        else
                            activity = 0;
                        context.Activity.Add(new ActivityModel
                        {
                            vievers = Int32.Parse(viewerscount),
                            activevievers = activechatusers.Count,
                            activity = activity,
                            created_at = DateTime.Now,
                        });
                        activechatusers.Clear();
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception e)
            {
                logService.Add($"{e.Message}", MessageType.Type.ERROR,
               MethodBase.GetCurrentMethod().DeclaringType);
            }
        }


        private async Task AddNewUser(TwitchUserModel user)
        {
            try
            {
                using (DataBaseContext context = new DataBaseContext())
                {
                    var users = await context?.Users?.SingleOrDefaultAsync(x => x.userId == user.userId);
                    if (users == null)
                    {
                        int expirience;
                        if (user.isSub == true && user.subMounthsCount > 0)
                            expirience = resubexp * user.subMounthsCount * Int32.Parse(user.SubPlan);
                        else expirience = 0;

                        context.Add(new TwitchUserModel
                        {
                            userId = user.userId,
                            userName = user.userName,
                            userType = user.userType,
                            SubPlan = user.SubPlan,
                            isVip = user.isVip,
                            isSub = user.isSub,
                            isModer = user.isModer,
                            created_at = DateTime.Now,
                            exp = expirience,
                            giftsCount = user.giftsCount,
                            subMounthsCount = user.subMounthsCount,
                            userActivity = 0,
                            messagescount = 0,
                            messagestostreamer = 0,
                            watchtime = 0,
                            userlevel = GetLevelFromExp(user.userName, expirience, context, expirience),
                        });
                        await context.SaveChangesAsync();

                        logService.Add($"New user : {user.userName}", MessageType.Type.STATS,
                    MethodBase.GetCurrentMethod().DeclaringType);
                    }
                }
            }
            catch (Exception e)
            {
                logService.Add($"{e.Message}", MessageType.Type.ERROR,
                 MethodBase.GetCurrentMethod().DeclaringType);
            }
        }

        public async Task<string> RunCommnamd(TwitchUserModel user, string message)
        {
            if (message == "!stats")
            {
                // return GetLevelFromExp(user);
                return null;
            }
            else
                return null;
        }
    }
}
