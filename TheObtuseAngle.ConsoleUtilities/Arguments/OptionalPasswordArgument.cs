using System;

namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public class OptionalPasswordArgument : Argument
    {
        public OptionalPasswordArgument()
            : this(null, null, null, null)
        {
        }

        public OptionalPasswordArgument(string name, Action<string> valueSetter)
            : this(name, null, null, valueSetter)
        {
        }

        public OptionalPasswordArgument(OptionalPasswordArgumentTemplate template, Action<string> valueSetter)
            : base(template, valueSetter)
        {
        }

        public OptionalPasswordArgument(string name, string description, Action<string> valueSetter)
            : this(name, null, description, valueSetter)
        {
        }

        public OptionalPasswordArgument(string name, string alias, string description, Action<string> valueSetter)
            : base(name, new[] { alias }, description, true, false, 1, true, valueSetter)
        {
        }
    }
}