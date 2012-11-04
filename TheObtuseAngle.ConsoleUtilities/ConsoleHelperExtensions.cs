using System;
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
            return consoleArgs != null && consoleArgs.Any(a => a.Equals(arg.Name, StringComparison.InvariantCultureIgnoreCase) || a.Equals(arg.Alias, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool HasArgument(this string[] args, string arg)
        {
            return args == null || args.Any(a => a.Equals(arg, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
