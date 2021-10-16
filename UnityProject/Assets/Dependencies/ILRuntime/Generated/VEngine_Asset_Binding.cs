using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class VEngine_Asset_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(VEngine.Asset);
            args = new Type[]{};
            method = type.GetMethod("get_asset", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_asset_0);
            args = new Type[]{typeof(System.String), typeof(System.Type), typeof(System.Action<VEngine.Asset>)};
            method = type.GetMethod("LoadAsync", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LoadAsync_1);

            field = type.GetField("completed", flag);
            app.RegisterCLRFieldGetter(field, get_completed_0);
            app.RegisterCLRFieldSetter(field, set_completed_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_completed_0, AssignFromStack_completed_0);


        }


        static StackObject* get_asset_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            VEngine.Asset instance_of_this_method = (VEngine.Asset)typeof(VEngine.Asset).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.asset;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* LoadAsync_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<VEngine.Asset> @completed = (System.Action<VEngine.Asset>)typeof(System.Action<VEngine.Asset>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Type @type = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.String @path = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = VEngine.Asset.LoadAsync(@path, @type, @completed);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


        static object get_completed_0(ref object o)
        {
            return ((VEngine.Asset)o).completed;
        }

        static StackObject* CopyToStack_completed_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((VEngine.Asset)o).completed;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_completed_0(ref object o, object v)
        {
            ((VEngine.Asset)o).completed = (System.Action<VEngine.Asset>)v;
        }

        static StackObject* AssignFromStack_completed_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<VEngine.Asset> @completed = (System.Action<VEngine.Asset>)typeof(System.Action<VEngine.Asset>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((VEngine.Asset)o).completed = @completed;
            return ptr_of_this_method;
        }



    }
}
