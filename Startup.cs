using AURankedPlugin.Modules.LockedGameSettings;
using AURankedPlugin.Plugins.ChatAnnounce;
using AURankedPlugin.Plugins.ImpostorChat;
using AURankedPlugin.Plugins.MatchLog;
using Impostor.Api.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AURankedPlugin
{
    internal class Startup : IPluginStartup
    {
        public void ConfigureHost(IHostBuilder host)
        {
            //throw new NotImplementedException();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //throw new NotImplementedException();
            services.AddTransient<LockedGameSettingsPlugin>();
            services.AddTransient<MatchLogPlugin>();
            services.AddTransient<ImpostorChatPlugin>();
            services.AddTransient<ChatAnnouncePlugin>();
        }
    }
}
