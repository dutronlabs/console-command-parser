using System;
using System.Collections.Generic;
using TheObtuseAngle.ConsoleUtilities;

namespace TheObtuseAngle.ParseArgsConsole
{
    public class Program
    {
        private readonly string[] consoleArgs;
        private string scriptBase;
        private string environment;
        private string instance;
        private string user;
        private string password;
        private bool restoreOnly;
        private bool backupOnly;

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
        }

        public void Run()
        {
            try
            {
                var parserOptions = new ParseOptions();
                parserOptions.ArgumentValueSeparator = '=';
                parserOptions.DebugFlagAction = DebugFlagAction.ThreadSleep;
                var parser = new CommandParser(parserOptions);

                if (parser.ParseArguments(consoleArgs, BuildAppArguments()) != ParseResult.Success)
                {
                    return;
                }

                Console.WriteLine("Script base: {0}", scriptBase);
                Console.WriteLine("Environment: {0}", environment);
                Console.WriteLine("Instance: {0}", instance);
                Console.WriteLine("User: {0}", user);
                Console.WriteLine("Password: {0}", password);
                Console.WriteLine("Restore only: {0}", restoreOnly);
                Console.WriteLine("Backup only: {0}", backupOnly);
            }
            catch (Exception e)
            {
                e.WriteToConsole();
            }
        }

        private IEnumerable<IArgument> BuildAppArguments()
        {
            return new IArgument[]
            {
                new Argument("-scriptBase", "-sb", "The base path of the folder that contains the upgrade scripts", true, true, val => scriptBase = val),
                new Argument("-environment", "-e", "The name of the environment to use for token switching", true, true, val => environment = val),
                new Argument("-instance", "-i", "The SQL Server instance name to connect to", true, true, val => instance = val),
                new Argument("-user", "-u", "The SQL Server user to connect as", true, true, val => user = val),
                new Argument("-password", "-pw", "The password of the SQL Server user", true, true, val => password = val),
                new Argument("-restoreOnly", "-ro", "Whether or not to bypass the upgrade and ONLY restore the DBs", false, false, val => restoreOnly = true),
                new Argument("-backupOnly", "-bo", "Whether or not to bypass the upgrade and ONLY backup the DBs", false, false, val => backupOnly = true)
            };
        }
    }
}
