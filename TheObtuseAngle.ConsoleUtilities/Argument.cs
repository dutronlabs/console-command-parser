using System;

namespace TheObtuseAngle.ConsoleUtilities
{
    public class Argument : IArgument
    {
        private static readonly IArgument quietModeArgument;
        private static readonly IArgument helpArgument;
        private string name;
        private string alias;
        private string description;
        private bool isRequired;
        private bool requiresValue;
        private Action<string> valueSetter;

        static Argument()
        {
            quietModeArgument = new Argument("-quiet", "-q", "Suppresses all output", false, false, null);
            helpArgument = new Argument("-help", "/?", "Displays this usage information", false, false, null);
        }

        public Argument()
        {
        }

        public Argument(string name, bool isRequired, Action<string> valueSetter)
            : this(name, null, null, false, isRequired, valueSetter)
        {
        }

        public Argument(string name, string alias, bool isRequired, Action<string> valueSetter)
            : this(name, alias, null, false, isRequired, valueSetter)
        {
        }

        public Argument(string name, bool requiresValue, bool isRequired, Action<string> valueSetter)
            : this(name, null, null, requiresValue, isRequired, valueSetter)
        {
        }

        public Argument(string name, string alias, bool requiresValue, bool isRequired, Action<string> valueSetter)
            : this(name, alias, null, requiresValue, isRequired, valueSetter)
        {
        }

        public Argument(string name, string alias, string description, bool requiresValue, bool isRequired, Action<string> valueSetter)
        {
            this.name = name;
            this.alias = alias;
            this.description = description;
            this.requiresValue = requiresValue;
            this.isRequired = isRequired;
            this.valueSetter = valueSetter;
        }

        internal static IArgument QuietModeArgument
        {
            get { return quietModeArgument; }
        }

        internal static IArgument HelpArgument
        {
            get { return helpArgument; }
        }

        public virtual string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public virtual string Alias
        {
            get { return this.alias; }
            set { this.alias = value; }
        }

        public virtual string Description
        {
            get { return description; }
            set { description = value; }
        }

        public bool RequiresValue
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
