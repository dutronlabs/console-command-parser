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

            if (consoleArgs.HasArgument(ParseOptions.DebugFlag))
            {
                HandleDebugFlag();
            }

            var argList = possibleArguments.ToList();

            if (ParseOptions.DisplayHelpArgument != null)
            {
                if (consoleArgs.HasArgument(ParseOptions.DisplayHelpArgument))
                {
                    WriteUsage(argList);
                    return ParseResult.DisplayedHelp;
                }

                argList.Add(ParseOptions.DisplayHelpArgument);
            }
            
            if (ParseOptions.QuietModeArgument != null)
            {
                argList.Add(ParseOptions.QuietModeArgument);
                quietMode = consoleArgs.HasArgument(ParseOptions.QuietModeArgument);
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

            if (consoleArgs.HasArgument(ParseOptions.DebugFlag))
            {
                HandleDebugFlag();
            }

            var commands = possibleCommands.ToList();
            commands.Add(new HelpCommand(commands, output, WriteUsage));

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
                WriteUsage(commands);
                return CommandParseResult.Failure;
            }

            var command = commands.SingleOrDefault(c => c.Name.Equals(consoleArgs[0], StringComparison.InvariantCultureIgnoreCase));

            if (command == null)
            {
                if (ParseOptions.AllowNoMatchingCommands)
                {
                    if (consoleArgs.HasArgument(ParseOptions.DisplayHelpArgument))
                    {
                        WriteUsage(commands);
                        return CommandParseResult.DisplayedHelp;
                    }
                    return CommandParseResult.NoMatchFound;
                }

                WriteLine("Invalid command.");
                WriteUsage(commands);
                return CommandParseResult.Failure;
            }

            var parseResult = ParseArgumentsInternal(consoleArgs, command.Arguments);
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

            var commandList = commands.ToList();
            if (!commandList.Any(c => c.Name.Equals(HelpCommand.HelpCommandName) && c.Description.Equals(HelpCommand.HelpCommandDescription)))
            {
                commandList.Add(new HelpCommand(commands, output, WriteUsage));
            }

            output.WriteLine();
            output.WriteLine("Usage: {0} <COMMAND>", AppDomain.CurrentDomain.FriendlyName);
            output.WriteLine();
            output.WriteLine("Available Commands:");

            foreach (var command in commandList)
            {
                WriteUsage(command, false, false);
            }
        }

        public void WriteUsage(ICommand command)
        {
            WriteUsage(command, true, true);
        }

        protected virtual void WriteUsage(ICommand command, bool writePrefix, bool writeArgumentDetails)
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

            output.Write("{0}{1} ", spacer, command.Name);

            if (writeArgumentDetails)
            {
                WriteUsage(command.Arguments, false);
            }
            else
            {
                var argumentUsageBuilder = new StringBuilder();

                foreach (var argument in command.Arguments.OrderByDescending(a => a.IsRequired))
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

        protected virtual void WriteUsage(IEnumerable<IArgument> arguments, bool writePrefix)
        {
            if (quietMode)
            {
                return;
            }

            int? offsetOverride = null;
            int maxNameLength = 0;
            bool hasRequiredArgs = false;
            var formatBuilder = new StringBuilder(" ");
            var orderedArgs = arguments.OrderByDescending(a => a.IsRequired).ToList();
            var argumentUsageBuilder = new StringBuilder();

            if (writePrefix)
            {
                offsetOverride = 7; // 7 = The length of the usage prefix, "Usage: "
                output.Write("Usage: {0} ", AppDomain.CurrentDomain.FriendlyName); 
            }

            foreach (var arg in orderedArgs)
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

            foreach (var arg in orderedArgs)
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

        private ArgumentParseResult ParseArgumentsInternal(string[] consoleArgs, IEnumerable<IArgument> possibleArguments)
        {
            var args = possibleArguments.ToList();
            var parsedArgs = new List<IArgument>();

            if (args.HasDuplicates(true, a => a.Name) || args.SelectMany(a => a.Aliases).HasDuplicates(true))
            {
                throw new ArgumentException("Duplicate arguments detected.", "possibleArguments");
            }

            for (int i = 0; i < consoleArgs.Length; i++)
            {
                string argName;
                string argValue;
                bool needsIncrement = false;

                if (ParseOptions.ArgumentValueSeparator == ' ')
                {
                    argName = consoleArgs[i];
                    argValue = i + 1 < consoleArgs.Length ? consoleArgs[i + 1] : null;
                    needsIncrement = true;
                }
                else
                {
                    var argParts = consoleArgs[i].Split(new[] { ParseOptions.ArgumentValueSeparator });
                    argName = argParts[0];
                    argValue = argParts.Length > 1 ? argParts[1] : null;
                }

                var matchingArg = args.FirstOrDefault(a =>
                    a.Name.Equals(argName, StringComparison.InvariantCultureIgnoreCase) ||
                    (a.Aliases != null && a.Aliases.Any(alias => alias != null && alias.Equals(argName, StringComparison.InvariantCultureIgnoreCase))));

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
                    if (ParseOptions.ThrowOnValueSetterException)
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

                if (needsIncrement)
                {
                    i++;
                }
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
