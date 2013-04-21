using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheObtuseAngle.ConsoleUtilities;

namespace TheObtuseAngle.TestConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var clouds = new[]
            {
                new Cloud("Development", "http://apps.apprenda.emonster", "This is the development cloud that should be used while apps are in development.", 1),
                new Cloud("Testing", "http://apps.apprenda.miller", "Use when apps are ready for QA.", 2),
                new Cloud("Staging", "http://apps.apprenda.staging", "Apps go here when they are ready for production.", 2),
                new Cloud("Production", "http://apps.apprenda.production", "Where production apps run.", 5)
            };

            var columnDefinitions = new[]
            {
                new ColumnDefinition("Name", ColumnWidthMode.Max),
                new ColumnDefinition("Url"),
                new ColumnDefinition("Description", ColumnWidthMode.Dynamic),
                new ColumnDefinition("Node Count")
            };

            Console.WriteLine("All Clouds:");
            Console.WriteLine();
            ConsoleHelper.WriteTable(Console.Out, columnDefinitions, clouds, c => c.ToRow());
            Console.WriteLine();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }

    public class Cloud
    {
        public Cloud()
        {
        }

        public Cloud(string name, string url, string description, int nodeCount)
        {
            Name = name;
            Url = url;
            Description = description;
            NodeCount = nodeCount;
        }

        public string Name { get; set; }

        public string Url { get; set; }

        public string Description { get; set; }

        public int NodeCount { get; set; }

        public object[] ToRow()
        {
            return new object[] { Name, Url, Description, NodeCount };
        }
    }
}
