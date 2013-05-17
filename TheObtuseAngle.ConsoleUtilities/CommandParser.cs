using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheObtuseAngle.ConsoleUtilities.Commands;

namespace TheObtuseAngle.ConsoleUtilities
{
    /// <summary>
    /// Provides methods and events used to parse raw console arguments into <see cref="TCommand"/> instances.
    /// </summary>
    /// <typeparam name="TCommand">The concrete <see cref="ICommand"/> implementation to parse the raw arguments into.</typeparam>
    public class CommandParser<TCommand> : ArgumentParser
        where TCommand : class, ICommand
    {
        /// <summary>
        /// Constructs a new <see cref="CommandParser{TCommand}"/> instance using the default <see cref="ParseOptions"/>.
        /// </summary>
        public CommandParser()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CommandParser{TCommand}"/> instance using the given <see cref="ParseOptions"/>.
        /// </summary>
        /// <param name="parseOptions">The <see cref="ParseOptions"/> to use.</param>
        public CommandParser(ParseOptions parseOptions)
            : base(parseOptions)
        {
        }

        /// <summary>
        /// Raised for every possible command when the parser is writing the usage help for all possible commands.
        /// This is an opportunity to override the command's properties or skip it entirely.
        /// </summary>
        public event Func<TCommand, WriteUsageOverrides> WritingSimpleCommandUsage;

        /// <summary>
        /// Raised when the parser is done writing the simple usage help for all possible commands.
        /// This is an opportunity to write additional text to the output stream from your application. 
        /// </summary>
        public event Action<TextWriter> DoneWritingSimpleCommandUsage;

        /// <summary>
        /// Raised when the parser is writing the detailed usage help for a single command.
        /// This is an opportunity to override the command's properties or skip it entirely.
        /// </summary>
        public event Func<TCommand, WriteUsageOverrides> WritingDetailedCommandUsage;

        /// <summary>
        /// Raised when the parser is done writing the detailed usage help for a single command.
        /// This is an opportunity to write additional text to the output stream from your application. 
        /// </summary>
        public event Action<TCommand, TextWriter> DoneWritingDetailedCommandUsage;

        /// <summary>
        /// Parses the given raw console arguments into a matching <see cref="TCommand"/> instances from the given collection of possible commands.
        /// </summary>
        /// <param name="consoleArgs">The raw console arguments.</param>
        /// <param name="possibleCommands">The collection of possible commands to look for when parsing.</param>
        /// <param name="matchingCommand">The matching command.</param>
        /// <returns>A <see cref="ParseResult"/> indicating the result of the parsing.</returns>
        public virtual ParseResult ParseCommand(string[] consoleArgs, IEnumerable<TCommand> possibleCommands, out TCommand matchingCommand)
        {
            matchingCommand = null;

            if (possibleCommands == null)
            {
                throw new ArgumentNullException("possibleCommands");
            }

            if (consoleArgs.HasArgument(ParseOptions.DebugFlag))
            {
                HandleDebugFlag();
            }

            ParseIncludedArguments(consoleArgs);
            var allCommands = possibleCommands.ToList();
            
            foreach (var cmd in allCommands)
            {
                cmd.OnBeforeParse();

                var commandBase = cmd as CommandBase;
                if (ParseOptions.UseArgumentComposition && commandBase != null)
                {
                    commandBase.ComposeArguments();
                }
            }

            if (consoleArgs.Length == 0)
            {
                if (ParseOptions.AllowNoMatchingCommands)
                {
                    return ParseResult.NoMatchFound;
                }

                ConsoleHelper.WriteLine("No arguments given.");
                WriteUsage(allCommands);
                return ParseResult.Failure;
            }

            var hasHelpArg = helpArgument != null && consoleArgs.HasArgument(helpArgument);

            // The help command needs some special treatment to keep this method as generic as it can be.
            if (ParseOptions.HelpCommandTemplate != null && consoleArgs[0].Equals(ParseOptions.HelpCommandTemplate.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                var helpCommand = new HelpCommand<TCommand>(allCommands, this);
                var helpParseResult = ParseArgumentsForCommand(helpCommand, consoleArgs, hasHelpArg);

                if (helpParseResult == ParseResult.Failure)
                {
                    return helpParseResult;
                }

                helpCommand.Execute();
                return ParseResult.DisplayedHelp;
            }

            var command = allCommands.SingleOrDefault(c => c.Name.Equals(consoleArgs[0], StringComparison.InvariantCultureIgnoreCase));

            if (command == null)
            {
                if (hasHelpArg)
                {
                    WriteUsage(allCommands);
                    return ParseResult.DisplayedHelp;
                }

                if (ParseOptions.AllowNoMatchingCommands)
                {
                    return ParseResult.NoMatchFound;
                }

                ConsoleHelper.WriteLine("Invalid command.");
                WriteUsage(allCommands);
                return ParseResult.Failure;
            }

            var result = ParseArgumentsForCommand(command, consoleArgs, hasHelpArg);

            if (result != ParseResult.Success)
            {
                return result;
            }

            matchingCommand = command;
            return ParseResult.Success;
        }

        /// <summary>
        /// Parses the given raw console arguments from the given collection of possible commands and executes the match immediately.
        /// </summary>
        /// <param name="consoleArgs">The raw console arguments.</param>
        /// <param name="possibleCommands">The collection of possible commands to look for when parsing.</param>
        /// <returns>A <see cref="ParseResult"/> indicating the result of the parsing.</returns>
        public virtual ParseResult ParseCommandAndExecute(string[] consoleArgs, IEnumerable<TCommand> possibleCommands)
        {
            TCommand command;
            var result = ParseCommand(consoleArgs, possibleCommands, out command);

            if (result != ParseResult.Success)
            {
                return result;
            }

            if (ParseOptions.ThrowOnParseAndExecuteException)
            {
                command.Execute();
                return ParseResult.Success;
            }

            try
            {
                command.Execute();
                return ParseResult.Success;
            }
            catch (Exception e)
            {
                ConsoleHelper.WriteLine("Error executing the '{0}' command", command.Name);
                ConsoleHelper.WriteException(e);
                return ParseResult.Failure;
            }
        }

        /// <summary>
        /// Writes the usage help for the given collection of commands.
        /// </summary>
        /// <param name="commands">The collection of commands.</param>
        public virtual void WriteUsage(IEnumerable<TCommand> commands)
        {
            if (quietMode)
            {
                return;
            }

            var templatesByCommand = new Dictionary<ICommand, CommandTemplate>();

            foreach (var command in commands)
            {
                var result = RaiseWritingSimpleCommandUsage(command);

                if (result != null && result.Skip)
                {
                    continue;
                }

                var pair = GetCommandTemplatePairFromOverride(command, result);
                templatesByCommand.Add(pair.Key, pair.Value);
            }

            if (ParseOptions.HelpCommandTemplate != null && !templatesByCommand.Any(pair => pair.Key.Name.Equals(ParseOptions.HelpCommandTemplate.Name) && pair.Key.Description.Equals(ParseOptions.HelpCommandTemplate.Description)))
            {
                var helpCommand = new HelpCommand<TCommand>(commands, this);
                templatesByCommand.Add(helpCommand, new CommandTemplate(helpCommand));
            }

            int maxCommandNameLength = templatesByCommand.Max(pair => pair.Value.Name.Length);
            ConsoleHelper.WriteLine();
            ConsoleHelper.WriteLine("Usage: {0} <COMMAND>", AppDomain.CurrentDomain.FriendlyName);
            ConsoleHelper.WriteLine();
            ConsoleHelper.WriteLine("Available Commands:");
            ConsoleHelper.WriteLine();

            foreach (var command in templatesByCommand.OrderBy(pair => pair.Key.Ordinal).ThenBy(pair => pair.Value.Name))
            {
                WriteUsage(command, true, maxCommandNameLength);

                if (ParseOptions.WriteBlankLineBetweenEachCommand)
                {
                    ConsoleHelper.WriteLine();
                }
            }

            RaiseDoneWritingSimpleCommandUsage();
        }

        /// <summary>
        /// Writes the usage of the given command.
        /// </summary>
        /// <param name="command">The command to write usage help for.</param>
        public void WriteUsage(TCommand command)
        {
            var result = RaiseWritingDetailedCommandUsage(command);

            if (result != null && result.Skip)
            {
                return;
            }

            WriteUsage(GetCommandTemplatePairFromOverride(command, result), false, null);
            RaiseDoneWritingDetailedCommandUsage(command);
        }

        private void WriteUsage(KeyValuePair<ICommand, CommandTemplate> commandTemplatePair, bool isWritingMultipleCommands, int? maxCommandNameLength)
        {
            if (quietMode)
            {
                return;
            }

            int? offsetOverride = null;
            var spacer = "  ";
            var command = commandTemplatePair.Key;
            var template = commandTemplatePair.Value;

            if (!isWritingMultipleCommands)
            {
                offsetOverride = 7; // 7 = The length of the usage prefix, "Usage: "
                spacer = string.Empty;
                ConsoleHelper.WriteLine();

                if (!string.IsNullOrWhiteSpace(template.Description))
                {
                    ConsoleHelper.WriteLine(template.Description);
                    ConsoleHelper.WriteLine();
                }

                ConsoleHelper.Write("Usage: {0} ", AppDomain.CurrentDomain.FriendlyName);
            }

            var orderedArgs = command.Arguments;
            var formatString = maxCommandNameLength.HasValue ? string.Format("{{0}}{{1,-{0}}} ", maxCommandNameLength.Value) : "{0}{1} ";
            ConsoleHelper.Write(formatString, spacer, template.Name);

            if (ParseOptions.ArgumentOrderByFunction != null)
            {
                orderedArgs = ParseOptions.ArgumentOrderByFunction(command.Arguments).ToList();
            }

            if (!isWritingMultipleCommands)
            {
                WriteUsage(orderedArgs, false, false, offsetOverride);
            }
            else
            {
                switch (ParseOptions.WriteUsageMode)
                {
                    case WriteUsageMode.CommandNameAndTitle:
                        ConsoleHelper.Write(template.Title);
                        break;

                    case WriteUsageMode.CommandNameAndArguments:
                        var argumentUsageBuilder = new StringBuilder();

                        foreach (var argument in orderedArgs)
                        {
                            argumentUsageBuilder.Append(GetArgumentUsageString(argument));
                            argumentUsageBuilder.Append(' ');
                        }

                        ConsoleHelper.WriteWrapped(argumentUsageBuilder.ToString());
                        break;
                }
            }

            ConsoleHelper.WriteLine();
        }

        private ParseResult ParseArgumentsForCommand(ICommand command, string[] consoleArgs, bool hasHelpArg)
        {
            var parseResult = consoleArgs.ParseArguments(command.Arguments.ToArray());

            if (hasHelpArg || ProcessArgumentParseResult(parseResult, command.Arguments, false) == ParseResult.Failure)
            {
                var typedCommand = command as TCommand;
                if (typedCommand == null)
                {
                    WriteUsage(GetCommandTemplatePairFromOverride(command, null), false, null);
                }
                else
                {
                    WriteUsage(typedCommand);
                }
                return hasHelpArg ? ParseResult.DisplayedHelp : ParseResult.Failure;
            }

            return ParseResult.Success;
        }

        private static KeyValuePair<ICommand, CommandTemplate> GetCommandTemplatePairFromOverride(ICommand command, WriteUsageOverrides overrideResult)
        {
            var template = new CommandTemplate(command);

            if (overrideResult != null)
            {
                if (!string.IsNullOrWhiteSpace(overrideResult.NameOverride) && !overrideResult.NameOverride.Equals(command.Name))
                {
                    template.Name = overrideResult.NameOverride;
                }
                if (!string.IsNullOrWhiteSpace(overrideResult.TitleOverride) && !overrideResult.TitleOverride.Equals(command.Title))
                {
                    template.Title = overrideResult.TitleOverride;
                }
                if (!string.IsNullOrWhiteSpace(overrideResult.DescriptionOverride) && !overrideResult.DescriptionOverride.Equals(command.Description))
                {
                    template.Description = overrideResult.DescriptionOverride;
                }
            }

            return new KeyValuePair<ICommand, CommandTemplate>(command, template);
        }

        private WriteUsageOverrides RaiseWritingSimpleCommandUsage(TCommand command)
        {
            var writingSimpleCommandUsage = WritingSimpleCommandUsage;
            return writingSimpleCommandUsage != null ? writingSimpleCommandUsage(command) : null;
        }

        private void RaiseDoneWritingSimpleCommandUsage()
        {
            var doneWritingSimpleCommandUsage = DoneWritingSimpleCommandUsage;
            if (doneWritingSimpleCommandUsage != null)
            {
                doneWritingSimpleCommandUsage(ParseOptions.OutputWriter);
            }
        }

        private WriteUsageOverrides RaiseWritingDetailedCommandUsage(TCommand command)
        {
            var writingDetailedCommandUsage = WritingDetailedCommandUsage;
            return writingDetailedCommandUsage != null ? writingDetailedCommandUsage(command) : null;
        }

        private void RaiseDoneWritingDetailedCommandUsage(TCommand command)
        {
            var doneWritingDetailedCommandUsage = DoneWritingDetailedCommandUsage;
            if (doneWritingDetailedCommandUsage != null)
            {
                doneWritingDetailedCommandUsage(command, ParseOptions.OutputWriter);
            }
        }
    }
}