using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace typegen
{
    public static class Extensions
    {
        public static bool IsStructOrClass(this Type t)
        {
            return t.IsClass || ((t.IsValueType && !t.IsEnum) && !t.IsPrimitive);
        }

        public static bool EnumHasCount(this Type t)
        {
            var name = t.GetEnumNames().Last().ToLowerInvariant();
            return name.EndsWith("size") || name.EndsWith("count");
        }

        public static int EnumIterations(this Type t)
        {
            return t.GetEnumNames().Length - (t.EnumHasCount() ? 1 : 0);
        }
    }
}
