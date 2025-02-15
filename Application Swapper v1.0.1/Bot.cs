using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Application_Swapper
{
    public class Bot
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandHandler _commandHandler;

        public Bot()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.Guilds | GatewayIntents.GuildMessages
            });

            _commandHandler = new CommandHandler(_client);
        }

        public async Task RunAsync()
        {
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;

            await _client.LoginAsync(TokenType.Bot, "token");
            await _client.StartAsync();
            await _commandHandler.InitializeAsync();
            await Task.Delay(-1);
        }

        private async Task ReadyAsync()
        {
            Console.WriteLine("Bot is online!");
            await _commandHandler.RegisterSlashCommandsAsync();
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }
    }
}
