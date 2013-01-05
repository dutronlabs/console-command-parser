using System;

namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public class Argument : ArgumentTemplate, IArgument
    {
        private int ordinal;
        private Action<string> valueSetter;

        public Argument()
        {
        }

        public Argument(ArgumentTemplate template, Action<string> valueSetter)
            : this(template.Name, template.Aliases, template.Description, template.RequiresValue, template.IsRequired, 0, valueSetter)
        {
        }

        public Argument(ArgumentTemplate template, int ordinal, Action<string> valueSetter)
            : this(template.Name, template.Aliases, template.Description, template.RequiresValue, template.IsRequired, ordinal, valueSetter)
        {
        }

        public Argument(string name, string alias, string description, bool requiresValue, bool isRequired, Action<string> valueSetter)
            : this(name, new[] { alias }, description, requiresValue, isRequired, 0, valueSetter)
        {
        }

        public Argument(string name, string alias, string description, bool requiresValue, bool isRequired, int ordinal, Action<string> valueSetter)
            : this(name, new[] { alias }, description, requiresValue, isRequired, ordinal, valueSetter)
        {
        }

        public Argument(string name, string[] aliases, string description, bool requiresValue, bool isRequired, Action<string> valueSetter)
            : this(name, aliases, description, requiresValue, isRequired, 0, valueSetter)
        {
        }

        public Argument(string name, string[] aliases, string description, bool requiresValue, bool isRequired, int ordinal, Action<string> valueSetter)
            : base(name, aliases, description, requiresValue, isRequired)
        {
            this.ordinal = ordinal;
            this.valueSetter = valueSetter;
        }

        public virtual int Ordinal
        {
            get { return this.ordinal; }
            set { this.ordinal = value; }
        }

        public virtual Action<string> ValueSetter
        {
            get { return this.valueSetter; }
            set { this.valueSetter = value; }
        }
    }
}