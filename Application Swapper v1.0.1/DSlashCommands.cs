using Discord.WebSocket;

public class FakeSlashCommand
{
    public SocketUser User { get; }
    public string Character { get; }

    public FakeSlashCommand(SocketUser user, string character)
    {
        User = user;
        Character = character;
    }

    public async Task RespondAsync(string response)
    {
        if (User is SocketUser socketUser)
        {
            var dmChannel = await socketUser.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync(response);
        }
    }
}