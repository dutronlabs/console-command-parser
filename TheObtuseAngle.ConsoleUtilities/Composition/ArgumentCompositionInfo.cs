using System;
using TheObtuseAngle.ConsoleUtilities.Arguments;
using TheObtuseAngle.ConsoleUtilities.Commands;

namespace TheObtuseAngle.ConsoleUtilities.Composition
{
    /// <summary>
    /// An object that holds information about a composed <see cref="IArgument"/>.
    /// </summary>
    public sealed class ArgumentCompositionInfo
    {
        internal ArgumentCompositionInfo(int rank, Type declaringType, IArgument argument)
        {
            InheritanceLevel = rank;
            DeclaringType = declaringType;
            Argument = argument;
        }

        /// <summary>
        /// The 1-based inheritance level of the declaring <see cref="ICommand"/> instance where 1 is the most-derived type.
        /// </summary>
        public int InheritanceLevel { get; private set; }

        /// <summary>
        /// The <see cref="Type"/> of the <see cref="ICommand"/> instance that contains this argument.
        /// </summary>
        public Type DeclaringType { get; set; }

        /// <summary>
        /// The <see cref="IArgument"/> instance that has been composed.
        /// </summary>
        public IArgument Argument { get; private set; }
    }
}