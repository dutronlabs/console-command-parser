using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheObtuseAngle.ConsoleUtilities
{
    public sealed class HelpCommand : CommandBase
    {
        private readonly IEnumerable<ICommand> possibleCommands;
        private readonly TextWriter output;
        private readonly Action<ICommand> writeUsageMethod;
        private string commandName;

        internal HelpCommand(IEnumerable<ICommand> possibleCommands, TextWriter output, Action<ICommand> writeUsageMethod)
            : base("Help", "Displays usage information for the given command")
        {
            this.possibleCommands = possibleCommands;
            this.output = output;
            this.writeUsageMethod = writeUsageMethod;
            this.Arguments.Add(new Argument("-command", "-c", "The name of the command to display help for", true, true, val => commandName = val));
        }

        public override bool Execute()
        {
            var matchingCommand = possibleCommands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.InvariantCultureIgnoreCase));

            if (matchingCommand == null)
            {
                output.WriteLine("Invalid command '{0}'", commandName);
                return false;
            }

            writeUsageMethod(matchingCommand);
            return true;
        }
    }
}
