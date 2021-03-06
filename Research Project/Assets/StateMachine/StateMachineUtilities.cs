﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace StateMachineUtilities {

    [System.Serializable]
    public class Parameter {
        public string name = string.Empty;
        [System.NonSerialized]
        public System.Type type = typeof(float);
        public object value {
            get {
                if ((internalValue == null) || (internalValue.GetType() != type)) {
                    if (!type.IsSubclassOf(typeof(Object))) {
                        internalValue = System.Activator.CreateInstance(type);
                    } else {
                        internalValue = null;
                    }
                }
                return internalValue;
            }
            set {
                if ((value == null) && type.IsSubclassOf(typeof(Object))) {
                    internalValue = null;
                } else if (value.GetType() == type) {
                    internalValue = value;
                } else {
                    throw new System.Exception("Invalid type for parameter");
                }
            }
        }

        [System.NonSerialized]
        private object internalValue = null;

        public string serializedType;
        public string serializedValue;
    }

    [System.Serializable]
    public class State {
        public string name = string.Empty;
        public List<Transition> transitions = new List<Transition>();
        public List<Method> actions = new List<Method>();
        public Vector2 position = Vector2.zero;
    }

    [System.Serializable]
    public class Transition {
        [System.NonSerialized]
        public State from = null;
        [System.NonSerialized]
        public State to = null;

        public List<Method> conditions = new List<Method>();

        public int serializedTo;
    }

    [System.Serializable]
    public class Method {
        public Method(System.Reflection.MethodInfo theMethod) {
            method = theMethod;
            var methodArguments = method.GetParameters();
            arguments = new Argument[methodArguments.Length];
            for (var i = 0; i < methodArguments.Length; ++i) {
                arguments[i] = new Argument();
                arguments[i].name = methodArguments[i].Name;
                arguments[i].type = methodArguments[i].ParameterType;
            }
        }

        [System.NonSerialized]
        public System.Reflection.MethodInfo method = null;
        public Argument[] arguments = null;

        public string serializedMethodType;
        public string serializedMethodName;
    }

    [System.Serializable]
    public class Argument {
        public enum Style {
            Constant, Parameter, Filter
        }

        public Style style = Style.Constant;
        [System.NonSerialized]
        public string name = string.Empty;
        [System.NonSerialized]
        public System.Type type = typeof(object);
        public object value {
            get {
                System.Type reqType = type;
                if (style == Style.Filter) {
                    reqType = typeof(Method);
                } else if (style == Style.Parameter) {
                    reqType = typeof(Parameter);
                }
                if ((internalValue == null) || (internalValue.GetType() != reqType)) {
                    if ((style == Style.Constant) && !reqType.IsSubclassOf(typeof(Object))) {
                        internalValue = System.Activator.CreateInstance(reqType);
                    } else {
                        internalValue = null;
                    }
                }
                return internalValue;
            }
            set {
                System.Type reqType = type;
                if (style == Style.Filter) {
                    reqType = typeof(Method);
                } else if (style == Style.Parameter) {
                    reqType = typeof(Parameter);
                }
                if ((value == null) && reqType.IsSubclassOf(typeof(Object))) {
                    internalValue = null;
                } else if (value.GetType() == reqType) {
                    internalValue = value;
                } else {
                    throw new System.Exception("Invalid type for argument");
                }
            }
        }

        [System.NonSerialized]
        private object internalValue = null;

        public string serializedValue;
    }


    //Serialization methods

    public class Serialization {

        public static string serializeObject(StateMachine machine, object value, System.Type type) {
            if (value == null) return "";
            if (type.IsSubclassOf(typeof(Object))) {
                if (!machine.serializedObjects.Contains((Object)value)) {
                    machine.serializedObjects.Add((Object)value);
                }
                return machine.serializedObjects.IndexOf((Object)value).ToString();
            }
            var serializer = new System.Xml.Serialization.XmlSerializer(type);
            var writer = new System.IO.StringWriter();
            serializer.Serialize(writer, System.Convert.ChangeType(value, type));
            return writer.ToString();
        }

        public static object deserializeObject(StateMachine machine, string value, System.Type type) {
            if (value == "") return null;
            if (type.IsSubclassOf(typeof(Object))) {
                return machine.serializedObjects[int.Parse(value)];
            }
            var serializer = new System.Xml.Serialization.XmlSerializer(type);
            var reader = new System.IO.StringReader(value);
            return serializer.Deserialize(reader);
        }

    }


    //Type conversion methods

    public class Conversion {

        private static Dictionary<System.Type, Dictionary<System.Type, bool>> cache = new Dictionary<System.Type, Dictionary<System.Type, bool>>();

        public static bool canConvert(System.Type from, System.Type to) {
            if (cache.ContainsKey(from)) {
                if (cache[from].ContainsKey(to)) {
                    return cache[from][to];
                }
            }

            bool possible = true;

            if (from != to) {
                if ((from == typeof(bool)) || (to == typeof(bool))) {
                    possible = false;
                }
            }

            if (from.IsSubclassOf(typeof(Object))) {
                possible = false;
            } else {
                try {
                    System.Convert.ChangeType(System.Activator.CreateInstance(from), to);
                } catch {
                    possible = false;
                }
            }
            
            if (to == typeof(object)) {
                possible = true;
            } else if (from == to) {
                possible = true;
            }

            if (!cache.ContainsKey(from))
                cache.Add(from, new Dictionary<System.Type, bool>());
            cache[from].Add(to, possible);

            return possible;
        }

        public static object convert(object value, System.Type type) {
            if (value == null) return value;
            if (type == typeof(object)) return value;
            return System.Convert.ChangeType(value, type);
        }

    }


    //Module definitions

    public class Modules {

        public static IEnumerable<System.Reflection.MethodInfo> getMethods() {
            return System.Reflection.Assembly.GetAssembly(typeof(Module)).GetTypes()
                .Where(type => type.IsSubclassOf(typeof(Module)))
                .SelectMany(type => type.GetMethods())
                .Where(method => (method.GetCustomAttributes(typeof(Method), true).Length > 0));
        }

        public static IEnumerable<System.Reflection.MethodInfo> getFilters(System.Type type) {
            return getMethods().Where(method => Conversion.canConvert(method.ReturnType, type));
        }

        public static IEnumerable<System.Reflection.MethodInfo> getActions() {
            return getMethods().Where(method => (method.ReturnType == typeof(void)));
        }

        private static Method getMethodAttributes(System.Reflection.MethodInfo method) {
            return (Method)method.GetCustomAttributes(typeof(Method), true)[0];
        }

        public static string getMethodName(System.Reflection.MethodInfo method) {
            if (method == null) return string.Empty;
            return getMethodAttributes(method).name;
        }

        //Inherit from this when defining conditions and actions
        public class Module { }

        [System.AttributeUsage(System.AttributeTargets.Method)]
        public class Method : System.Attribute {
            public readonly string name = string.Empty;

            public Method(string name) {
                this.name = name;
            }
        }

    }

}