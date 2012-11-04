using System.Collections.Generic;

namespace TheObtuseAngle.ConsoleUtilities
{
    public interface ICommand
    {
        string Name { get; }

        string Description { get; }

        IList<IArgument> Arguments { get; }

        bool Execute();
    }
}
