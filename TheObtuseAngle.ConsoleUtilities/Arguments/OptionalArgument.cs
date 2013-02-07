using System;

namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public class OptionalArgument : Argument
    {
        public OptionalArgument()
            : this(null, (string[])null, null, null)
        {
        }

        public OptionalArgument(string name, Action<string> valueSetter)
            : this(name, (string[])null, null, valueSetter)
        {
        }

        public OptionalArgument(OptionalArgumentTemplate template, Action<string> valueSetter)
            : base(template, valueSetter)
        {
        }

        public OptionalArgument(string name, string description, Action<string> valueSetter)
            : this(name, (string[])null, description, valueSetter)
        {
        }

        public OptionalArgument(string name, string alias, string description, Action<string> valueSetter)
            : this(name, new[] { alias }, description, valueSetter)
        {
        }

        public OptionalArgument(string name, string[] aliases, string description, Action<string> valueSetter)
            : base(name, aliases, description, false, false, 0, valueSetter)
        {
        }
    }
}