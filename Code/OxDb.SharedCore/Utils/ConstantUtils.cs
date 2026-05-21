using OxDb.SharedCore.Names.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OxDb.SharedCore.Utils
{

    public static class ConstantUtils
    {

        public static List<KeyValue> GetStringConstants(Type t)
        {

            List<KeyValue> retval = new List<KeyValue>();

            List<FieldInfo> fields = t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly) // Ensuring only constants
            .Where(f => f.FieldType == typeof(string)).ToList(); // Filtering for numeric types

            foreach (FieldInfo field in fields)
            {
                try
                {
                    retval.Add(new KeyValue()
                    {

                        Key = field.Name,
                        Val = (string)field.GetValue(null)
                    });
                }
                catch (Exception ex)
                {
                }
            }

            return retval;
        }

        public static List<NameValue> GetNumericConstants(Type t)
        {

            List<NameValue> retval = new List<NameValue>();

            List<FieldInfo> fields = t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly) // Ensuring only constants
            .Where(f => f.FieldType.IsPrimitive).ToList(); // Filtering for numeric types

            foreach (FieldInfo field in fields)
            {
                try
                {
                    retval.Add(new NameValue()
                    {
                        IdKey = (long)field.GetValue(null),
                        Name = field.Name,
                    });
                }
                catch (Exception ex)
                {
                    try
                    {
                        retval.Add(new NameValue()
                        {
                            IdKey = (int)field.GetValue(null),
                            Name = field.Name,
                        });
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine(ex2.ToString() + " " + ex2.StackTrace + " Parent: " + ex.Message);
                    }
                }
            }

            return retval;
        }
    }
}
