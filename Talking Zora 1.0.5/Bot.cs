using Application_Swapper;
using Discord.WebSocket;
using Discord;

public class Bot
{
    private readonly DiscordSocketClient _client;
    private readonly CommandHandler _commandHandler;
    private readonly SlashCommandRegistrar _slashCommandRegistrar;

    public Bot()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.Guilds | GatewayIntents.GuildMessages
        });

        _commandHandler = new CommandHandler(_client);
        _slashCommandRegistrar = new SlashCommandRegistrar(_client); // Initialize the registrar here
    }

    private static async Task Main(string[] args)
    {
        var bot = new Bot(); // initialize bot
        await bot.RunAsync(); // start the bot
    }

    public async Task RunAsync()
    {
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;

        await _client.LoginAsync(TokenType.Bot, "test");
        await _client.StartAsync();
        await _commandHandler.InitializeAsync();
        await Task.Delay(-1);
    }

    private async Task ReadyAsync()
    {
        Console.WriteLine("Bot is online!"); // When the bot connects

        // Delete all existing guild-specific commands
        ulong guildId = 1335950055110082591; // Replace with your guild ID
        var guild = _client.GetGuild(guildId);
        if (guild != null)
        {
            /*
            var existingCommands = await guild.GetApplicationCommandsAsync();
            foreach (var command in existingCommands)
            {
                await command.DeleteAsync();
                Console.WriteLine($"Deleted {command}");
            
            }
            */
        }

        // Register commands again
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }
}
