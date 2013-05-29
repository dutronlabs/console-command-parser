using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheObtuseAngle.ConsoleUtilities.Arguments;
using TheObtuseAngle.ConsoleUtilities.Composition;

namespace TheObtuseAngle.ConsoleUtilities.Commands
{
    /// <summary>
    /// An abstract <see cref="ICommand"/> implementation to use when building commands for an application.
    /// </summary>
    /// <remarks>
    /// In order to use Argument Composition commands must be derived from this base class.
    /// </remarks>
    public abstract class CommandBase : CommandTemplate, ICommand
    {
        private const BindingFlags bindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private const int defaultOrdinal = int.MaxValue - 1;
        private MethodInfo writeUsageMethodInfo;
        private Object reflectionParserInstance;
        private CommandParser<CommandBase> commandParser;
        private int ordinal;

        /// <summary>
        /// Constructs a new <see cref="CommandBase"/> instance with the given name and description.
        /// </summary>
        /// <param name="name">The name of the command. This is what the user would type to invoke the command.</param>
        /// <param name="description">A description of what the command does.</param>
        protected CommandBase(string name, string description)
            : this(name, string.Empty, description, defaultOrdinal, null)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CommandBase"/> instance with the given name, description, and ordinal.
        /// </summary>
        /// <param name="name">The name of the command. This is what the user would type to invoke the command.</param>
        /// <param name="description">A description of what the command does.</param>
        /// <param name="ordinal">The order the command should be listed in the on-screen usage help.</param>
        protected CommandBase(string name, string description, int ordinal)
            : this(name, string.Empty, description, ordinal, null)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CommandBase"/> instance with the given name, title, and description.
        /// </summary>
        /// <param name="name">The name of the command. This is what the user would type to invoke the command.</param>
        /// <param name="title">The short title of the command to use when writing on-screen help with the <see cref="WriteUsageMode.CommandNameAndTitle"/> mode.</param>
        /// <param name="description">A description of what the command does.</param>
        protected CommandBase(string name, string title, string description)
            : this(name, title, description, defaultOrdinal, null)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CommandBase"/> instance with the given name, title, description, and ordinal.
        /// </summary>
        /// <param name="name">The name of the command. This is what the user would type to invoke the command.</param>
        /// <param name="title">The short title of the command to use when writing on-screen help with the <see cref="WriteUsageMode.CommandNameAndTitle"/> mode.</param>
        /// <param name="description">A description of what the command does.</param>
        /// <param name="ordinal">The order the command should be listed in the on-screen usage help.</param>
        protected CommandBase(string name, string title, string description, int ordinal)
            : this(name, title, description, ordinal, null)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CommandBase"/> instance with the given name, description, and arguments.
        /// </summary>
        /// <param name="name">The name of the command. This is what the user would type to invoke the command.</param>
        /// <param name="description">A description of what the command does.</param>
        /// <param name="arguments">The collection of <see cref="IArgument"/> for this command.</param>
        protected CommandBase(string name, string description, IEnumerable<IArgument> arguments)
            : this(name, string.Empty, description, defaultOrdinal, arguments)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CommandBase"/> instance with the given name, description, ordinal, and arguments.
        /// </summary>
        /// <param name="name">The name of the command. This is what the user would type to invoke the command.</param>
        /// <param name="description">A description of what the command does.</param>
        /// <param name="ordinal">The order the command should be listed in the on-screen usage help.</param>
        /// <param name="arguments">The collection of <see cref="IArgument"/> for this command.</param>
        protected CommandBase(string name, string description, int ordinal, IEnumerable<IArgument> arguments)
            : this(name, string.Empty, description, ordinal, arguments)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CommandBase"/> instance with the given name, title, description, and arguments.
        /// </summary>
        /// <param name="name">The name of the command. This is what the user would type to invoke the command.</param>
        /// <param name="title">The short title of the command to use when writing on-screen help with the <see cref="WriteUsageMode.CommandNameAndTitle"/> mode.</param>
        /// <param name="description">A description of what the command does.</param>
        /// <param name="arguments">The collection of <see cref="IArgument"/> for this command.</param>
        protected CommandBase(string name, string title, string description, IEnumerable<IArgument> arguments)
            : this(name, title, description, defaultOrdinal, arguments)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CommandBase"/> instance with the given name, title, description, ordinal, and arguments.
        /// </summary>
        /// <param name="name">The name of the command. This is what the user would type to invoke the command.</param>
        /// <param name="title">The short title of the command to use when writing on-screen help with the <see cref="WriteUsageMode.CommandNameAndTitle"/> mode.</param>
        /// <param name="description">A description of what the command does.</param>
        /// <param name="ordinal">The order the command should be listed in the on-screen usage help.</param>
        /// <param name="arguments">The collection of <see cref="IArgument"/> for this command.</param>
        protected CommandBase(string name, string title, string description, int ordinal, IEnumerable<IArgument> arguments)
            : base(name, title, description)
        {
            this.ordinal = ordinal;
            this.Arguments = new List<IArgument>();

            if (arguments != null)
            {
                this.Arguments.AddRange(arguments);
            }
        }

        /// <summary>
        /// The order the command should be listed in the on-screen usage help.
        /// </summary>
        public virtual int Ordinal
        {
            get { return this.ordinal; }
            set { this.ordinal = value; }
        }

        /// <summary>
        /// The collection of <see cref="IArgument"/> for this command.
        /// </summary>
        public List<IArgument> Arguments { get; private set; }

        /// <summary>
        /// When implemented in a derived class, executes the command and returns the result.
        /// </summary>
        /// <returns>True when the command executes successfully, false otherwise.</returns>
        public abstract bool Execute();

        /// <summary>
        /// Writes the on-screen usage help for this command.
        /// </summary>
        /// <remarks>
        /// When invoked after this command has been run through a <see cref="CommandParser{TCommand}"/> instance it will use the <see cref="ParseOptions"/> 
        /// used by that <see cref="CommandParser{TCommand}"/> instance. Otherwise it will write the on-screen help using the default <see cref="ParseOptions"/>.
        /// </remarks>
        public virtual void WriteUsage()
        {
            if (commandParser != null)
            {
                commandParser.WriteUsage(this);
            }
            else
            {
                WriteUsageMethodInfo.Invoke(ReflectionParserInstance, new object[] { this });
            }
        }

        /// <summary>
        /// Called on each command passed into a <see cref="CommandParser{TCommand}"/> instance before the raw console arguments are parsed into commands.
        /// </summary>
        public virtual void OnBeforeParse()
        {
        }

        /// <summary>
        /// Orders the collection of <see cref="ArgumentCompositionInfo"/> objects to determine the order composed arguments will be written using the on-screen help.
        /// </summary>
        /// <param name="argumentCompositionInfo">The collection of <see cref="ArgumentCompositionInfo"/> objects to order.</param>
        /// <returns>The ordered collection of <see cref="ArgumentCompositionInfo"/> objects.</returns>
        protected virtual IEnumerable<ArgumentCompositionInfo> OrderComposedArguments(IEnumerable<ArgumentCompositionInfo> argumentCompositionInfo)
        {
            return argumentCompositionInfo.OrderByDescending(ci => ci.Argument.IsRequired).ThenBy(ci => ci.InheritanceLevel);
        }

        internal void InjectWriteUsageMethod(ParseOptions parseOptions)
        {
            commandParser = new CommandParser<CommandBase>(parseOptions);
        }

        internal void ComposeArguments()
        {
            int rank = 0;
            var typeRankMap = new Dictionary<Type, int>();
            var argumentCompositionInfo = new List<ArgumentCompositionInfo>();

            for (var type = GetType(); type != null; type = type.BaseType)
            {
                typeRankMap.Add(type, ++rank);
            }

            foreach (var member in GetType().GetMembers(bindingFlags).Where(member => member.DeclaringType != null && IsValidMember(member)))
            {
                if (member.HasAttribute<ExportArgumentAttribute>())
                {
                    var argument = member.GetArgument(this);

                    if (argument == null)
                    {
                        continue;
                    }

                    argumentCompositionInfo.Add(new ArgumentCompositionInfo(typeRankMap[member.DeclaringType], member.DeclaringType, argument));
                }
                else if (member.HasAttribute<ExportManyArgumentsAttribute>())
                {
                    var localMemberInfo = member;
                    argumentCompositionInfo.AddRange(member.GetManyArguments(this).Select(argument => new ArgumentCompositionInfo(typeRankMap[localMemberInfo.DeclaringType], localMemberInfo.DeclaringType, argument)));
                }
            }

            Arguments = OrderComposedArguments(argumentCompositionInfo).Select(ci => ci.Argument).ToList();
        }

        private Object ReflectionParserInstance
        {
            get
            {
                if (reflectionParserInstance == null)
                {
                    var parserType = typeof(CommandParser<>).MakeGenericType(GetType());
                    reflectionParserInstance = Activator.CreateInstance(parserType);
                }

                return reflectionParserInstance;
            }
        }

        private MethodInfo WriteUsageMethodInfo
        {
            get { return writeUsageMethodInfo ?? (writeUsageMethodInfo = ReflectionParserInstance.GetType().GetMethod("WriteUsage", new[] { GetType() })); }
        }

        private static bool IsValidMember(MemberInfo member)
        {
            return member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Method;
        }
    }
}
