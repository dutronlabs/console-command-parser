using System;

namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public class RequiredPasswordArgument : Argument
    {
        public RequiredPasswordArgument()
            : this(null, null, null, null)
        {
        }

        public RequiredPasswordArgument(string name, Action<string> valueSetter)
            : this(name, null, null, valueSetter)
        {
        }

        public RequiredPasswordArgument(RequiredPasswordArgumentTemplate template, Action<string> valueSetter)
            : base(template, valueSetter)
        {
        }

        public RequiredPasswordArgument(string name, string description, Action<string> valueSetter)
            : this(name, null, description, valueSetter)
        {
        }

        public RequiredPasswordArgument(string name, string alias, string description, Action<string> valueSetter)
            : base(name, new[] { alias }, description, true, true, 1, true, valueSetter)
        {
        }
    }
}