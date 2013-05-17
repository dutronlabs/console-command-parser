using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheObtuseAngle.ConsoleUtilities.Arguments;

namespace TheObtuseAngle.ConsoleUtilities
{
    /// <summary>
    /// Contains a collection of extension methods that help with building console applications.
    /// </summary>
    public static class ConsoleHelperExtensions
    {
        /// <summary>
        /// Parses the string and converts it to a boolean representation.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The boolean representation of the string.</returns>
        public static bool ToBool(this string value)
        {
            return ConsoleHelper.ParseBoolArg(value);
        }

        /// <summary>
        /// Writes the exception to the console, including all inner exceptions.
        /// </summary>
        /// <param name="e">The exception to write.</param>
        public static void WriteToConsole(this Exception e)
        {
            ConsoleHelper.WriteException(e, true);
        }

        /// <summary>
        /// Builds a string representation of the exception, including all inner exceptions.
        /// </summary>
        /// <param name="e">The exception to convert to a string.</param>
        /// <returns>The string representation of the exception.</returns>
        public static string ToFullDepthString(this Exception e)
        {
            return ConsoleHelper.GetFullDepthExceptionString(e);
        }

        /// <summary>
        /// Determines whether or not the collection of arguments contains the given argument.
        /// </summary>
        /// <param name="consoleArgs">The collection of console arguments.</param>
        /// <param name="arg">The argument to check for containment.</param>
        /// <returns>True when the collection of arguments contains the given argument, otherwise false.</returns>
        public static bool HasArgument(this string[] consoleArgs, IArgument arg)
        {
            return consoleArgs != null && arg != null &&
                consoleArgs.Any(a => a.Equals(arg.Name, StringComparison.InvariantCultureIgnoreCase) || (arg.Aliases != null && arg.Aliases.Any(alias => alias.Equals(a, StringComparison.InvariantCultureIgnoreCase))));
        }

        /// <summary>
        /// Determines whether or not the collection of arguments contains an argument that matches the given template.
        /// </summary>
        /// <param name="consoleArgs">The collection of console arguments.</param>
        /// <param name="template">The argument template to check for containment.</param>
        /// <returns>True when the collection of arguments contains the given template, otherwise false.</returns>
        public static bool HasArgument(this string[] consoleArgs, ArgumentTemplate template)
        {
            return consoleArgs != null && template != null &&
                consoleArgs.Any(a => a.Equals(template.Name, StringComparison.InvariantCultureIgnoreCase) || (template.Aliases != null && template.Aliases.Any(alias => alias.Equals(a, StringComparison.InvariantCultureIgnoreCase))));
        }

        /// <summary>
        /// Determines whether or not the collection of arguments contains the given argument.
        /// </summary>
        /// <param name="args">The collection of console arguments.</param>
        /// <param name="arg">The argument to check for containment.</param>
        /// <returns>True when the collection of arguments contains the given argument, otherwise false.</returns>
        public static bool HasArgument(this string[] args, string arg)
        {
            return args == null || args.Any(a => a.Equals(arg, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Writes the given string to the output stream wrapping the text at word boundaries.
        /// </summary>
        /// <param name="output">The <see cref="TextWriter"/> to write output to.</param>
        /// <param name="textToWrap">The text to write.</param>
        /// <param name="offsetOverride">A number to use as the left offset for new lines. By default the current horizontal position on the console.</param>
        public static void WriteWrapped(this TextWriter output, string textToWrap, int? offsetOverride = null)
        {
            ConsoleHelper.WriteWrapped(output, textToWrap, offsetOverride);
        }

        /// <summary>
        /// Writes the collection of rows to the output stream in tabular form using the default <see cref="TableOptions"/>.
        /// </summary>
        /// <param name="output">The <see cref="TextWriter"/> to write output to.</param>
        /// <param name="dataSource">The collection of data to write.</param>
        public static void WriteTable(this TextWriter output, IEnumerable<IEnumerable<string>> dataSource)
        {
            ConsoleHelper.WriteTable(output, null, dataSource, o => o);
        }

        /// <summary>
        /// Writes the collection of rows to the output stream in tabular form using the given <see cref="ColumnDefinition"/> collection and the default <see cref="TableOptions"/>.
        /// </summary>
        /// <param name="output">The <see cref="TextWriter"/> to write output to.</param>
        /// <param name="columnDefinitions">A collection of <see cref="ColumnDefinition"/> to use when writing the table.</param>
        /// <param name="dataSource">The collection of data to write.</param>
        public static void WriteTable(this TextWriter output, IEnumerable<ColumnDefinition> columnDefinitions, IEnumerable<IEnumerable<string>> dataSource)
        {
            ConsoleHelper.WriteTable(output, columnDefinitions, dataSource, o => o);
        }

        /// <summary>
        /// Writes the given enumerable data source to the output stream in tabular form using the given transform function to produce a single table row.
        /// </summary>
        /// <typeparam name="T">The type of object to translate to a table row.</typeparam>
        /// <param name="output">The <see cref="TextWriter"/> to write output to.</param>
        /// <param name="dataSource">The collection of data to write in tabular form.</param>
        /// <param name="rowProducer">The transform function to invoke to produce a single table row.</param>
        public static void WriteTable<T>(this TextWriter output, IEnumerable<T> dataSource, Func<T, IEnumerable<string>> rowProducer)
        {
            ConsoleHelper.WriteTable(output, null, dataSource, rowProducer);
        }

        /// <summary>
        /// Writes the given enumerable data source to the output stream in tabular form using the given <see cref="ColumnDefinition"/> collection and transform function to produce a single table row.
        /// </summary>
        /// <typeparam name="T">The type of object to translate to a table row.</typeparam>
        /// <param name="output">The <see cref="TextWriter"/> to write output to.</param>
        /// <param name="columnDefinitions">A collection of <see cref="ColumnDefinition"/> to use when writing the table.</param>
        /// <param name="dataSource">The collection of data to write in tabular form.</param>
        /// <param name="rowProducer">The transform function to invoke to produce a single table row.</param>
        public static void WriteTable<T>(this TextWriter output, IEnumerable<ColumnDefinition> columnDefinitions, IEnumerable<T> dataSource, Func<T, IEnumerable<string>> rowProducer)
        {
            ConsoleHelper.WriteTable(output, columnDefinitions, dataSource, rowProducer);
        }

        /// <summary>
        /// Parses the collection of arguments into the given set of possible arguments.
        /// </summary>
        /// <param name="consoleArgs">The collection of console arguments.</param>
        /// <param name="possibleArguments">The set of possible arguments to parse the console args into.</param>
        /// <returns>An <see cref="ArgumentParseResult"/> instance representing the result.</returns>
        public static ArgumentParseResult ParseArguments(this string[] consoleArgs, params IArgument[] possibleArguments)
        {
            return ConsoleHelper.ParseArguments(consoleArgs, possibleArguments);
        }
    }
}
