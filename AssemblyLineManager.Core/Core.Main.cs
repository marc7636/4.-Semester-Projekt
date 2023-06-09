﻿using AssemblyLineManager.CommonLib;
using System.Reflection;

namespace AssemblyLineManager.Core;

public partial class Core
{
    private static Core? instance;

    readonly Dictionary<string, ICommunicationController> instances = new Dictionary<string, ICommunicationController>();
    AssemblyLineThreadManager assemblyLineThreadManager;
    string customLibraryPath = "";

    public static Core Instance(string customLibraryPath = "")
    {
        instance ??= new Core(customLibraryPath);
        return instance;
    }

    internal Core()
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
    }

    private Core(string customLibraryPath)
    {
        this.customLibraryPath = customLibraryPath;
        LoadModules();
        assemblyLineThreadManager = new AssemblyLineThreadManager(instances);
    }
    public bool IsRunning()
    {
        return assemblyLineThreadManager.isRunning();
    }
    public void StartAssemblyLine(bool allItems, int id)
    {
        assemblyLineThreadManager.StartThread(allItems, id);
    }

    public void PauseAssemblyLine()
    {
        assemblyLineThreadManager.PauseThread();
    }

    public void ResumeAssemblyLine()
    {
        assemblyLineThreadManager.ResumeThread();
    }

    public void StopAssemblyLine()
    {
        assemblyLineThreadManager.StopThread();
    }

    public string[] GetModules()
    {
        return instances.Keys.ToArray();
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

    public Dictionary<string, string> GetState(string moduleName)
    {
        Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
        foreach (KeyValuePair<string, string> keyValuePair in instances[moduleName].GetState())
        {
            keyValuePairs.Add(keyValuePair.Key, keyValuePair.Value);
        }
        return keyValuePairs;
        
    }

    public Dictionary<int, string> GetInventory(string moduleName)
    {
        Dictionary<int, string> dick = new Dictionary<int, string>();
        if (instances[moduleName] is ICommunicationControllerWithInventory inventory)
        {
            foreach (KeyValuePair<int, string> kvp in inventory.GetInventory())
            {
                dick.Add(kvp.Key, kvp.Value);
            }
            return dick;
        }
        else
        {
            return dick;
        }
    }

    void LoadModules()
    {
        List<ICommunicationController> instances = new List<ICommunicationController>();
        Assembly.LoadFrom(customLibraryPath + @"AssemblyLineManager.AGV.dll");
        Assembly.LoadFrom(customLibraryPath + @"AssemblyLineManager.AssemblyStation.dll");
        Assembly.LoadFrom(customLibraryPath + @"AssemblyLineManager.Warehouse.dll");
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var types = assemblies.SelectMany(a => a.GetTypes()).Where(t => typeof(ICommunicationController).IsAssignableFrom(t) && t.IsClass);
        instances.AddRange(types.Select(t => Activator.CreateInstance(t)).OfType<ICommunicationController>());
        foreach (var instance in instances)
        {
            this.instances.Add(instance.Name, instance);
        }
    }
}
