using System;
using System.IO;
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

                foreach (var word in textToWrap.Split(new[] { ' ' }))
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
    }
}
