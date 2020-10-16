using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Discord;

namespace GoalSettingDiscordBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Factory.StartNew(() => Bot.AsyncMain(args));

            while (true) { }
        }
    }
}
