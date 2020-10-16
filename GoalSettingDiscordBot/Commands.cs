using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using GoalSettingDiscordBot.Models;
using Newtonsoft.Json;

namespace GoalSettingDiscordBot
{
    // ReSharper disable once UnusedMember.Global, this is used through reflection
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("listgoals")]
        public async Task ListGoals()
        {
            //get user and their goals
            var user = Context.User;
            var goals = await GetGoals(user);

            //if they have any goals, list them, otherwise, tell them they have no goals
            if (goals.Any())
            {
                int i = 0;
                await ReplyAsync("Your current goals:\r\n" + string.Join("\r\n", goals.Select(g => $"{i++}: " + g)));
            }
            else
            {
                await ReplyAsync("You have no goals currently!");
            }

            //log
            Console.WriteLine($"Listed goals for {user.Username}#{user.DiscriminatorValue}");
        }

        [Command("addgoal")]
        public async Task AddGoal(params string[] args)
        {
            //holder vars for the args
            string goal;
            int daysToComplete;

            //get user and goals
            var user = Context.User;
            var userGoals = await GetGoals(user);

            //3 args means they named this goal, so use the constructor with a name
            if (args.Length == 3)
            {
                string goalName = args[0];
                goal = args[1];
                daysToComplete = int.Parse(args[2]);
                userGoals.Add(new UserGoal(user, goalName, goal, daysToComplete));
            }
            //2 args means this goal has no name, so use the constructor without a name
            else if (args.Length == 2)
            {
                goal = args[0];
                daysToComplete = int.Parse(args[1]);
                userGoals.Add(new UserGoal(user, goal, daysToComplete));
            }
            //otherwise, something is wrong
            else
            {
                await ReplyAsync("Malformed command");
                return;
            }

            //log
            Console.WriteLine($"Created goal \"{goal}\" from {user.Username}#{user.DiscriminatorValue}");

            //save goal changes
            await SaveGoals(user, userGoals.ToArray());

            //send confirmation message
            await ReplyAsync("Goal set!");
        }

        [Command("removegoal")]
        public async Task RemoveGoal(string goalName)
        {
            //get user goals
            var user = Context.User;
            var userGoals = await GetGoals(user);

            //remove any goals with the specified name
            userGoals.RemoveAll(g => g.GoalName.ToLower() == goalName.ToLower());

            //log
            Console.WriteLine($"Removed goal \"{goalName}\" from {user.Username}#{user.DiscriminatorValue}");

            //save goal changes
            await SaveGoals(user, userGoals.ToArray());

            //send confirmation message
            await ReplyAsync("Goal removed!");
        }

        [Command("removegoal")]
        public async Task RemoveGoal(int goalIndex)
        {
            //get user goals
            var user = Context.User;
            var userGoals = await GetGoals(user);

            //remove the goal at goalIndex
            userGoals.Remove(userGoals[goalIndex]);

            //log
            Console.WriteLine($"Removed goal #{goalIndex} from {user.Username}#{user.DiscriminatorValue}");

            //save goal changes
            await SaveGoals(user, userGoals.ToArray());

            //send confirmation message
            await ReplyAsync("Goal removed!");
        }

        private async Task<List<UserGoal>> GetGoals(SocketUser user)
        {
            //log
            Console.WriteLine($"Retrieving goals for {user.Username}#{user.DiscriminatorValue}");

            //build filename from user details
            var fileName = $"UserGoals\\{user.Username}_{user.DiscriminatorValue}.json";

            //read usergoals from file if it exists, otherwise, return an empty list
            return File.Exists(fileName) ? JsonConvert.DeserializeObject<List<UserGoal>>(await File.ReadAllTextAsync(fileName)) : new List<UserGoal>();
        }

        private async Task SaveGoals(SocketUser user, params UserGoal[] goals)
        {
            //log
            Console.WriteLine($"Saving goals for {user.Username}#{user.DiscriminatorValue}");

            //build filename from user details
            var fileName = $"UserGoals\\{user.Username}_{user.DiscriminatorValue}.json";

            //write goals to file in a human friendly format
            await File.WriteAllTextAsync(fileName, JsonConvert.SerializeObject(goals, Formatting.Indented));
        }
    }
}