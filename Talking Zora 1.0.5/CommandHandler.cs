using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Application_Swapper
{
    internal class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly SlashCommandRegistrar _slashCommandRegistrar;

        // declare things for functions
        private readonly ulong _guildId = 1335950055110082591; // DL ID
        private readonly ulong _modChannelId = 1337883290463371304; // modbotapp ID
        private readonly ulong _appChannelId = 1335950203769065492; // charapp ID
        private readonly ulong _charDiscId = 1335950203769065490; // chardisc ID
        private ulong _fightMessageId;

        public CommandHandler(DiscordSocketClient client)
        {
            _client = client;
            _interactions = new InteractionService(_client);
            _client.InteractionCreated += InteractionCreatedAsync;
            _slashCommandRegistrar = new SlashCommandRegistrar(_client);
        }

        public async Task InitializeAsync()
        {
            await _slashCommandRegistrar.RegisterSlashCommandsAsync();
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        private async Task InteractionCreatedAsync(SocketInteraction interaction)
        {
            if (interaction is not SocketSlashCommand slashCommand) return;

            var context = new SocketInteractionContext(_client, interaction);

            // Handle the command here directly
            await HandleSCommandAsync(slashCommand);
        }

        public async Task HandleSCommandAsync(SocketSlashCommand slashCommand)
        {
            var guild = (slashCommand.Channel as SocketGuildChannel)?.Guild;

            switch (slashCommand.Data.Name)
            {
                case "speak":
                    // Extract the channel option as a channel object
                    var channelOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "channel")?.Value as IChannel;
                    // Extract the message option
                    var messageOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "message")?.Value?.ToString();

                    Console.WriteLine($"Channel option: {channelOption?.Id}, Message option: {messageOption}");

                    if (channelOption != null && channelOption is ITextChannel channelSend && !string.IsNullOrEmpty(messageOption))
                    {
                        // Send the message to the text channel
                        await channelSend.SendMessageAsync(messageOption);
                    }
                    else
                    {
                        // Handle the case where the channel or message is invalid
                        await slashCommand.RespondAsync("Could not find the specified channel or message content.");
                    }
                    break;
            }
            _client.MessageReceived += MessageReceivedAsync;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            // ignore messages from bots
            if (message is not SocketUserMessage userMessage || message.Author.IsBot)
            {
                return;
            }

            ulong appChannelId = 1335950203769065492; // Application submission channel ID
            ulong modChannelId = 1337883290463371304; // Moderator review channel ID

            var channel = message.Channel as SocketTextChannel;
            var guild = channel?.Guild;
            var modChannel = guild?.GetTextChannel(modChannelId);

            if (channel?.Id == appChannelId && modChannel != null)
            {
                // Forward the message content to the mod channel
                var embed = new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithAuthor(message.Author)
                    .WithDescription(message.Content)
                    .WithTimestamp(DateTimeOffset.Now)
                    .WithFooter("Application Submission Forwarded")
                    .Build();

                await modChannel.SendMessageAsync(embed: embed);

                return; // exit to prevent further processing
            }

            // convert message to lowercase to catch all variations
            string messageContent = message.Content.ToLower();

            int argPos = 0;

            if (messageContent.Contains("vidow"))
            {
                string[] responses = { "VIDOW MENTIONED", "vidow,,,", "i was held at gunpoint to add a 'vidow mentioned' meme joke to the bot", "vidow :)", "guys its vidow", "VIDOW MENTION", "vidow", "vidow mentioned" };

                var random = new Random();
                await message.Channel.SendMessageAsync(responses[random.Next(responses.Length)]); // pick a random response
            }
            else if (messageContent.Contains("ravioli"))
            {
                string[] responses = { "RAVIOLI MENTIONED", "ravioli,,,", "i wasn't held at gunpoint for ravioli but i added it to be fair", "ravioli :)", "RAVIOLI MENTION", "guys its ravioli", "ravioli", "ravioli mentioned" };
                var random = new Random();
                await message.Channel.SendMessageAsync(responses[random.Next(responses.Length)]);
            }
        }
    }
}