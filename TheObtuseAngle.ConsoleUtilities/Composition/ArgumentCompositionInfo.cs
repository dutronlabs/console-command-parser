using System;
using TheObtuseAngle.ConsoleUtilities.Arguments;

namespace TheObtuseAngle.ConsoleUtilities.Composition
{
    public class ArgumentCompositionInfo
    {
        public ArgumentCompositionInfo(int rank, Type declaringType, IArgument argument)
        {
            InheritanceLevel = rank;
            DeclaringType = declaringType;
            Argument = argument;
        }

        public int InheritanceLevel { get; private set; }

        public Type DeclaringType { get; set; }

        public IArgument Argument { get; private set; }
    }
}