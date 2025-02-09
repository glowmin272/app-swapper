using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application_Swapper
{
    class Program
    {
        private static async Task Main(string[] args) => await new Program().RunBotAsync();

        private DiscordSocketClient client;
        private async Task RunBotAsync()
        {
            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.Guilds | GatewayIntents.GuildMessages
            });

            var commandService = new CommandService();
            var interactionService = new InteractionService(client);

            client.Log += LogAsync;
            client.Ready += ReadyAsync;
            client.InteractionCreated += InteractionCreatedAsync;
            client.MessageReceived += MessageReceivedAsync;

            string token = "token";
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            await Task.Delay(-1);  // keeps the bot running
        }

        private async Task ReadyAsync()
        {
            Console.WriteLine("Bot is online and ready!");
            // pass client over
            await RegisterSlashCommandsAsync(client);
        }

        private async Task RegisterSlashCommandsAsync(DiscordSocketClient client)
        {
            if (client == null)
            {
                Console.WriteLine("Discord client is not initialized.");
                return;
            }

            ulong guildId = 1335950055110082591; // server id
            var guild = client.GetGuild(guildId);
            if (guild == null) return;

            var commands = new[] // all the commands are built here
            {
        new SlashCommandBuilder().WithName("choose").WithDescription("Choose a random option from a list.")
        .AddOption("choices", ApplicationCommandOptionType.String, "The list of options, separated by commas.", isRequired: true).Build(),
        new SlashCommandBuilder().WithName("assist").WithDescription("Get help with the bot.").Build(),
        new SlashCommandBuilder().WithName("reserve").WithDescription("Reserve a character.")
        .AddOption("name", ApplicationCommandOptionType.String, "The name of the character.", isRequired: true).Build(),
        new SlashCommandBuilder().WithName("reject").WithDescription("Reject an application.")
        .AddOption("name", ApplicationCommandOptionType.String, "The name of the character.", isRequired: true).AddOption("reason", ApplicationCommandOptionType.String, "Why the character is rejected.", isRequired: true).Build(),
        new SlashCommandBuilder().WithName("approve").WithDescription("Approve an application.")
        .AddOption("name", ApplicationCommandOptionType.String, "The name of the character to approve.", isRequired: true).Build(),
        new SlashCommandBuilder().WithName("halfapp").WithDescription("1/2 approval on an application.")
        .AddOption("name", ApplicationCommandOptionType.String, "The name of the character to approve.", isRequired: true).Build()
    };

            foreach (var command in commands)
                await guild.CreateApplicationCommandAsync(command);

            Console.WriteLine("Slash commands registered.");
        }


        private async Task InteractionCreatedAsync(SocketInteraction interaction)
        {
            ulong disChannelId = 1335950685774155806; // Channel ID for discussion
            ulong modChannelId = 1337883290463371304;  // Channel ID for mod messages

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

                    break;
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine($"Log: {log}");
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage arg)
        {
            // ignore bot's own messages
            if (arg is not SocketUserMessage message || arg.Author.IsBot) return;

            ulong appChannelId = 1335950203769065492; // app channel
            ulong modChannelId = 1337883290463371304;  // mod channel

            var channel = arg.Channel as SocketTextChannel;
            if (channel == null) return;

            // double check channel id
            if (channel.Id == appChannelId)
            {
                // get the mod channel
                var modChannel = channel.Guild.GetTextChannel(modChannelId);
                if (modChannel != null)
                {
                    // gorward the message to the mod channel
                    await modChannel.SendMessageAsync($"New submission from {message.Author.Username}: {message.Content}");
                }
            }
        }
    }
    }
