using Xunit;
using Xunit.Abstractions;

namespace test3;

public class MonitorTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private Dictionary<int, long> Resolved { get; set; } = new Dictionary<int, long>();

    public MonitorTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    private void MonitorTestMethod()
    {
        for (var j = 10; j > 0; j--)
        {
            var number = j;
            var thread = new Thread(() => { _testOutputHelper.WriteLine($"ThreadId: {Thread.CurrentThread.ManagedThreadId} j: {number} Value: {CalculateFactorial(10, 0).ToString()}"); });
            thread.Start();
        }
        Thread.Sleep(10000);
        Assert.Equal(true, Resolved.Count == 10);
    }


    /// <summary>
    /// Возврат значения long как object
    /// Принцип работы VisitCache
    /// </summary>
    /// <param name="n"></param>
    /// <param name="contextLock"></param>
    /// <returns></returns>
    private long CalculateFactorial(int n, int contextLock)
    {
        bool lockTaken = false;
        object sync = Resolved;
        // Блокировка только на верхнем уровне для contextLock = 0
        if (contextLock == 0)
        {
            Monitor.Enter(sync, ref lockTaken);
        }

        try
        {
            // Критическая область
            Thread.Sleep(100);
            if (Resolved.TryGetValue(n, out long resolved))
            {
                return resolved;
            }

            if (n <= 1)
            {
                _testOutputHelper.WriteLine(
                    $"Value: 1, resolved by ThreadId: {Thread.CurrentThread.ManagedThreadId.ToString()}");
                Resolved.Add(n, 1);
                return 1;
            }
            // Context перезаписывается, нет блокировки для своего потока
            var value = n * CalculateFactorial(n - 1, 1);
            _testOutputHelper.WriteLine(
                $"Value: {value}, resolved by ThreadId:{Thread.CurrentThread.ManagedThreadId.ToString()}");
            Resolved.Add(n, value);
            return value;
        }
        finally
        {
            if (lockTaken)
            {
                Monitor.Exit(sync);
            }
        }
    }
}