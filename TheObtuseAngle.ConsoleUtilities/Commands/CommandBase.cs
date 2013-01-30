using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheObtuseAngle.ConsoleUtilities.Arguments;
using TheObtuseAngle.ConsoleUtilities.Composition;

namespace TheObtuseAngle.ConsoleUtilities.Commands
{
    public abstract class CommandBase : CommandTemplate, ICommand
    {
        private const BindingFlags bindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private const int defaultOrdinal = int.MaxValue - 1;
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
            this.Arguments = new List<IArgument>();

            if (arguments != null)
            {
                this.Arguments.AddRange(arguments);
            }
        }

        public virtual int Ordinal
        {
            get { return this.ordinal; }
            set { this.ordinal = value; }
        }

        public List<IArgument> Arguments { get; private set; }

        public abstract bool Execute();

        public virtual void WriteUsage()
        {
            WriteUsageMethod.Invoke(ParserInstance, new object[] { this });
        }

        public virtual void OnBeforeParse()
        {
        }

        protected virtual IEnumerable<ArgumentCompositionInfo> OrderComposedArguments(IEnumerable<ArgumentCompositionInfo> argumentCompositionInfo)
        {
            return argumentCompositionInfo.OrderByDescending(ci => ci.Argument.IsRequired).ThenBy(ci => ci.InheritanceLevel);
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

        private static bool IsValidMember(MemberInfo member)
        {
            return member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Method;
        }
    }
}
