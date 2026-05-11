using System.Collections.Concurrent;
using Xunit;
using Xunit.Abstractions;

namespace test4
{
    /// <summary>
    /// Тест демонстрирует принцип контекстной блокировки,
    /// который используется в методе VisitCache реального CallSiteRuntimeResolver.
    /// Суть: при рекурсивном обходе графа зависимостей монитор захватывается
    /// только один раз на верхнем уровне рекурсии. Вложенные вызовы получают
    /// контекст (флаг), что блокировка уже удерживается, и не пытаются
    /// захватить её повторно.
    /// </summary>
    public class ContextLockTest
    {
        private readonly ITestOutputHelper _output;

        // Общий кэш (аналог ResolvedServices в ServiceProviderEngineScope)
        private readonly Dictionary<int, long> _cache = new Dictionary<int, long>();

        // Очередь логов для потокобезопасного сбора сообщений (в реальном xUnit выводе)
        private readonly ConcurrentQueue<string> _logs = new ConcurrentQueue<string>();

        public ContextLockTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ContextLock_PreventsReentrantMonitorEnter()
        {
            // Arrange: запускаем 10 потоков, каждый вычисляет факториал своего числа от 10 до 1.
            var threads = new List<Thread>();
            for (int i = 10; i >= 1; i--)
            {
                int number = i; // локальная копия для замыкания
                var thread = new Thread(() =>
                {
                    long result = CalculateFactorialWithContextLock(number, acquiredLocks: 0);
                    _logs.Enqueue($"Thread {Thread.CurrentThread.ManagedThreadId}, number={number}, result={result}");
                });
                threads.Add(thread);
                thread.Start();
            }

            // Ждём завершения всех потоков
            foreach (var t in threads)
                t.Join();

            // Выводим логи в отчёт xUnit
            foreach (var log in _logs)
                _output.WriteLine(log);

            // Проверяем, что кэш заполнился для всех чисел (10 записей)
            Assert.Equal(10, _cache.Count);
        }

        /// <summary>
        /// Рекурсивное вычисление факториала с контекстной блокировкой.
        /// </summary>
        /// <param name="n">Число, для которого вычисляется факториал.</param>
        /// <param name="acquiredLocks">
        /// Флаг, показывающий, была ли уже захвачена блокировка на вышестоящем уровне рекурсии.
        /// 0 – блокировка не захвачена (нужно захватить в этом вызове),
        /// 1 – блокировка уже захвачена (не захватываем повторно).
        /// В .NET 8 используется битовая маска (enum RuntimeResolverLock), здесь упрощённо.
        /// </param>
        private long CalculateFactorialWithContextLock(int n, int acquiredLocks)
        {
            bool lockTaken = false;
            object syncRoot = _cache; // объект синхронизации

            // Аналог проверки (context.AcquiredLocks & lockType) == 0
            if (acquiredLocks == 0)
            {
                // Захватываем монитор только на верхнем уровне рекурсии
                Monitor.Enter(syncRoot, ref lockTaken);
                _output.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] Захватил монитор на уровне n={n}");
            }
            else
            {
                _output.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] Уже в блокировке, повторный захват не требуется (n={n})");
            }

            try
            {
                // Имитируем "тяжёлую" операцию создания сервиса (в реальном DI – вызов конструктора)
                Thread.Sleep(5);

                // Потокобезопасное чтение из кэша (под защитой монитора)
                if (_cache.TryGetValue(n, out long cached))
                {
                    _output.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] Кэш для {n} = {cached}");
                    return cached;
                }

                // Базовый случай рекурсии
                if (n <= 1)
                {
                    _cache[n] = 1;
                    _output.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] Вычислил 1 для n={n}");
                    return 1;
                }

                // Рекурсивный вызов: передаём acquiredLocks = 1, чтобы вложенные вызовы не пытались захватить монитор.
                // Это аналог создания нового RuntimeResolverContext с обновлёнными AcquiredLocks.
                long subResult = CalculateFactorialWithContextLock(n - 1, acquiredLocks: 1);
                long result = n * subResult;

                // Безопасная запись в кэш (монитор всё ещё удерживается)
                _cache[n] = result;
                _output.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] Вычислил {n}! = {result}");
                return result;
            }
            finally
            {
                // Освобождаем монитор, только если мы его захватили в этом вызове
                if (lockTaken)
                {
                    Monitor.Exit(syncRoot);
                    _output.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] Освободил монитор (n={n})");
                }
            }
        }
    }
}