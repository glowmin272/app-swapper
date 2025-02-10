using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application_Swapper
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private InteractionService _interactions;
        private IServiceProvider _services;

        static async Task Main(string[] args) => await new Program().RunBotAsync();

        private async Task RunBotAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.Guilds | GatewayIntents.GuildMessages
            });

            _commands = new CommandService();
            _interactions = new InteractionService(_client);

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _client.InteractionCreated += InteractionCreatedAsync;

            string token = "token"; // bot token
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task ReadyAsync()
        {
            Console.WriteLine("Bot is online!"); // when the bot connects
            await _client.Rest.DeleteAllGlobalCommandsAsync();
            await RegisterSlashCommandsAsync();
        }

        private async Task RegisterSlashCommandsAsync()
        {
            ulong guildId = 1335950055110082591; // destiny linked ID
            var guild = _client.GetGuild(guildId);

            if (guild == null)
            {
                Console.WriteLine("Guild not found! Make sure the bot is in the server.");
                return;
            }

            var existingCommands = await guild.GetApplicationCommandsAsync();

            // check if commands are already registered
            if (existingCommands.Any())
            {
                Console.WriteLine("Slash commands are already registered. Skipping registration.");
                return;
            }
            // if they aren't registered, go on to registry
            Console.WriteLine("Registering slash commands...");

            var commands = new[]
            {
        new SlashCommandBuilder().WithName("choose")
            .WithDescription("Choose a random option from a list.")
            .AddOption("choices", ApplicationCommandOptionType.String, "The list of options, separated by commas.", isRequired: true)
            .Build(),

        new SlashCommandBuilder().WithName("assist")
            .WithDescription("Get help with the bot.")
            .Build(),

        new SlashCommandBuilder().WithName("reserve")
            .WithDescription("Reserve a character.")
            .AddOption("name", ApplicationCommandOptionType.String, "The name of the character.", isRequired: true)
            .Build(),

        new SlashCommandBuilder().WithName("reject")
            .WithDescription("Reject an application.")
            .AddOption("name", ApplicationCommandOptionType.String, "The name of the character.", isRequired: true)
            .AddOption("reason", ApplicationCommandOptionType.String, "Why the character is rejected.", isRequired: true)
            .Build(),

        new SlashCommandBuilder().WithName("approve")
            .WithDescription("Approve an application.")
            .AddOption("name", ApplicationCommandOptionType.String, "The name of the character to approve.", isRequired: true)
            .Build(),

        new SlashCommandBuilder().WithName("halfapp")
            .WithDescription("1/2 approval on an application.")
            .AddOption("name", ApplicationCommandOptionType.String, "The name of the character to approve.", isRequired: true)
            .Build(),

        new SlashCommandBuilder().WithName("speak")
            .WithDescription("Make the bot speak.")
            .AddOption("channel", ApplicationCommandOptionType.String, "The channel ID.", isRequired: true)
            .AddOption("message", ApplicationCommandOptionType.String, "The message to send.", isRequired: true)
            .Build()
    };

            try
            {
                foreach (var command in commands)
                    await guild.CreateApplicationCommandAsync(command);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to register slash commands: {ex.Message}");
            }
        }



        private async Task MessageReceivedAsync(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage message || message.Author.IsBot)
                return;

            // convert message to lowercase to catch all variations
            string messageContent = message.Content.ToLower();

            // if "vidow" appears anywhere in the message, prank em john
            if (messageContent.Contains("vidow"))
            {
                string[] responses = { "VIDOW MENTIONED", "vidow,,,", "i was held at gunpoint to add a 'vidow mentioned' meme joke to the bot", "vidow :)" };

                var random = new Random();
                await message.Channel.SendMessageAsync(responses[random.Next(responses.Length)]);
            }

            int argPos = 0;
            if (message.HasCharPrefix('?', ref argPos))
            {
                var context = new SocketCommandContext(_client, message);
                await HandleTextCommand(context, message, argPos);
            }

            // application channel handling
            ulong appChannelId = 1335950203769065492; // app channel id

            if (message.Channel.Id == appChannelId)
            {
                ulong targetChannelId = 1337883290463371304; // mod channel
                var targetChannel = _client.GetChannel(targetChannelId) as IMessageChannel;

                if (targetChannel != null)
                {
                    await targetChannel.SendMessageAsync($"A new app from {message.Author.Username} has been submitted: {message.Content}");
                }

            }

        }


        private async Task HandleTextCommand(SocketCommandContext context, SocketUserMessage message, int argPos) // dreamer specific commands
        {
            string command = message.Content[argPos..].Split(' ')[0].ToLower();
            string messageContent = message.Content.Substring(argPos + command.Length).Trim();

            ulong modChannelId = 1337883290463371304;
            ulong targetChannelId = 1335950685774155806;

            var guild = context.Guild;
            var targetChannel = guild?.GetTextChannel(targetChannelId);
            var modChannel = guild?.GetTextChannel(modChannelId);

            if (message.Channel.Id == modChannelId)
            {
                switch (command)
                {
                    case "approve":
                        if (targetChannel != modChannel)
                            await targetChannel.SendMessageAsync($"Full approval issued for: {messageContent}");
                        await modChannel?.SendMessageAsync("Approval message sent.");
                        break;

                    case "halfapprove":
                        if (targetChannel != modChannel)
                            await targetChannel.SendMessageAsync($"One approval has been issued for: {messageContent}");
                        await modChannel?.SendMessageAsync("Half approval message sent.");
                        break;

                    case "review":
                        if (targetChannel != modChannel)
                            await targetChannel.SendMessageAsync($"Review is in progress for: {messageContent}");
                        await modChannel?.SendMessageAsync("Review message sent.");

                        break;

                    case "choose":
                        var choices = messageContent.Split(',').Select(choice => choice.Trim()).ToArray();
                        if (choices.Length > 0)
                        {
                            var random = new Random();
                            var chosen = choices[random.Next(choices.Length)];
                            await context.Channel.SendMessageAsync($"I have chosen: {chosen}");
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync("Please provide a list of choices.");
                        }
                        break;
                }
            }
            else
            {
                await context.Channel.SendMessageAsync("You do not have permission to use this command. Only moderators can perform this action.");
            }
        }


        private async Task InteractionCreatedAsync(SocketInteraction interaction)
        {
            ulong disChannelId = 1335950685774155806; // channel ID for discussion
            ulong modChannelId = 1337883290463371304;  // channel ID for mod messages
            ulong resChannelId = 1335975220296683571; // channel ID for reservations

            if (interaction is not SocketSlashCommand slashCommand) return;

            var user = (SocketUser)slashCommand.User;
            var guildUser = (SocketGuildUser)user;
            var modRole = guildUser.Guild.Roles.FirstOrDefault(role => role.Name.Equals("mods", StringComparison.OrdinalIgnoreCase));

            // make sure user is moderator
            async Task<bool> IsModerator()
            {
                if (modRole == null || !guildUser.Roles.Contains(modRole))
                {
                    await slashCommand.RespondAsync("You do not have permission to use this command. Only moderators can perform this action.");
                    return false;
                }
                return true;
            }

            // send message to channel; is passed information through the function
            async Task SendMessageToChannel(ulong channelId, string message)
            {
                var channel = guildUser.Guild.GetTextChannel(channelId);
                if (channel != null)
                {
                    await channel.SendMessageAsync(message);
                }
                else
                {
                    await slashCommand.RespondAsync("Failed to find the specified channel.");
                }
            }

            switch (slashCommand.Data.Name)
            {
                case "choose":
                    var choicesString = slashCommand.Data.Options.FirstOrDefault()?.Value?.ToString();
                    if (choicesString != null)
                    {
                        var choices = choicesString.Split(',').Select(choice => choice.Trim()).ToArray();
                        var random = new Random();
                        var chosenOption = choices[random.Next(choices.Length)];
                        await slashCommand.RespondAsync($"I have chosen: {chosenOption}");
                    }
                    else
                    {
                        await slashCommand.RespondAsync("Please provide a list of choices.");
                    }
                    break;

                case "assist":
                    var assistMessage = new StringBuilder()
                        .AppendLine("Hi! I'm the application helper bot. I know a few commands and am here to help streamline the process of getting applications done! Here's a list of what I can do!")
                        .AppendLine("/assist is the command you just ran to see everything I've got up my... uh. Metaphorical sleeves.")
                        .AppendLine("/choose is where I pick a random item from a list of things you give me. Try /choose apple, orange!")
                        .AppendLine("/reserve is where you can place character reservations. This only works in the reservation channel though.")
                        .AppendLine("/approve is a command to send a full 2/2 approval message into character-discussion. This can only be done by mods.")
                        .AppendLine("/halfapprove is a command to send 1/2 approval into character-discussion. This can only be done by mods.")
                        .AppendLine("/review is a command that is sent to character-discussion to show that a character's application is being reviewed. This can only be done by mods.")
                        .AppendLine("/reject is a command that is sent to character-discussion. It includes the character name and why it was rejected. This can only be done by mods.")
                        .AppendLine("More features are in the works!")
                        .ToString();
                    await slashCommand.RespondAsync(assistMessage);
                    break;

                case "approve":
                case "halfapp":
                case "reject":
                    if (!await IsModerator()) return;

                    var characterName = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "name")?.Value?.ToString();
                    if (string.IsNullOrEmpty(characterName))
                    {
                        await slashCommand.RespondAsync("Please provide the name of the character.");
                        return;
                    }

                    if (slashCommand.Data.Name == "reject")
                    {
                        var rejectReason = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "reason")?.Value?.ToString();
                        if (string.IsNullOrEmpty(rejectReason))
                        {
                            await slashCommand.RespondAsync("Please provide the reason for rejection.");
                            return;
                        }

                        string rejectionMessage = $"The character '{characterName}' has been rejected. Reason: {rejectReason}";
                        await slashCommand.RespondAsync("Rejection message sent.");
                    }
                    else
                    {
                        string approvalMessage = slashCommand.Data.Name == "approve" ? $"The character '{characterName}' has been approved!" :
                                                 slashCommand.Data.Name == "halfapp" ? $"The character '{characterName}' has 1/2 approval." :
                                                 string.Empty;

                        await SendMessageToChannel(disChannelId, approvalMessage);
                        await slashCommand.RespondAsync($"{slashCommand.Data.Name} message sent.");
                    }
                    break;
                case "reserve":
                    if (slashCommand.ChannelId != resChannelId)
                    {
                        await slashCommand.RespondAsync("This command can only be used in the reservations channel.");
                        return;
                    }
                    else
                    {
                        var reserveCharacterName = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "name")?.Value?.ToString();
                        if (string.IsNullOrEmpty(reserveCharacterName))
                        {
                            await slashCommand.RespondAsync("Please provide the name of the character you wish to reserve.");
                            return;
                        }

                        string reservationMessage = $"The character '{reserveCharacterName}' has been requested to be reserved.";
                        await SendMessageToChannel(modChannelId, reservationMessage);
                        await slashCommand.RespondAsync($"The character '{reserveCharacterName}' has been reserved and the message has been sent to the mod channel.");
                        break;
                    }

                case "speak":
                    if (!await IsModerator()) return;
                    var channelIdOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "channel")?.Value;
                    var messageOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "message")?.Value?.ToString();

                    if (channelIdOption == null || messageOption == null)
                    {
                        await slashCommand.RespondAsync("Please provide a valid channel ID and message.", ephemeral: true);
                        return;
                    }

                    if (ulong.TryParse(channelIdOption.ToString(), out ulong channelId))
                    {
                        var guild = ((SocketGuildChannel)interaction.Channel).Guild;
                        var channel = guild.GetTextChannel(channelId);

                        if (channel != null)
                        {
                            await channel.SendMessageAsync(messageOption);
                            await slashCommand.RespondAsync($"Message sent to <#{channelId}>.", ephemeral: true);
                        }
                        else
                        {
                            await slashCommand.RespondAsync("Invalid channel ID.", ephemeral: true);
                        }
                    }
                    else
                    {
                        await slashCommand.RespondAsync("Invalid channel ID format.", ephemeral: true);
                    }
                    break;

                default:
                    await slashCommand.RespondAsync("Unknown command.", ephemeral: true);
                    break;
            }
        }


        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine($"Log: {log}");
            return Task.CompletedTask;
        }
    }
}
