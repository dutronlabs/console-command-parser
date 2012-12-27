using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheObtuseAngle.ConsoleUtilities
{
    public sealed class HelpCommand<TCommand> : CommandBase
        where TCommand : class, ICommand
    {
        private readonly IEnumerable<TCommand> possibleCommands;
        private readonly TextWriter output;
        private readonly Action<TCommand> writeUsageMethod;
        private string commandName;

        internal HelpCommand(IEnumerable<TCommand> possibleCommands, TextWriter output, HelpCommandTemplate template, Action<TCommand> writeUsageMethod)
            : base(template.Name, template.Title, template.Description)
        {
            this.possibleCommands = possibleCommands;
            this.output = output;
            this.writeUsageMethod = writeUsageMethod;
            this.Arguments.Add(new Argument(template.CommandArgumentTemplate.Name, template.CommandArgumentTemplate.Aliases, template.CommandArgumentTemplate.Description, true, true, val => commandName = val));
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
