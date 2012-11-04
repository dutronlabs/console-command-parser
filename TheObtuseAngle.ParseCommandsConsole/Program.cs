using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TheObtuseAngle.ConsoleUtilities;

namespace TheObtuseAngle.ParseCommandsConsole
{
    public class Program
    {
        private readonly IEnumerable<ICommand> commands;
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
            this.commands = new ICommand[]
            {
                new AddUserCommand(),
                new ListUsersCommand(),
                new LoginCommand()
            };
        }

        public void Run()
        {
            try
            {
                var parserOptions = new ParseOptions();
                parserOptions.ArgumentValueSeparator = '=';
                parserOptions.DebugFlagAction = DebugFlagAction.ThreadSleep;
                var parser = new CommandParser(parserOptions);
                var result = parser.ParseCommandAndExecute(consoleArgs, commands);

                Console.WriteLine();
                Console.WriteLine(result);
            }
            catch (Exception e)
            {
                e.WriteToConsole();
            }
        }
    }

    public class AddUserCommand : CommandBase
    {
        private string userName;
        private string password;
        private string nickname;

        public AddUserCommand()
            : base("AddUser", "Adds a user to the user store")
        {
            this.Arguments.Add(new Argument("-userName", "-n", "The name of the new user", true, true, val => userName = val));
            this.Arguments.Add(new Argument("-password", "-pw", "The password for the new user", true, true, val => password = val));
            this.Arguments.Add(new Argument("-nickname", "-nn", "The nickname of the new user", true, false, val => nickname = val));
        }

        public override bool Execute()
        {
            Console.WriteLine("Adding user:{0}  Username: {1}{0}  Password: {2}{0}  Nickname: {3}", Environment.NewLine, userName, password, nickname);
            return true;
        }
    }

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
