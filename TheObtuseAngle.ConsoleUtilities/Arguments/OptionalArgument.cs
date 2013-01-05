using System;

namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public class OptionalArgument : Argument
    {
        public OptionalArgument()
        {
        }

        public OptionalArgument(string name, Action<string> valueSetter)
            : this(name, (string[])null, null, 0, valueSetter)
        {
        }

        public OptionalArgument(string name, int ordinal, Action<string> valueSetter)
            : this(name, (string[])null, null, ordinal, valueSetter)
        {
        }

        public OptionalArgument(OptionalArgumentTemplate template, Action<string> valueSetter)
            : base(template, valueSetter)
        {
        }

        public OptionalArgument(OptionalArgumentTemplate template, int ordinal, Action<string> valueSetter)
            : base(template, ordinal, valueSetter)
        {
        }

        public OptionalArgument(string name, string description, Action<string> valueSetter)
            : this(name, (string[])null, description, 0, valueSetter)
        {
        }

        public OptionalArgument(string name, string description, int ordinal, Action<string> valueSetter)
            : this(name, (string[])null, description, ordinal, valueSetter)
        {
        }

        public OptionalArgument(string name, string alias, string description, Action<string> valueSetter)
            : this(name, new[] { alias }, description, 0, valueSetter)
        {
        }

        public OptionalArgument(string name, string[] aliases, string description, Action<string> valueSetter)
            : this(name, aliases, description, 0, valueSetter)
        {
        }

        public OptionalArgument(string name, string alias, string description, int ordinal, Action<string> valueSetter)
            : this(name, new[] { alias }, description, ordinal, valueSetter)
        {
        }

        public OptionalArgument(string name, string[] aliases, string description, int ordinal, Action<string> valueSetter)
            : base(name, aliases, description, false, false, ordinal, valueSetter)
        {
        }
    }
}