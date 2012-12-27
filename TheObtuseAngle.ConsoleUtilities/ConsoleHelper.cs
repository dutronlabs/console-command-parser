using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheObtuseAngle.ConsoleUtilities
{
    public static class ConsoleHelper
    {
        private static TextWriter output;

        static ConsoleHelper()
        {
            output = Console.Out;
        }

        public static void Initialize(TextWriter output)
        {
            ConsoleHelper.output = output;
        }

        public static void WriteException(Exception e)
        {
            while (e != null)
            {
                output.WriteLine(e.Message);
                output.WriteLine(e.StackTrace);
                output.WriteLine();
                e = e.InnerException;
            }
        }

        public static bool ParseBoolArg(string value)
        {
            string val = value.ToLowerInvariant();
            return val == "y" || val == "1" || val == "yes";
        }

        public static void WriteWrapped(string textToWrap)
        {
            WriteWrapped(output, textToWrap);
        }

        public static void WriteWrapped(TextWriter output, string textToWrap, int? offsetOverride = null)
        {
            if (!object.ReferenceEquals(output, Console.Out) || Console.CursorLeft + textToWrap.Length < Console.BufferWidth)
            {
                output.Write(textToWrap);
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
                        output.Write(lineBuilder.ToString());
                        output.WriteLine();
                        output.Write(string.Empty.PadLeft(position));
                        lineLength = subsequentLineLength;
                        lineBuilder.Clear();
                    }

                    lineBuilder.Append(word);
                    lineBuilder.Append(' ');
                }

                if (lineBuilder.Length > 0)
                {
                    output.Write(lineBuilder.ToString());
                }
            }
        }

        public static ArgumentParseResult ParseArguments(string[] consoleArgs, ParseOptions parseOptions, bool isInteractiveMode, params IArgument[] possibleArguments)
        {
            var parsedArgs = new List<IArgument>();

            if (possibleArguments.HasDuplicates(true, a => a.Name) || possibleArguments.Where(a => a.Aliases != null && a.Aliases.Length > 0).SelectMany(a => a.Aliases).HasDuplicates(true))
            {
                throw new ArgumentException("Duplicate arguments detected.", "possibleArguments");
            }

            for (int i = 0; i < consoleArgs.Length; i++)
            {
                string argName;
                string argValue = null;
                bool needsIncrement = false;
                Func<string> argValueProducer;

                if (parseOptions.ArgumentValueSeparator == ' ')
                {
                    int index = i;
                    argName = consoleArgs[i];
                    argValueProducer =
                        () =>
                        {
                            needsIncrement = true;
                            return index + 1 < consoleArgs.Length ? consoleArgs[index + 1] : null;
                        };
                }
                else
                {
                    var argParts = consoleArgs[i].Split(parseOptions.ArgumentValueSeparator);
                    argName = argParts[0];
                    argValueProducer = () => argParts.Length > 1 ? argParts[1] : null;
                }

                var matchingArg = possibleArguments.FirstOrDefault(a =>
                    a.Name.Equals(argName, StringComparison.InvariantCultureIgnoreCase) ||
                    (a.Aliases != null && a.Aliases.Any(alias => alias != null && alias.Equals(argName, StringComparison.InvariantCultureIgnoreCase))));

                if (matchingArg == null)
                {
                    continue;
                }

                if (matchingArg.RequiresValue)
                {
                    argValue = argValueProducer();

                    if (string.IsNullOrWhiteSpace(argValue) && isInteractiveMode && parseOptions.IsUsingConsoleOutput)
                    {
                        argValue = PromptForArgumentValue(matchingArg);
                    }

                    if (string.IsNullOrWhiteSpace(argValue))
                    {
                        return new ArgumentParseResult(matchingArg);
                    }
                }

                var setterResult = SetArgumentValue(matchingArg, argValue, parseOptions);
                
                if (setterResult != null)
                {
                    return setterResult;
                }
                
                parsedArgs.Add(matchingArg);

                if (needsIncrement)
                {
                    i++;
                }
            }

            if (isInteractiveMode && parseOptions.IsUsingConsoleOutput)
            {
                foreach (var arg in possibleArguments.Where(a => a.IsRequired && a.RequiresValue && !parsedArgs.Contains(a)).ToList())
                {
                    var value = PromptForArgumentValue(arg);

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return new ArgumentParseResult(arg);
                    }

                    var setterResult = SetArgumentValue(arg, value, parseOptions);

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
            Console.WriteLine("Please provide a value for the argument {0} - {1}", argument.Name, argument.Description);
            Console.Write("{0}: ", argument.Name);
            var value = Console.ReadLine();
            Console.WriteLine();
            return value;
        }

        private static ArgumentParseResult SetArgumentValue(IArgument argument, string value, ParseOptions parseOptions)
        {
            if (argument.ValueSetter == null)
            {
                return null;
            }
            
            if (parseOptions.ThrowOnValueSetterException)
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
