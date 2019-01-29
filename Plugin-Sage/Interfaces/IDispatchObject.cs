using System;
using System.Reflection;

namespace Plugin_Sage.Interfaces
{
    public interface IDispatchObject : IDisposable
    {
        object InvokeMethodByRef(string sMethodName, object[] aryParams);
        object InvokeMethod(string sMethodName, params object[] aryParams);
        object InvokeMethod(string sMethodName, object[] aryParams, ParameterModifier[] pmods);
        object GetProperty(string sPropertyName);
        object SetProperty(string sPropertyName, object oValue);
        object GetObject();
    }
}