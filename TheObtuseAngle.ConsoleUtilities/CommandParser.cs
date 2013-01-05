using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TheObtuseAngle.ConsoleUtilities.Arguments;
using TheObtuseAngle.ConsoleUtilities.Commands;

namespace TheObtuseAngle.ConsoleUtilities
{
    public class CommandParser
    {
        protected IArgument quietModeArgument;
        protected IArgument helpArgument;
        protected IArgument interactiveModeArgument;
        protected bool quietMode;
        protected bool interactiveMode;

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

            ParseOptions = parseOptions;
            ParseOptions.ArgumentTemplateChanged += OnArgumentTemplateChanged;
            ConsoleHelper.Initialize(parseOptions);
            SetAllIncludedArguments();
        }

        public ParseOptions ParseOptions { get; set; }

        public bool InteractiveMode
        {
            get { return interactiveMode; }
        }

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

            var argumentArray = possibleArguments.ToArray();
            ParseIncludedArguments(consoleArgs);

            if (helpArgument != null && consoleArgs.HasArgument(helpArgument))
            {
                WriteUsage(argumentArray);
                return ParseResult.DisplayedHelp;
            }

            var parseResult = consoleArgs.ParseArguments(argumentArray);
            return ProcessArgumentParseResult(parseResult, argumentArray, true);
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

            var argList = arguments.OrderByDescending(a => a.IsRequired).ThenBy(a => a.Ordinal).ThenBy(a => a.Name).ToList();
            if (helpArgument != null && !argList.Contains(helpArgument))
            {
                argList.Add(helpArgument);
            }
            if (quietModeArgument != null && !argList.Contains(quietModeArgument))
            {
                argList.Add(quietModeArgument);
            }
            if (interactiveModeArgument != null && !argList.Contains(interactiveModeArgument))
            {
                argList.Add(interactiveModeArgument);
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
                ConsoleHelper.Write("Usage: {0} ", AppDomain.CurrentDomain.FriendlyName); 
            }

            foreach (var arg in argList)
            {
                int length = arg.RequiresValue ? arg.Name.Length + ParseOptions.ArgumentValueIndicator.Length + 1 : arg.Name.Length;
                maxNameLength = length > maxNameLength ? length : maxNameLength;
                hasRequiredArgs = hasRequiredArgs || arg.IsRequired;
                argumentUsageBuilder.Append(GetArgumentUsageString(arg));
                argumentUsageBuilder.Append(' ');
            }

            ConsoleHelper.WriteWrapped(argumentUsageBuilder.ToString(), offsetOverride);
            ConsoleHelper.WriteLine();
            ConsoleHelper.WriteLine();
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

                ConsoleHelper.Write(format,
                    arg.IsRequired ? ParseOptions.RequiredArgumentIndicator : string.Empty,
                    arg.RequiresValue ? string.Format("{0}{1}{2}", arg.Name, ParseOptions.ArgumentValueSeparator, ParseOptions.ArgumentValueIndicator) : arg.Name);

                ConsoleHelper.WriteWrapped(descriptionBuilder.ToString(), Console.CursorLeft + 3); // 3 = The length of the description prefix, " - "
                ConsoleHelper.WriteLine();
            }
        }

        protected virtual string GetArgumentUsageString(IArgument argument)
        {
            return string.Format(argument.IsRequired ? ParseOptions.RequiredArgumentFormat : ParseOptions.OptionalArgumentFormat, argument.Name);
        }

        protected ParseResult ProcessArgumentParseResult(ArgumentParseResult parseResult, IEnumerable<IArgument> possibleArguments, bool writeUsage)
        {
            if (parseResult.IsSuccess)
            {
                return ParseResult.Success;
            }
            
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

            ConsoleHelper.WriteError(errorMessage);

            if (parseResult.ArgumentValueSetterExceptionPair != null)
            {
                ConsoleHelper.WriteException(parseResult.ArgumentValueSetterExceptionPair.Item2);
            }

            if (writeUsage)
            {
                WriteUsage(possibleArguments);
            }

            return ParseResult.Failure;
        }

        protected void HandleDebugFlag()
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
                    ConsoleHelper.WriteLine("Pausing for 10 seconds to allow you to attach to the process...");
                    Thread.Sleep(TimeSpan.FromSeconds(ParseOptions.ThreadSleepTime));
                    break;                
            }
        }

        protected void ParseIncludedArguments(string[] consoleArgs)
        {
            // First reset all the args in case this is the second time through here for the same parser.
            quietMode = false;
            interactiveMode = false;

            if (quietModeArgument != null && consoleArgs.HasArgument(quietModeArgument))
            {
                quietMode = true;
                interactiveMode = false;
                return;
            }

            if (interactiveModeArgument != null && consoleArgs.HasArgument(interactiveModeArgument))
            {
                interactiveMode = true;
            }

            if (ParseOptions.InvertInteractiveModeArgument)
            {
                interactiveMode = !interactiveMode;
            }

            ConsoleHelper.Initialize(ParseOptions, quietMode, interactiveMode);
        }

        private void OnArgumentTemplateChanged(ArgumentTemplateType type, ArgumentTemplate template)
        {
            var newArgument = template == null ? null : new Argument(template, null);

            switch (type)
            {
                case ArgumentTemplateType.Help:
                    helpArgument = newArgument;
                    break;
                case ArgumentTemplateType.QuietMode:
                    quietModeArgument = newArgument;
                    break;
                case ArgumentTemplateType.InteractiveMode:
                    interactiveModeArgument = newArgument;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        private void SetAllIncludedArguments()
        {
            OnArgumentTemplateChanged(ArgumentTemplateType.Help, ParseOptions.HelpArgumentTemplate);
            OnArgumentTemplateChanged(ArgumentTemplateType.QuietMode, ParseOptions.QuietModeArgumentTemplate);
            OnArgumentTemplateChanged(ArgumentTemplateType.InteractiveMode, ParseOptions.InteractiveModeArgumentTemplate);
        }
    }

    public class CommandParser<TCommand> : CommandParser
        where TCommand : class, ICommand
    {
        public CommandParser()
        {
        }

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

            if (consoleArgs.Length == 0)
            {
                if (ParseOptions.AllowNoMatchingCommands)
                {
                    return ParseResult.NoMatchFound;
                }

                ConsoleHelper.WriteLine("No arguments given.");
                WriteUsage(possibleCommands);
                return ParseResult.Failure;
            }

            var hasHelpArg = helpArgument != null && consoleArgs.HasArgument(helpArgument);

            // The help command needs some special treatment to keep this method as generic as it can be.
            if (ParseOptions.HelpCommandTemplate != null && consoleArgs[0].Equals(ParseOptions.HelpCommandTemplate.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                var helpCommand = new HelpCommand<TCommand>(possibleCommands, this);
                var helpParseResult = ParseArgumentsForCommand(helpCommand, consoleArgs, hasHelpArg);

                if (helpParseResult == ParseResult.Failure)
                {
                    return helpParseResult;
                }

                helpCommand.Execute();
                return ParseResult.DisplayedHelp;
            }

            var command = possibleCommands.SingleOrDefault(c => c.Name.Equals(consoleArgs[0], StringComparison.InvariantCultureIgnoreCase));

            if (command == null)
            {
                if (hasHelpArg)
                {
                    WriteUsage(possibleCommands);
                    return ParseResult.DisplayedHelp;
                }

                if (ParseOptions.AllowNoMatchingCommands)
                {
                    return ParseResult.NoMatchFound;
                }

                ConsoleHelper.WriteLine("Invalid command.");
                WriteUsage(possibleCommands);
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

                if (ParseOptions.WriteUsageMode == WriteUsageMode.CommandNameAndArguments)
                {
                    ConsoleHelper.WriteLine();
                }
            }

            RaiseDoneWritingSimpleCommandUsage();
        }

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

            var formatString = maxCommandNameLength.HasValue ? string.Format("{{0}}{{1,-{0}}} ", maxCommandNameLength.Value) : "{0}{1} ";
            ConsoleHelper.Write(formatString, spacer, template.Name);

            if (!isWritingMultipleCommands)
            {
                WriteUsage(command.Arguments, false, offsetOverride);
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

                        foreach (var argument in command.Arguments.OrderByDescending(a => a.IsRequired).ThenBy(a => a.Ordinal).ThenBy(a => a.Name))
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

    public class WriteUsageOverrides
    {
        public bool Skip { get; set; }

        public string NameOverride { get; set; }

        public string TitleOverride { get; set; }

        public string DescriptionOverride { get; set; }
    }
}
