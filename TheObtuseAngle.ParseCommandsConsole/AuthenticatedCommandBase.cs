using System.Collections.Generic;
using System.ComponentModel.Composition;
using TheObtuseAngle.ConsoleUtilities.Arguments;
using System.Linq;

namespace TheObtuseAngle.ParseCommandsConsole
{
    [Export(typeof(ExampleCommandBase))]
    public abstract class AuthenticatedCommandBase : ExampleCommandBase
    {
        protected string userName;
        protected string password;

        protected AuthenticatedCommandBase(string name, string description)
            : base(name, description)
        {
        }

        protected override IEnumerable<IArgument> GetArguments()
        {
            var arguments = base.GetArguments().ToList();
            arguments.InsertRange(0, new IArgument[]
            {
                new RequiredValueArgument("-userName", "-n", "The name of the new user", val => userName = val),
                new RequiredValueArgument("-password", "-pw", "The password for the new user", val => password = val),
                new OptionalArgument("-optional", string.Empty, "This is an optional argument added in AuthenticatedCommandBase.", null)
            });
            return arguments;
        }
    }
}