using System;

namespace TheObtuseAngle.ConsoleUtilities
{
    public class Argument : IArgument
    {
        private string name;
        private string[] aliases;
        private string description;
        private bool isRequired;
        private bool requiresValue;
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

        public Argument(string name, string[] aliases, string description, bool requiresValue, bool isRequired, Action<string> valueSetter)
        {
            this.name = name;
            this.aliases = aliases;
            this.description = description;
            this.requiresValue = requiresValue;
            this.isRequired = isRequired;
            this.valueSetter = valueSetter;
        }

        public virtual string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public virtual string[] Aliases
        {
            get { return this.aliases; }
            set { this.aliases = value; }
        }

        public virtual string Description
        {
            get { return description; }
            set { description = value; }
        }

        public virtual bool RequiresValue
        {
            get { return this.requiresValue; }
            set { this.requiresValue = value; }
        }

        public virtual bool IsRequired
        {
            get { return this.isRequired; }
            set { this.isRequired = value; }
        }

        public virtual Action<string> ValueSetter
        {
            get { return this.valueSetter; }
            set { this.valueSetter = value; }
        }

        public void SetValue(string value)
        {
            this.valueSetter(value);
        }
    }
}
