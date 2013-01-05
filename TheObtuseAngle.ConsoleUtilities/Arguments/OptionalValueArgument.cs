using System;

namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public class OptionalValueArgument : Argument
    {
        public OptionalValueArgument()
        {
        }

        public OptionalValueArgument(string name, Action<string> valueSetter)
            : this(name, (string[])null, null, 0, valueSetter)
        {
        }

        public OptionalValueArgument(string name, int ordinal, Action<string> valueSetter)
            : this(name, (string[])null, null, ordinal, valueSetter)
        {
        }

        public OptionalValueArgument(OptionalValueArgumentTemplate template, Action<string> valueSetter)
            : base(template, valueSetter)
        {
        }

        public OptionalValueArgument(OptionalValueArgumentTemplate template, int ordinal, Action<string> valueSetter)
            : base(template, ordinal, valueSetter)
        {
        }

        public OptionalValueArgument(string name, string description, Action<string> valueSetter)
            : this(name, (string[])null, description, 0, valueSetter)
        {
        }

        public OptionalValueArgument(string name, string description, int ordinal, Action<string> valueSetter)
            : this(name, (string[])null, description, ordinal, valueSetter)
        {
        }

        public OptionalValueArgument(string name, string alias, string description, Action<string> valueSetter)
            : this(name, new[] { alias }, description, 0, valueSetter)
        {
        }

        public OptionalValueArgument(string name, string[] aliases, string description, Action<string> valueSetter)
            : this(name, aliases, description, 0, valueSetter)
        {
        }

        public OptionalValueArgument(string name, string alias, string description, int ordinal, Action<string> valueSetter)
            : this(name, new[] { alias }, description, ordinal, valueSetter)
        {
        }

        public OptionalValueArgument(string name, string[] aliases, string description, int ordinal, Action<string> valueSetter)
            : base(name, aliases, description, true, false, ordinal, valueSetter)
        {
        }
    }
}
