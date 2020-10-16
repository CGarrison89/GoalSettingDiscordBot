using Discord.WebSocket;
using System;
using Newtonsoft.Json;

namespace GoalSettingDiscordBot.Models
{
    public class UserGoal
    {
        [JsonIgnore]
        public string Username { get; set; }
        [JsonIgnore]
        public ushort? Discriminator { get; set; }
        public string GoalName { get; set; }
        public string Goal { get; set; }
        public DateTimeOffset GoalSetOn { get; set; }
        public DateTimeOffset GoalDueDate { get; set; }
        public DateTimeOffset? LastRemider { get; set; }

        [JsonConstructor]
        public UserGoal(string goalName, string goal, DateTimeOffset goalSetOn, DateTimeOffset goalDueDate, DateTimeOffset? lastReminder)
        {
            GoalName = goalName;
            Goal = goal;
            GoalSetOn = goalSetOn;
            GoalDueDate = goalDueDate;
            LastRemider = lastReminder;
        }

        public UserGoal(SocketUser user, string goalName, string goal, int daysToComplete)
        {
            Username = user?.Username;
            Discriminator = user?.DiscriminatorValue;
            GoalName = goalName;
            Goal = goal;
            GoalSetOn = DateTime.Now;
            GoalDueDate = GoalSetOn.AddDays(daysToComplete);
        }

        public UserGoal(SocketUser user, string goal, int daysToComplete)
        {
            Username = user?.Username;
            Discriminator = user?.DiscriminatorValue;
            Goal = goal;
            GoalSetOn = DateTime.Now;
            GoalDueDate = GoalSetOn.AddDays(daysToComplete);
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(GoalName))
            {
                return $"{Goal} ({Math.Round((GoalDueDate - DateTimeOffset.Now).TotalDays)} days remaining)";
            }
            else
            {
                return $"\"{GoalName}\" - {Goal} ({Math.Round((GoalDueDate - DateTimeOffset.Now).TotalDays)} days remaining)";
            }
        }
    }
}