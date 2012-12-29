using System;
using System.Collections.Generic;
using System.Reflection;

namespace TheObtuseAngle.ConsoleUtilities
{
    public abstract class CommandBase : CommandTemplate, ICommand
    {
        private readonly IList<IArgument> arguments;
        private MethodInfo writeUsageMethod;
        private Object parserInstance;

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

        public virtual void WriteUsage()
        {
            WriteUsageMethod.Invoke(ParserInstance, new[] { this });
        }

        private Object ParserInstance
        {
            get
            {
                if (parserInstance == null)
                {
                    var parserType = typeof(CommandParser<>).MakeGenericType(GetType());
                    parserInstance = Activator.CreateInstance(parserType);
                }

                return parserInstance;
            }
        }

        private MethodInfo WriteUsageMethod
        {
            get { return writeUsageMethod ?? (writeUsageMethod = ParserInstance.GetType().GetMethod("WriteUsage", new[] { GetType() })); }
        }
    }
}
