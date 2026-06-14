using DICore4;
using DICore4.Abstractions;
using test4.TestClasses.ClosedGeneric;
using Xunit;
using Xunit.Abstractions;

namespace test4.TestClasses;

  public class ClosedGenericsTest
{
    private readonly ITestOutputHelper _output;
    private readonly IServiceProvider _serviceProvider;

    // ✅ ITestOutputHelper внедряется xUnit автоматически
    public ClosedGenericsTest(ITestOutputHelper output)
    {
        _output = output;

        // Настраиваем DI контейнер для тестов
        var services = new ServiceCollection();

        // Регистрируем закрытый generic репозитория
        services.AddTransient<IRepository<User>, Repository<User>>();

        // Регистрируем сервис
        services.AddTransient<UserService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void GetUser_ExistingId_ReturnsUser()
    {
        // Arrange
        _output.WriteLine("=== Тест: GetUser_ExistingId_ReturnsUser ===");
        var userService = _serviceProvider.GetService<UserService>();

        // Act
        var user = userService.GetUser(1);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(1, user.Id);
        Assert.Equal("Иван Иванов", user.Name);
        Assert.Equal("ivan@example.com", user.Email);
        
        _output.WriteLine($"✓ Найден пользователь: {user.Name} ({user.Email})");
    }

    [Fact]
    public void GetUser_NonExistingId_ReturnsNull()
    {
        // Arrange
        _output.WriteLine("=== Тест: GetUser_NonExistingId_ReturnsNull ===");
        var userService = _serviceProvider.GetService<UserService>();

        // Act
        var user = userService.GetUser(999);

        // Assert
        Assert.Null(user);
        
        _output.WriteLine("✓ Пользователь не найден (возвращен null)");
    }

    [Fact]
    public void GetAllUsers_ReturnsAllUsers()
    {
        // Arrange
        _output.WriteLine("=== Тест: GetAllUsers_ReturnsAllUsers ===");
        var userService = _serviceProvider.GetService<UserService>();

        // Act
        var users = userService.GetAllUsers().ToList();

        // Assert
        Assert.NotNull(users);
        Assert.Equal(3, users.Count);
        Assert.Contains(users, u => u.Name == "Иван Иванов");
        Assert.Contains(users, u => u.Name == "Петр Петров");
        Assert.Contains(users, u => u.Name == "Анна Сидорова");
        
        _output.WriteLine($"✓ Найдено пользователей: {users.Count}");
        foreach (var user in users)
        {
            _output.WriteLine($"  - {user.Name} ({user.Email})");
        }
    }

    [Fact]
    public void SaveUser_DoesNotThrow()
    {
        // Arrange
        _output.WriteLine("=== Тест: SaveUser_DoesNotThrow ===");
        var userService = _serviceProvider.GetService<UserService>();
        var newUser = new User
        {
            Id = 10,
            Name = "Новый Пользователь",
            Email = "new@example.com"
        };

        // Act & Assert
        var exception = Record.Exception(() => userService.SaveUser(newUser));
        Assert.Null(exception);
        
        _output.WriteLine("✓ Пользователь успешно сохранен (без исключений)");
    }

    [Theory]
    [InlineData(1, "Иван Иванов", "ivan@example.com")]
    [InlineData(2, "Петр Петров", "petr@example.com")]
    [InlineData(3, "Анна Сидорова", "anna@example.com")]
    public void GetUser_Theory_ReturnsCorrectData(int id, string expectedName, string expectedEmail)
    {
        // Arrange
        _output.WriteLine($"=== Тест: GetUser_Theory id={id} ===");
        var userService = _serviceProvider.GetService<UserService>();

        // Act
        var user = userService.GetUser(id);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(expectedName, user.Name);
        Assert.Equal(expectedEmail, user.Email);
        
        _output.WriteLine($"✓ id={id} → {user.Name} ({user.Email})");
    }
}    