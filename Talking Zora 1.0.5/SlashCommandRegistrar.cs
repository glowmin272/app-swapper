using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application_Swapper
{
    public class SlashCommandRegistrar
    {
        private readonly DiscordSocketClient _client;
        public readonly ulong guildId = 1335950055110082591;

        public SlashCommandRegistrar(DiscordSocketClient client)
        {
            _client = client;
        }
        public async Task RegisterSlashCommandsAsync()
        {
            await Task.Delay(2000); // Wait for 2 seconds
            var guild = _client.GetGuild(guildId);
            if (guild == null)
            {
                Console.WriteLine("Guild not found or bot is not in the server.");
                return;
            }

            // If the guild is found, continue with fetching existing commands
            var existingCommands = await guild.GetApplicationCommandsAsync();

            // Check if commands are already registered
            if (existingCommands.Any())
            {
                Console.WriteLine("Slash commands are already registered. Skipping registration.");
                return;
            }

            // Register commands if not already registered
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
            {
                await guild.CreateApplicationCommandAsync(command);
                Console.WriteLine($"Registered command: {command.Name}");
            }

        }
    }
}/*
            foreach (var command in existingCommands)
            {
                await command.DeleteAsync();
                Console.WriteLine($"Deleted command: {command.Name}");
            }
            */