namespace TheObtuseAngle.ConsoleUtilities
{
    public class ArgumentTemplate
    {
        private string name;
        private string[] aliases;
        private string description;
        private bool isRequired;
        private bool requiresValue;

        public ArgumentTemplate()
        {
        }

        public ArgumentTemplate(string name, string[] aliases, string description, bool requiresValue, bool isRequired)
        {
            this.name = name;
            this.aliases = aliases;
            this.description = description;
            this.requiresValue = requiresValue;
            this.isRequired = isRequired;
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
    }
}