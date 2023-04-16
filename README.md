# UnityServiceLocator

Simple event-driven service locator. Since intialization order and lifespan of Components in Unity cannot be relied on, you register to receive a callback when a service is registered/unregistered. If the service has already been registered the callback is invoked immediately.

In theory it's easier to write tests that depend on a service's interface (which can be mocked during testing) than it is to write tests that depend on a singleton instance (harder to mock).