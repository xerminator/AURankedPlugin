using AURankedPlugin.Plugins.ImpostorChat.EventListeners;
using Impostor.Api.Events.Managers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AURankedPlugin.Plugins.ImpostorChat
{
    public class ImpostorChatPlugin : IPluginHandler
    {

        public readonly ILogger<ImpostorChatPlugin> _logger;
        public readonly IEventManager _eventManager;

        private IDisposable _unregister;

        public ImpostorChatPlugin(ILogger<ImpostorChatPlugin> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventManager = eventManager;
        }

        public async ValueTask onEnableAsync()
        {
            _logger.LogInformation("ImpostorChatPlugin enabled!");
            _unregister = _eventManager.RegisterListener(new ImpostorChatListener(_logger));
            await Task.CompletedTask;
        }

        public async ValueTask onDisableAsync()
        {
            _logger.LogInformation("ImpostorChatPlugin disabled!");
            _unregister.Dispose();
            await Task.CompletedTask;
        }

    }
}
