using System;

namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public class RequiredValueArgument : Argument
    {
        public RequiredValueArgument()
        {
        }

        public RequiredValueArgument(string name, Action<string> valueSetter)
            : this(name, (string[])null, null, 0, valueSetter)
        {
        }

        public RequiredValueArgument(string name, int ordinal, Action<string> valueSetter)
            : this(name, (string[])null, null, ordinal, valueSetter)
        {
        }

        public RequiredValueArgument(RequiredValueArgumentTemplate template, Action<string> valueSetter)
            : base(template, valueSetter)
        {
        }

        public RequiredValueArgument(RequiredValueArgumentTemplate template, int ordinal, Action<string> valueSetter)
            : base(template, ordinal, valueSetter)
        {
        }

        public RequiredValueArgument(string name, string description, Action<string> valueSetter)
            : this(name, (string[])null, description, 0, valueSetter)
        {
        }

        public RequiredValueArgument(string name, string description, int ordinal, Action<string> valueSetter)
            : this(name, (string[])null, description, ordinal, valueSetter)
        {
        }

        public RequiredValueArgument(string name, string alias, string description, Action<string> valueSetter)
            : this(name, new[] { alias }, description, 0, valueSetter)
        {
        }

        public RequiredValueArgument(string name, string[] aliases, string description, Action<string> valueSetter)
            : this(name, aliases, description, 0, valueSetter)
        {
        }

        public RequiredValueArgument(string name, string alias, string description, int ordinal, Action<string> valueSetter)
            : this(name, new[] { alias }, description, ordinal, valueSetter)
        {
        }

        public RequiredValueArgument(string name, string[] aliases, string description, int ordinal, Action<string> valueSetter)
            : base(name, aliases, description, true, true, ordinal, valueSetter)
        {
        }
    }
}