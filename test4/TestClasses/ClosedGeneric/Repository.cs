namespace test4.TestClasses.ClosedGeneric;


public class Repository<T> : IRepository<T> where T : class
{
    private readonly Dictionary<int, T> _fakeDb = new();

    public Repository()
    {
        // Фиктивные данные только для User
        if (typeof(T) == typeof(User))
        {
            _fakeDb[1] = (T)(object)new User { Id = 1, Name = "Иван Иванов", Email = "ivan@example.com" };
            _fakeDb[2] = (T)(object)new User { Id = 2, Name = "Петр Петров", Email = "petr@example.com" };
            _fakeDb[3] = (T)(object)new User { Id = 3, Name = "Анна Сидорова", Email = "anna@example.com" };
        }
    }

    public T? GetById(int id)
    {
        return _fakeDb.TryGetValue(id, out var item) ? item : default;
    }

    public IEnumerable<T> GetAll()
    {
        return _fakeDb.Values;
    }

    public void Save(T entity)
    {
        // В реальном коде здесь была бы логика сохранения
    }
}