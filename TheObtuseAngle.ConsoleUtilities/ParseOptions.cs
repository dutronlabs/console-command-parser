using System;
using System.IO;

namespace TheObtuseAngle.ConsoleUtilities
{
    public enum DebugFlagAction
    {
        DebuggerLaunch,
        ThreadSleep
    }

    public sealed class ParseOptions
    {
        public static readonly ParseOptions Defaults = new ParseOptions();
        private const char defaultArgumentValueSeparator = ' ';
        private const string defaultArgumentValueIndicator = "<value>";
        private const string defaultRequiredArgumentIndicator = "(*) ";
        private const string defaultRequiredArgumentFormat = "<{0}>";
        private const string defaultOptionalArgumentFormat = "[{0}]";
        private const string defaultDebugFlag = "--debug";

        public ParseOptions()
        {
            ArgumentValueSeparator = defaultArgumentValueSeparator;
            ThrowOnValueSetterException = true;
            ThrowOnParseAndExecuteException = true;
            EnableValueOnlyParsing = false;
            OutputWriter = Console.Out;
            ArgumentValueIndicator = defaultArgumentValueIndicator;
            RequiredArgumentIndicator = defaultRequiredArgumentIndicator;
            RequiredArgumentFormat = defaultRequiredArgumentFormat;
            OptionalArgumentFormat = defaultOptionalArgumentFormat;
            DebugFlagAction = TheObtuseAngle.ConsoleUtilities.DebugFlagAction.DebuggerLaunch;
            DebugFlag = defaultDebugFlag;
#if DEBUG
            WriteExceptionsToConsole = true;
            EnableDebugFlag = true;
#else
            WriteExceptionsToConsole = false;
            EnableDebugFlag = false;
#endif
        }

        public char ArgumentValueSeparator { get; set; }

        public bool ThrowOnValueSetterException { get; set; }

        public bool ThrowOnParseAndExecuteException { get; set; }

        public bool WriteExceptionsToConsole { get; set; }

        public bool EnableValueOnlyParsing { get; set; }

        public TextWriter OutputWriter { get; set; }

        public string ArgumentValueIndicator { get; set; }

        public string RequiredArgumentIndicator { get; set; }

        public string RequiredArgumentFormat { get; set; }

        public string OptionalArgumentFormat { get; set; }

        public bool EnableDebugFlag { get; set; }

        public DebugFlagAction DebugFlagAction { get; set; }

        public string DebugFlag { get; set; }

        internal bool IsUsingConsoleOutput
        {
            get { return object.ReferenceEquals(OutputWriter, Console.Out); }
        }
    }
}
