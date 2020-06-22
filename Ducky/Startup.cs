using Ducky.Helpers;
using Ducky.Helpers.Twitch;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Ducky
{
    public class Startup
    {
        public static string streamerName { get; set; } = "loeya";
        private static TwitchChatBot twitchbot;
        private static ViewersHelper viewhelp;
        private static LogService logservice;
        private static ExperienceEditor expEditor;

        public static TwitchChatBot GetBot()
        {
            if (twitchbot == null)
            {
                twitchbot = new TwitchChatBot();
                return twitchbot;
            }
            else
                return twitchbot;
        }
        public static ViewersHelper GetViewer()
        {
            if (viewhelp == null)
            {
                viewhelp = new ViewersHelper();
                return viewhelp;
            }
            else
                return viewhelp;
        }
        public static LogService GetLog()
        {
            if (logservice == null)
            {
                logservice = new LogService();
                return logservice;
            }
            else
                return logservice;
        }
        public static ExperienceEditor GetEditor()
        {
            if (expEditor == null)
            {
                expEditor = new ExperienceEditor();
                return expEditor;
            }
            else
                return expEditor;
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;


            twitchbot = twitchbot ?? new TwitchChatBot();
            viewhelp = viewhelp ?? new ViewersHelper();
            logservice = logservice ?? new LogService();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddMvc().AddJsonOptions(options => { options.SerializerSettings.Formatting = Formatting.Indented; });

            services.AddMvc().AddRazorPagesOptions(options =>
            {
                options.RootDirectory = "/Pages";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseStaticFiles();
        }

    }
}
