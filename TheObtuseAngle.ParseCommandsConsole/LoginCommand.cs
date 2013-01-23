using System;
using System.ComponentModel.Composition;

namespace TheObtuseAngle.ParseCommandsConsole
{
    [Export(typeof(ExampleCommandBase))]
    public sealed class LoginCommand : AuthenticatedCommandBase
    {
        public LoginCommand()
            : base("Login", "Attempts to login as the specified user")
        {
        }

        public override bool Execute()
        {
            Console.WriteLine("Logging in as '{0}' with password '{1}'...", userName, password);
            return true;
        }
    }
}