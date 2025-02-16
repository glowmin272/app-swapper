using Discord.WebSocket;
using Discord;

public class QueueManager
{
    private readonly DiscordSocketClient _client;
    private ulong _fightMessageId;
    private readonly ulong _fightChannelId = 1340168827010285649; // #battle-details

    public QueueManager(DiscordSocketClient client)
    {
        _client = client;
    }

    // post a new fight queue
    public async Task StartFightAsync(SocketSlashCommand? command = null)
    {
        var fightChannel = await _client.GetChannelAsync(_fightChannelId) as IMessageChannel;
        if (fightChannel != null)
        {
            // Await the send message to get the actual IUserMessage
            var fightMessage = await fightChannel.SendMessageAsync("Fight Queue:\n(Use /joinqueue to join and /leavequeue to leave)");
            _fightMessageId = fightMessage.Id;
            Console.WriteLine($"Fight message posted with ID: {_fightMessageId}");

            if (command != null)
            {
                await command.RespondAsync("Fight queue started!", ephemeral: true);
            }
        }
        else if (command != null)
        {
            await command.RespondAsync("Could not find the fight channel.", ephemeral: true);
        }
    }

    // handle join queue from both slash and ?; currently doesn't work for ?
    public async Task JoinQueueAsync(SocketUser user, string character)
    {
        if (string.IsNullOrWhiteSpace(character))
        {
            Console.WriteLine("Invalid character name.");
            return;
        }

        var fightChannel = await _client.GetChannelAsync(_fightChannelId) as IMessageChannel;
        if (fightChannel != null)
        {
            var joinFightMessage = fightChannel.GetMessageAsync(_fightMessageId) as IUserMessage;
            if (joinFightMessage != null)
            {
                var entry = $"{user.Username} - {character}";
                if (!joinFightMessage.Content.Contains(entry))
                {
                    var newContent = $"{joinFightMessage.Content}\n{entry}";
                    await joinFightMessage.ModifyAsync(msg => msg.Content = newContent);
                    Console.WriteLine($"{user.Username} joined the fight queue with {character}.");
                }
                else
                {
                    Console.WriteLine($"{user.Username} is already in the queue with {character}.");
                }
            }
        }
    }

    // leave queue from both slash and ?; currently doesn't work for ?
    public async Task LeaveQueueAsync(SocketUser user, string character)
    {
        if (string.IsNullOrWhiteSpace(character))
        {
            Console.WriteLine("Invalid character name.");
            return;
        }

        var fightChannel = await _client.GetChannelAsync(_fightChannelId) as IMessageChannel;
        if (fightChannel != null)
        {
            var leaveFightMessage = fightChannel.GetMessageAsync(_fightMessageId) as IUserMessage;
            if (leaveFightMessage != null)
            {
                var entryToRemove = $"{user.Username} - {character}";
                if (leaveFightMessage.Content.Contains(entryToRemove))
                {
                    var updatedContent = leaveFightMessage.Content.Replace(entryToRemove, "").Trim();
                    await leaveFightMessage.ModifyAsync(msg => msg.Content = updatedContent);
                    Console.WriteLine($"{user.Username} left the fight queue.");
                }
            }
        }
    }
}
