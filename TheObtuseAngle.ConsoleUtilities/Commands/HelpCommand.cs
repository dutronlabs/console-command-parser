using System;
using System.Collections.Generic;
using System.Linq;
using TheObtuseAngle.ConsoleUtilities.Arguments;

namespace TheObtuseAngle.ConsoleUtilities.Commands
{
    internal class HelpCommand<TCommand> : CommandBase
        where TCommand : class, ICommand
    {
        private readonly IEnumerable<TCommand> possibleCommands;
        private readonly CommandParser<TCommand> parser;
        private string commandName;

        public HelpCommand(IEnumerable<TCommand> possibleCommands, CommandParser<TCommand> parser)
            : base(parser.ParseOptions.HelpCommandTemplate.Name, parser.ParseOptions.HelpCommandTemplate.Title, parser.ParseOptions.HelpCommandTemplate.Description, int.MaxValue)
        {
            this.possibleCommands = possibleCommands;
            this.parser = parser;

            var commandArgTemplate = parser.ParseOptions.HelpCommandTemplate.CommandArgumentTemplate;
            Arguments.Add(new Argument(commandArgTemplate.Name, commandArgTemplate.Aliases, commandArgTemplate.Description, true, true, val => commandName = val));
        }

        public override bool Execute()
        {
            var matchingCommand = possibleCommands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.InvariantCultureIgnoreCase));

            if (matchingCommand == null)
            {
                ConsoleHelper.WriteLine("Invalid command '{0}'", commandName);
                return false;
            }

            parser.WriteUsage(matchingCommand);
            return true;
        }
    }
}
