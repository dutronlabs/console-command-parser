using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheObtuseAngle.ConsoleUtilities.Arguments;
using TheObtuseAngle.ConsoleUtilities.Commands;

namespace TheObtuseAngle.ConsoleUtilities
{
    /// <summary>
    /// The possible actions to take when the debug flag argument is given.
    /// </summary>
    public enum DebugFlagAction
    {
        /// <summary>
        /// Ignore the argument and do nothing.
        /// </summary>
        None,

        /// <summary>
        /// Launch the debugger using Debugger.Launch.  NOTE: This will not work in Express versions of Visual Studio.
        /// </summary>
        DebuggerLaunch,

        /// <summary>
        /// Pauses for 10 seconds to allow you to attach to the debugger.
        /// </summary>
        ThreadSleep
    }

    /// <summary>
    /// The possible ways to write command usage to the output stream.
    /// </summary>
    public enum WriteUsageMode
    {
        /// <summary>
        /// Outputs only the command name.
        /// </summary>
        CommandNameOnly,

        /// <summary>
        /// Outputs the command name followed by the title of the command.
        /// </summary>
        CommandNameAndTitle,

        /// <summary>
        /// Outputs the command name followed by all the possible arguments for that command.
        /// </summary>
        CommandNameAndArguments
    }

    internal enum ArgumentTemplateType
    {
        Help,
        QuietMode,
        InteractiveMode
    }

    /// <summary>
    /// An object that holds all the possible options that the command parser will use while parsing commands.
    /// </summary>
    public sealed class ParseOptions
    {
        public const char DefaultArgumentValueSeparator = ' ';
        public const string DefaultArgumentValueIndicator = "<value>";
        public const string DefaultRequiredArgumentIndicator = "(*) ";
        public const string DefaultRequiredArgumentFormat = "<{0}>";
        public const string DefaultOptionalArgumentFormat = "[{0}]";
        public const string DefaultDebugFlag = "--debug";
        public const int DefaultThreadSleepTime = 10;
        private ArgumentTemplate helpArgumentTemplate;
        private ArgumentTemplate quietModeArgumentTemplate;
        private ArgumentTemplate interactiveModeArgumentTemplate;

        public static readonly ArgumentTemplate DefaultQuietModeArgumentTemplate = new ArgumentTemplate
        {
            Name = "-quiet",
            Aliases = new[] { "-q", "/q" },
            Description = "Suppresses all output"
        };

        public static readonly ArgumentTemplate DefaultHelpArgumentTemplate = new ArgumentTemplate
        {
            Name = "-help",
            Aliases = new[] { "-?", "/?" },
            Description = "Displays this usage information"
        };

        public static readonly ArgumentTemplate DefaultInteractiveModeArgumentTemplate = new ArgumentTemplate
        {
            Name = "-interactive",
            Description = "When provided, the user will be prompted for missing required arguments"
        };

        public static readonly HelpCommandTemplate DefaultHelpCommandTemplate = new HelpCommandTemplate
        {
            Name = "Help",
            Title = "Displays usage information for the given command",
            Description = "Displays usage information for the given command",
            CommandArgumentTemplate = new ArgumentTemplate
            {
                Name = "-command",
                Aliases = new[] { "-c" },
                Description = "The name of the command to display help for",
                IsRequired = false,
                RequiresValue = true
            }
        };

        public static readonly Func<IEnumerable<IArgument>, IEnumerable<IArgument>> DefaultArgumentOrderByFunction =
            args => args.OrderByDescending(a => a.IsRequired).ThenBy(a => a.Name);
        
        public static readonly ParseOptions Defaults = new ParseOptions();
        
        public ParseOptions()
        {
            ArgumentValueSeparator = DefaultArgumentValueSeparator;
            AllowNoMatchingCommands = false;
            ThrowOnValueSetterException = true;
            ThrowOnParseAndExecuteException = true;
            EnableValueOnlyParsing = false;
            InvertInteractiveModeArgument = false;
            WriteBlankLineBetweenEachCommand = false;
            OutputWriter = Console.Out;
            ArgumentValueIndicator = DefaultArgumentValueIndicator;
            RequiredArgumentIndicator = DefaultRequiredArgumentIndicator;
            RequiredArgumentFormat = DefaultRequiredArgumentFormat;
            OptionalArgumentFormat = DefaultOptionalArgumentFormat;
            DebugFlagAction = DebugFlagAction.DebuggerLaunch;
            DebugFlag = DefaultDebugFlag;
            QuietModeArgumentTemplate = DefaultQuietModeArgumentTemplate;
            HelpArgumentTemplate = DefaultHelpArgumentTemplate;
            InteractiveModeArgumentTemplate = DefaultInteractiveModeArgumentTemplate;
            WriteUsageMode = WriteUsageMode.CommandNameAndArguments;
            HelpCommandTemplate = DefaultHelpCommandTemplate;
            ThreadSleepTime = DefaultThreadSleepTime;
            OutputConsoleColor = ConsoleColor.White;
            ErrorConsoleColor = ConsoleColor.Red;
            PromptConsoleColor = ConsoleColor.White;
            ArgumentOrderByFunction = DefaultArgumentOrderByFunction;
#if DEBUG
            WriteExceptionsToConsole = true;
            EnableDebugFlag = true;
#else
            WriteExceptionsToConsole = false;
            EnableDebugFlag = false;
#endif
        }

        /// <summary>
        /// Gets or sets the character to be used as a separator between argument names and values.  The default is space, ' '.  ex) .\Process.exe -argument value
        /// </summary>
        public char ArgumentValueSeparator { get; set; }

        /// <summary>
        /// When true, the ParseCommand method will return "NoMatchFound" when no matching command is found.  When false, the usage text will be written to the output stream.  The default is false.
        /// </summary>
        public bool AllowNoMatchingCommands { get; set; }

        /// <summary>
        /// When true, the parser will allow any exceptions thrown inside of argument value setters to bubble up.  When false, any such exceptions will be caught.  The default is true.
        /// </summary>
        public bool ThrowOnValueSetterException { get; set; }

        /// <summary>
        /// When true, the parser will allow any exceptions thrown inside of a command's Execute method to bubble up.  When false, any such exceptions will be caught.  The default is true.
        /// </summary>
        public bool ThrowOnParseAndExecuteException { get; set; }

        /// <summary>
        /// When true, any caught exception will be written to the console.  This is true by default in a debug build, false by default in a release build.
        /// </summary>
        public bool WriteExceptionsToConsole { get; set; }

        /// <summary>
        /// When true, arguments will be parsed by ordinal.  That is, no argument names should be given.  The default is false.  ex) .\Process.exe arg1 arg2 arg3
        /// </summary>
        public bool EnableValueOnlyParsing { get; set; }

        /// <summary>
        /// When true, the interactive mode argument template is not null, and the interactive mode argument is supplied then its value will be inversed.  In other words, the presence of this argument changes to be non-interactive.  The default is false.
        /// </summary>
        public bool InvertInteractiveModeArgument { get; set; }

        /// <summary>
        /// Gets or sets the text writer to use for output.  The default is Console.Out.
        /// </summary>
        public TextWriter OutputWriter { get; set; }

        /// <summary>
        /// Gets or sets the string to use to designate an argument value when writing on-screen help.  The default is "&lt;value&gt;".
        /// </summary>
        public string ArgumentValueIndicator { get; set; }

        /// <summary>
        /// Gets or sets the string to use to show that an argument is required.  The default is "(*)".
        /// </summary>
        public string RequiredArgumentIndicator { get; set; }

        /// <summary>
        /// Gets or sets the format string to use when displaying required arguments.  The default is "&lt;{0}&gt;".
        /// </summary>
        public string RequiredArgumentFormat { get; set; }

        /// <summary>
        /// Gets or sets the format string to use when displaying optional arguments.  The default is "[{0}]".
        /// </summary>
        public string OptionalArgumentFormat { get; set; }

        /// <summary>
        /// When true, the DebugFlagAction will be taken when the debug flag is specified on the command line.  This is true by default in a debug build, false by default in a release build.
        /// </summary>
        public bool EnableDebugFlag { get; set; }

        /// <summary>
        /// Gets or sets the DebugFlagAction to take when the debug flag is specified on the command line.  The default is "DebuggerLaunch".
        /// </summary>
        public DebugFlagAction DebugFlagAction { get; set; }

        /// <summary>
        /// Gets or sets the debug flag.  The default is "--debug".
        /// </summary>
        public string DebugFlag { get; set; }

        /// <summary>
        /// The amount of time, in seconds, to pause when the debug flag action is "ThreadSleep" and the debug argument is given.  The default is 10.
        /// </summary>
        public int ThreadSleepTime { get; set; }

        /// <summary>
        /// Gets or sets the console color to use when writing output.  This is ignored when the output writer is something other than Console.Out.  The default is White.
        /// </summary>
        public ConsoleColor OutputConsoleColor { get; set; }

        /// <summary>
        /// Gets or sets the console color to use when writing error messages.  This is ignored when the output writer is something other than Console.Out.  The default is Red.
        /// </summary>
        public ConsoleColor ErrorConsoleColor { get; set; }

        /// <summary>
        /// Gets or sets the console color to use when prompting the user for input in interactive mode.  This is ignored when the output writer is something other than Console.Out.  The default is White.
        /// </summary>
        public ConsoleColor PromptConsoleColor { get; set; }

        /// <summary>
        /// Gets or sets the argument template that, when provided, will be used to construct the usage help argument.  Null is an allowed value.  The default is the "-help" argument.
        /// </summary>
        public ArgumentTemplate HelpArgumentTemplate
        {
            get { return helpArgumentTemplate; }
            set
            {
                helpArgumentTemplate = value;
                RaiseArgumentTemplateChangedEvent(ArgumentTemplateType.Help, value);
            }
        }

        /// <summary>
        /// Gets or sets the argument template that, when provided, will be used to construct the quiet mode argument.  Null is an allowed value.  The default is the "-quiet" argument.
        /// </summary>
        public ArgumentTemplate QuietModeArgumentTemplate
        {
            get { return quietModeArgumentTemplate; }
            set
            {
                quietModeArgumentTemplate = value;
                RaiseArgumentTemplateChangedEvent(ArgumentTemplateType.QuietMode, value);
            }
        }

        /// <summary>
        /// Gets or sets the argument template that, when provided, will be used to construct the interactive mode argument.  Null is an allowed value.  The default is the "-interactive" argument.
        /// </summary>
        public ArgumentTemplate InteractiveModeArgumentTemplate
        {
            get { return interactiveModeArgumentTemplate; }
            set
            {
                interactiveModeArgumentTemplate = value;
                RaiseArgumentTemplateChangedEvent(ArgumentTemplateType.InteractiveMode, value);
            }
        }

        /// <summary>
        /// Gets or sets the WriteUsageMode to use when writing command usage information.  The default is "CommandNameAndArguments".
        /// </summary>
        public WriteUsageMode WriteUsageMode { get; set; }

        /// <summary>
        /// When true, a blank line will be written to the console between each command when writing the usage for all commands.  The default is false;
        /// </summary>
        public bool WriteBlankLineBetweenEachCommand { get; set; }

        /// <summary>
        /// Gets or sets the command template that, when provided, will be used to construct the help command.  Null is an allowed value.  The default is the "Help -command" command."
        /// </summary>
        public HelpCommandTemplate HelpCommandTemplate { get; set; }

        /// <summary>
        /// Gets or sets the function to be invoked when ordering arguments for display.  Null is an allowed value.  The default is: args => args.OrderByDescending(a => a.IsRequired).ThenBy(a => a.Name);
        /// </summary>
        public Func<IEnumerable<IArgument>, IEnumerable<IArgument>> ArgumentOrderByFunction { get; set; }

        /// <summary>
        /// Gets a value indicating whether or not the parser is using Console.Out as the output stream.
        /// </summary>
        internal bool IsUsingConsoleOutput
        {
            get { return object.ReferenceEquals(OutputWriter, Console.Out); }
        }

        internal event Action<ArgumentTemplateType, ArgumentTemplate> ArgumentTemplateChanged;

        private void RaiseArgumentTemplateChangedEvent(ArgumentTemplateType type, ArgumentTemplate newTemplate)
        {
            var handler = ArgumentTemplateChanged;

            if (handler != null)
            {
                handler(type, newTemplate);
            }
        }
    }
}
