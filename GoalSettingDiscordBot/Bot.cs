using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GoalSettingDiscordBot.Models;
using Newtonsoft.Json;

namespace GoalSettingDiscordBot
{
    public static class Bot
    {
        private static DiscordSocketClient client;
        private static CommandHandler handler;

        public static BotConfig Config { get; set; }

        public static async void AsyncMain(string[] args)
        {
            //create client and register events
            client = new DiscordSocketClient();
            client.Log += Log;
            client.Ready += Client_Ready;

            //load the config file
            Config = JsonConvert.DeserializeObject<BotConfig>(await File.ReadAllTextAsync("config.json"));

            //create and setup the command handler
            handler = new CommandHandler(client, new CommandService(new CommandServiceConfig()));
            await handler.InstallCommandsAsync();

            //login the bot and start it
            await client.LoginAsync(TokenType.Bot, Config.Token);
            await client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private static Task Client_Ready()
        {
            //start a task that runs every hour to check for reminders
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await CheckForReminders();
                    Thread.Sleep(1000*60*60);
                }
            });

            return Task.CompletedTask;
        }

        private static async Task CheckForReminders()
        {
            //log
            Console.WriteLine($"Checking for reminders...");

            //iterate through every users goal list
            foreach (string filePath in Directory.EnumerateFiles("./UserGoals"))
            {
                //get the user details for this file
                string file = Path.GetFileNameWithoutExtension(filePath);
                string username = file.Split('_')[0];
                string discriminator = file.Split('_')[1];

                //load user from discard
                var user = client.GetUser(username, discriminator);

                //load the user's goals
                var userGoals = JsonConvert.DeserializeObject<List<UserGoal>>(await File.ReadAllTextAsync(filePath));

                //iterate through the user's goals and send reminders if it is time
                for (int i = userGoals.Count - 1; i >= 0; i--)
                {
                    //if the goal's due date has passed
                    var goal = userGoals[i];
                    if (DateTimeOffset.Now > goal.GoalDueDate && (goal.LastRemider == null || (DateTimeOffset.Now - goal.LastRemider)?.TotalDays > 1))
                    {
                        //send the reminder
                        await user.SendMessageAsync($"How are you doing on: {goal.Goal}?");

                        //update last reminder time
                        goal.LastRemider = DateTimeOffset.Now;

                        //log
                        Console.WriteLine($"Sent {username}#{discriminator} a reminder about their goal: \"{goal.Goal}\"");
                    }
                }

                //save updated goals
                await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(userGoals, Formatting.Indented));
            }

            //log
            Console.WriteLine($"Done.");
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}