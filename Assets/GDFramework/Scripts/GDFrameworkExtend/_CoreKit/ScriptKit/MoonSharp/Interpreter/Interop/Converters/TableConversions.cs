﻿using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Compatibility;

namespace MoonSharp.Interpreter.Interop.Converters
{
    internal static class TableConversions
    {
        /// <summary>
        /// Converts an IList to a Lua table.
        /// </summary>
        internal static Table ConvertIListToTable(Script script, System.Collections.IList list)
        {
            var t = new Table(script);
            for (var i = 0; i < list.Count; i++) t[i + 1] = ClrToScriptConversions.ObjectToDynValue(script, list[i]);
            return t;
        }

        /// <summary>
        /// Converts an IDictionary to a Lua table.
        /// </summary>
        internal static Table ConvertIDictionaryToTable(Script script, System.Collections.IDictionary dict)
        {
            var t = new Table(script);

            foreach (System.Collections.DictionaryEntry kvp in dict)
            {
                var key = ClrToScriptConversions.ObjectToDynValue(script, kvp.Key);
                var val = ClrToScriptConversions.ObjectToDynValue(script, kvp.Value);
                t.Set(key, val);
            }

            return t;
        }

        /// <summary>
        /// Determines whether the specified table can be converted to the specified type
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="t">The type.</param>
        /// <returns></returns>
        internal static bool CanConvertTableToType(Table table, Type t)
        {
            if (Framework.Do.IsAssignableFrom(t, typeof(Dictionary<object, object>)))
                return true;
            else if (Framework.Do.IsAssignableFrom(t, typeof(Dictionary<DynValue, DynValue>)))
                return true;
            else if (Framework.Do.IsAssignableFrom(t, typeof(List<object>)))
                return true;
            else if (Framework.Do.IsAssignableFrom(t, typeof(List<DynValue>)))
                return true;
            else if (Framework.Do.IsAssignableFrom(t, typeof(object[])))
                return true;
            else if (Framework.Do.IsAssignableFrom(t, typeof(DynValue[])))
                return true;

            if (Framework.Do.IsGenericType(t))
            {
                var generic = t.GetGenericTypeDefinition();

                if (generic == typeof(List<>)
                    || generic == typeof(IList<>)
                    || generic == typeof(ICollection<>)
                    || generic == typeof(IEnumerable<>))
                    return true;
                else if (generic == typeof(Dictionary<,>)
                         || generic == typeof(IDictionary<,>))
                    return true;
            }

            if (t.IsArray && t.GetArrayRank() == 1)
                return true;

            return false;
        }


        /// <summary>
        /// Converts a table to a CLR object of a given type
        /// </summary>
        internal static object ConvertTableToType(Table table, Type t)
        {
            if (Framework.Do.IsAssignableFrom(t, typeof(Dictionary<object, object>)))
                return TableToDictionary<object, object>(table, v => v.ToObject(), v => v.ToObject());
            else if (Framework.Do.IsAssignableFrom(t, typeof(Dictionary<DynValue, DynValue>)))
                return TableToDictionary<DynValue, DynValue>(table, v => v, v => v);
            else if (Framework.Do.IsAssignableFrom(t, typeof(List<object>)))
                return TableToList<object>(table, v => v.ToObject());
            else if (Framework.Do.IsAssignableFrom(t, typeof(List<DynValue>)))
                return TableToList<DynValue>(table, v => v);
            else if (Framework.Do.IsAssignableFrom(t, typeof(object[])))
                return TableToList<object>(table, v => v.ToObject()).ToArray();
            else if (Framework.Do.IsAssignableFrom(t, typeof(DynValue[])))
                return TableToList<DynValue>(table, v => v).ToArray();

            if (Framework.Do.IsGenericType(t))
            {
                var generic = t.GetGenericTypeDefinition();

                if (generic == typeof(List<>)
                    || generic == typeof(IList<>)
                    || generic == typeof(ICollection<>)
                    || generic == typeof(IEnumerable<>))
                    return ConvertTableToListOfGenericType(t, Framework.Do.GetGenericArguments(t)[0], table);
                else if (generic == typeof(Dictionary<,>)
                         || generic == typeof(IDictionary<,>))
                    return ConvertTableToDictionaryOfGenericType(t, Framework.Do.GetGenericArguments(t)[0],
                        Framework.Do.GetGenericArguments(t)[1], table);
            }

            if (t.IsArray && t.GetArrayRank() == 1)
                return ConvertTableToArrayOfGenericType(t, t.GetElementType(), table);

            return null;
        }


        /// <summary>
        /// Converts a table to a <see cref="Dictionary{K,V}"/>
        /// </summary>
        internal static object ConvertTableToDictionaryOfGenericType(Type dictionaryType, Type keyType, Type valueType,
            Table table)
        {
            if (dictionaryType.GetGenericTypeDefinition() != typeof(Dictionary<,>))
            {
                dictionaryType = typeof(Dictionary<,>);
                dictionaryType = dictionaryType.MakeGenericType(keyType, valueType);
            }

            var dic = (System.Collections.IDictionary)Activator.CreateInstance(dictionaryType);

            foreach (var kvp in table.Pairs)
            {
                var key = ScriptToClrConversions.DynValueToObjectOfType(kvp.Key, keyType, null, false);
                var val = ScriptToClrConversions.DynValueToObjectOfType(kvp.Value, valueType, null, false);

                dic.Add(key, val);
            }

            return dic;
        }

        /// <summary>
        /// Converts a table to a T[]
        /// </summary>
        internal static object ConvertTableToArrayOfGenericType(Type arrayType, Type itemType, Table table)
        {
            var lst = new List<object>();

            for (int i = 1, l = table.Length; i <= l; i++)
            {
                var v = table.Get(i);
                var o = ScriptToClrConversions.DynValueToObjectOfType(v, itemType, null, false);
                lst.Add(o);
            }

            var array = (System.Collections.IList)Activator.CreateInstance(arrayType, new object[] { lst.Count });

            for (var i = 0; i < lst.Count; i++)
                array[i] = lst[i];

            return array;
        }


        /// <summary>
        /// Converts a table to a <see cref="List{T}"/>
        /// </summary>
        internal static object ConvertTableToListOfGenericType(Type listType, Type itemType, Table table)
        {
            if (listType.GetGenericTypeDefinition() != typeof(List<>))
            {
                listType = typeof(List<>);
                listType = listType.MakeGenericType(itemType);
            }

            var lst = (System.Collections.IList)Activator.CreateInstance(listType);

            for (int i = 1, l = table.Length; i <= l; i++)
            {
                var v = table.Get(i);
                var o = ScriptToClrConversions.DynValueToObjectOfType(v, itemType, null, false);
                lst.Add(o);
            }

            return lst;
        }

        /// <summary>
        /// Converts a table to a <see cref="List{T}"/>, known in advance
        /// </summary>
        internal static List<T> TableToList<T>(Table table, Func<DynValue, T> converter)
        {
            var lst = new List<T>();

            for (int i = 1, l = table.Length; i <= l; i++)
            {
                var v = table.Get(i);
                var o = converter(v);
                lst.Add(o);
            }

            return lst;
        }

        /// <summary>
        /// Converts a table to a Dictionary, known in advance
        /// </summary>
        internal static Dictionary<TK, TV> TableToDictionary<TK, TV>(Table table, Func<DynValue, TK> keyconverter,
            Func<DynValue, TV> valconverter)
        {
            var dict = new Dictionary<TK, TV>();

            foreach (var kvp in table.Pairs)
            {
                var key = keyconverter(kvp.Key);
                var val = valconverter(kvp.Value);

                dict.Add(key, val);
            }

            return dict;
        }
    }
}