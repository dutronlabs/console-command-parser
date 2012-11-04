using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace TheObtuseAngle.ConsoleUtilities
{
    public static class ConsoleHelper
    {
        private static readonly object writeLock = new object();

        static ConsoleHelper()
        {
            Output = Console.Out;
        }

        private static TextWriter Output { get; set; }

        public static void Initialize(TextWriter output)
        {
            Output = output;
        }

        public static void WriteException(Exception e)
        {
            while (e != null)
            {
                Output.WriteLine(e.Message);
                Output.WriteLine(e.StackTrace);
                Output.WriteLine();
                e = e.InnerException;
            }
        }

        public static bool ParseBoolArg(string value)
        {
            string val = value.ToLowerInvariant();
            return val == "y" || val == "1" || val == "yes";
        }

        internal static void HandleDebugFlag(bool debugFlagEnabled, DebugFlagAction action)
        {
            if (!debugFlagEnabled)
            {
                return;
            }

            switch (action)
            {
                case DebugFlagAction.DebuggerLaunch:
                    Debugger.Launch();
                    break;
                case DebugFlagAction.ThreadSleep:
                    Output.WriteLine("Pausing for 10 seconds to allow you to attach to the process...");
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                    break;
            }
        }
    }
}
