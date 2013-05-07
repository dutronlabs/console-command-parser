using System;
using System.Collections.Generic;
using TheObtuseAngle.ConsoleUtilities;
using TheObtuseAngle.ConsoleUtilities.Arguments;

namespace TheObtuseAngle.ParseArgsConsole
{
    public class Program
    {
        private readonly string[] consoleArgs;
        private readonly HashSet<string> instances = new HashSet<string>();
        private string scriptBase;
        private string environment;
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
                var parser = new ArgumentParser();

                if (parser.ParseArguments(consoleArgs, BuildAppArguments()) != ParseResult.Success)
                {
                    return;
                }

                Console.WriteLine("Script base: {0}", scriptBase);
                Console.WriteLine("Environment: {0}", environment);
                Console.WriteLine("Instance(s): {0}", string.Join(", ", instances));
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
                new RequiredValueArgument("-scriptBase", "-sb", "The base path of the folder that contains the upgrade scripts", val => scriptBase = val),
                new RequiredValueArgument("-environment", "-e", "The name of the environment to use for token switching", val => environment = val),
                new RequiredValueArgument("-instances", "-i", "The three (3) SQL Server instances to connect to.", 3, ParseInstancesArgument),
                new RequiredValueArgument("-user", "-u", "The SQL Server user to connect as", val => user = val),
                new RequiredPasswordArgument("-password", "-pw", "The password of the SQL Server user", val => password = val),
                new OptionalArgument("-restoreOnly", "-ro", "Whether or not to bypass the upgrade and ONLY restore the DBs", _ => restoreOnly = true),
                new OptionalArgument("-backupOnly", "-bo", "Whether or not to bypass the upgrade and ONLY backup the DBs", _ => backupOnly = true)
            };
        }

        private void ParseInstancesArgument(string rawInstances)
        {
            foreach (var instance in rawInstances.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                instances.Add(instance);
            }
        }
    }
}
