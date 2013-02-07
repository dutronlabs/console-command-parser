using System;

namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public class Argument : ArgumentTemplate, IArgument
    {
        private Action<string> valueSetter;

        public Argument()
            : this(null, (string[])null, null, false, false, 1, null)
        {
        }

        public Argument(ArgumentTemplate template, Action<string> valueSetter)
            : this(template.Name, template.Aliases, template.Description, template.RequiresValue, template.IsRequired, 1, valueSetter)
        {
        }

        public Argument(string name, string alias, string description, bool requiresValue, bool isRequired, Action<string> valueSetter)
            : this(name, new[] { alias }, description, requiresValue, isRequired, 1, valueSetter)
        {
        }

        public Argument(string name, string alias, string description, bool requiresValue, bool isRequired, int numberOfValueArgs, Action<string> valueSetter)
            : this(name, new[] { alias }, description, requiresValue, isRequired, numberOfValueArgs, valueSetter)
        {
        }

        public Argument(string name, string[] aliases, string description, bool requiresValue, bool isRequired, Action<string> valueSetter)
            : this(name, aliases, description, requiresValue, isRequired, 1, valueSetter)
        {
        }

        public Argument(string name, string[] aliases, string description, bool requiresValue, bool isRequired, int numberOfValueArgs, Action<string> valueSetter)
            : base(name, aliases, description, requiresValue, isRequired, numberOfValueArgs)
        {
            this.valueSetter = valueSetter;
        }

        public virtual Action<string> ValueSetter
        {
            get { return this.valueSetter; }
            set { this.valueSetter = value; }
        }
    }
}