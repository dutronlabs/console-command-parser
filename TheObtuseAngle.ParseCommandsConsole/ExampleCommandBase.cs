using System.Collections.Generic;
using System.ComponentModel.Composition;
using TheObtuseAngle.ConsoleUtilities.Arguments;
using TheObtuseAngle.ConsoleUtilities.Commands;

namespace TheObtuseAngle.ParseCommandsConsole
{
    [Export(typeof(ExampleCommandBase))]
    public abstract class ExampleCommandBase : CommandBase
    {
        protected bool isVerbose;

        protected ExampleCommandBase(string name, string description)
            : base(name, description)
        {
        }

        protected virtual IEnumerable<IArgument> GetArguments()
        {
            return new IArgument[]
            {
                new OptionalArgument("-zzTop", string.Empty, "This argument was added first in the base class.", null),
                new OptionalArgument("-verbose", "-v", "When specified, verbose debugging information will be logged.", _ => isVerbose = true),
                new RequiredArgument("-xyRequired", string.Empty, "This is a required argument added win the base class.", null)
            };
        }

        public override void OnBeforeParse()
        {
            foreach (var argument in GetArguments())
            {
                Arguments.Add(argument);
            }
        }
    }
}