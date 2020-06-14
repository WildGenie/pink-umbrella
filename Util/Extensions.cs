using System;
using System.ComponentModel;
using System.Reflection;
using PinkUmbrella.Models;

namespace PinkUmbrella.Util
{
    public static class Extensions
    {
        // https://stackoverflow.com/a/479417/11765486
        public static string GetDescription<T>(this T enumerationValue)
        {
            Type type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
            }

            //Tries to find a DescriptionAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    //Pull out the description value
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            //If we have no description attribute, just return the ToString of the enum
            return enumerationValue.ToString();
        }

        public static string GetDisplayName<T>(this T enumerationValue)
        {
            Type type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
            }

            //Tries to find a DisplayName for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(NameAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    //Pull out the value
                    return ((NameAttribute)attrs[0]).Name;
                }
            }
            //If we have no DisplayName attribute, just return the ToString of the enum
            return enumerationValue.ToString();
        }

        public static string GetPropertyDescription(this object root, string propertyName)
        {
            Type type = root.GetType();

            //Tries to find a DescriptionAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type.GetMember(propertyName);
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    //Pull out the description value
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            //If we have no description attribute, just return the ToString of the enum
            return propertyName;
        }

        public static Visibility Min(this Visibility a, Visibility b)
        {
            switch (a)
            {
                case Visibility.HIDDEN: return Visibility.HIDDEN;
                case Visibility.VISIBLE_TO_FOLLOWERS:
                {
                    switch (b)
                    {
                        case Visibility.HIDDEN: return b;
                    }
                } break;
                case Visibility.VISIBLE_TO_REGISTERED:
                {
                    switch (b)
                    {
                        case Visibility.HIDDEN: return b;
                        case Visibility.VISIBLE_TO_FOLLOWERS: return b;
                    }
                } break;
                case Visibility.VISIBLE_TO_WORLD:
                {
                    switch (b)
                    {
                        case Visibility.HIDDEN: return b;
                        case Visibility.VISIBLE_TO_FOLLOWERS: return b;
                        case Visibility.VISIBLE_TO_REGISTERED: return b;
                    }
                } break;
            }
            return a;
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            if (string.IsNullOrEmpty(toCheck) || string.IsNullOrEmpty(source))
                return true;

            return source.IndexOf(toCheck, comp) >= 0;
        } 
    }
}