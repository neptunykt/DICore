using DICore3.Abstractions;

namespace DICore3.ServiceLookup;

internal static class ServiceDescriptorExtensions
{
    public static bool HasImplementationInstance(this ServiceDescriptor serviceDescriptor) => GetImplementationInstance(serviceDescriptor) != null;

    public static bool HasImplementationFactory(this ServiceDescriptor serviceDescriptor) => GetImplementationFactory(serviceDescriptor) != null;

    public static bool HasImplementationType(this ServiceDescriptor serviceDescriptor) => GetImplementationType(serviceDescriptor) != null;

    public static object? GetImplementationInstance(this ServiceDescriptor serviceDescriptor)
    {
        return serviceDescriptor.ImplementationInstance;
    }

    public static object? GetImplementationFactory(this ServiceDescriptor serviceDescriptor)
    {
        return serviceDescriptor.ImplementationFactory;
    }
    
    public static Type? GetImplementationType(this ServiceDescriptor serviceDescriptor)
    {
        return serviceDescriptor.ImplementationType;
    }

    public static bool TryGetImplementationType(this ServiceDescriptor serviceDescriptor, out Type? type)
    {
        type = GetImplementationType(serviceDescriptor);
        return type != null;
    }
}