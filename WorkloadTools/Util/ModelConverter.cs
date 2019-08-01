﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using WorkloadTools;

namespace WorkloadTools.Util
{
    public class ModelConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                List<Type> result = new List<Type>();
                Assembly currentAssembly = Assembly.GetExecutingAssembly();
                string nameSpace = "WorkloadTools";
                Type[] types = currentAssembly.GetTypes().Where(t => t != null && t.FullName.StartsWith(nameSpace) & !t.FullName.Contains("+")).ToArray();
                foreach (Type t in types)
                {
                    try
                    {
                        result.Add(t);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                return result;
            }
        }



        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            object p;
            try
            {
                // try to create the object using its parameterless constructor
                p = Activator.CreateInstance(type);
            }
            catch {
                // try to create the object using this scary initializer that 
                // doesn't need the parameterless constructor
                p = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
            }

            var props = type.GetProperties();

            foreach (string key in dictionary.Keys)
            {
                var prop = props.Where(t => t.Name == key).FirstOrDefault();
                if (prop != null)
                {
                    if (prop.Name.EndsWith("Filter"))
                    {
                        if (dictionary[key] is string)
                            prop.SetValue(p, new string[] { (string)dictionary[key] }, null);
                        else
                            prop.SetValue(p, (string[])((ArrayList)dictionary[key]).ToArray(typeof(string)), null);
                    }
                    else
                    {
                        if((dictionary[key] is Dictionary<string, object>))
                            prop.SetValue(p, Deserialize((Dictionary<string, object>)dictionary[key], prop.PropertyType, serializer), null);
                        else
                        {
                            if (dictionary[key] is IList && prop.PropertyType.IsGenericType)
                            {
                                object obj = Activator.CreateInstance(prop.PropertyType);
                                foreach(var itm in (IEnumerable)dictionary[key])
                                {
                                    ((IList)obj).Add(itm);
                                }
                                prop.SetValue(p, obj, null);
                            }
                            else
                                prop.SetValue(p, dictionary[key], null);
                        }
                           
                    }
                }
            }

            return p;
        }


        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

    }
}
