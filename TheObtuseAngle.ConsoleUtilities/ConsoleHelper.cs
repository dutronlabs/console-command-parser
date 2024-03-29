﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheObtuseAngle.ConsoleUtilities.Arguments;

namespace TheObtuseAngle.ConsoleUtilities
{
    /// <summary>
    /// Contains a collection of methods that help with building console applications.
    /// </summary>
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

        /// <summary>
        /// Configures the console helper to use the given output writer.
        /// </summary>
        /// <param name="outputWriter">The output writer to use.</param>
        public static void Initialize(TextWriter outputWriter)
        {
            output = outputWriter;
        }

        /// <summary>
        /// Builds a string representation of the given exception, including all inner exceptions.
        /// </summary>
        /// <param name="e">The exception to convert to a string.</param>
        /// <returns>The string representation of the exception.</returns>
        public static string GetFullDepthExceptionString(Exception e)
        {
            var builder = new StringBuilder();

            while (e != null)
            {
                builder.AppendLine(e.GetType().FullName);
                builder.AppendLine(e.Message);
                builder.AppendLine(e.StackTrace);
                builder.AppendLine();
                e = e.InnerException;
            }

            return builder.ToString();
        }

        /// <summary>
        /// Parses the given string and converts it to a boolean representation.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The boolean representation of the string.</returns>
        public static bool ParseBoolArg(string value)
        {
            string val = value.ToLowerInvariant();
            return val == "y" || val == "1" || val == "yes";
        }

        /// <summary>
        /// Writes the given exception to the console, including all inner exceptions.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
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

        /// <summary>
        /// Writes the given string format to the console using the configures console error color.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The format arguments.</param>
        public static void WriteError(string format, params object[] args)
        {
            Write(format, Options.ErrorConsoleColor, true, args);
        }

        /// <summary>
        /// Writes the given string format to the configured output stream using the configured console error color.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The format arguments.</param>
        public static void Write(string format, params object[] args)
        {
            Write(format, Options.OutputConsoleColor, false, args);
        }

        /// <summary>
        /// Writes an empty line to the configured output stream.
        /// </summary>
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

        /// <summary>
        /// Writes the given string format to the configured output stream, followed by a new line, using the configured console error color.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The format arguments.</param>
        public static void WriteLine(string format, params object[] args)
        {
            Write(format, Options.OutputConsoleColor, true, args);
        }

        /// <summary>
        /// Writes the given string to the configured output stream wrapping the text at word boundaries.
        /// </summary>
        /// <param name="textToWrap">The text to write.</param>
        /// <param name="offsetOverride">A number to use as the left offset for new lines. By default the current horizontal position on the console.</param>
        public static void WriteWrapped(string textToWrap, int? offsetOverride = null)
        {
            WriteWrapped(Output, textToWrap, offsetOverride);
        }

        /// <summary>
        /// Writes the collection of rows to the output stream in tabular form using the configured <see cref="TableOptions"/>.
        /// </summary>
        /// <param name="dataSource">The collection of data to write.</param>
        public static void WriteTable(IEnumerable<IEnumerable<string>> dataSource)
        {
            WriteTable(Output, null, dataSource, o => o);
        }

        /// <summary>
        /// Writes the collection of rows to the configured output stream in tabular form using the given <see cref="ColumnDefinition"/> collection and the configured <see cref="TableOptions"/>.
        /// </summary>
        /// <param name="columnDefinitions">A collection of <see cref="ColumnDefinition"/> to use when writing the table.</param>
        /// <param name="dataSource">The collection of data to write.</param>
        public static void WriteTable(IEnumerable<ColumnDefinition> columnDefinitions, IEnumerable<IEnumerable<string>> dataSource)
        {
            WriteTable(Output, columnDefinitions, dataSource, o => o);
        }

        /// <summary>
        /// Writes the given enumerable data source to the configured output stream in tabular form using the given transform function to produce a single table row.
        /// </summary>
        /// <typeparam name="T">The type of object to translate to a table row.</typeparam>
        /// <param name="dataSource">The collection of data to write in tabular form.</param>
        /// <param name="rowProducer">The transform function to invoke to produce a single table row.</param>
        public static void WriteTable<T>(IEnumerable<T> dataSource, Func<T, IEnumerable<string>> rowProducer)
        {
            WriteTable(Output, null, dataSource, rowProducer);
        }

        /// <summary>
        /// Writes the given enumerable data source to the configured output stream in tabular form using the given <see cref="ColumnDefinition"/> collection and transform function to produce a single table row.
        /// </summary>
        /// <typeparam name="T">The type of object to translate to a table row.</typeparam>
        /// <param name="columnDefinitions">A collection of <see cref="ColumnDefinition"/> to use when writing the table.</param>
        /// <param name="dataSource">The collection of data to write in tabular form.</param>
        /// <param name="rowProducer">The transform function to invoke to produce a single table row.</param>
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

        /// <summary>
        /// Writes the given string to the output stream wrapping the text at word boundaries.
        /// </summary>
        /// <param name="outputOverride">The <see cref="TextWriter"/> to write output to.</param>
        /// <param name="textToWrap">The text to write.</param>
        /// <param name="offsetOverride">A number to use as the left offset for new lines. By default the current horizontal position on the console.</param>
        public static void WriteWrapped(TextWriter outputOverride, string textToWrap, int? offsetOverride = null)
        {
            if (!ReferenceEquals(outputOverride, Console.Out) || Console.CursorLeft + textToWrap.Length < Console.BufferWidth)
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

        /// <summary>
        /// Writes the given enumerable data source to the output stream in tabular form using the given transform function to produce a single table row.
        /// </summary>
        /// <typeparam name="T">The type of object to translate to a table row.</typeparam>
        /// <param name="outputOverride">The <see cref="TextWriter"/> to write output to.</param>
        /// <param name="dataSource">The collection of data to write in tabular form.</param>
        /// <param name="rowProducer">The transform function to invoke to produce a single table row.</param>
        public static void WriteTable<T>(TextWriter outputOverride, IEnumerable<T> dataSource, Func<T, IEnumerable<object>> rowProducer)
        {
            WriteTable(outputOverride, null, dataSource, rowProducer);
        }

        /// <summary>
        /// Writes the given enumerable data source to the output stream in tabular form using the given <see cref="ColumnDefinition"/> collection and transform function to produce a single table row.
        /// </summary>
        /// <typeparam name="T">The type of object to translate to a table row.</typeparam>
        /// <param name="outputOverride">The <see cref="TextWriter"/> to write output to.</param>
        /// <param name="columnDefinitions">A collection of <see cref="ColumnDefinition"/> to use when writing the table.</param>
        /// <param name="dataSource">The collection of data to write in tabular form.</param>
        /// <param name="rowProducer">The transform function to invoke to produce a single table row.</param>
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

            var isUsingConsoleOutput = ReferenceEquals(outputOverride, Console.Out);
            var columnList = (columnDefinitions ?? Enumerable.Empty<ColumnDefinition>()).ToList();
            var data = dataSource.ToList();
            var columnWidths = new List<int>();
            var maxTotalWidth = isUsingConsoleOutput ? Console.BufferWidth : int.MaxValue;
            var rows = new List<string[]>();
            var columnCount = columnList.Count;
            var hasHeaderRow = false;

            // If we have a header row, add that as the first row in the table.
            if (columnCount > 0)
            {
                hasHeaderRow = true;
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

            // If we cannot give every column the max width it needs then scale the table columns according to
            // the column definitions and global defaults.
            if (actualTotalWidth > maxTotalWidth)
            {
                ScaleTableColumns(actualTotalWidth, maxTotalWidth, columnCount, columnWidths, columnList);
            }

            // Build the format string to use for each row.
            var rowFormatParts = new List<string>();

            for (int i = 0; i < columnCount; i++)
            {
                rowFormatParts.Add(string.Format("{{{0},-{1}}}", i, columnWidths[i]));
            }

            var rowFormat = string.Join(" ", rowFormatParts);
            var rowNum = 1;
            var hasWrittenHeaderRow = false;

            // Write the table
            foreach (var row in rows)
            {
                while (row.Any(s => !string.IsNullOrWhiteSpace(s)))
                {
                    // Declaring as object[] so there is not a co-variant conversion when passing into string.Format.
                    var formatParams = new object[columnCount];

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
                                row[i] = column.Substring(formatParams[i].ToString().Length).TrimStart();
                            }
                        }
                        else
                        {
                            row[i] = string.Empty;
                            formatParams[i] = column;
                        }
                    }

                    var color = ConsoleColor.White;

                    if (rowNum % 2 != 0)
                    {
                        if (!hasWrittenHeaderRow && hasHeaderRow && rowNum == 1)
                        {
                            color = Options.TableOptions.HeaderRowColor;
                            hasWrittenHeaderRow = true;
                            rowNum--;
                        }
                        else
                        {
                            color = Options.TableOptions.RowColor;
                        }
                    }
                    else if (Options.TableOptions.AlternateRowColor)
                    {
                        color = Options.TableOptions.AlternatingRowColor;
                    }

                    SetConsoleColor(color, isUsingConsoleOutput);
                    var line = string.Format(rowFormat, formatParams);
                    outputOverride.Write(line);

                    if (actualTotalWidth <= maxTotalWidth)
                    {
                        outputOverride.WriteLine();
                    }
                    if (Options.TableOptions.WriteEmptyLineBetweenRows)
                    {
                        outputOverride.WriteLine();
                    }
                }

                rowNum++;
            }

            ResetConsoleColor(isUsingConsoleOutput);
        }

        private static void ScaleTableColumns(int actualTotalWidth, int maxTotalWidth, int columnCount, List<int> columnWidths, List<ColumnDefinition> columnList)
        {
            int remainingWidth = maxTotalWidth;
            var dynamicColumnInfo = new List<Tuple<int, ColumnDefinition>>();

            for (int i = 0; i < columnCount; i++)
            {
                var definition = columnList.ElementAtOrDefault(i);
                var mode = definition == null ? Options.TableOptions.DefaultColumnWidthMode : definition.WidthMode;

                if (mode == ColumnWidthMode.Fixed && (definition == null || definition.Width <= 0))
                {
                    mode = ColumnWidthMode.Auto;
                }

                switch (mode)
                {
                    case ColumnWidthMode.Auto:
                        columnWidths[i] = GetAdjustedWidth((columnWidths[i] * maxTotalWidth) / actualTotalWidth, definition);
                        remainingWidth -= columnWidths[i];
                        break;
                    case ColumnWidthMode.Dynamic:
                        dynamicColumnInfo.Add(new Tuple<int, ColumnDefinition>(i, definition));
                        break;
                    case ColumnWidthMode.Max:
                        columnWidths[i] = GetAdjustedWidth(columnWidths[i], definition);
                        remainingWidth -= columnWidths[i];
                        break;
                    case ColumnWidthMode.Fixed:
                        var width = definition == null ? columnWidths[i] : definition.Width;
                        columnWidths[i] = GetAdjustedWidth(width, definition);
                        remainingWidth -= columnWidths[i];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Compute and set the actual widths of all the dynamic columns now that the amount of remaining space is determined.
            if (dynamicColumnInfo.Any())
            {
                var dynamicColumnCount = dynamicColumnInfo.Count;

                foreach (var columnTuple in dynamicColumnInfo)
                {
                    var dynamicWidth = remainingWidth / dynamicColumnCount;
                    var originalAdjustedWidth = GetAdjustedWidth(columnWidths[columnTuple.Item1], columnTuple.Item2);
                    var adjustedWidth = GetAdjustedWidth(dynamicWidth, columnTuple.Item2);
                    dynamicColumnCount--;

                    if (originalAdjustedWidth < adjustedWidth)
                    {
                        // This means that the max width needed by this column is less than the space that's available.
                        // In this case just use the smallest width possible to leave room for other dynamic columns.
                        columnWidths[columnTuple.Item1] = originalAdjustedWidth;
                    }
                    else
                    {
                        columnWidths[columnTuple.Item1] = adjustedWidth;
                    }

                    remainingWidth -= columnWidths[columnTuple.Item1];
                }
            }

            // Correct rounding issues as a result of integer division.
            // Add width back into a dynamic column if there is one, otherwise just tack it onto the end.
            var missingWidth = maxTotalWidth - columnWidths.Sum();
            if (missingWidth > 0)
            {
                var index = dynamicColumnInfo.Any() ? dynamicColumnInfo[0].Item1 : columnWidths.Count - 1;
                columnWidths[index] += missingWidth;
            }
        }

        private static int GetAdjustedWidth(int width, ColumnDefinition definition)
        {
            var minWidth = definition != null && definition.MinWidth > 0 ? definition.MinWidth : Options.TableOptions.MinimumColumnWidth;
            return minWidth > 0 && width < minWidth ? minWidth : width;
        }

        private static void SetConsoleColor(ConsoleColor color, bool isUsingConsoleOutput = true)
        {
            if (isUsingConsoleOutput)
            {
                Console.ForegroundColor = color;
            }
        }

        private static void ResetConsoleColor(bool isUsingConsoleOutput = true)
        {
            if (isUsingConsoleOutput)
            {
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Parses the given collection of arguments into the given set of possible arguments.
        /// </summary>
        /// <param name="consoleArgs">The collection of console arguments.</param>
        /// <param name="possibleArguments">The set of possible arguments to parse the console args into.</param>
        /// <returns>An <see cref="ArgumentParseResult"/> instance representing the result.</returns>
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

            var missingRequiredArgs = possibleArguments.Where(a => a.IsRequired && !parsedArgs.Contains(a)).ToList();
            return missingRequiredArgs.Any() ? new ArgumentParseResult(missingRequiredArgs) : ArgumentParseResult.Success;
        }

        private static string PromptForArgumentValue(IArgument argument)
        {
            string value;
            WriteLine("Please provide a value for the{0} argument {1} - {2}", argument.IsPassword ? " password" : string.Empty, argument.Name, argument.Description);
            Write("{0}: ", argument.Name);

            if (argument.IsPassword)
            {
                var charStack = new Stack<char>();
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                    if (key.Key == ConsoleKey.Backspace && charStack.Any())
                    {
                        charStack.Pop();
                    }
                    else
                    {
                        charStack.Push(key.KeyChar);
                    }
                }
                value = string.Join(string.Empty, charStack.Reverse());
            }
            else
            {
                value = Console.ReadLine();
            }

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
