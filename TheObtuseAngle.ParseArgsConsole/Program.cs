using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

                if (restoreOnly && backupOnly)
                {
                    Console.WriteLine("Restore only and backup only are mutually exclusive.  Make up your mind.");
                    return;
                }

                var connectionStringBuilder = new SqlConnectionStringBuilder();
                connectionStringBuilder.DataSource = instance;
                connectionStringBuilder.UserID = user;
                connectionStringBuilder.Password = password;

                Console.WriteLine("Working in the directory: {0}", scriptBase);
                Console.WriteLine("Selected environment: {0}", environment);
                Console.WriteLine("Connection string:{0}  {1}", Environment.NewLine, connectionStringBuilder.ToString());

                if (backupOnly)
                {
                    Console.WriteLine("Performing backup only");
                }
                else if (restoreOnly)
                {
                    Console.WriteLine("Performing restore only");
                }
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
