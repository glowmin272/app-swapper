using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Application_Swapper
{
    class Program
    {
        private static async Task Main(string[] args) => await new Program().RunBotAsync();

        private async Task RunBotAsync()
        {
            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                // gets the data from Discord Developer portal for permissions
                GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.Guilds | GatewayIntents.GuildMessages
            });

            client.Log += LogAsync;
            client.MessageReceived += MessageReceivedAsync;
            client.MessageReceived += ModAsync;
            client.MessageReceived += CommandReceivedAsync;
            client.Ready += ReadyAsync;

            // bot token. 
            string token = "token";

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            // keep program running until closed
            await Task.Delay(-1);
        }

        private async Task CommandReceivedAsync(SocketMessage arg)
        {
            ulong resChannelId = 1335975220296683571; // channel id of reserve channel
            ulong modChannelId = 1337883290463371304;  // channel id of mod channel

            // ignore messages from the bot itself
            if (arg is not SocketUserMessage message || message.Author.IsBot) return;

            if (message.Channel is SocketTextChannel textChannel && textChannel.Id == resChannelId)
            {
                var resChannel = textChannel.Guild.GetTextChannel(resChannelId);
                var modChannel = textChannel.Guild.GetTextChannel(modChannelId);

                if (resChannel == null)
                {
                    Console.WriteLine("Target channel not found.");
                    return;
                }
                // can only be ran in character-reservation
                if (message.Content.StartsWith("/reserve"))
                {
                    string content = message.Content["/reserve ".Length..].Trim();
                    await modChannel.SendMessageAsync($"A reserve has been requested from {message.Author.Username}. See: {content}");
                    await resChannel.SendMessageAsync("Your message has been sent.");
                }
            }
            // commands that can be ran OUTSIDE of mod-bot-app
            else if (message.Channel is SocketTextChannel textChannel1)
            {
                if (message.Content.StartsWith("/choose"))
                {
                    string choicesString = message.Content["/choose ".Length..].Trim();
                    if (!string.IsNullOrEmpty(choicesString))
                    {
                        var choices = choicesString.Split(',').Select(choice => choice.Trim()).ToArray();
                        var random = new Random();
                        var chosenOption = choices[random.Next(choices.Length)];
                        await textChannel1.SendMessageAsync($"I have chosen: {chosenOption}");
                    }
                    else
                    {
                        await textChannel1.SendMessageAsync("Please provide a list of choices.");
                    }
                }
                else if (message.Content.StartsWith("/assist"))
                {
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

                    await textChannel1.SendMessageAsync(assistMessage);
                }
            }
        }

        private async Task ModAsync(SocketMessage arg)
        {
            ulong modChannelId = 1337883290463371304;  // channel id of mod channel
            ulong appChannelId = 1335950685774155806;  // channel id of application channel

            // ignore messages from the bot itself
            if (arg is not SocketUserMessage message || message.Author.IsBot) return;

            // make sure message is correct and not from a dm/somewhere the bot cannot access
            if (message.Channel is SocketTextChannel textChannel && textChannel.Id == modChannelId)
            {
                // define common behavior for retrieving the channels
                var appChannel = textChannel.Guild.GetTextChannel(appChannelId);
                var modChannel = textChannel.Guild.GetTextChannel(modChannelId);
                if (appChannel == null)
                {
                    Console.WriteLine("Target channel not found.");
                    return;
                }

                // handle different commands
                if (message.Content.StartsWith("/approve")) // await [channel] = channel the message is being sent to
                {
                    string content = message.Content["/approve ".Length..].Trim();
                    await appChannel.SendMessageAsync($"Full approval issued for: {content}"); 
                    await modChannel.SendMessageAsync("Approval message sent.");
                }
                else if (message.Content.StartsWith("/halfapprove"))
                {
                    string content = message.Content["/halfapprove ".Length..].Trim();
                    await appChannel.SendMessageAsync($"One approval has been issued for: {content}");
                    await modChannel.SendMessageAsync("1/2 approval for an application has been sent.");
                }
                else if (message.Content.StartsWith("/review"))
                {
                    string content = message.Content["/review ".Length..].Trim();
                    await appChannel.SendMessageAsync($"Review is in progress for: {content}");
                    await modChannel.SendMessageAsync("Message regarding review has been sent.");
                }
                else if (message.Content.StartsWith("/reserve"))
                {
                    string content = message.Content["/reserve ".Length..].Trim();
                    await modChannel.SendMessageAsync($"A reserve has been issued. See: {content}");
                    await appChannel.SendMessageAsync("Your message has been sent.");
                }
                else if (message.Content.StartsWith("/reject"))
                {
                    string content = message.Content["/reject ".Length..].Trim();
                    await appChannel.SendMessageAsync($"A rejection has been issued. See: {content}");
                    await modChannel.SendMessageAsync("Rejection message sent.");
                }
            }
        }
        private Task LogAsync(LogMessage log)
        {
            // show the status of the bot (connecting, ready, etc)
            Console.WriteLine($"Log: {log}");
            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            // if connection is successful
            Console.WriteLine("Bot is online and ready!");
        }

        private async Task MessageReceivedAsync(SocketMessage arg)
        {
            // ignore messages from the bot itself
            if (arg is not SocketUserMessage message) return;
            if (message.Author.IsBot) return;

            // make sure message is correct and not from a dm/somewhere the bot cannot access
            if (message.Channel is SocketTextChannel textChannel)
            {
                ulong sourceChannelId = 1335950203769065492;  // channel id of application channel
                if (textChannel.Id == sourceChannelId)
                {
                    ulong targetChannelId = 1337883290463371304;  // channel id of mod channel

                    // double check channel id
                    var targetChannel = textChannel.Guild.GetTextChannel(targetChannelId);

                    if (targetChannel != null)
                    {
                        // send message
                        await targetChannel.SendMessageAsync($"New Application from: {message.Author.Username}\nApplication Contents: {message.Content}");
                    }
                    else
                    {
                        // if the channel id is incorrect
                        Console.WriteLine("Target channel not found.");
                    }
                }
            }
            else
            {
                // error handling
                Console.WriteLine("Message error regarding receiving app. Please try again.");
            }

        }
    }
}
