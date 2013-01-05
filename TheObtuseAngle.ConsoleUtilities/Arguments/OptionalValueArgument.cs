using System;

namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public class OptionalValueArgument : Argument
    {
        public OptionalValueArgument()
        {
        }

        public OptionalValueArgument(string name, Action<string> valueSetter)
            : this(name, (string[])null, null, valueSetter)
        {
        }

        public OptionalValueArgument(OptionalValueArgumentTemplate template, Action<string> valueSetter)
            : base(template, valueSetter)
        {
        }

        public OptionalValueArgument(string name, string description, Action<string> valueSetter)
            : this(name, (string[])null, description, valueSetter)
        {
        }

        public OptionalValueArgument(string name, string alias, string description, Action<string> valueSetter)
            : this(name, new[] { alias }, description, valueSetter)
        {
        }

        public OptionalValueArgument(string name, string[] aliases, string description, Action<string> valueSetter)
            : base(name, aliases, description, true, false, valueSetter)
        {
        }
    }
}
