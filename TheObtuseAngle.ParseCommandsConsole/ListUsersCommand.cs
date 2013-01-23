using System;
using System.ComponentModel.Composition;

namespace TheObtuseAngle.ParseCommandsConsole
{
    [Export(typeof(ExampleCommandBase))]
    public class ListUsersCommand : ExampleCommandBase
    {
        public ListUsersCommand()
            : base("ListUsers", "Lists all users that have been registered with the system")
        {
        }

        public override bool Execute()
        {
            Console.WriteLine("Listing users...");
            return true;
        }
    }
}