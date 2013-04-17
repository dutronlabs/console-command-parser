using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheObtuseAngle.ConsoleUtilities.Arguments;

namespace TheObtuseAngle.ConsoleUtilities
{
    public static class ConsoleHelper
    {
        [ThreadStatic]
        private static TextWriter output;

        [ThreadStatic]
        private static ParseOptions parseOptions;

        [ThreadStatic]
        private static bool quietMode;

        [ThreadStatic]
        private static bool interactiveMode;

        private static TextWriter Output
        {
            get
            {
                if (output == null)
                {
                    Initialize(ParseOptions.Defaults);
                }
                return output;
            }
        }

        private static ParseOptions Options
        {
            get
            {
                if (parseOptions == null)
                {
                    Initialize(ParseOptions.Defaults);
                }
                return parseOptions;
            }
        }

        internal static void Initialize(ParseOptions options, bool isQuietMode = false, bool isInteractiveMode = false)
        {
            output = options.OutputWriter;
            parseOptions = options;
            quietMode = isQuietMode;
            interactiveMode = isInteractiveMode;
        }

        public static void Initialize(TextWriter outputWriter)
        {
            output = outputWriter;
        }

        public static string GetFullDepthExceptionString(Exception e)
        {
            var builder = new StringBuilder();

            while (e != null)
            {
                builder.AppendLine(e.Message);
                builder.AppendLine(e.StackTrace);
                builder.AppendLine();
                e = e.InnerException;
            }

            return builder.ToString();
        }

        public static bool ParseBoolArg(string value)
        {
            string val = value.ToLowerInvariant();
            return val == "y" || val == "1" || val == "yes";
        }

        public static void WriteException(Exception exception)
        {
            WriteException(exception, false);
        }

        internal static void WriteException(Exception exception, bool forceWrite)
        {
            if (!forceWrite && !Options.WriteExceptionsToConsole)
            {
                return;
            }

            WriteError(GetFullDepthExceptionString(exception));
        }

        public static void WriteError(string format, params object[] args)
        {
            Write(format, Options.ErrorConsoleColor, true, args);
        }

        public static void Write(string format, params object[] args)
        {
            Write(format, Options.OutputConsoleColor, false, args);
        }

        public static void WriteLine()
        {
            WriteLine(Output);
        }

        private static void WriteLine(TextWriter outputOverride)
        {
            if (!quietMode)
            {
                outputOverride.WriteLine();
            }
        }

        public static void WriteLine(string format, params object[] args)
        {
            Write(format, Options.OutputConsoleColor, true, args);
        }

        public static void WriteWrapped(string textToWrap, int? offsetOverride = null)
        {
            WriteWrapped(Output, textToWrap, offsetOverride);
        }

        public static void WriteTable<T>(IEnumerable<T> items, Func<T, IEnumerable<string>> rowProducer)
        {
            WriteTable(Output, items, rowProducer);
        }

        private static void Write(string format, ConsoleColor color, bool newLine, params object[] args)
        {
            Write(Output, format, color, newLine, args);
        }

        private static void Write(TextWriter outputOverride, string format, ConsoleColor color, bool newLine, params object[] args)
        {
            if (quietMode)
            {
                return;
            }

            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;

            if (newLine)
            {
                outputOverride.WriteLine(format, args);
            }
            else
            {
                outputOverride.Write(format, args);
            }

            Console.ForegroundColor = originalColor;
        }

        public static void WriteWrapped(TextWriter outputOverride, string textToWrap, int? offsetOverride = null)
        {
            if (!object.ReferenceEquals(outputOverride, Console.Out) || Console.CursorLeft + textToWrap.Length < Console.BufferWidth)
            {
                Write(outputOverride, textToWrap, Options.OutputConsoleColor, false);
            }
            else
            {
                int position = offsetOverride.HasValue ? offsetOverride.Value : Console.CursorLeft;
                int lineLength = Console.BufferWidth - Console.CursorLeft - 1; // The extra -1 is to prevent an extra line if the word ends exactly where the buffer ends.
                int subsequentLineLength = Console.BufferWidth - position - 1;
                var lineBuilder = new StringBuilder();

                foreach (var word in textToWrap.Split(' '))
                {
                    if (lineBuilder.Length + word.Length + 1 > lineLength)
                    {
                        Write(outputOverride, lineBuilder.ToString(), Options.OutputConsoleColor, false);
                        WriteLine(outputOverride);
                        Write(outputOverride, string.Empty.PadLeft(position), Options.OutputConsoleColor, false);
                        lineLength = subsequentLineLength;
                        lineBuilder.Clear();
                    }

                    lineBuilder.Append(word);
                    lineBuilder.Append(' ');
                }

                if (lineBuilder.Length > 0)
                {
                    Write(outputOverride, lineBuilder.ToString(), Options.OutputConsoleColor, false);
                }
            }
        }

        public static void WriteTable<T>(TextWriter outputOverride, IEnumerable<T> items, Func<T, IEnumerable<string>> rowProducer)
        {
            // I don't think there's any way around multiple iterations here since I first need to figure out the number of columns
            // and the maximum length for each column.  So, the best I can do is force the enumeration once in case it is expensive.
            var itemList = items.ToList();
            var rows = new List<string[]>();
            var formatBuilder = new StringBuilder();
            int columnCount = 0;

            foreach (var item in itemList)
            {
                var itemRows = rowProducer(item).ToArray();
                rows.Add(itemRows);

                if (itemRows.Length > columnCount)
                {
                    columnCount = itemRows.Length;
                }
            }

            for (int i = 0; i < columnCount; i++)
            {
                var maxLength = rows.Select(r => r.Length > i ? r[i] : string.Empty).Max(s => s.Length);
                formatBuilder.AppendFormat("{{{0},-{1}}} ", i, maxLength);
            }

            string format = formatBuilder.ToString();

            foreach (var row in rows)
            {
                var columnValues = row.ToList();

                while (columnValues.Count < columnCount)
                {
                    columnValues.Add(string.Empty);
                }

                outputOverride.WriteLine(format, columnValues.ToArray());
            }
        }

        public static ArgumentParseResult ParseArguments(string[] consoleArgs, params IArgument[] possibleArguments)
        {
            var parsedArgs = new List<IArgument>();

            if (possibleArguments.HasDuplicates(true, a => a.Name) || possibleArguments.Where(a => a.Aliases != null && a.Aliases.Length > 0).SelectMany(a => a.Aliases).HasDuplicates(true))
            {
                throw new ArgumentException("Duplicate arguments detected.", "possibleArguments");
            }

            for (int i = 0; i < consoleArgs.Length; i++)
            {
                string argValue = null;
                string[] argParts = consoleArgs[i].Split(Options.ArgumentValueSeparator);
                string argName = Options.ArgumentValueSeparator == ' ' ? consoleArgs[i] : argParts[0];
                int incrementCount = 0;

                var matchingArg = possibleArguments.FirstOrDefault(a =>
                    a.Name.Equals(argName, StringComparison.InvariantCultureIgnoreCase) ||
                    (a.Aliases != null && a.Aliases.Any(alias => alias != null && alias.Equals(argName, StringComparison.InvariantCultureIgnoreCase))));

                if (matchingArg == null)
                {
                    continue;
                }

                if (matchingArg.RequiresValue)
                {
                    if (Options.ArgumentValueSeparator == ' ')
                    {
                        incrementCount = matchingArg.NumberOfValueArgs;
                        argValue = i + incrementCount < consoleArgs.Length ? string.Join(" ", consoleArgs.Skip(i + 1).Take(incrementCount)) : null;
                    }
                    else
                    {
                        argValue = argParts.Length > 1 ? argParts[1] : null;
                    }

                    if (string.IsNullOrWhiteSpace(argValue) && interactiveMode && Options.IsUsingConsoleOutput)
                    {
                        argValue = PromptForArgumentValue(matchingArg);
                    }

                    if (string.IsNullOrWhiteSpace(argValue))
                    {
                        return new ArgumentParseResult(matchingArg);
                    }
                }

                var setterResult = SetArgumentValue(matchingArg, argValue);
                
                if (setterResult != null)
                {
                    return setterResult;
                }
                
                parsedArgs.Add(matchingArg);
                i += incrementCount;
            }

            if (interactiveMode && Options.IsUsingConsoleOutput && (Options.HelpArgumentTemplate == null || !consoleArgs.HasArgument(Options.HelpArgumentTemplate)))
            {
                foreach (var arg in possibleArguments.Where(a => a.IsRequired && a.RequiresValue && !parsedArgs.Contains(a)).ToList())
                {
                    var value = PromptForArgumentValue(arg);

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return new ArgumentParseResult(arg);
                    }

                    var setterResult = SetArgumentValue(arg, value);

                    if (setterResult != null)
                    {
                        return setterResult;
                    }

                    parsedArgs.Add(arg);
                }
            }

            var missingRequiredArgs = possibleArguments.Where(a => a.IsRequired && !parsedArgs.Contains(a));

            if (missingRequiredArgs.Any())
            {
                return new ArgumentParseResult(missingRequiredArgs);
            }

            return ArgumentParseResult.Success;
        }

        private static string PromptForArgumentValue(IArgument argument)
        {
            WriteLine("Please provide a value for the argument {0} - {1}", argument.Name, argument.Description);
            Write("{0}: ", argument.Name);
            var value = Console.ReadLine();
            WriteLine();
            return value;
        }

        private static ArgumentParseResult SetArgumentValue(IArgument argument, string value)
        {
            if (argument.ValueSetter == null)
            {
                return null;
            }
            
            if (Options.ThrowOnValueSetterException)
            {
                argument.ValueSetter(value);
            }
            else
            {
                try
                {
                    argument.ValueSetter(value);
                }
                catch (Exception valueSetterException)
                {
                    return new ArgumentParseResult(new Tuple<IArgument, Exception>(argument, valueSetterException));
                }
            }

            return null;
        }
    }
}
