using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class RpcManager : MonoBehaviour
{
    public static RpcManager Instance;
    public Dictionary<string, MethodInfo> Rpcs = new Dictionary<string, MethodInfo>();

    private void Awake()
    {
        Instance = this;
        Assembly assembly = Assembly.GetExecutingAssembly();

        var methods = assembly.GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes(typeof(NetRPCAttribute), false).Length > 0)
            .ToArray();

        foreach (var info in methods)
        {
            Rpcs.Add(info.Name, info);
        }

        foreach (var rpc in Rpcs)
        {
            Debug.Log(rpc.Key);
        }
    }
}

public class NetRPCAttribute : Attribute
{
}