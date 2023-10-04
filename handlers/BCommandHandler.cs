
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using dcnet.Commands;

namespace dcnet.Handlers
{
    public class BCommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfiguration _config;

        public BCommandHandler(DiscordSocketClient client, CommandService commands, IConfiguration config)
        {
            _commands = commands;
            _client = client;
            _config = config;
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                await _commands.AddModulesAsync(Assembly.GetEntryAssembly() ,services: null);
            }
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            var commandPrefix = _config["Commands:Prefix"] ?? "!";

            int argPos = 0;

            if (!(message.HasStringPrefix(commandPrefix, ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);

            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null
            );
        }
    }
}