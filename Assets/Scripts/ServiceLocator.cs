using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A centralized registry for system services.
/// Allows decoupled access to core systems like Inventory, Spawning, and Game data.
/// </summary>
public static class ServiceLocator
{
    // A dictionary holding references to our services, mapped by their Type.
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static void Register<T>(T service)
    {
        Type type = typeof(T);
        if (!_services.ContainsKey(type))
        {
            _services.Add(type, service);
            Debug.Log($"[Service Locator] Registered {type.Name}");
        }
        else
        {
            Debug.LogWarning($"[Service Locator] {type.Name} is already registered! Overwriting is not allowed.");
        }
    }

    public static void Unregister<T>()
    {
        Type type = typeof(T);
        if (_services.ContainsKey(type))
        {
            _services.Remove(type);
            Debug.Log($"[Service Locator] Unregistered {type.Name}");
        }
    }

    public static T Get<T>()
    {
        Type type = typeof(T);
        if (_services.TryGetValue(type, out object service))
        {
            return (T)service;
        }

        Debug.LogError($"[Service Locator] Attempted to get {type.Name}, but it is not registered!");
        return default;
    }
}