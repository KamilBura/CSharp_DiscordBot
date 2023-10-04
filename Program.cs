using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using dcnet.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace dcnet 
{   
    public class Program
    {
        private readonly IServiceProvider _serviceProvider;

        private IConfiguration _config;
        private DiscordSocketClient _client;
        private CommandService _commands;

        public Program()
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config/config.json")
                .Build();

            var discordSocketConfig = new DiscordSocketConfig
            {
                MessageCacheSize = _config.GetValue<int>("Discord:MessageCacheSize", 100)
            };

            _client = new DiscordSocketClient(discordSocketConfig);
            _commands = new CommandService();
            _serviceProvider = ServiceProviderSetup.ConfigureServices(_config);

            new LoggingService(_client, _commands);
    }


    public static void Main(string[] args)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    public async Task MainAsync()
    {
        using (var services = _serviceProvider.CreateScope())
        {
            var client = services.ServiceProvider.GetRequiredService<DiscordSocketClient>();
            var commandHandler = services.ServiceProvider.GetRequiredService<BCommandHandler>();

            _client.Log += Log;
            _client.MessageUpdated += MessageUpdated;

            var token = _config["Main:token"];
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await commandHandler.InstallCommandsAsync();

            await SetRichPresence();

            await Task.Delay(-1);

        }
    }

    private Task Log(LogMessage message)
    {
        return Task.CompletedTask;
    }

    private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
    {
        var message = await before.GetOrDownloadAsync();
        Console.WriteLine($"{message} -> {after}");
    }

    private async Task SetRichPresence()
    {
        var presenceText = _config["Presence:Text"] ?? "Error 404❗";
        var presenceTypeString = _config["Presence:Type"]?.ToLower() ?? "listening";
        ActivityType presenceType;

        switch (presenceTypeString)
        {
            case "listening":
                presenceType = ActivityType.Listening;
                break;
            case "playing":
                presenceType = ActivityType.Playing;
                break;
            case "streaming":
                presenceType = ActivityType.Streaming;
                break;
            case "watching":
                presenceType = ActivityType.Watching;
                break;
            default:
                presenceType = ActivityType.Listening;
                break;
        }
        var presenceStatusString = _config["Presence:Status"] ?? "Online";
        var presenceStatus = Enum.TryParse(presenceStatusString, true, out UserStatus status) ? status : UserStatus.Online;

        await _client.SetGameAsync(presenceText, type: presenceType);
        await _client.SetStatusAsync(presenceStatus);
    }

}
}
