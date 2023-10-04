using dcnet.Handlers;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceProviderSetup
{
    public static IServiceProvider ConfigureServices(IConfiguration config)
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = Discord.GatewayIntents.Guilds | 
                                Discord.GatewayIntents.GuildMessages | 
                                Discord.GatewayIntents.GuildVoiceStates |
                                Discord.GatewayIntents.MessageContent |
                                Discord.GatewayIntents.GuildInvites |
                                Discord.GatewayIntents.GuildMembers |
                                Discord.GatewayIntents.GuildPresences |
                                Discord.GatewayIntents.GuildMessageReactions |
                                Discord.GatewayIntents.DirectMessages
            }))
            .AddSingleton(new CommandService())
            .AddSingleton<BCommandHandler>()
            .BuildServiceProvider();

        return serviceProvider;
    }
}