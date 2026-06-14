namespace test4.TestClasses.ClosedGeneric;


public class UserService
{
    private readonly IRepository<User> _userRepository;

    // DI внедряет только закрытый generic репозитория
    public UserService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public User? GetUser(int id)
    {
        return _userRepository.GetById(id);
    }

    public IEnumerable<User> GetAllUsers()
    {
        return _userRepository.GetAll();
    }

    public void SaveUser(User user)
    {
        _userRepository.Save(user);
    }
}