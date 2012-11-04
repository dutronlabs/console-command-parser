using System.Collections.Generic;

namespace TheObtuseAngle.ConsoleUtilities
{
    public abstract class CommandBase : ICommand
    {
        private readonly string name;
        private readonly string description;
        private readonly IList<IArgument> arguments;

        protected CommandBase(string name, string description)
            : this(name, description, null)
        {
        }

        protected CommandBase(string name, string description, IEnumerable<IArgument> arguments)
        {
            this.name = name;
            this.description = description;
            this.arguments = new List<IArgument>();

            if (arguments != null)
            {
                this.arguments.AddRange(arguments);
            }
        }

        public virtual string Name
        {
            get { return name; }
        }

        public virtual string Description
        {
            get { return description; }
        }

        public virtual IList<IArgument> Arguments
        {
            get { return arguments; }
        }

        public abstract bool Execute();
    }
}
