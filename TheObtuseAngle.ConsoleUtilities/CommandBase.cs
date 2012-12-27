using System.Collections.Generic;

namespace TheObtuseAngle.ConsoleUtilities
{
    public abstract class CommandBase : CommandTemplate, ICommand
    {
        private readonly IList<IArgument> arguments;

        protected CommandBase(string name, string description)
            : this(name, string.Empty, description, null)
        {
        }

        protected CommandBase(string name, string title, string description)
            : this(name, title, description, null)
        {
        }

        protected CommandBase(string name, string description, IEnumerable<IArgument> arguments)
            : this(name, string.Empty, description, arguments)
        {
        }

        protected CommandBase(string name, string title, string description, IEnumerable<IArgument> arguments)
            : base(name, title, description)
        {
            this.arguments = new List<IArgument>();

            if (arguments != null)
            {
                this.arguments.AddRange(arguments);
            }
        }

        public virtual IList<IArgument> Arguments
        {
            get { return arguments; }
        }

        public abstract bool Execute();
    }
}
