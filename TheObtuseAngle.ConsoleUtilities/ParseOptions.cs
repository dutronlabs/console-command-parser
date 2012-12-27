using System;
using System.IO;

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

    /// <summary>
    /// An object that holds all the possible options that the command parser will use while parsing commands.
    /// </summary>
    public sealed class ParseOptions
    {
        private static readonly IArgument quietModeArgument = new Argument("-quiet", new[] { "-q", "/q" }, "Suppresses all output", false, false, null);
        private static readonly IArgument displayHelpArgument = new Argument("-help", new[] { "-?", "/?" }, "Displays this usage information", false, false, null);
        private const char defaultArgumentValueSeparator = ' ';
        private const string defaultArgumentValueIndicator = "<value>";
        private const string defaultRequiredArgumentIndicator = "(*) ";
        private const string defaultRequiredArgumentFormat = "<{0}>";
        private const string defaultOptionalArgumentFormat = "[{0}]";
        private const string defaultDebugFlag = "--debug";

        public static readonly HelpCommandTemplate DefaultHelpCommandTemplate = new HelpCommandTemplate
        {
            Name = "Help",
            Title = "Displays usage information for the given command",
            Description = "Displays usage information for the given command",
            CommandArgumentTemplate = new ArgumentTemplate
            {
                Name = "-command",
                Aliases = new[] { "-c" },
                Description = "The name of the command to display help for"
            }
        };
        
        public static readonly ParseOptions Defaults = new ParseOptions();
        
        public ParseOptions()
        {
            ArgumentValueSeparator = defaultArgumentValueSeparator;
            AllowNoMatchingCommands = false;
            ThrowOnValueSetterException = true;
            ThrowOnParseAndExecuteException = true;
            EnableValueOnlyParsing = false;
            OutputWriter = Console.Out;
            ArgumentValueIndicator = defaultArgumentValueIndicator;
            RequiredArgumentIndicator = defaultRequiredArgumentIndicator;
            RequiredArgumentFormat = defaultRequiredArgumentFormat;
            OptionalArgumentFormat = defaultOptionalArgumentFormat;
            DebugFlagAction = DebugFlagAction.DebuggerLaunch;
            DebugFlag = defaultDebugFlag;
            QuietModeArgument = quietModeArgument;
            HelpArgument = displayHelpArgument;
            WriteUsageMode = WriteUsageMode.CommandNameAndArguments;
            IncludeHelpCommand = true;
            HelpCommandTemplate = DefaultHelpCommandTemplate;
#if DEBUG
            WriteExceptionsToConsole = true;
            EnableDebugFlag = true;
#else
            WriteExceptionsToConsole = false;
            EnableDebugFlag = false;
#endif
        }

        private HelpCommandTemplate helpCommandTemplate;

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
        /// Gets or sets the argument that when provided will display usage help.  Null is an allowed value.  The default is the "-help" argument.
        /// </summary>
        public IArgument HelpArgument { get; set; }

        /// <summary>
        /// Gets or sets the argument that when provided will enable quiet mode.  Null is an allowed value.  The default is the "-quiet" argument.
        /// </summary>
        public IArgument QuietModeArgument { get; set; }

        /// <summary>
        /// Gets or sets the WriteUsageMode to use when writing command usage information.  The default is "CommandNameAndArguments".
        /// </summary>
        public WriteUsageMode WriteUsageMode { get; set; }

        /// <summary>
        /// When true, the built in help command will be added to the list of possible commands when parsing.  When false it will be omitted.  The default is true.
        /// </summary>
        public bool IncludeHelpCommand { get; set; }

        /// <summary>
        /// Gets or sets the template to use for the help command.  A command template defines name, title, and description of a command without specifying any execution logic.
        /// </summary>
        public HelpCommandTemplate HelpCommandTemplate
        {
            get { return helpCommandTemplate ?? (helpCommandTemplate = DefaultHelpCommandTemplate); }
            set { helpCommandTemplate = value; }
        }

        /// <summary>
        /// Gets a value indicating whether or not the parser is using Console.Out as the output stream.
        /// </summary>
        internal bool IsUsingConsoleOutput
        {
            get { return object.ReferenceEquals(OutputWriter, Console.Out); }
        }
    }
}
