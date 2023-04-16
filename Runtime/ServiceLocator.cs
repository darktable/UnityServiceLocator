using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("com.darktable.servicelocator.editor.tests")]

namespace com.darktable
{
    public interface IService { }

    public static class ServiceLocator
    {
        private delegate void ServiceLocatorDelegate(IService service, bool registered);

        private static readonly Dictionary<Type, IService> k_Services = new();
        private static readonly Dictionary<Type, ServiceLocatorDelegate> k_Callbacks = new();
        private static readonly Dictionary<Delegate, ServiceLocatorDelegate> k_RegisteredCallbacks = new();

        public static bool Register<T>(T service) where T : class, IService
        {
            var type = typeof(T);

            if (!k_Services.TryAdd(type, service))
            {
                // Throw exception or just warn and return false?
                Debug.LogWarning($"Service of type {type.Name} already registered");
                return false;
            }

            if (k_Callbacks.TryGetValue(type, out var serviceCallback))
            {
                serviceCallback?.Invoke(service, true);
            }

            return true;
        }

        public static bool Unregister<T>(T service) where T : class, IService
        {
            var type = typeof(T);

            if (k_Services.TryGetValue(type, out var temp) && service == temp)
            {
                if (k_Callbacks.TryGetValue(type, out var serviceCallback))
                {
                    serviceCallback?.Invoke(service, false);
                }

                k_Services.Remove(type);
                return true;
            }

            Debug.LogWarning($"This instance of {type.Name} is not registered");
            return false;
        }

        private static bool TryGetService<T>(out T result) where T : class, IService
        {
            if (k_Services.TryGetValue(typeof(T), out var service))
            {
                result = (T)service;
                return true;
            }

            result = default;
            return false;
        }

        public static void AddServiceListener<T>(Action<T, bool> serviceCallback) where T : class, IService
        {
            if (k_RegisteredCallbacks.ContainsKey(serviceCallback))
            {
                Debug.LogWarning($"Attempted to register listener twice: {serviceCallback.Method.Name}");
                return;
            }

            if (TryGetService<T>(out var service))
            {
                serviceCallback.Invoke(service, true);
            }

            var type = typeof(T);

            ServiceLocatorDelegate callbackWrapper = (s, b) => serviceCallback((T)s, b);

            if (k_Callbacks.ContainsKey(type))
            {
                k_Callbacks[type] += callbackWrapper;
            }
            else
            {
                k_Callbacks[type] = callbackWrapper;
            }

            k_RegisteredCallbacks.Add(serviceCallback, callbackWrapper);
        }

        public static void RemoveServiceListener<T>(Action<T, bool> serviceCallback) where T : class, IService
        {
            if (!k_RegisteredCallbacks.TryGetValue(serviceCallback, out var callbackWrapper))
            {
                return;
            }

            var type = typeof(T);

            if (k_Callbacks.ContainsKey(type))
            {
                k_Callbacks[type] -= callbackWrapper;

                if (k_Callbacks[type] == null)
                {
                    k_Callbacks.Remove(type);
                }
            }

            k_RegisteredCallbacks.Remove(serviceCallback);
        }

        internal static void Reset()
        {
            k_Services.Clear();
            k_Callbacks.Clear();
            k_RegisteredCallbacks.Clear();
        }
    }
}
