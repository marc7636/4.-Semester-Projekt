﻿using AssemblyLineManager.CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyLineManager.Core;

public partial class Core
{
    static readonly Dictionary<string, ICommunicationController> instances = new Dictionary<string, ICommunicationController>();
    AssemblyLineThreadManager assemblyLineThreadManager;

    public Core()
    {
        LoadModules();
        /*foreach (var instance in instances)
        {
            Console.WriteLine(instance.GetState().ToString());
            if (instance is ICommunicationControllerWithInventory)
            {
                Console.WriteLine("woohoo");
            }
        }*/
        assemblyLineThreadManager = new AssemblyLineThreadManager(instances);
        assemblyLineThreadManager.Start();
    }

    public void PauseAssemblyLine()
    {
        assemblyLineThreadManager.PauseThread();
    }

    public void ResumeAssemblyLine()
    {
        assemblyLineThreadManager.ResumeThread();
    }

    public Dictionary<string, KeyValuePair<string, string>[]> GetStates()
    {
        Dictionary<string, KeyValuePair<string, string>[]> states = new Dictionary<string, KeyValuePair<string, string>[]>();
        foreach (var instance in instances)
        {
            states.Add(instance.Key, instance.Value.GetState());
        }
        return states;
    }

    public KeyValuePair<string, string>[] GetState(string moduleName)
    {
        return instances[moduleName].GetState();
    }

    static void LoadModules()
    {
        List<ICommunicationController> instances = new List<ICommunicationController>();
        Assembly.LoadFrom(@"AssemblyLineManager.AGV.dll");
        Assembly.LoadFrom(@"AssemblyLineManager.AssemblyStation.dll");
        Assembly.LoadFrom(@"AssemblyLineManager.Warehouse.dll");
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var types = assemblies.SelectMany(a => a.GetTypes()).Where(t => typeof(ICommunicationController).IsAssignableFrom(t) && t.IsClass);
        instances.AddRange(types.Select(t => Activator.CreateInstance(t)).OfType<ICommunicationController>());
        foreach (var instance in instances)
        {
            Core.instances.Add(instance.Name, instance);
        }
    }
}
