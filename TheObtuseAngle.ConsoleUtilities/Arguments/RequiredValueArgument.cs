using System;

namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public class RequiredValueArgument : Argument
    {
        public RequiredValueArgument()
        {
        }

        public RequiredValueArgument(string name, Action<string> valueSetter)
            : this(name, (string[])null, null, valueSetter)
        {
        }

        public RequiredValueArgument(RequiredValueArgumentTemplate template, Action<string> valueSetter)
            : base(template, valueSetter)
        {
        }

        public RequiredValueArgument(string name, string description, Action<string> valueSetter)
            : this(name, (string[])null, description, valueSetter)
        {
        }

        public RequiredValueArgument(string name, string alias, string description, Action<string> valueSetter)
            : this(name, new[] { alias }, description, valueSetter)
        {
        }

        public RequiredValueArgument(string name, string[] aliases, string description, Action<string> valueSetter)
            : base(name, aliases, description, true, true, valueSetter)
        {
        }
    }
}