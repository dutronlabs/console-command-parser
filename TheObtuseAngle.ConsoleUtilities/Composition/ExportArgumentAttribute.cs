using System;
using TheObtuseAngle.ConsoleUtilities.Arguments;

namespace TheObtuseAngle.ConsoleUtilities.Composition
{
    /// <summary>
    /// Specifies that the field, property, or method is or returns an <see cref="IArgument"/> instance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class ExportArgumentAttribute : Attribute
    {
    }
}
