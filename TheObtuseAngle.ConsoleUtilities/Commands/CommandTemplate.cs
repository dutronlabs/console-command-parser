namespace TheObtuseAngle.ConsoleUtilities.Commands
{
    /// <summary>
    /// Defines a minimum command definition.
    /// </summary>
    public class CommandTemplate
    {
        private string name;
        private string title;
        private string description;

        /// <summary>
        /// Constructs a new <see cref="CommandTemplate"/> instance.
        /// </summary>
        public CommandTemplate()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CommandTemplate"/> instance using the given name, title, and description.
        /// </summary>
        /// <param name="name">The name of the command. This is what the user would type to invoke the command.</param>
        /// <param name="title">The short title of the command to use when writing on-screen help with the <see cref="WriteUsageMode.CommandNameAndTitle"/> mode.</param>
        /// <param name="description">A description of what the command does.</param>
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

        /// <summary>
        /// The name of the command. This is what the user would type to invoke the command.
        /// </summary>
        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// The short title of the command to use when writing on-screen help with the <see cref="WriteUsageMode.CommandNameAndTitle"/> mode.
        /// </summary>
        public virtual string Title
        {
            get { return title; }
            set { title = value; }
        }

        /// <summary>
        /// A description of what the command does.
        /// </summary>
        public virtual string Description
        {
            get { return description; }
            set { description = value; }
        }
    }
}