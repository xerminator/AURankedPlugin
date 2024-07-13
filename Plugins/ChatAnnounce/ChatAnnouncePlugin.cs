using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AURankedPlugin.Plugins.ChatAnnounce;
public class ChatAnnouncePlugin : IPluginHandler
{
    private readonly ILogger<ChatAnnouncePlugin> _logger;
    private readonly IEventManager _eventManager;
    private IDisposable _unregister;

    private string ConfigDirectoryPath = Path.Combine(Environment.CurrentDirectory, "config");
    private const string ConfigPath = "announce.json";


    public ChatAnnouncePlugin(ILogger<ChatAnnouncePlugin> logger, IEventManager eventManager)
    {
        _logger = logger;
        _eventManager = eventManager;
    }

    public ChatAnnounceConfig LoadConfig()
    {
        string config_path = Path.Combine(ConfigDirectoryPath, ConfigPath);
        ChatAnnounceConfig config;

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        if (File.Exists(config_path))
        {
            config = JsonSerializer.Deserialize<ChatAnnounceConfig>(File.ReadAllText(config_path));
        }
        else
        {
            config = new ChatAnnounceConfig();
            File.WriteAllText(config_path, JsonSerializer.Serialize(config, options));
        }
        return config;

    }

    public void initialize()
    {
        bool directoryExists = Directory.Exists(ConfigDirectoryPath);

        if (!directoryExists)
        {
            Directory.CreateDirectory(ConfigDirectoryPath);
        }
    }


    public async ValueTask onEnableAsync()
    {
        initialize();
        var config = LoadConfig();

        _logger.LogInformation("ChatAnnouncePlugin is enabled.");
        _unregister = _eventManager.RegisterListener(new ChatAnnounceListener(_logger, config));
        await Task.CompletedTask;
    }

    public async ValueTask onDisableAsync()
    {
        _logger.LogInformation("ChatAnnouncePlugin is disabled.");
        _unregister.Dispose();
        await Task.CompletedTask;
    }

    public ValueTask ReloadAsync()
    {
        throw new NotImplementedException();
    }
}