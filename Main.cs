using AURankedPlugin.Modules.LockedGameSettings;
using AURankedPlugin.Plugins;
using AURankedPlugin.Plugins.ChatAnnounce;
using AURankedPlugin.Plugins.ImpostorChat;
using AURankedPlugin.Plugins.MatchLog;
using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AURankedPlugin
{

    [ImpostorPlugin(
        id: "AmongsUsRankedPlugin"
        )]

    public class Main : PluginBase
    {

        private readonly ILogger<Main> _logger;
        private readonly IEventManager _eventManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<IPluginHandler> _pluginHandlers = new List<IPluginHandler>();
        private int version = 1;


        public Main(ILogger<Main> logger, IEventManager eventManager, IServiceProvider serviceProvider)
        {

            _logger = logger;
            _eventManager = eventManager;
            _serviceProvider = serviceProvider;
            initialize();
        }

        public void initialize() 
        {
            RegisterPluginHandlers();
        }

        public override ValueTask EnableAsync()
        {

            _logger.LogInformation($"AURankedPlugin {version.ToString()} has been enabled");

            foreach(var plugin in _pluginHandlers) {
                plugin.onEnableAsync();
            }

            return base.EnableAsync();
        }

        public override ValueTask DisableAsync()
        {

            _logger.LogInformation($"AURankedPlugin {version.ToString()} has been disabled");

            foreach (var plugin in _pluginHandlers)
            {
                plugin.onDisableAsync();
            }

            return base.DisableAsync();
     
        }

        private void RegisterPluginHandlers() 
        {
            //_pluginHandlers.Add(_serviceProvider.GetRequiredService<LockedGameSettingsPlugin>());
            //_pluginHandlers.Add(_serviceProvider.GetRequiredService<MatchLogPlugin>());

            var lockedGameSettingsPlugin = _serviceProvider.GetRequiredService<LockedGameSettingsPlugin>();
            if (lockedGameSettingsPlugin == null)
            {
                _logger.LogError("Failed to resolve LockedGameSettingsPlugin");
                return;
            }
            _pluginHandlers.Add(lockedGameSettingsPlugin);

            var matchLogPlugin = _serviceProvider.GetRequiredService<MatchLogPlugin>();
            if (matchLogPlugin == null)
            {
                _logger.LogError("Failed to resolve MatchLogPlugin");
                return;
            }
            _pluginHandlers.Add(matchLogPlugin);

            var impostorChatPlugin= _serviceProvider.GetRequiredService<ImpostorChatPlugin>();
            if (impostorChatPlugin == null)
            {
                _logger.LogError("Failed to resolve ImpostorChatPlugin");
                return;
            }

            _pluginHandlers.Add(impostorChatPlugin);

            var chatAnnouncePlugin = _serviceProvider.GetRequiredService<ChatAnnouncePlugin>();
            if (chatAnnouncePlugin == null)
            {
                _logger.LogError("Failed to resolve ChatAnnouncePlugin");
                return;
            }
            _pluginHandlers.Add(chatAnnouncePlugin);
        }


    }
}
