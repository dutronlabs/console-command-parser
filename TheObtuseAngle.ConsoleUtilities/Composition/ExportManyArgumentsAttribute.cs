using System;
using TheObtuseAngle.ConsoleUtilities.Arguments;

namespace TheObtuseAngle.ConsoleUtilities.Composition
{
    /// <summary>
    /// Specifies that the field, property, or method is or returns a collection of <see cref="IArgument"/> instances.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class ExportManyArgumentsAttribute : Attribute
    {
    }
}