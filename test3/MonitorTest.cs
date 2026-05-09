using System.Collections.Concurrent;
using Xunit;
using Xunit.Abstractions;

namespace test3;

public class MonitorTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private Dictionary<int, long> Resolved { get; set; } = new Dictionary<int, long>();
    // Очередь для сбора логов из разных потоков
    private ConcurrentQueue<string> Logs { get; } = new();
    
    public MonitorTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    private void MonitorTestMethod()
    {
        var threads = new List<Thread>();
        for (var j = 10; j > 0; j--)
        {
            var number = j;
            // Запуск в 10 потоках расчета факториала
            var thread = new Thread(() =>
            {
                var result = CalculateFactorial(number, 0);
                Logs.Enqueue($"ThreadId: {Thread.CurrentThread.ManagedThreadId} j: {number} Value: {result}");
            });
            threads.Add(thread);
            thread.Start();
        }
        // Ждем завершения всех потоков (вместо Sleep)
        foreach (var t in threads) t.Join();
        // Теперь, когда все потоки завершены, безопасно выводим логи в отчет xUnit
        foreach (var log in Logs) _testOutputHelper.WriteLine(log);
        Assert.Equal(10, Resolved.Count);
    }


    /// <summary>
    /// Принцип работы VisitCache из DI-контейнера
    /// </summary>
    private long CalculateFactorial(int n, int contextLock)
    {
        bool lockTaken = false;
        object sync = Resolved;

        // ВАЖНО: Сначала захватываем замок, если мы на верхнем уровне.
        // Это гарантирует, что TryGetValue ниже будет потокобезопасным.
        if (contextLock == 0)
        {
            Monitor.Enter(sync, ref lockTaken);
        }

        try
        {
            // Имитируем задержку как при создании тяжелого сервиса
            Thread.Sleep(10); 

            // Теперь чтение из Dictionary безопасно, так как мы под локом
            if (Resolved.TryGetValue(n, out long resolved))
            {
                return resolved;
            }

            long value;
            if (n <= 1)
            {  
                Resolved[n] = 1;
                Logs.Enqueue($"Value: 1, resolved by ThreadId: {Thread.CurrentThread.ManagedThreadId}");
                return 1;
                
            }
            
                // Рекурсия: передаем contextLock = 1, замок не перезахватывается, 
                // но продолжает удерживаться текущим потоком.
                value = n * CalculateFactorial(n - 1, 1);
                Logs.Enqueue($"Value: {value}, resolved by ThreadId: {Thread.CurrentThread.ManagedThreadId}");

            // Запись безопасна, так как замок не отпускался с самого начала вызова
            Resolved.Add(n, value);
            return value;
        }
        finally
        {
            // Отпускаем замок только на самом верхнем уровне рекурсии
            if (lockTaken)
            {
                Monitor.Exit(sync);
            }
        }
    }
}