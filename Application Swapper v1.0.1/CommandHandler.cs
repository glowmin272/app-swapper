using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Application_Swapper
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly QueueManager _queueManager;

        public CommandHandler(DiscordSocketClient client)
        {
            _client = client;
            _interactions = new InteractionService(_client);
            _queueManager = new QueueManager(_client);

            _client.MessageReceived += MessageReceivedAsync;
            _client.InteractionCreated += InteractionCreatedAsync;
        }

        public async Task InitializeAsync()
        {
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        public async Task RegisterSlashCommandsAsync()
        {
            ulong guildId = 1335950055110082591;
            var guild = _client.GetGuild(guildId);

            if (guild == null)
            {
                Console.WriteLine("Guild not found! Make sure the bot is in the server.");
                return;
            }
            var existingCommands = await guild.GetApplicationCommandsAsync();

            //check if commands are already registered
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
            .Build(),
        new SlashCommandBuilder().WithName("startfight")
            .WithDescription("Start a fight and create a queue post.")
            .Build(),
        new SlashCommandBuilder().WithName("joinqueue")
            .WithDescription("Join the fight queue.")
            .AddOption("character", ApplicationCommandOptionType.String, "The character you are using.", isRequired: true)
            .Build(),
        new SlashCommandBuilder().WithName("leavequeue")
            .WithDescription("Leave the fight queue.")
            .AddOption("character", ApplicationCommandOptionType.String, "The character you are using.", isRequired: true)
            .Build(),
        new SlashCommandBuilder().WithName("linkfightpost")
            .WithDescription("Link the fight post.")
            .AddOption("message", ApplicationCommandOptionType.String, "The message of the fight post.", isRequired: true)
            .Build(),
        new SlashCommandBuilder().WithName("edit")
            .WithDescription("Edit one of the bot's messages.")
            .AddOption("message", ApplicationCommandOptionType.String, "The message you want to edit.", isRequired: true)
            .AddOption("newmessage", ApplicationCommandOptionType.String, "The new contents of the message.", isRequired: true)
            .Build()
    };

            foreach (var command in commands)
                await guild.CreateApplicationCommandAsync(command);
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            // ignore messages from the bot
            if (message is not SocketUserMessage userMessage || message.Author.IsBot)
                return;

            // convert message to lowercase to catch all variations
            string messageContent = message.Content.ToLower();

            int argPos = 0;
            if (userMessage.HasCharPrefix('?', ref argPos))
            {
                var context = new SocketCommandContext(_client, userMessage);
                await HandleTextCommand(context, userMessage, argPos); // call for ? commands
            }
            else if (messageContent.Contains("vidow"))
            {
                string[] responses = { "VIDOW MENTIONED", "vidow,,,", "i was held at gunpoint to add a 'vidow mentioned' meme joke to the bot", "vidow :)", "guys its vidow", "VIDOW MENTION" };

                var random = new Random();
                await message.Channel.SendMessageAsync(responses[random.Next(responses.Length)]); // pick a random response
            }
        }

        // dreamer's commands
        private async Task HandleTextCommand(SocketCommandContext context, SocketUserMessage message, int argPos)
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

                    case "reject":
                        if (targetChannel != modChannel)
                            await targetChannel.SendMessageAsync($"A rejection has been issued. See details: {messageContent}.");
                        await modChannel?.SendMessageAsync("Rejection message sent.");
                        break;
                }
            }
            else if (message.Channel.Id != modChannelId)
            {
                switch (command)
                {
                    case "startfight":
                        await _queueManager.StartFightAsync();
                        await context.Channel.SendMessageAsync("The fight queue has started.");
                        break;

                    case "joinqueue":
                        await _queueManager.JoinQueueAsync(context.User, messageContent); // passing user and character name
                        await context.Channel.SendMessageAsync($"{context.User.Username} has joined the fight with {messageContent}.");
                        break;

                    case "leavequeue":
                        await _queueManager.LeaveQueueAsync(context.User, messageContent); // passing user and character name
                        await context.Channel.SendMessageAsync($"{context.User.Username} has left the fight queue.");
                        break;
                }
            }
        }

        private async Task InteractionCreatedAsync(SocketInteraction interaction)
        {
            _client.SlashCommandExecuted += HandleSlashCommandAsync;
            if (interaction is not SocketSlashCommand slashCommand) return;
            var user = (SocketUser)slashCommand.User;
            var guildUser = (SocketGuildUser)user;
            var context = new SocketInteractionContext(_client, interaction);
            var modRole = guildUser.Guild.Roles.FirstOrDefault(role => role.Name.Equals("mods", StringComparison.OrdinalIgnoreCase));
            await _interactions.ExecuteCommandAsync(context, null);
        }
        private ulong _fightMessageId; // stores the message ID of the queue post
        private async Task HandleSlashCommandAsync(SocketSlashCommand command)
        {
            ulong fightChannelId = 1340168827010285649; // #battle-details
            ulong targetChannelId = 1335950685774155806;  // Channel where approval messages are sent

            var guild = (command.Channel as SocketGuildChannel)?.Guild;
            var fightChannel = guild?.GetTextChannel(fightChannelId);
            var targetChannel = guild?.GetTextChannel(targetChannelId);

            string messageContent = command.Data.Options.FirstOrDefault()?.Value?.ToString() ?? "";

            switch (command.Data.Name)
            {
                case "startfight":
                    await _queueManager.StartFightAsync(command);
                    break;

                case "joinqueue":
                    var joinCharacterOption = command.Data.Options.FirstOrDefault(o => o.Name == "character");
                    if (joinCharacterOption != null && joinCharacterOption.Value is string character)
                    {
                        await _queueManager.JoinQueueAsync(command.User, character);
                    }
                    else
                    {
                        await command.RespondAsync("Please provide a valid character name.");
                    }
                    break;

                case "leavequeue":
                    var leaveCharacterOption = command.Data.Options.FirstOrDefault(o => o.Name == "character");
                    if (leaveCharacterOption != null && leaveCharacterOption.Value is string leaveCharacter)
                    {
                        await _queueManager.LeaveQueueAsync(command.User, leaveCharacter);
                    }
                    else
                    {
                        await command.RespondAsync("Please provide a valid character name.");
                    }
                    break;


                case "approve":
                    await command.RespondAsync($"Full approval issued for: {messageContent}");
                    if (targetChannel != null)
                        await targetChannel.SendMessageAsync($"Full approval issued for: {messageContent}");
                    break;

                case "halfapprove":
                    await command.RespondAsync($"One approval has been issued for: {messageContent}");
                    if (targetChannel != null)
                        await targetChannel.SendMessageAsync($"One approval has been issued for: {messageContent}");
                    break;

                case "reject":
                    await command.RespondAsync($"A rejection has been issued. See details: {messageContent}.");
                    if (targetChannel != null)
                        await targetChannel.SendMessageAsync($"A rejection has been issued. See details: {messageContent}.");
                    break;

                case "choose":
                    var choiceOption = command.Data.Options.FirstOrDefault(o => o.Name == "choices");
                    if (choiceOption != null && choiceOption.Value is string choicesString)
                    {
                        var choices = choicesString.Split(',').Select(choice => choice.Trim()).Where(choice => !string.IsNullOrWhiteSpace(choice)).ToArray();
                        if (choices.Length > 0)
                        {
                            var random = new Random();
                            var chosen = choices[random.Next(choices.Length)];
                            await command.RespondAsync($"I have chosen: {chosen}");
                        }
                        else
                        {
                            await command.RespondAsync("Please provide a list of choices separated by commas.");
                        }
                    }
                    else
                    {
                        await command.RespondAsync("Invalid input. Use `/choose choices: option1, option2, option3`.");
                    }
                    break;

                case "edit":
                    // get details for slash command
                    ulong messageId = Convert.ToUInt64(command.Data.Options.First(o => o.Name == "message").Value);
                    string newContent = (string)command.Data.Options.First(o => o.Name == "newmessage").Value;

                    // get the channel where the command was used
                    var channel = command.Channel as IMessageChannel;
                    if (channel == null) // if there is no channel
                    {
                        await command.RespondAsync("Could not determine the message channel.", ephemeral: true);
                        return;
                    }

                    try
                    {
                        // fetch the message by ID
                        var message = await channel.GetMessageAsync(messageId) as IUserMessage;
                        if (message == null)
                        {
                            await command.RespondAsync("Message not found or not editable.", ephemeral: true);
                            return;
                        }

                        // make sure its the bot's message
                        if (message.Author.Id != _client.CurrentUser.Id)
                        {
                            await command.RespondAsync("I can only edit my own messages!", ephemeral: true);
                            return;
                        }

                        // edit the message
                        await message.ModifyAsync(msg => msg.Content = newContent);
                        await command.RespondAsync("Message successfully edited!", ephemeral: true);
                    }
                    catch
                    {
                        await command.RespondAsync("An error occurred while editing the message.", ephemeral: true);
                    }
                    break;

                default:
                    await command.RespondAsync("Unknown command.");
                    break;
            }
        }
    }
}
