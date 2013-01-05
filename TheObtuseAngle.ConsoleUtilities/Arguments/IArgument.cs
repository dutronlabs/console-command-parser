using System;

namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public interface IArgument
    {
        string Name { get; set; }

        string[] Aliases { get; set; }

        string Description { get; set; }

        bool RequiresValue { get; set; }

        bool IsRequired { get; set; }

        int Ordinal { get; set; }

        Action<string> ValueSetter { get; set; }
    }
}
