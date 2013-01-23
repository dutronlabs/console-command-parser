using System;
using System.Collections.Generic;
using System.Reflection;
using TheObtuseAngle.ConsoleUtilities.Arguments;

namespace TheObtuseAngle.ConsoleUtilities.Commands
{
    public abstract class CommandBase : CommandTemplate, ICommand
    {
        private const int defaultOrdinal = int.MaxValue - 1;
        private readonly IList<IArgument> arguments;
        private MethodInfo writeUsageMethod;
        private Object parserInstance;
        private int ordinal;

        protected CommandBase(string name, string description)
            : this(name, string.Empty, description, defaultOrdinal, null)
        {
        }

        protected CommandBase(string name, string description, int ordinal)
            : this(name, string.Empty, description, ordinal, null)
        {
        }

        protected CommandBase(string name, string title, string description)
            : this(name, title, description, defaultOrdinal, null)
        {
        }

        protected CommandBase(string name, string title, string description, int ordinal)
            : this(name, title, description, ordinal, null)
        {
        }

        protected CommandBase(string name, string description, IEnumerable<IArgument> arguments)
            : this(name, string.Empty, description, defaultOrdinal, arguments)
        {
        }

        protected CommandBase(string name, string description, int ordinal, IEnumerable<IArgument> arguments)
            : this(name, string.Empty, description, ordinal, arguments)
        {
        }

        protected CommandBase(string name, string title, string description, IEnumerable<IArgument> arguments)
            : this(name, title, description, defaultOrdinal, arguments)
        {
        }

        protected CommandBase(string name, string title, string description, int ordinal, IEnumerable<IArgument> arguments)
            : base(name, title, description)
        {
            this.ordinal = ordinal;
            this.arguments = new List<IArgument>();

            if (arguments != null)
            {
                this.arguments.AddRange(arguments);
            }
        }

        public virtual int Ordinal
        {
            get { return this.ordinal; }
            set { this.ordinal = value; }
        }

        public virtual IList<IArgument> Arguments
        {
            get { return arguments; }
        }

        public abstract bool Execute();

        public virtual void WriteUsage()
        {
            WriteUsageMethod.Invoke(ParserInstance, new object[] { this });
        }

        public virtual void OnBeforeParse()
        {
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
