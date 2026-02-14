// See https://aka.ms/new-console-template for more information

using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;


BenchmarkRunner.Run<ObjectCreationBenchmark>();

[MemoryDiagnoser]
[RankColumn]
public class ObjectCreationBenchmark
{
    private readonly Func<int, string, int, Person> _expressionFactory;

    public ObjectCreationBenchmark()
    {
        
        // 1. Создаем скомпилированное дерево выражений для конструктора с параметрами
        // Создаем ParameterExpression-ы для конструктора
        var idParam = Expression.Parameter(typeof(int), "id");
        var nameParam = Expression.Parameter(typeof(string), "name");
        var ageParam = Expression.Parameter(typeof(int), "age");
        // получаем конструктор constructorWithArgs типа ConstructorInfo
        var constructorWithArgs = typeof(Person).GetConstructor(new[]
        {
            typeof(int),
            typeof(string),
            typeof(int)
        });
        // создаем выражение типа NewExpression
        var newExpression = Expression.New(
            constructorWithArgs!,
            idParam,
            nameParam,
            ageParam
        );
        // создаем лямбду для выражения (создание узла дерева выражений)
        var lambda = Expression.Lambda<Func<int, string, int, Person>>(
            newExpression,
            idParam,
            nameParam,
            ageParam
        );
        // создаем делегат из лямбды
        _expressionFactory = lambda.Compile();
    }

    // 1. Прямое создание (new) - базовый метод
    [Benchmark(Baseline = true)]
    public Person DirectCreation()
    {
        return new Person(1, "John Doe", 30);
    }
    // 2. Рефлексия
    [Benchmark]
    public Person ConstructorInfoInvoke()
    {
        var constructor = typeof(Person).GetConstructor(
            new[] { typeof(int), typeof(string), typeof(int) }
        );

        return (Person)constructor!.Invoke(new object[] { 1, "John Doe", 30 });
    }

    // 3. Expression Tree (скомпилированное)
    [Benchmark]
    public Person ExpressionTree()
    {
        return _expressionFactory(1, "John Doe", 30);
    }
    
}

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }

    public Person()
    {
    }

    public Person(int id, string name, int age)
    {
        Id = id;
        Name = name;
        Age = age;
    }

    public override string ToString() => $"Person[{Id}: {Name}, {Age} лет]";
}