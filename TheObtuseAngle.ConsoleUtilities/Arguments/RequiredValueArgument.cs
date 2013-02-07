using System;

namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public class RequiredValueArgument : Argument
    {
        public RequiredValueArgument()
            : this(null, (string[])null, null, 1, null)
        {
        }

        public RequiredValueArgument(string name, Action<string> valueSetter)
            : this(name, (string[])null, null, 1, valueSetter)
        {
        }

        public RequiredValueArgument(RequiredValueArgumentTemplate template, Action<string> valueSetter)
            : base(template, valueSetter)
        {
        }

        public RequiredValueArgument(string name, string description, Action<string> valueSetter)
            : this(name, (string[])null, description, 1, valueSetter)
        {
        }

        public RequiredValueArgument(string name, string alias, string description, Action<string> valueSetter)
            : this(name, new[] { alias }, description, 1, valueSetter)
        {
        }

        public RequiredValueArgument(string name, string alias, string description, int numberOfValueArgs, Action<string> valueSetter)
            : this(name, new[] { alias }, description, numberOfValueArgs, valueSetter)
        {
        }

        public RequiredValueArgument(string name, string[] aliases, string description, Action<string> valueSetter)
            : this(name, aliases, description, 1, valueSetter)
        {
        }

        public RequiredValueArgument(string name, string[] aliases, string description, int numberOfValueArgs, Action<string> valueSetter)
            : base(name, aliases, description, true, true, numberOfValueArgs, valueSetter)
        {
        }
    }
}