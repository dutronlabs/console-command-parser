﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using TheObtuseAngle.ConsoleUtilities.Arguments;

namespace TheObtuseAngle.ConsoleUtilities
{
    /// <summary>
    /// Provides methods used to parse raw console arguments into given collections of <see cref="IArgument"/> instances.
    /// </summary>
    public class ArgumentParser
    {
        protected IArgument quietModeArgument;
        protected IArgument helpArgument;
        protected IArgument interactiveModeArgument;
        protected bool quietMode;
        protected bool interactiveMode;

        /// <summary>
        /// Constructs a new <see cref="ArgumentParser"/> instance using the default <see cref="ParseOptions"/>.
        /// </summary>
        public ArgumentParser()
            : this(ParseOptions.Defaults)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ArgumentParser"/> instance using the given <see cref="ParseOptions"/>.
        /// </summary>
        /// <param name="parseOptions">The <see cref="ConsoleUtilities.ParseOptions"/> instance to use.</param>
        public ArgumentParser(ParseOptions parseOptions)
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

        /// <summary>
        /// Raised for each argument when the parser is writing the usage help for all possible arguments.
        /// This is an opportunity to override the arguments's properties or skip it entirely.
        /// </summary>
        public event Func<IArgument, ArgumentWriteUsageOverride> WritingArgumentUsage;

        /// <summary>
        /// Gets or sets the <see cref="ConsoleUtilities.ParseOptions"/>
        /// </summary>
        public ParseOptions ParseOptions { get; set; }

        /// <summary>
        /// Gets a flag indicating whether or not the parser should prompt for missing required arguments.
        /// </summary>
        public bool InteractiveMode
        {
            get { return interactiveMode; }
        }

        /// <summary>
        /// Parses the given raw console arguments into the collection of possible arguments.
        /// </summary>
        /// <param name="consoleArgs">The raw console arguments.</param>
        /// <param name="possibleArguments">The collection of <see cref="IArgument"/> instances to parse the raw arguments into.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Writes the collection of arguments to the output stream.
        /// </summary>
        /// <param name="arguments">The collection of <see cref="IArgument"/> objects to write.</param>
        public void WriteUsage(IEnumerable<IArgument> arguments)
        {
            WriteUsage(arguments, true, true, null);
        }

        protected virtual void WriteUsage(IEnumerable<IArgument> arguments, bool writePrefix, bool applyOrdering, int? offsetOverride)
        {
            if (quietMode)
            {
                return;
            }

            var argList = ParseOptions.ArgumentOrderByFunction == null || !applyOrdering ? arguments.ToList() : ParseOptions.ArgumentOrderByFunction(arguments).ToList();

            if (interactiveModeArgument != null && !argList.Contains(interactiveModeArgument))
            {
                argList.Add(interactiveModeArgument);
            }
            if (quietModeArgument != null && !argList.Contains(quietModeArgument))
            {
                argList.Add(quietModeArgument);
            }
            if (helpArgument != null && !argList.Contains(helpArgument))
            {
                argList.Add(helpArgument);
            }

            var argumentsToRemove = new List<IArgument>();

            foreach (var arg in argList)
            {
                var overrides = RaiseWritingArgumentUsage(arg);
                
                if (overrides == null)
                {
                    continue;
                }
                
                if (overrides.Skip)
                {
                    argumentsToRemove.Add(arg);
                }
                else if (!string.IsNullOrWhiteSpace(overrides.DescriptionOverride))
                {
                    arg.Description = overrides.DescriptionOverride;
                }
            }

            argList.RemoveRange(argumentsToRemove);

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

        private ArgumentWriteUsageOverride RaiseWritingArgumentUsage(IArgument argument)
        {
            var writingArgumentUsage = WritingArgumentUsage;
            return writingArgumentUsage != null ? writingArgumentUsage(argument) : null;
        }
    }
}
