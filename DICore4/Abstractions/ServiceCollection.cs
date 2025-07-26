using System.Collections;
using DICore4.Abstractions;

namespace DICore4.Abstractions;

public class ServiceCollection : IServiceCollection
{
    private readonly List<ServiceDescriptor> _descriptors = new List<ServiceDescriptor>();
    public IEnumerator<ServiceDescriptor> GetEnumerator()
    {
        return _descriptors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(ServiceDescriptor item)
    {
        _descriptors.Add(item);
    }

    public void Clear()
    {
        _descriptors.Clear();
    }

    public bool Contains(ServiceDescriptor item)
    {
        return _descriptors.Contains(item);
    }

    public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
    {
        _descriptors.CopyTo(array, arrayIndex);
    }

    public bool Remove(ServiceDescriptor item)
    {
        return _descriptors.Remove(item);
    }

    public int Count => _descriptors.Count;
    public bool IsReadOnly { get; }
    public int IndexOf(ServiceDescriptor item)
    {
        return _descriptors.IndexOf(item);
    }

    public void Insert(int index, ServiceDescriptor item)
    {
        _descriptors.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _descriptors.RemoveAt(index);
    }

    public ServiceDescriptor this[int index]
    {
        get => _descriptors[index];
        set => _descriptors[index] = value;
    }
    
}