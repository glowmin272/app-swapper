using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Talking_Zora_1._0._5
{
    internal class QueueManager : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ulong _fightChannelId = 1340168827010285649; 

        [SlashCommand("startfight", "Starts a fight with a specified locale.")]
        public async Task StartFightAsync(string locale)
        {
            var user = Context.User;
            var fightChannel = Context.Guild.GetTextChannel(_fightChannelId);

            if (fightChannel == null)
            {
                await RespondAsync("Error: Fight channel not found.", ephemeral: true);
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle($"FIGHT IN {locale} | HOSTED BY {user}")
                .WithDescription($"Use `/joinqueue <messageId>` to join and `/leavequeue <messageId>` to leave.\n\n**Queue:**\n*(empty)*")
                .WithColor(Color.Purple)
                .WithTimestamp(DateTimeOffset.Now)
                .WithFooter($"Started by {user}")
                .Build();

            var message = await fightChannel.SendMessageAsync(embed: embed);

            // Acknowledge the command to avoid interaction failure
            await RespondAsync("Fight started!", ephemeral: true);
        }
    }
}

