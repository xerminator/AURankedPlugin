﻿using AURankedPlugin.Models;
using AURankedPlugin.Plugins;
using AURankedPlugin.Plugins.LockedGameSettings.EventListeners;
using Impostor.Api.Events;
using Impostor.Api.Events.Managers;
using Impostor.Api.Innersloth.GameOptions;
using Impostor.Api.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace AURankedPlugin.Modules.LockedGameSettings
{
    public sealed class LockedGameSettingsPlugin : IPluginHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LockedGameSettingsPlugin> _logger;
        private readonly IEventManager _eventManager;

        private MultiDisposable? _disposable;

        public LockedGameSettingsPlugin(IServiceProvider serviceProvider, ILogger<LockedGameSettingsPlugin> logger, IEventManager eventManager)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _eventManager = eventManager;
        }

        private string ConfigDirectoryPath = Path.Combine(Environment.CurrentDirectory, "config");
        private const string ConfigPath = "lobby.json";
        private const string RoleValuesConfigPath = "roles.json";

        private NormalGameOptions LoadConfig()
        {

            var config_path = Path.Combine(ConfigDirectoryPath, ConfigPath);

            var jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreReadOnlyProperties = true,
                Converters = {
                new JsonStringEnumConverter(),
            },
                WriteIndented = true
            };

            NormalGameOptions options;

            if (File.Exists(config_path))
            {
                options = JsonSerializer.Deserialize<NormalGameOptions>(File.ReadAllText(config_path), jsonSerializerOptions)
                           ?? throw new Exception("Deserialized options but they were null");

                _logger.LogInformation("Loaded options from {Path}", config_path);
            }
            else
            {
                options = new NormalGameOptions();
                File.WriteAllText(config_path, JsonSerializer.Serialize(options, jsonSerializerOptions));

                _logger.LogWarning("Saved default options to {Path}", config_path);
            }

            return options;
        }

        private RoleValues LoadConfigRoles()
        {
            var config_path = Path.Combine(ConfigDirectoryPath, RoleValuesConfigPath);

            var jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreReadOnlyProperties = true,
                Converters = {
                new JsonStringEnumConverter(),
            },
                WriteIndented = true
            };

            //NormalGameOptions options;
            RoleValues options;

            if (File.Exists(config_path))
            {
                options = JsonSerializer.Deserialize<RoleValues>(File.ReadAllText(config_path), jsonSerializerOptions)
                           ?? throw new Exception("Deserialized rolevalues but they were null");

                _logger.LogInformation("Loaded rolevalues from {Path}", config_path);
            }
            else
            {
                options = new RoleValues();
                File.WriteAllText(config_path, JsonSerializer.Serialize(options, jsonSerializerOptions));

                _logger.LogWarning("Saved default rolevalues to {Path}", config_path);
            }

            return options;
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
            var options = LoadConfig();
            var rolevalues = LoadConfigRoles();
            _logger.LogInformation($"LockedGameSettingsPlugin has been loaded!");

            _disposable = new MultiDisposable(
                _eventManager.RegisterListener(ActivatorUtilities.CreateInstance<LockedGameSettingsListener>(_serviceProvider, options, rolevalues))
            );

            await Task.CompletedTask;
        }

        public async ValueTask onDisableAsync()
        {
            _disposable?.Dispose();
            _logger.LogInformation($"LockedGameSettingsPlugin has been unloaded!");

            await Task.CompletedTask;
        }
    }
}
