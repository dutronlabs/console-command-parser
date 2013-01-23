using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using TheObtuseAngle.ConsoleUtilities.Arguments;
using System.Linq;

namespace TheObtuseAngle.ParseCommandsConsole
{
    [Export(typeof(ExampleCommandBase))]
    public sealed class AddUsersCommand : ExampleCommandBase
    {
        private readonly List<User> users = new List<User>();
        private bool hasValidUsers = true;

        public AddUsersCommand()
            : base("AddUsers", "Adds multiple users to the user store")
        {
        }

        protected override IEnumerable<IArgument> GetArguments()
        {
            var arguments = base.GetArguments().ToList();
            arguments.Insert(0, new RequiredValueArgument("-user", "-u", "The definition of a new user. ex) -u userName|password|nickname", ParseUser));
            return arguments;
        }

        private void ParseUser(string userArgs)
        {
            if (string.IsNullOrWhiteSpace(userArgs))
            {
                hasValidUsers = false;
                return;
            }

            var userParts = userArgs.Split('|');

            if (userParts.Length < 2)
            {
                hasValidUsers = false;
                return;
            }

            var user = new User
            {
                UserName = userParts[0],
                Password = userParts[1],
                Nickname = userParts.Length >= 3 ? userParts[2] : string.Empty
            };
            users.Add(user);
        }

        public override bool Execute()
        {
            if (!hasValidUsers)
            {
                Console.WriteLine("There are some invalid args.");
                return false;
            }

            foreach (var user in users)
            {
                Console.WriteLine("  Adding user:  Username: '{0}'  Password: '{1}'  Nickname: '{2}'", user.UserName, user.Password, user.Nickname);
            }

            return true;
        }

        private class User
        {
            public string UserName { get; set; }

            public string Password { get; set; }

            public string Nickname { get; set; }
        }
    }
}