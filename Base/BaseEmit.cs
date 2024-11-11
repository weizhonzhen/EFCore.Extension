﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace EFCore.Extension.Base
{
    internal static class BaseEmit
    {
        private static ConcurrentDictionary<string, MethodInfo> cacheMethod = new ConcurrentDictionary<string, MethodInfo>();
        private static ConcurrentDictionary<string, DynamicMethod> cacheDyn = new ConcurrentDictionary<string, DynamicMethod>();

        private static MethodInfo GetMethod(string key)
        {
            if (cacheMethod.ContainsKey(key))
                return cacheMethod[key];
            else
            {
                return null;
            }
        }

        private static void SetMethod(string key, MethodInfo method)
        {
            if (!cacheMethod.ContainsKey(key))
            {
                cacheMethod.TryAdd(key, method);
            }
            else
            {
                cacheMethod.TryRemove(key, out _);
                cacheMethod.TryAdd(key, method);
            }
        }

        private static void SetDyn(string key, DynamicMethod dyn)
        {
            if (!cacheDyn.ContainsKey(key))
            {
                cacheDyn.TryAdd(key, dyn);
            }
            else
            {
                cacheDyn.TryRemove(key, out _);
                cacheDyn.TryAdd(key, dyn);
            }
        }

        private static DynamicMethod GetDyn(string key)
        {
            if (cacheDyn.ContainsKey(key))
                return cacheDyn[key];
            else
            {
                return null;
            }
        }

        internal static void Set<T>(T model, Dictionary<string, object> dic)
        {
            try
            {
                var type = typeof(T);
                var dynamicMethod = new DynamicMethod("SetEmit", null, new[] { type, typeof(Dictionary<string, object>) }, type.Module);
                var iL = dynamicMethod.GetILGenerator();

                foreach (var item in dic)
                {
                    var key = $"set_{item.Key}_{type.FullName}";
                    var method = GetMethod(key);
                    if (method == null)
                    {
                        method = type.GetMethod($"set_{item.Key}");
                        if (method == null)
                            return;
                        SetMethod(key, method);
                    }
                    var parameter = method.GetParameters()[0];
                    if (parameter == null)
                        return;

                    Type defType = parameter.ParameterType;
                    var local = iL.DeclareLocal(defType, true);

                    ExecIlType(iL, local, defType, item, method);
                }

                iL.Emit(OpCodes.Ret);
                var dyn = dynamicMethod.CreateDelegate(typeof(Action<T, Dictionary<string, object>>)) as Action<T, Dictionary<string, object>>;
                dyn(model, dic);
            }
            catch (Exception ex) { }
        }

        internal static void Set<T>(List<T> list, List<Dictionary<string, object>> dics)
        {
            try
            {
                var listType = typeof(List<T>);
                var type = typeof(T);

                var dynKey = $"SetEmit_{listType}_{listType.Module}";
                var dynamicMethod = GetDyn(dynKey);
                if (dynamicMethod == null)
                {
                    dynamicMethod = new DynamicMethod("SetEmit", null, new[] { listType, typeof(List<Dictionary<string, object>>) }, type.Module);
                    SetDyn(dynKey, dynamicMethod);
                }
                var iL = dynamicMethod.GetILGenerator();

                var addKey = $"Add_{listType.FullName}";
                var addMethod = GetMethod(addKey);
                if (addMethod == null)
                {
                    addMethod = listType.GetMethod("Add");
                    if (addMethod == null)
                        return;
                    SetMethod(addKey, addMethod);
                }

                dics.ForEach(dic =>
                {
                    var model = iL.DeclareLocal(typeof(T));
                    iL.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
                    iL.Emit(OpCodes.Stloc, model);

                    foreach (var item in dic)
                    {
                        var key = $"set_{item.Key}_{type.FullName}";
                        var method = GetMethod(key);
                        if (method == null)
                        {
                            method = type.GetMethod($"set_{item.Key}");
                            if (method == null)
                                return;
                            SetMethod(key, method);
                        }
                        var parameter = method.GetParameters()[0];
                        if (parameter == null)
                            return;

                        var defType = parameter.ParameterType;
                        var info = EmitParam(defType);

                        iL.Emit(OpCodes.Ldloc, model);
                        if (defType == typeof(bool) || defType == typeof(bool?))
                        {
                            if (item.Value == null && defType == typeof(bool?))
                                iL.Emit(OpCodes.Ldnull);
                            else
                            {
                                if (item.Value.ToStr().ToInt(9) == 0)
                                    iL.Emit(OpCodes.Ldc_I4_0);
                                else
                                    iL.Emit(OpCodes.Ldc_I4_1);

                                if (defType == typeof(bool))
                                    iL.Emit(OpCodes.Box, typeof(bool));
                                else
                                    iL.Emit(OpCodes.Newobj, typeof(bool?).GetConstructor(new Type[] { typeof(bool) }));
                            }
                        }
                        else
                        {
                            if (info.OpCodeParam == OpCodes.Ldc_R8)
                                iL.Emit(info.OpCodeParam.Value, item.Value.ToStr().ToDouble(0));
                            else if (info.OpCodeParam == OpCodes.Ldstr)
                                iL.Emit(info.OpCodeParam.Value, item.Value.ToStr());
                            else if (info.OpCodeParam == OpCodes.Ldc_R4)
                                iL.Emit(info.OpCodeParam.Value, item.Value.ToStr().ToFloat(0));
                            else if (info.OpCodeParam == OpCodes.Ldc_I4)
                                iL.Emit(info.OpCodeParam.Value, item.Value.ToStr().ToInt(0));
                            else if (info.OpCodeParam == OpCodes.Ldc_I4_S)
                                iL.Emit(info.OpCodeParam.Value, item.Value.ToStr().ToInt16(0));
                            else if (info.OpCodeParam == OpCodes.Ldc_I8 && (info.type == typeof(long) || info.GenericType == typeof(long)))
                                iL.Emit(info.OpCodeParam.Value, item.Value.ToStr().ToLong(0));
                            else if (info.OpCodeParam == OpCodes.Ldc_I8 && (info.type == typeof(DateTime) || info.GenericType == typeof(DateTime)))
                                iL.Emit(info.OpCodeParam.Value, item.Value.ToStr().ToDate().Value.Ticks);
                            else if (info.OpCodeParam == OpCodes.Ldc_I8 && (info.type == typeof(TimeSpan) || info.GenericType == typeof(TimeSpan)))
                                iL.Emit(info.OpCodeParam.Value, item.Value.ToStr().ToDate().Value.Ticks);

                            if (!info.IsGenericType && info.OpCodeNewobj != null)
                                iL.Emit(info.OpCodeNewobj.Value, info.type.GetConstructor(new Type[] { info.GenericType }));
                            if (info.IsGenericType && info.GenericOpCodeNewobj != null && info.IsDateTime)
                                iL.Emit(info.OpCodeNewobj.Value, info.GenericType.GetConstructor(new Type[] { typeof(long) }));
                            if (info.IsGenericType && info.GenericOpCodeNewobj != null)
                                iL.Emit(info.GenericOpCodeNewobj.Value, info.type.GetConstructor(new Type[] { info.GenericType }));
                        }
                        iL.EmitCall(OpCodes.Callvirt, method, null);
                    }

                    iL.Emit(OpCodes.Ldarg_0);
                    iL.Emit(OpCodes.Ldloc, model);
                    iL.EmitCall(OpCodes.Callvirt, addMethod, null);
                });

                iL.Emit(OpCodes.Ret);
                var dyn = dynamicMethod.CreateDelegate(typeof(Action<List<T>, List<Dictionary<string, object>>>)) as Action<List<T>, List<Dictionary<string, object>>>;
                dyn(list, dics);
            }
            catch (Exception ex) { }
        }

        private static void ExecIL(ILGenerator iL, LocalBuilder local, MethodInfo method)
        {
            iL.Emit(OpCodes.Stloc, local);
            iL.Emit(OpCodes.Ldarg_0);
            iL.Emit(OpCodes.Ldloc, local);
            iL.EmitCall(OpCodes.Callvirt, method, null);
        }

        private static void ExecIlType(ILGenerator iL, LocalBuilder local, Type defType, KeyValuePair<string, object> item, MethodInfo method)
        {
            if (defType == typeof(bool) || defType == typeof(bool?))
            {
                if (item.Value.ToStr().ToInt(9) == 0)
                    iL.Emit(OpCodes.Ldc_I4_0);
                else
                    iL.Emit(OpCodes.Ldc_I4_1);

                if (defType == typeof(bool))
                    iL.Emit(OpCodes.Box, typeof(bool));
                else
                    iL.Emit(OpCodes.Newobj, typeof(bool?).GetConstructor(new Type[] { typeof(bool) }));

                ExecIL(iL, local, method);
                return;
            }

            var info = EmitParam(defType);
            if (info.IsGenericType && item.Value == null)
                iL.Emit(OpCodes.Ldnull);
            else
            {
                if (info.OpCodeParam == OpCodes.Ldc_R8)
                    iL.Emit(info.OpCodeParam.Value, item.Value.ToStr().ToDouble(0));
                else if (info.OpCodeParam == OpCodes.Ldstr)
                    iL.Emit(info.OpCodeParam.Value, item.Value.ToStr());
                else if (info.OpCodeParam == OpCodes.Ldc_R4)
                    iL.Emit(info.OpCodeParam.Value, item.Value.ToStr().ToFloat(0));
                else if (info.OpCodeParam == OpCodes.Ldc_I4)
                    iL.Emit(info.OpCodeParam.Value, item.Value.ToStr().ToInt(0));
                else if (info.OpCodeParam == OpCodes.Ldc_I4_S)
                    iL.Emit(info.OpCodeParam.Value, item.Value.ToStr().ToInt16(0));
                else if (info.OpCodeParam == OpCodes.Ldc_I8 && (info.type == typeof(long) || info.GenericType == typeof(long)))
                    iL.Emit(info.OpCodeParam.Value, item.Value.ToStr().ToLong(0));
                else if (info.OpCodeParam == OpCodes.Ldc_I8 && (info.type == typeof(DateTime) || info.GenericType == typeof(DateTime)))
                    iL.Emit(info.OpCodeParam.Value, item.Value.ToStr().ToDate().Value.Ticks);
                else if (info.OpCodeParam == OpCodes.Ldc_I8 && (info.type == typeof(TimeSpan) || info.GenericType == typeof(TimeSpan)))
                    iL.Emit(info.OpCodeParam.Value, item.Value.ToStr().ToDate().Value.Ticks);

                if (!info.IsGenericType && info.OpCodeNewobj != null)
                    iL.Emit(info.OpCodeNewobj.Value, info.type.GetConstructor(new Type[] { info.GenericType }));
                if (info.IsGenericType && info.GenericOpCodeNewobj != null && info.IsDateTime)
                    iL.Emit(info.OpCodeNewobj.Value, info.GenericType.GetConstructor(new Type[] { typeof(long)}));
                if (info.IsGenericType && info.GenericOpCodeNewobj != null)
                    iL.Emit(info.GenericOpCodeNewobj.Value, info.type.GetConstructor(new Type[] { info.GenericType }));
            }
            ExecIL(iL, local, method);
        }

        internal static object Get(object model, string name)
        {
            if (model == null || name.ToStr() == "")
                return null;

            var type = model.GetType();

            var dynKey = $"GetEmit_{type}_{type.Module}";
            var dynamicMethod = GetDyn(dynKey);
            if (dynamicMethod == null)
            {
                dynamicMethod = new DynamicMethod("GetEmit", typeof(object), new[] { typeof(object) }, type, true);
                SetDyn(dynKey, dynamicMethod);
            }

            var key = $"get_{name}_{type.FullName}";
            var method = GetMethod(key);

            if (method == null)
            {
                method = type.GetMethod($"get_{name}");
                if (method == null)
                    return null;
                SetMethod(key, method);
            }

            var property = type.GetProperty(name);

            var iL = dynamicMethod.GetILGenerator();
            iL.Emit(OpCodes.Ldarg_0);

            if (type.IsValueType)
                iL.Emit(OpCodes.Unbox, type);
            else
                iL.Emit(OpCodes.Castclass, type);

            iL.EmitCall(OpCodes.Callvirt, method, null);

            if (property.PropertyType.IsValueType)
                iL.Emit(OpCodes.Box, property.PropertyType);

            iL.Emit(OpCodes.Ret);
            var dyn = (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
            return dyn(model);
        }

        private delegate object EmitInvoke(object target, object[] paramters);

        internal static object Invoke(object model, MethodInfo methodInfo, object[] param)
        {
            if (methodInfo == null || model == null)
                return null;
            try
            {
                var dynKey = $"InvokeEmit_{typeof(EmitInvoke).Module}";
                var dynamicMethod = GetDyn(dynKey);
                if (dynamicMethod == null)
                {
                    dynamicMethod = new DynamicMethod("InvokeEmit", typeof(object), new Type[] { typeof(object), typeof(object[]) }, typeof(EmitInvoke).Module);
                    SetDyn(dynKey, dynamicMethod);
                }

                var iL = dynamicMethod.GetILGenerator();
                var info = methodInfo.GetParameters();
                var type = new Type[info.Length];
                var local = new LocalBuilder[type.Length];

                for (int i = 0; i < info.Length; i++)
                {
                    if (info[i].ParameterType.IsByRef)
                        type[i] = info[i].ParameterType.GetElementType();
                    else
                        type[i] = info[i].ParameterType;
                }

                for (int i = 0; i < type.Length; i++)
                {
                    local[i] = iL.DeclareLocal(type[i], true);
                }

                for (int i = 0; i < type.Length; i++)
                {
                    iL.Emit(OpCodes.Ldarg_1);
                    iL.Emit(OpCodes.Ldc_I4, i);
                    iL.Emit(OpCodes.Ldelem_Ref);
                    if (type[i].IsValueType)
                        iL.Emit(OpCodes.Unbox_Any, type[i]);
                    else
                        iL.Emit(OpCodes.Castclass, type[i]);
                    iL.Emit(OpCodes.Stloc, local[i]);
                }

                if (!methodInfo.IsStatic)
                    iL.Emit(OpCodes.Ldarg_0);

                for (int i = 0; i < type.Length; i++)
                {
                    if (info[i].ParameterType.IsByRef)
                        iL.Emit(OpCodes.Ldloca_S, local[i]);
                    else
                        iL.Emit(OpCodes.Ldloc, local[i]);
                }

                if (methodInfo.IsStatic)
                    iL.EmitCall(OpCodes.Call, methodInfo, null);
                else
                    iL.EmitCall(OpCodes.Callvirt, methodInfo, null);

                if (methodInfo.ReturnType == typeof(void))
                    iL.Emit(OpCodes.Ldnull);
                else if (methodInfo.ReturnType.IsValueType)
                    iL.Emit(OpCodes.Box, methodInfo.ReturnType);


                for (int i = 0; i < type.Length; i++)
                {
                    if (info[i].ParameterType.IsByRef)
                    {
                        iL.Emit(OpCodes.Ldarg_1);
                        iL.Emit(OpCodes.Ldc_I4, i);
                        iL.Emit(OpCodes.Ldloc, local[i]);
                        if (local[i].LocalType.IsValueType)
                            iL.Emit(OpCodes.Box, local[i].LocalType);
                        iL.Emit(OpCodes.Stelem_Ref);
                    }
                }

                iL.Emit(OpCodes.Ret);
                var dyn = dynamicMethod.CreateDelegate(typeof(EmitInvoke)) as EmitInvoke;
                return dyn(model, param);
            }
            catch (Exception ex)
            {
                return methodInfo.Invoke(model, param);
            }
        }

        private static EmitParam EmitParam(Type type)
        {
            var info = new EmitParam();
            info.type = type;
            info.IsGenericType = type.GenericTypeArguments.Length > 0;

            if (type == typeof(string) || type == typeof(String))
            {
                info.OpCodeParam = OpCodes.Ldstr;
            }
            else if (info.IsGenericType)
            {
                info.GenericOpCodeNewobj = OpCodes.Newobj;
                info.GenericType = type.GenericTypeArguments[0];
            }
            else
            {
                info.OpCodeNewobj = OpCodes.Newobj;
                info.GenericType = type;

                if (type == typeof(decimal))
                    info.GenericType = typeof(double);

                if (type == typeof(TimeSpan))
                    info.GenericType = typeof(long);

                if (type == typeof(DateTime))
                    info.GenericType = typeof(long);
            }
            if (type == typeof(DateTime) || info.GenericType == typeof(DateTime)
                || type == typeof(TimeSpan) || info.GenericType == typeof(TimeSpan))
            {
                info.IsDateTime = true;
                info.OpCodeNewobj = OpCodes.Newobj;
            }

            if (type == typeof(decimal) || info.GenericType == typeof(decimal) || type == typeof(double) || info.GenericType == typeof(double))
                info.OpCodeParam = OpCodes.Ldc_R8;

            if (type == typeof(DateTime) || info.GenericType == typeof(DateTime)
                || type == typeof(long) || info.GenericType == typeof(long)
                || type == typeof(TimeSpan) || info.GenericType == typeof(TimeSpan))
                info.OpCodeParam = OpCodes.Ldc_I8;

            if (type == typeof(int) || info.GenericType == typeof(int)
                 || type == typeof(byte) || info.GenericType == typeof(byte))
                info.OpCodeParam = OpCodes.Ldc_I4;

            if (type == typeof(sbyte) || info.GenericType == typeof(sbyte)
                 || type == typeof(short) || info.GenericType == typeof(short))
                info.OpCodeParam = OpCodes.Ldc_I4_S;

            if (type == typeof(float) || info.GenericType == typeof(float))
                info.OpCodeParam = OpCodes.Ldc_R4;

            return info;
        }
    }

    internal class EmitParam
    {
        public Type type { get; set; }

        public OpCode? OpCodeParam { get; set; }

        public OpCode? OpCodeNewobj { get; set; }

        public bool IsGenericType { get; set; }

        public Type GenericType { get; set; }

        public OpCode? GenericOpCodeNewobj { get; set; }

        public bool IsDateTime { get; set; }
    }
}