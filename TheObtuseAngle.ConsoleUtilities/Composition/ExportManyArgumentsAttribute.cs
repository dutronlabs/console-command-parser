using System;

namespace TheObtuseAngle.ConsoleUtilities.Composition
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class ExportManyArgumentsAttribute : Attribute
    {
    }
}