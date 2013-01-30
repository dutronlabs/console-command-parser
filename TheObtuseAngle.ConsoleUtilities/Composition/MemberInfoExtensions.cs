using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheObtuseAngle.ConsoleUtilities.Arguments;

namespace TheObtuseAngle.ConsoleUtilities.Composition
{
    internal static class MemberInfoExtensions
    {
        public static bool HasAttribute<TAttribute>(this MemberInfo memberInfo)
            where TAttribute : Attribute
        {
            return HasAttribute(memberInfo, typeof(TAttribute));
        }

        public static bool HasAttribute(this MemberInfo memberInfo, Type attributeType, bool inherit = false)
        {
            return memberInfo.GetCustomAttributes(attributeType, inherit).Any();
        }

        public static IArgument GetArgument(this MemberInfo memberInfo, object instance)
        {
            return GetValue(memberInfo, instance) as IArgument;
        }

        public static IEnumerable<IArgument> GetManyArguments(this MemberInfo memberInfo, object instance)
        {
            var enumerable = GetValue(memberInfo, instance) as IEnumerable;
            return enumerable == null ? Enumerable.Empty<IArgument>() : enumerable.OfType<IArgument>();
        }

        public static object GetValue(this MemberInfo memberInfo, object instance)
        {
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(instance);
            }

            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(instance, null);
            }

            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null)
            {
                return methodInfo.Invoke(instance, null);
            }

            return null;
        }
    }
}