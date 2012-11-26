using System;
using System.IO;
using System.Linq;

namespace TheObtuseAngle.ConsoleUtilities
{
    public static class ConsoleHelperExtensions
    {
        public static bool ToBool(this string value)
        {
            return ConsoleHelper.ParseBoolArg(value);
        }

        public static void WriteToConsole(this Exception e)
        {
            ConsoleHelper.WriteException(e);
        }

        public static bool HasArgument(this string[] consoleArgs, IArgument arg)
        {
            return consoleArgs != null && arg != null && consoleArgs.Any(a => a.Equals(arg.Name, StringComparison.InvariantCultureIgnoreCase) || arg.Aliases.Any(alias => alias.Equals(a, StringComparison.InvariantCultureIgnoreCase)));
        }

        public static bool HasArgument(this string[] args, string arg)
        {
            return args == null || args.Any(a => a.Equals(arg, StringComparison.InvariantCultureIgnoreCase));
        }

        public static void WriteWrapped(this TextWriter output, string textToWrap, int? offsetOverride = null)
        {
            ConsoleHelper.WriteWrapped(output, textToWrap, offsetOverride);
        }

        public static ArgumentParseResult ParseArguments(this string[] consoleArgs, params IArgument[] possibleArguments)
        {
            return ConsoleHelper.ParseArguments(consoleArgs, ParseOptions.Defaults, possibleArguments);
        }

        public static ArgumentParseResult ParseArguments(this string[] consoleArgs, ParseOptions parseOptions, params IArgument[] possibleArguments)
        {
            return ConsoleHelper.ParseArguments(consoleArgs, parseOptions, possibleArguments);
        }
    }
}
