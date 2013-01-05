namespace TheObtuseAngle.ConsoleUtilities.Commands
{
    public class CommandTemplate
    {
        private string name;
        private string title;
        private string description;

        public CommandTemplate()
        {
        }

        public CommandTemplate(string name, string title, string description)
        {
            this.name = name;
            this.title = title;
            this.description = description;
        }

        internal CommandTemplate(ICommand command)
        {
            this.name = command.Name;
            this.title = command.Title;
            this.description = command.Description;
        }

        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }

        public virtual string Title
        {
            get { return title; }
            set { title = value; }
        }

        public virtual string Description
        {
            get { return description; }
            set { description = value; }
        }
    }
}