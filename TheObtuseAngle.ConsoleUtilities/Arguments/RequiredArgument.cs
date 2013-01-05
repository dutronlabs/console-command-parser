using System;

namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public class RequiredArgument : Argument
    {
        public RequiredArgument()
        {
        }

        public RequiredArgument(string name, Action<string> valueSetter)
            : this(name, (string[])null, null, valueSetter)
        {
        }

        public RequiredArgument(RequiredArgumentTemplate template, Action<string> valueSetter)
            : base(template, valueSetter)
        {
        }

        public RequiredArgument(string name, string description, Action<string> valueSetter)
            : this(name, (string[])null, description, valueSetter)
        {
        }

        public RequiredArgument(string name, string alias, string description, Action<string> valueSetter)
            : this(name, new[] { alias }, description, valueSetter)
        {
        }

        public RequiredArgument(string name, string[] aliases, string description, Action<string> valueSetter)
            : base(name, aliases, description, false, true, valueSetter)
        {
        }
    }
}