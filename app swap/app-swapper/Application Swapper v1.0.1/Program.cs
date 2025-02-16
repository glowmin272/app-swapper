using System;
using System.Threading.Tasks;
using Application_Swapper;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    private static async Task Main(string[] args) 
    {
        var bot = new Bot(); // initialize bot
        await bot.RunAsync(); // start the bot
    }
}
