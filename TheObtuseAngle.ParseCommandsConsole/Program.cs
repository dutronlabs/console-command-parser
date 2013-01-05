using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using TheObtuseAngle.ConsoleUtilities;
using TheObtuseAngle.ConsoleUtilities.Arguments;
using TheObtuseAngle.ConsoleUtilities.Commands;

namespace TheObtuseAngle.ParseCommandsConsole
{
    public class Program
    {
        private readonly string[] consoleArgs;

        public static void Main(string[] args)
        {
            Console.WriteLine();
            var program = new Program(args);
            program.Run();
            Console.WriteLine();
        }

        public Program(string[] consoleArgs)
        {
            this.consoleArgs = consoleArgs;
            Compose();
        }

        private void Compose()
        {
            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        [ImportMany]
        public IEnumerable<CommandBase> Commands { get; set; }

        public void Run()
        {
            try
            {
                var parser = new CommandParser<ICommand>();
                var result = parser.ParseCommandAndExecute(consoleArgs, Commands);

                Console.WriteLine();
                Console.WriteLine(result);
            }
            catch (Exception e)
            {
                e.WriteToConsole();
            }
        }
    }

    [Export(typeof(CommandBase))]
    public sealed class AddUserCommand : CommandBase
    {
        private readonly List<Argument> orderedArgs;
        private string userName;
        private string password;
        private string nickname;

        public AddUserCommand()
            : base("AddUser", "Adds a user to the user store")
        {
            orderedArgs = new List<Argument>
            {
                new Argument("-userName", "-n", "The name of the new user", true, true, val => userName = val),
                new Argument("-password", "-pw", "The password for the new user", true, true, val => password = val),
                new Argument("-nickname", "-nn", "The nickname of the new user", true, false, val => nickname = val)
            };
            Arguments.Add(orderedArgs[0]);
            Arguments.Add(orderedArgs[1]);
            Arguments.Add(orderedArgs[2]);
        }

        public override bool Execute()
        {
            Console.WriteLine("Adding user:{0}  Username: {1}{0}  Password: {2}{0}  Nickname: {3}", Environment.NewLine, userName, password, nickname);
            return true;
        }

        public override IEnumerable<IArgument> GetOrderedArguments()
        {
            return orderedArgs;
        }
    }

    [Export(typeof(CommandBase))]
    public class AddUsersCommand : CommandBase
    {
        private readonly List<User> users = new List<User>();
        private bool hasValidUsers = true;

        public AddUsersCommand()
            : base("AddUsers", "Adds multiple users to the user store")
        {
            this.Arguments.Add(new Argument("-user", "-u", "The definition of a new user. ex) -u userName|password|nickname", true, true, ParseUser));
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

    [Export(typeof(CommandBase))]
    public class ListUsersCommand : CommandBase
    {
        public ListUsersCommand()
            : base("ListUsers", "Lists all users that have been registered with the system")
        {
        }

        public override bool Execute()
        {
            Console.WriteLine("Listing users...");
            return true;
        }
    }

    [Export(typeof(CommandBase))]
    public class LoginCommand : CommandBase
    {
        private string userName;
        private string password;

        public LoginCommand()
            : base("Login", "Attempts to login as the specified user")
        {
            this.Arguments.Add(new Argument("-userName", "-n", "The name of the new user", true, true, val => userName = val));
            this.Arguments.Add(new Argument("-password", "-pw", "The password for the new user", true, true, val => password = val));
        }

        public override bool Execute()
        {
            Console.WriteLine("Logging in as '{0}' with password '{1}'...", userName, password);
            return true;
        }
    }
}
