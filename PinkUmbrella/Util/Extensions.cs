using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Tides.Models;
using Tides.Util;
using Microsoft.Extensions.DependencyInjection;
using Estuary.Services;
using Estuary.Services.BoxProviders;
using Estuary.Pipes.Read;
using PinkUmbrella.Services.ActivityStream.Read;
using PinkUmbrella.Services.ActivityStream.Write.Outbox;
using PinkUmbrella.Services.ActivityStream.Write.Inbox;

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

            // Tries to find a DescriptionAttribute for a potential friendly name
            // for the enum
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
            // If we have no description attribute, just return the ToString of the enum
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

        public static string GetPropertyDescription<T>(this T root, string propertyName)
        {
            Type type = typeof(T);

            //Tries to find a DescriptionAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type?.GetMember(propertyName);
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

        public static string GetPropertyDisplayName<T>(this T root, string propertyName)
        {
            Type type = typeof(T);

            //Tries to find a DisplayNameAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type?.GetMember(propertyName);
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DisplayNameAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    //Pull out the display name value
                    return ((DisplayNameAttribute)attrs[0]).DisplayName;
                }
            }
            //If we have no description attribute, just return the ToString of the enum
            return propertyName;
        }

        public static string GetPropertyPlaceHolder<T>(this T root, string propertyName)
        {
            Type type = typeof(T);

            //Tries to find a DescriptionAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type?.GetMember(propertyName);
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(InputPlaceHolderAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    //Pull out the description value
                    return ((InputPlaceHolderAttribute)attrs[0]).Text;
                }
            }
            //If we have no description attribute, just return an empty string
            return string.Empty;
        }

        public static string GetDebugValue<T>(this T root, string propertyName)
        {
            Type type = typeof(T);

            //Tries to find a DebugValueAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type?.GetMember(propertyName);
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DebugValueAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    //Pull out the debug value
                    return ((DebugValueAttribute)attrs[0]).Value;
                }
            }
            //If we have no debug attribute, just return an empty string
            return string.Empty;
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

        // https://stackoverflow.com/a/46013305/11765486
        public static List<T> RawSqlQuery<T>(this DbContext context, string query, Func<DbDataReader, T> map)
        {
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                context.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    var entities = new List<T>();

                    while (result.Read())
                    {
                        entities.Add(map(result));
                    }

                    return entities;
                }
            }
        }










        public static void UseActivityStreamBoxProviders(this IServiceCollection services)
        {
            services.AddScoped<IActivityStreamBoxProvider, ActorActivityStreamBoxProvider>();
            services.AddScoped<IActivityStreamBoxProvider, PersonActivityStreamBoxProvider>();
            services.AddScoped<IActivityStreamBoxProvider, SharedActivityStreamBoxProvider>();
        }

        public static void UseActivityStreamReadPipes(this IServiceCollection services)
        {
            services.AddScoped<IActivityStreamPipe, FilterObjTypeWhenReading>();
            services.AddScoped<IActivityStreamPipe, BindSqlReferencesToActivityStreamWhenReading>();
            services.AddScoped<IActivityStreamPipe, CanViewActivityStreamWhenReading>();
        }

        public static void UseActivityStreamWritePipes(this IServiceCollection services)
        {
            services.AddScoped<IActivityStreamPipe, OutboxActivityActionHandlerWhenWritingPipe>();
            services.AddScoped<IActivityStreamPipe, OutboxActivityActionObjectHandlerWhenWritingPipe>();

            services.AddScoped<IActivityStreamPipe, InboxActivityActionHandlerWhenWritingPipe>();
            services.AddScoped<IActivityStreamPipe, InboxActivityActionObjectHandlerWhenWritingPipe>();

            // services.AddScoped<IActivityStreamPipe, OutboxActivityActionHandlerWhenWritingPipe>();
            // services.AddScoped<IActivityStreamPipe, OutboxActivityActionObjectHandlerWhenWritingPipe>();

            // services.AddScoped<IActivityStreamPipe, InboxActivityActionHandlerWhenWritingPipe>();
            // services.AddScoped<IActivityStreamPipe, InboxActivityActionObjectHandlerWhenWritingPipe>();
        }
    }
}