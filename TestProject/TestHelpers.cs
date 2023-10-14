using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TestProject
{
    public static class TestHelpers
    {
        /// <summary>
        /// Resturns a list public static T field values from type <paramref name="type"/>
        /// </summary>
        /// <typeparam name="T">Type of field</typeparam>
        /// <param name="type">Type to scan for public static fields</param>
        public static List<(string name, T? value)> GetAllPublicStaticFieldValuesInType<T>(Type type)
            => type
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.FieldType == typeof(T))
                .Select
                (
                    field => (name: field.Name, value: (T?)field.GetValue(null))
                ).ToList();
    }
}
