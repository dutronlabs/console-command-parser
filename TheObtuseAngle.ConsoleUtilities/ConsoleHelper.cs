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

        public static void WriteTable(IEnumerable<IEnumerable<string>> dataSource)
        {
            WriteTable(Output, null, dataSource, o => o);
        }

        public static void WriteTable(IEnumerable<ColumnDefinition> columnDefinitions, IEnumerable<IEnumerable<string>> dataSource)
        {
            WriteTable(Output, columnDefinitions, dataSource, o => o);
        }

        public static void WriteTable<T>(IEnumerable<T> dataSource, Func<T, IEnumerable<string>> rowProducer)
        {
            WriteTable(Output, null, dataSource, rowProducer);
        }

        public static void WriteTable<T>(IEnumerable<ColumnDefinition> columnDefinitions, IEnumerable<T> dataSource, Func<T, IEnumerable<string>> rowProducer)
        {
            WriteTable(Output, columnDefinitions, dataSource, rowProducer);
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

        public static void WriteTable<T>(TextWriter outputOverride, IEnumerable<T> dataSource, Func<T, IEnumerable<object>> rowProducer)
        {
            WriteTable(outputOverride, null, dataSource, rowProducer);
        }

        public static void WriteTable<T>(TextWriter outputOverride, IEnumerable<ColumnDefinition> columnDefinitions, IEnumerable<T> dataSource, Func<T, IEnumerable<object>> rowProducer)
        {
            if (dataSource == null)
            {
                throw new ArgumentNullException("dataSource");
            }
            if (rowProducer == null)
            {
                throw new ArgumentNullException("rowProducer");
            }
            if (outputOverride == null)
            {
                outputOverride = output;
            }

            var columnList = (columnDefinitions ?? Enumerable.Empty<ColumnDefinition>()).ToList();
            var data = dataSource.ToList();
            var columnWidths = new List<int>();
            var maxTotalWidth = ReferenceEquals(outputOverride, Console.Out) ? Console.BufferWidth : int.MaxValue;
            var rows = new List<string[]>();
            var columnCount = columnList.Count;

            // If we have a header row, add that as the first row in the table.
            if (columnCount > 0)
            {
                rows.Add(columnList.Select(c => c.Header).ToArray());
            }

            // Now get all the table data from the data source.
            foreach (var item in data)
            {
                var itemRows = rowProducer(item).Select(o => o.ToString()).ToArray();
                rows.Add(itemRows);

                if (itemRows.Length > columnCount)
                {
                    columnCount = itemRows.Length;
                }
            }

            // Now figure out what the column widths are.
            // Initial column widths are longest item in that column.
            for (int i = 0; i < columnCount; i++)
            {
                columnWidths.Add(rows.Select(r => r.Length > i ? r[i] : string.Empty).Max(s => s.Length));
            }

            // Calculate the actual column width if the total width is greater than the max possible width.
            var actualTotalWidth = columnWidths.Sum();
            maxTotalWidth = (maxTotalWidth - columnCount) + 1;

            if (actualTotalWidth > maxTotalWidth)
            {
                int remainingWidth = maxTotalWidth;
                var dynamicColumnIndicies = new List<int>();

                for (int i = 0; i < columnCount; i++)
                {
                    var definition = columnList.ElementAtOrDefault(i);
                    var mode = definition == null ? ColumnWidthMode.Auto : definition.WidthMode;

                    if (mode == ColumnWidthMode.Fixed && definition.Width <= 0)
                    {
                        mode = ColumnWidthMode.Auto;
                    }

                    switch (mode)
                    {
                        case ColumnWidthMode.Auto:
                            columnWidths[i] = (columnWidths[i] * maxTotalWidth) / actualTotalWidth;
                            remainingWidth -= columnWidths[i];
                            break;
                        case ColumnWidthMode.Dynamic:
                            dynamicColumnIndicies.Add(i);
                            break;
                        case ColumnWidthMode.Max:
                            remainingWidth -= columnWidths[i];
                            break;
                        case ColumnWidthMode.Fixed:
                            columnWidths[i] = definition.Width;
                            remainingWidth -= definition.Width;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (dynamicColumnIndicies.Any())
                {
                    var dynamicWidth = remainingWidth / dynamicColumnIndicies.Count;
                    dynamicColumnIndicies.ForEach(i => columnWidths[i] = dynamicWidth);
                }
            }

            // Build the format string to use for each row.
            var rowFormatParts = new List<string>();

            for (int i = 0; i < columnCount; i++)
            {
                rowFormatParts.Add(string.Format("{{{0},-{1}}}", i, columnWidths[i]));
            }

            var rowFormat = string.Join(" ", rowFormatParts);

            // Write the table
            foreach (var row in rows)
            {
                while (row.Any(s => !string.IsNullOrWhiteSpace(s)))
                {
                    var formatParams = new string[columnCount];

                    for (int i = 0; i < columnCount; i++)
                    {
                        var column = row[i];
                        var width = columnWidths[i];

                        if (column.Length > width)
                        {
                            int length = 0;
                            var words = column.Split(' ');

                            if (words.Any(w => w.Length > width))
                            {
                                // If there are any words that are longer than the total width of the column then we will need
                                // to wrap in the middle of a word no matter what.  Rather than deal with the complexities introduced
                                // by that scenario just take the quick approach here and don't try to wrap on word boundaries.
                                row[i] = column.Substring(width);
                                formatParams[i] = column.Substring(0, width);
                            }
                            else
                            {
                                // The text in the column is too wide, but all the individual words can fit.
                                // This means we can be nice and wrap on word boundaries.
                                formatParams[i] = string.Join(" ", words.TakeWhile(
                                    word =>
                                    {
                                        length += word.Length + 1; // add in the space that will be added back in after every word
                                        return length <= width;
                                    }));
                                row[i] = column.Substring(formatParams[i].Length).TrimStart();
                            }
                        }
                        else
                        {
                            row[i] = string.Empty;
                            formatParams[i] = column;
                        }
                    }

                    var line = string.Format(rowFormat, formatParams);
                    Write(line);
                }
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
