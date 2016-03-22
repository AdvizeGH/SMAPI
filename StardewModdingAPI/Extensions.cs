﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewModdingAPI
{
    public static class Extensions
    {
        public static Random Random = new Random();

        public static bool IsKeyDown(this Keys key)
        {
            return Keyboard.GetState().IsKeyDown(key);
        }

        public static Color RandomColour()
        {
            return new Color(Random.Next(0, 255), Random.Next(0, 255), Random.Next(0, 255));
        }

        public static string ToSingular(this IEnumerable<Object> enumerable, string split = ", ")
        {
            string result = string.Join(split, enumerable);
            return result;
        }

        public static bool IsInt32(this object o)
        {
            int i;
            return Int32.TryParse(o.ToString(), out i);
        }

        public static Int32 AsInt32(this object o)
        {
            return Int32.Parse(o.ToString());
        }

        public static bool IsBool(this object o)
        {
            bool b;
            return Boolean.TryParse(o.ToString(), out b);
        }

        public static bool AsBool(this object o)
        {
            return Boolean.Parse(o.ToString());
        }
        
        public static int GetHash(this IEnumerable enumerable)
        {
            int hash = 0;
            foreach (var v in enumerable)
            {
                hash ^= v.GetHashCode();
            }
            return hash;
        }

        public static T Cast<T>(this object o) where T : class
        {
            return o as T;
        }

        public static FieldInfo[] GetPrivateFields(this object o)
        {
            return o.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static FieldInfo GetBaseFieldInfo(this Type t, string name)
        {
            return t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static T GetBaseFieldValue<T>(this Type t, object o, string name) where T : class
        {
            return t.GetBaseFieldInfo(name).GetValue(o) as T;
        }

        /*
        public static T GetBaseFieldValue<T>(this object o, string name) where T : class
        {
            return o.GetType().GetBaseFieldInfo(name).GetValue(o) as T;
        }*/

        public static object GetBaseFieldValue(this object o, string name)
        {
            return o.GetType().GetBaseFieldInfo(name).GetValue(o);
        }

        public static void SetBaseFieldValue (this object o, string name, object newValue)
        {
            o.GetType().GetBaseFieldInfo(name).SetValue(o, newValue);
        }
    }
}