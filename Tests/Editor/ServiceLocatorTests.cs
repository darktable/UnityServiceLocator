using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.darktable.tests
{
    public interface IDoIt : IService
    {
        void DoSomething();
    }

    public class ServiceA : IDoIt
    {
        private readonly string message;

        public ServiceA(string msg)
        {
            message = msg;
        }

        public void DoSomething()
        {
            Debug.Log($"{nameof(ServiceA)} is doing something. {message}");
        }
    }

    public class ServiceB : IDoIt
    {
        private readonly string message;

        public ServiceB(string msg)
        {
            message = msg;
        }

        public void DoSomething()
        {
            Debug.Log($"{nameof(ServiceB)} is doing something. {message}");
        }
    }

    public class ServiceLocatorTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Reset();
        }

        [Test]
        public void ServiceLocatorInterfaceTest()
        {
            // Register an interface instead of class
            ServiceLocator.AddServiceListener<IDoIt>(FoundService);

            var serviceA = new ServiceA("InterfaceTest");

            ServiceLocator.Register<IDoIt>(serviceA);

            LogAssert.Expect(LogType.Log, "ServiceA registered: True");
            LogAssert.Expect(LogType.Log, "ServiceA is doing something. InterfaceTest");

            void FoundService(IDoIt service, bool registered)
            {
                Debug.Log($"{service.GetType().Name} registered: {registered}");

                if (registered)
                {
                    service.DoSomething();
                }
            }
        }

        [Test]
        public void ServiceLocatorDoubleRegister()
        {
            // Register an interface instead of class
            var serviceA = new ServiceA("Double Register");

            ServiceLocator.Register<IDoIt>(serviceA);

            ServiceLocator.AddServiceListener<IDoIt>(FoundService);

            ServiceLocator.Register<IDoIt>(serviceA);

            LogAssert.Expect(LogType.Log, "ServiceA registered: True");
            LogAssert.Expect(LogType.Log, "ServiceA is doing something. Double Register");
            LogAssert.Expect(LogType.Warning, "Service of type IDoIt already registered");

            void FoundService(IDoIt service, bool registered)
            {
                Debug.Log($"{service.GetType().Name} registered: {registered}");

                if (registered)
                {
                    service.DoSomething();
                }
            }
        }

        [Test]
        public void ServiceLocatorGenericListener()
        {
            ServiceLocator.AddServiceListener<ServiceA>(FoundIService);
            ServiceLocator.AddServiceListener<ServiceB>(FoundIService);

            var serviceA = new ServiceA("Service A");
            var serviceB = new ServiceB("Service B");

            ServiceLocator.Register(serviceA);
            ServiceLocator.Register(serviceB);

            LogAssert.Expect(LogType.Log, "ServiceA registered: True");
            LogAssert.Expect(LogType.Log, "ServiceA is doing something. Service A");
            LogAssert.Expect(LogType.Log, "ServiceB registered: True");
            LogAssert.Expect(LogType.Log, "ServiceB is doing something. Service B");

            void FoundIService<T>(T service, bool registered) where T : class, IService
            {
                Debug.Log($"{service.GetType().Name} registered: {registered}");

                if (registered && service is IDoIt doIt)
                {
                    doIt.DoSomething();
                }
            }
        }

        [Test]
        public void ServiceLocatorUnregister()
        {
            ServiceLocator.AddServiceListener<ServiceA>(FoundIService);

            var serviceA = new ServiceA("Unregister Test");

            ServiceLocator.Register(serviceA);
            ServiceLocator.Unregister(serviceA);

            LogAssert.Expect(LogType.Log, "ServiceA registered: True");
            LogAssert.Expect(LogType.Log, "ServiceA is doing something. Unregister Test");
            LogAssert.Expect(LogType.Log, "ServiceA registered: False");

            void FoundIService(ServiceA service, bool registered)
            {
                Debug.Log($"{service.GetType().Name} registered: {registered}");

                if (registered)
                {
                    service.DoSomething();
                }
            }
        }

        [Test]
        public void ServiceLocatorDoubleListen()
        {
            ServiceLocator.AddServiceListener<ServiceA>(FoundIService);
            ServiceLocator.AddServiceListener<ServiceA>(FoundIService);

            var serviceA = new ServiceA("Double Listen Test");

            ServiceLocator.Register(serviceA);

            LogAssert.Expect(LogType.Warning, "Attempted to register listener twice: <ServiceLocatorDoubleListen>g__FoundIService|5_0");
            LogAssert.Expect(LogType.Log, "ServiceA registered: True");
            LogAssert.Expect(LogType.Log, "ServiceA is doing something. Double Listen Test");

            void FoundIService(ServiceA service, bool registered)
            {
                Debug.Log($"{service.GetType().Name} registered: {registered}");

                if (registered)
                {
                    service.DoSomething();
                }
            }
        }

        [Test]
        public void ServiceLocatorRemoveListener()
        {
            ServiceLocator.AddServiceListener<ServiceA>(FoundIService);

            var serviceA = new ServiceA("Remove Listener Test");

            ServiceLocator.Register(serviceA);

            Debug.Log("Remove listener");
            ServiceLocator.RemoveServiceListener<ServiceA>(FoundIService);

            Debug.Log("Unregistering");
            ServiceLocator.Unregister(serviceA);
            Debug.Log("Re-registering");
            ServiceLocator.Register(serviceA);

            Debug.Log("Re-add listener");
            ServiceLocator.AddServiceListener<ServiceA>(FoundIService);

            LogAssert.Expect(LogType.Log, "ServiceA registered: True");
            LogAssert.Expect(LogType.Log, "ServiceA is doing something. Remove Listener Test");
            LogAssert.Expect(LogType.Log, "Remove listener");
            LogAssert.Expect(LogType.Log, "Unregistering");
            LogAssert.Expect(LogType.Log, "Re-registering");
            LogAssert.Expect(LogType.Log, "Re-add listener");
            LogAssert.Expect(LogType.Log, "ServiceA registered: True");
            LogAssert.Expect(LogType.Log, "ServiceA is doing something. Remove Listener Test");

            void FoundIService(ServiceA service, bool registered)
            {
                Debug.Log($"{service.GetType().Name} registered: {registered}");

                if (registered)
                {
                    service.DoSomething();
                }
            }
        }

        [Test]
        public void ServiceLocatorTwoListeners()
        {
            ServiceLocator.AddServiceListener<ServiceA>(ListenerOne);
            ServiceLocator.AddServiceListener<ServiceA>(ListenerTwo);

            var serviceA = new ServiceA("Two Listener Test");

            ServiceLocator.Register(serviceA);
            ServiceLocator.Unregister(serviceA);

            LogAssert.Expect(LogType.Log, "ListenerOne ServiceA registered: True");
            LogAssert.Expect(LogType.Log, "ServiceA is doing something. Two Listener Test");
            LogAssert.Expect(LogType.Log, "ListenerTwo ServiceA registered: True");
            LogAssert.Expect(LogType.Log, "ServiceA is doing something. Two Listener Test");
            LogAssert.Expect(LogType.Log, "ListenerOne ServiceA registered: False");
            LogAssert.Expect(LogType.Log, "ListenerTwo ServiceA registered: False");

            void ListenerOne(ServiceA service, bool registered)
            {
                Debug.Log($"ListenerOne {service.GetType().Name} registered: {registered}");

                if (registered)
                {
                    service.DoSomething();
                }
            }

            void ListenerTwo<T>(T service, bool registered) where T : IDoIt
            {
                Debug.Log($"ListenerTwo {service.GetType().Name} registered: {registered}");

                if (registered)
                {
                    service.DoSomething();
                }
            }

        }
    }
}
