using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheObtuseAngle.ConsoleUtilities
{
    public class CommandParser
    {
        private readonly TextWriter output;
        private readonly ParseOptions parseOptions;
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

            this.parseOptions = parseOptions;
            this.output = parseOptions.OutputWriter;
            ConsoleHelper.Initialize(output);
        }

        public virtual ParseResult ParseArguments(string[] consoleArgs, IEnumerable<IArgument> possibleArguments)
        {
            if (possibleArguments == null)
            {
                throw new ArgumentNullException("possibleArguments");
            }

            if (consoleArgs.HasArgument(parseOptions.DebugFlag))
            {
                ConsoleHelper.HandleDebugFlag(parseOptions.EnableDebugFlag, parseOptions.DebugFlagAction);
            }

            var argList = possibleArguments.ToList();
            argList.AddRange(new[] { Argument.QuietModeArgument, Argument.HelpArgument });
            quietMode = consoleArgs.HasArgument(Argument.QuietModeArgument);

            if (consoleArgs.HasArgument(Argument.HelpArgument))
            {
                WriteUsage(argList);
                return ParseResult.DisplayedHelp;
            }

            var parseResult = ParseArgumentsInternal(consoleArgs, argList);
            return ProcessArgumentParseResult(parseResult, argList, true);
        }

        public virtual CommandParseResult ParseCommand(string[] consoleArgs, IEnumerable<ICommand> possibleCommands)
        {
            if (possibleCommands == null)
            {
                throw new ArgumentNullException("possibleCommands");
            }

            if (consoleArgs.HasArgument(parseOptions.DebugFlag))
            {
                ConsoleHelper.HandleDebugFlag(parseOptions.EnableDebugFlag, parseOptions.DebugFlagAction);
            }

            var commands = possibleCommands.ToList();
            commands.Add(new HelpCommand(possibleCommands, output, WriteCommandUsage));
            quietMode = consoleArgs.HasArgument(Argument.QuietModeArgument);

            if (consoleArgs.Length == 0)
            {
                WriteLine("No arguments given.");
                WriteUsage(commands);
                return CommandParseResult.Failure;
            }

            var command = commands.SingleOrDefault(c => c.Name.Equals(consoleArgs[0], StringComparison.InvariantCultureIgnoreCase));

            if (command == null)
            {
                WriteLine("Invalid command.");
                WriteUsage(commands);
                return CommandParseResult.Failure;
            }

            var parseResult = ParseArgumentsInternal(consoleArgs, command.Arguments);
            var hasHelpArg = consoleArgs.HasArgument(Argument.HelpArgument);

            if (hasHelpArg || ProcessArgumentParseResult(parseResult, command.Arguments, false) == ParseResult.Failure)
            {
                WriteCommandUsage(command);
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

            if (parseOptions.ThrowOnParseAndExecuteException)
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

        protected virtual void WriteUsage(IEnumerable<ICommand> commands)
        {
            if (quietMode)
            {
                return;
            }

            output.WriteLine();
            output.WriteLine("Usage: {0} <COMMAND>", AppDomain.CurrentDomain.FriendlyName);
            output.WriteLine();
            output.WriteLine("Available Commands:");

            foreach (var command in commands)
            {
                WriteCommandUsage(command, false, false);
            }
        }

        private void WriteCommandUsage(ICommand command)
        {
            WriteCommandUsage(command, true, true);
        }

        protected virtual void WriteCommandUsage(ICommand command, bool writePrefix, bool writeArgumentDetails)
        {
            if (quietMode)
            {
                return;
            }

            if (writePrefix)
            {
                output.WriteLine();
                output.WriteLine("Usage:");
            }

            output.Write("\t{0} ", command.Name);

            if (writeArgumentDetails)
            {
                WriteUsage(command.Arguments, false);
            }
            else
            {
                foreach (var argument in command.Arguments.OrderByDescending(a => a.IsRequired))
                {
                    WriteArgument(argument, true);
                }
            }

            output.WriteLine();
        }

        protected virtual void WriteUsage(IEnumerable<IArgument> arguments, bool writePrefix = true)
        {
            if (quietMode)
            {
                return;
            }

            int maxNameLength = 0;
            bool hasRequiredArgs = false;
            var formatBuilder = new StringBuilder(" ");
            var orderedArgs = arguments.OrderByDescending(a => a.IsRequired).ToList();

            if (writePrefix)
            {
                output.Write("Usage: {0} ", AppDomain.CurrentDomain.FriendlyName); 
            }

            foreach (var arg in orderedArgs)
            {
                int length = arg.RequiresValue ? arg.Name.Length + parseOptions.ArgumentValueIndicator.Length + 1 : arg.Name.Length;
                maxNameLength = length > maxNameLength ? length : maxNameLength;
                hasRequiredArgs = hasRequiredArgs || arg.IsRequired;
                WriteArgument(arg, false);
            }

            output.WriteLine();
            output.WriteLine();
            formatBuilder.Append(hasRequiredArgs ? string.Format("{{0,-{0}}}", parseOptions.RequiredArgumentIndicator.Length) : "{0}");
            formatBuilder.AppendFormat("{{1,-{0}}} - ", maxNameLength);
            string format = formatBuilder.ToString();

            foreach (var arg in arguments)
            {
                var description = string.IsNullOrWhiteSpace(arg.Alias)
                    ? arg.Description
                    : string.Format("{0} (alias: {1})", arg.Description, arg.Alias);

                output.Write(format,
                    arg.IsRequired ? parseOptions.RequiredArgumentIndicator : string.Empty,
                    arg.RequiresValue ? string.Format("{0}{1}{2}", arg.Name, parseOptions.ArgumentValueSeparator, parseOptions.ArgumentValueIndicator) : arg.Name);

                if (!parseOptions.IsUsingConsoleOutput || Console.CursorLeft + description.Length < Console.BufferWidth)
                {
                    output.Write(description);
                }
                else
                {
                    int position = Console.CursorLeft;
                    int lineLength = Console.BufferWidth - position - 1; // The extra -1 is to prevent an extra line if the word ends exactly where the buffer ends.
                    var lineBuilder = new StringBuilder();

                    foreach (var word in description.Split(new[] { ' ' }))
                    {
                        if (lineBuilder.Length + word.Length + 1 > lineLength)
                        {
                            output.Write(lineBuilder.ToString());
                            output.WriteLine();
                            output.Write(string.Empty.PadLeft(position));
                            lineBuilder.Clear();
                        }

                        lineBuilder.Append(word);
                        lineBuilder.Append(' ');
                    }

                    if (lineBuilder.Length > 0)
                    {
                        output.Write(lineBuilder.ToString());
                    }
                }

                output.WriteLine();
            }
        }

        protected virtual void WriteArgument(IArgument argument, bool useRequiredFormatter)
        {
            if (quietMode)
            {
                return;
            }

            if (argument.IsRequired)
            {
                output.Write(useRequiredFormatter ? string.Format(parseOptions.RequiredArgumentFormat, argument.Name) : argument.Name);
            }
            else
            {
                output.Write(parseOptions.OptionalArgumentFormat, argument.Name);
            }

            output.Write(' ');
        }

        protected virtual void WriteException(Exception exception)
        {
            if (!quietMode && parseOptions.WriteExceptionsToConsole)
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

        private ArgumentParseResult ParseArgumentsInternal(string[] consoleArgs, IEnumerable<IArgument> possibleArguments)
        {
            if (consoleArgs.Length == 0)
            {
                return new ArgumentParseResult(hasNoConsoleArgs: false);
            }

            var args = possibleArguments.ToList();
            var parsedArgs = new List<IArgument>();

            if (args.HasDuplicates(a => a.Name) || args.HasDuplicates(a => a.Alias))
            {
                throw new ArgumentException("Duplicate arguments detected.", "possibleArguments");
            }

            for (int i = 0; i < consoleArgs.Length; i++)
            {
                string argName;
                string argValue;

                if (parseOptions.ArgumentValueSeparator == ' ')
                {
                    argName = consoleArgs[i];
                    argValue = i + 1 < consoleArgs.Length ? consoleArgs[i + 1] : null;
                    i++;
                }
                else
                {
                    var argParts = consoleArgs[i].Split(new[] { parseOptions.ArgumentValueSeparator });
                    argName = argParts[0];
                    argValue = argParts.Length > 1 ? argParts[1] : null;
                }

                var matchingArg = args.FirstOrDefault(a => a.Name.Equals(argName, StringComparison.InvariantCultureIgnoreCase) || a.Alias.Equals(argName, StringComparison.InvariantCultureIgnoreCase));

                if (matchingArg == null)
                {
                    continue;
                }

                if (matchingArg.RequiresValue && string.IsNullOrWhiteSpace(argValue))
                {
                    return new ArgumentParseResult(matchingArg);
                }

                if (matchingArg.ValueSetter != null)
                {
                    if (parseOptions.ThrowOnValueSetterException)
                    {
                        matchingArg.SetValue(argValue);
                    }
                    else
                    {
                        try
                        {
                            matchingArg.SetValue(argValue);
                        }
                        catch (Exception valueSetterException)
                        {
                            return new ArgumentParseResult(new Tuple<IArgument, Exception>(matchingArg, valueSetterException));
                        }
                    } 
                }

                parsedArgs.Add(matchingArg);
            }

            var missingRequiredArgs = args.Where(a => a.IsRequired && !parsedArgs.Contains(a));

            if (missingRequiredArgs.Any())
            {
                return new ArgumentParseResult(missingRequiredArgs);
            }

            return new ArgumentParseResult();
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

                if (parseResult.HasNoConsoleArgs)
                {
                    errorMessage = "No arguments given.";
                }
                else if (parseResult.ArgumentWithMissingValue != null)
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

                if (parseOptions.WriteExceptionsToConsole && parseResult.ArgumentValueSetterExceptionPair != null)
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
    }
}
