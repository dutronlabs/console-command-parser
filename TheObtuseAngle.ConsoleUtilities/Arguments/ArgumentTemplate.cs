namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public class ArgumentTemplate
    {
        private string name;
        private string[] aliases;
        private string description;
        private bool isRequired;
        private bool requiresValue;
        private bool isPassword;
        private int numberOfValueArgs;

        public ArgumentTemplate()
            : this(null, null, null, false, false, 1)
        {
        }

        public ArgumentTemplate(string name, string[] aliases, string description, bool requiresValue, bool isRequired)
            : this(name, aliases, description, requiresValue, isRequired, 1)
        {
        }

        public ArgumentTemplate(string name, string[] aliases, string description, bool requiresValue, bool isRequired, int numberOfValueArgs, bool isPassword = false)
        {
            this.name = name;
            this.aliases = aliases;
            this.description = description;
            this.requiresValue = requiresValue;
            this.isRequired = isRequired;
            this.numberOfValueArgs = numberOfValueArgs;
            this.isPassword = isPassword;
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

        public virtual bool IsPassword
        {
            get { return this.isPassword; }
            set { this.isPassword = value; }
        }

        public virtual int NumberOfValueArgs
        {
            get { return this.numberOfValueArgs; }
            set { this.numberOfValueArgs = value; }
        }
    }
}