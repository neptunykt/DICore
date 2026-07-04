// See https://aka.ms/new-console-template for more information

using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;


BenchmarkRunner.Run<ObjectCreationBenchmark>();

[MemoryDiagnoser]
[RankColumn]
public class ObjectCreationBenchmark
{
    private ConstructorInfo _ctor { get; set; }
    private Func<int, string, int, Person> _compiled { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _ctor = typeof(Person).GetConstructor(new[] { typeof(int), typeof(string), typeof(int) });
        
        var idParam = Expression.Parameter(typeof(int), "id");
        var nameParam = Expression.Parameter(typeof(string), "name");
        var ageParam = Expression.Parameter(typeof(int), "age");
        
        var newExpr = Expression.New(_ctor, idParam, nameParam, ageParam);
        var lambda = Expression.Lambda<Func<int, string, int, Person>>(
            newExpr, idParam, nameParam, ageParam);
        _compiled = lambda.Compile();
    }

    [Benchmark(Baseline = true)]
    public Person DirectNew()
    {
      return  new Person(1000, "John Doe", 3000);
    }

    [Benchmark]
    public object Reflection()
    {
        return _ctor.Invoke(new object[] { 1000, "John Doe", 3000 });
    }

    [Benchmark]
    public Person ExpressionTree()
    {

        return _compiled(1000, "John Doe", 3000);
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

    public override string ToString()
    {
      return  $"Person[{Id}: {Name}, {Age} лет]";
    }
}