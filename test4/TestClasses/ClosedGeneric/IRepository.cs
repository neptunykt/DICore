namespace test4.TestClasses.ClosedGeneric;

// === ИНТЕРФЕЙС ===
public interface IRepository<T> where T : class
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    void Save(T entity);
}