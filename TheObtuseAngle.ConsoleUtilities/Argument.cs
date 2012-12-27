using System;

namespace TheObtuseAngle.ConsoleUtilities
{
    public class Argument : ArgumentTemplate, IArgument
    {
        private Action<string> valueSetter;

        public Argument()
        {
        }

        public Argument(string name, bool isRequired, Action<string> valueSetter)
            : this(name, new string[] { null }, null, false, isRequired, valueSetter)
        {
        }

        public Argument(string name, string alias, bool isRequired, Action<string> valueSetter)
            : this(name, new[] { alias }, null, false, isRequired, valueSetter)
        {
        }

        public Argument(string name, string[] aliases, bool isRequired, Action<string> valueSetter)
            : this(name, aliases, null, false, isRequired, valueSetter)
        {
        }

        public Argument(string name, bool requiresValue, bool isRequired, Action<string> valueSetter)
            : this(name, new string[] { null }, null, requiresValue, isRequired, valueSetter)
        {
        }

        public Argument(string name, string alias, bool requiresValue, bool isRequired, Action<string> valueSetter)
            : this(name, new[] { alias }, null, requiresValue, isRequired, valueSetter)
        {
        }

        public Argument(string name, string[] aliases, bool requiresValue, bool isRequired, Action<string> valueSetter)
            : this(name, aliases, null, requiresValue, isRequired, valueSetter)
        {
        }

        public Argument(string name, string alias, string description, bool requiresValue, bool isRequired, Action<string> valueSetter)
            : this(name, new[] { alias }, description, requiresValue, isRequired, valueSetter)
        {
        }

        public Argument(ArgumentTemplate template, Action<string> valueSetter)
            : this(template.Name, template.Aliases, template.Description, template.RequiresValue, template.IsRequired, valueSetter)
        {
        }

        public Argument(string name, string[] aliases, string description, bool requiresValue, bool isRequired, Action<string> valueSetter)
            : base(name, aliases, description, requiresValue, isRequired)
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
