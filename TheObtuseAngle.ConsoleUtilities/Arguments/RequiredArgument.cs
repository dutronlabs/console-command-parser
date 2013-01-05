using System;

namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public class RequiredArgument : Argument
    {
        public RequiredArgument()
        {
        }

        public RequiredArgument(string name, Action<string> valueSetter)
            : this(name, (string[])null, null, 0, valueSetter)
        {
        }

        public RequiredArgument(string name, int ordinal, Action<string> valueSetter)
            : this(name, (string[])null, null, ordinal, valueSetter)
        {
        }

        public RequiredArgument(RequiredArgumentTemplate template, Action<string> valueSetter)
            : base(template, valueSetter)
        {
        }

        public RequiredArgument(RequiredArgumentTemplate template, int ordinal, Action<string> valueSetter)
            : base(template, ordinal, valueSetter)
        {
        }

        public RequiredArgument(string name, string description, Action<string> valueSetter)
            : this(name, (string[])null, description, 0, valueSetter)
        {
        }

        public RequiredArgument(string name, string description, int ordinal, Action<string> valueSetter)
            : this(name, (string[])null, description, ordinal, valueSetter)
        {
        }

        public RequiredArgument(string name, string alias, string description, Action<string> valueSetter)
            : this(name, new[] { alias }, description, 0, valueSetter)
        {
        }

        public RequiredArgument(string name, string[] aliases, string description, Action<string> valueSetter)
            : this(name, aliases, description, 0, valueSetter)
        {
        }

        public RequiredArgument(string name, string alias, string description, int ordinal, Action<string> valueSetter)
            : this(name, new[] { alias }, description, ordinal, valueSetter)
        {
        }

        public RequiredArgument(string name, string[] aliases, string description, int ordinal, Action<string> valueSetter)
            : base(name, aliases, description, false, true, ordinal, valueSetter)
        {
        }
    }
}