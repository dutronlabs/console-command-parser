using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using TheObtuseAngle.ConsoleUtilities;

namespace TheObtuseAngle.ParseCommandsConsole
{
    public class Program
    {
        private readonly string[] consoleArgs;

        public static void Main(string[] args)
        {
            Console.WriteLine();
            var program = new Program(args);
            program.Run();
            Console.WriteLine();
        }

        public Program(string[] consoleArgs)
        {
            this.consoleArgs = consoleArgs;
            Compose();
        }

        private void Compose()
        {
            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        [ImportMany]
        public IEnumerable<ExampleCommandBase> Commands { get; set; }

        public void Run()
        {
            try
            {
                var parser = new CommandParser<ExampleCommandBase>();
                var result = parser.ParseCommandAndExecute(consoleArgs, Commands);

                Console.WriteLine();
                Console.WriteLine(result);
            }
            catch (Exception e)
            {
                e.WriteToConsole();
            }
        }
    }
}
