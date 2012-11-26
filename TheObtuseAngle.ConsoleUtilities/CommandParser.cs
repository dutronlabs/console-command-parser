using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace TheObtuseAngle.ConsoleUtilities
{
    public class CommandParser
    {
        private readonly TextWriter output;
        private bool quietMode;

        public CommandParser()
            : this(ParseOptions.Defaults)
        {
        }

        public CommandParser(ParseOptions parseOptions)
        {
            if (parseOptions == null)
            {
                parseOptions = ParseOptions.Defaults;
            }

            this.output = parseOptions.OutputWriter;
            ParseOptions = parseOptions;
            ConsoleHelper.Initialize(output);
        }

        public ParseOptions ParseOptions { get; set; }

        public virtual ParseResult ParseArguments(string[] consoleArgs, IEnumerable<IArgument> possibleArguments)
        {
            if (possibleArguments == null)
            {
                throw new ArgumentNullException("possibleArguments");
            }

            var argumentArray = possibleArguments.ToArray();

            if (consoleArgs.HasArgument(ParseOptions.DebugFlag))
            {
                HandleDebugFlag();
            }

            if (ParseOptions.DisplayHelpArgument != null)
            {
                if (consoleArgs.HasArgument(ParseOptions.DisplayHelpArgument))
                {
                    WriteUsage(argumentArray);
                    return ParseResult.DisplayedHelp;
                }
            }
            
            if (ParseOptions.QuietModeArgument != null)
            {
                quietMode = consoleArgs.HasArgument(ParseOptions.QuietModeArgument);
            }

            var parseResult = consoleArgs.ParseArguments(ParseOptions, argumentArray);
            return ProcessArgumentParseResult(parseResult, argumentArray, true);
        }

        public virtual CommandParseResult ParseCommand(string[] consoleArgs, IEnumerable<ICommand> possibleCommands)
        {
            if (possibleCommands == null)
            {
                throw new ArgumentNullException("possibleCommands");
            }

            if (consoleArgs.HasArgument(ParseOptions.DebugFlag))
            {
                HandleDebugFlag();
            }

            if (ParseOptions.QuietModeArgument != null)
            {
                quietMode = consoleArgs.HasArgument(ParseOptions.QuietModeArgument);
            }

            if (consoleArgs.Length == 0)
            {
                if (ParseOptions.AllowNoMatchingCommands)
                {
                    return CommandParseResult.NoMatchFound;
                }

                WriteLine("No arguments given.");
                WriteUsage(possibleCommands);
                return CommandParseResult.Failure;
            }

            var commandsToSearch = possibleCommands.ToList();
            commandsToSearch.Add(new HelpCommand(commandsToSearch, output, WriteUsage));
            var command = commandsToSearch.SingleOrDefault(c => c.Name.Equals(consoleArgs[0], StringComparison.InvariantCultureIgnoreCase));

            if (command == null)
            {
                if (consoleArgs.HasArgument(ParseOptions.DisplayHelpArgument))
                {
                    WriteUsage(possibleCommands);
                    return CommandParseResult.DisplayedHelp;
                }

                if (ParseOptions.AllowNoMatchingCommands)
                {
                    return CommandParseResult.NoMatchFound;
                }

                WriteLine("Invalid command.");
                WriteUsage(possibleCommands);
                return CommandParseResult.Failure;
            }

            var parseResult = consoleArgs.ParseArguments(ParseOptions, command.Arguments.ToArray());
            var hasHelpArg = ParseOptions.DisplayHelpArgument != null && consoleArgs.HasArgument(ParseOptions.DisplayHelpArgument);

            if (hasHelpArg || ProcessArgumentParseResult(parseResult, command.Arguments, false) == ParseResult.Failure)
            {
                WriteUsage(command);
                return hasHelpArg ? CommandParseResult.DisplayedHelp : CommandParseResult.Failure;
            }

            return new CommandParseResult(ParseResult.Success, command);
        }

        public virtual ParseResult ParseCommandAndExecute(string[] consoleArgs, IEnumerable<ICommand> possibleCommands)
        {
            var result = ParseCommand(consoleArgs, possibleCommands);

            if (result.ParseResult != ParseResult.Success)
            {
                return result.ParseResult;
            }

            if (ParseOptions.ThrowOnParseAndExecuteException)
            {
                result.Command.Execute();
                return ParseResult.Success;
            }

            try
            {
                result.Command.Execute();
                return ParseResult.Success;
            }
            catch (Exception e)
            {
                WriteLine("Error executing the '{0}' command", result.Command.Name);
                WriteException(e);
                return ParseResult.Failure;
            }
        }        

        public virtual void WriteUsage(IEnumerable<ICommand> commands)
        {
            if (quietMode)
            {
                return;
            }

            var commandList = commands.OrderBy(c => c.Name).ToList();
            if (!commandList.Any(c => c.Name.Equals(HelpCommand.HelpCommandName) && c.Description.Equals(HelpCommand.HelpCommandDescription)))
            {
                commandList.Add(new HelpCommand(commands, output, WriteUsage));
            }

            int maxCommandNameLength = commandList.Max(c => c.Name.Length);
            output.WriteLine();
            output.WriteLine("Usage: {0} <COMMAND>", AppDomain.CurrentDomain.FriendlyName);
            output.WriteLine();
            output.WriteLine("Available Commands:");

            foreach (var command in commandList)
            {
                WriteUsage(command, false, false, maxCommandNameLength);
                output.WriteLine();
            }
        }

        public void WriteUsage(ICommand command)
        {
            WriteUsage(command, true, true);
        }

        protected virtual void WriteUsage(ICommand command, bool writePrefix, bool writeArgumentDetails, int? maxCommandNameLength = null)
        {
            if (quietMode)
            {
                return;
            }

            int? offsetOverride = null;
            var spacer = "  ";

            if (writePrefix)
            {
                offsetOverride = 7; // 7 = The length of the usage prefix, "Usage: "
                spacer = string.Empty;
                output.WriteLine();
                output.Write("Usage: {0} ", AppDomain.CurrentDomain.FriendlyName);
            }

            var formatString = maxCommandNameLength.HasValue ? string.Format("{{0}}{{1,-{0}}} ", maxCommandNameLength.Value) : "{0}{1} ";
            output.Write(formatString, spacer, command.Name);

            if (writeArgumentDetails)
            {
                WriteUsage(command.Arguments, false, offsetOverride);
            }
            else
            {
                var argumentUsageBuilder = new StringBuilder();

                foreach (var argument in command.Arguments.OrderByDescending(a => a.IsRequired).ThenBy(a => a.Name))
                {
                    argumentUsageBuilder.Append(GetArgumentUsageString(argument));
                    argumentUsageBuilder.Append(' ');
                }

                output.WriteWrapped(argumentUsageBuilder.ToString(), offsetOverride);
            }

            output.WriteLine();
        }

        public void WriteUsage(IEnumerable<IArgument> arguments)
        {
            WriteUsage(arguments, true);
        }

        protected virtual void WriteUsage(IEnumerable<IArgument> arguments, bool writePrefix, int? offsetOverride = null)
        {
            if (quietMode)
            {
                return;
            }

            var argList = arguments.OrderByDescending(a => a.IsRequired).ThenBy(a => a.Name).ToList();
            if (ParseOptions.DisplayHelpArgument != null && !argList.Contains(ParseOptions.DisplayHelpArgument))
            {
                argList.Add(ParseOptions.DisplayHelpArgument);
            }
            if (ParseOptions.QuietModeArgument != null && !argList.Contains(ParseOptions.QuietModeArgument))
            {
                argList.Add(ParseOptions.QuietModeArgument);
            }

            int maxNameLength = 0;
            bool hasRequiredArgs = false;
            var formatBuilder = new StringBuilder(" ");
            var argumentUsageBuilder = new StringBuilder();

            if (writePrefix)
            {
                if (!offsetOverride.HasValue)
                {
                    offsetOverride = 7; // 7 = The length of the usage prefix, "Usage: "
                }
                output.Write("Usage: {0} ", AppDomain.CurrentDomain.FriendlyName); 
            }

            foreach (var arg in argList)
            {
                int length = arg.RequiresValue ? arg.Name.Length + ParseOptions.ArgumentValueIndicator.Length + 1 : arg.Name.Length;
                maxNameLength = length > maxNameLength ? length : maxNameLength;
                hasRequiredArgs = hasRequiredArgs || arg.IsRequired;
                argumentUsageBuilder.Append(GetArgumentUsageString(arg));
                argumentUsageBuilder.Append(' ');
            }

            output.WriteWrapped(argumentUsageBuilder.ToString(), offsetOverride);
            output.WriteLine();
            output.WriteLine();
            formatBuilder.Append(hasRequiredArgs ? string.Format("{{0,-{0}}}", ParseOptions.RequiredArgumentIndicator.Length) : "{0}");
            formatBuilder.AppendFormat("{{1,-{0}}}", maxNameLength);
            string format = formatBuilder.ToString();

            foreach (var arg in argList)
            {
                var aliases = arg.Aliases.IsEmpty() ? new List<string>() : arg.Aliases.Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
                bool hasDescription = !string.IsNullOrWhiteSpace(arg.Description);
                bool hasAliases = aliases.Count > 0;
                var descriptionBuilder = new StringBuilder();

                if (hasDescription || hasAliases)
                {
                    descriptionBuilder.Append(" -");

                    if (hasDescription)
                    {
                        descriptionBuilder.Append(' ');
                        descriptionBuilder.Append(arg.Description);
                    }
                    if (hasAliases)
                    {
                        descriptionBuilder.AppendFormat(" (alias{0}: {1})", aliases.Count > 1 ? "es" : string.Empty, string.Join(", ", aliases));
                    }
                }

                output.Write(format,
                    arg.IsRequired ? ParseOptions.RequiredArgumentIndicator : string.Empty,
                    arg.RequiresValue ? string.Format("{0}{1}{2}", arg.Name, ParseOptions.ArgumentValueSeparator, ParseOptions.ArgumentValueIndicator) : arg.Name);

                output.WriteWrapped(descriptionBuilder.ToString(), Console.CursorLeft + 3); // 3 = The length of the description prefix, " - "
                output.WriteLine();
            }
        }

        protected virtual string GetArgumentUsageString(IArgument argument)
        {
            return string.Format(argument.IsRequired ? ParseOptions.RequiredArgumentFormat : ParseOptions.OptionalArgumentFormat, argument.Name);
        }

        protected virtual void WriteException(Exception exception)
        {
            if (!quietMode && ParseOptions.WriteExceptionsToConsole)
            {
                exception.WriteToConsole();
            }
        }

        protected void Write(string format, params object[] args)
        {
            if (!quietMode)
            {
                output.Write(format, args);
            }
        }

        protected void WriteLine(string format, params object[] args)
        {
            if (!quietMode)
            {
                output.WriteLine(format, args);
            }
        }

        private ParseResult ProcessArgumentParseResult(ArgumentParseResult parseResult, IEnumerable<IArgument> possibleArguments, bool writeUsage)
        {
            if (parseResult.IsSuccess)
            {
                return ParseResult.Success;
            }

            if (!quietMode)
            {
                string errorMessage;

                if (parseResult.ArgumentWithMissingValue != null)
                {
                    errorMessage = string.Format("Missing argument value for '{0}'.", parseResult.ArgumentWithMissingValue.Name);
                }
                else if (parseResult.ArgumentValueSetterExceptionPair != null)
                {
                    errorMessage = string.Format("Error setting the value for the argument '{0}'.", parseResult.ArgumentValueSetterExceptionPair.Item1.Name);
                }
                else if (parseResult.MissingRequiredArguments.HasItems())
                {
                    errorMessage = string.Format("Missing the following required arguments: {0}", string.Join(", ", parseResult.MissingRequiredArguments.Select(a => a.Name)));
                }
                else
                {
                    errorMessage = "No valid arguments given.";
                }

                output.WriteLine(errorMessage);

                if (ParseOptions.WriteExceptionsToConsole && parseResult.ArgumentValueSetterExceptionPair != null)
                {
                    parseResult.ArgumentValueSetterExceptionPair.Item2.WriteToConsole();
                }

                if (writeUsage)
                {
                    WriteUsage(possibleArguments);
                }
            }

            return ParseResult.Failure;
        }

        private void HandleDebugFlag()
        {
            if (!ParseOptions.EnableDebugFlag)
            {
                return;
            }

            switch (ParseOptions.DebugFlagAction)
            {
                case DebugFlagAction.None:
                    return;
                case DebugFlagAction.DebuggerLaunch:
                    Debugger.Launch();
                    break;
                case DebugFlagAction.ThreadSleep:
                    output.WriteLine("Pausing for 10 seconds to allow you to attach to the process...");
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                    break;                
            }
        }
    }
}
