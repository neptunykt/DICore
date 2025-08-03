// See https://aka.ms/new-console-template for more information

using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;


BenchmarkRunner.Run<ExpressionBenchmarkTest>();

        public class ExpressionBenchmarkTest
        {
           private Func<object> ExpressionByServiceDescriptor =
                ExpressionEx.GetDelegate(typeof(Qux1));

            [Benchmark]
            public void WithExpression()
            {
                // вызов делегата
                object service =  ExpressionByServiceDescriptor();

            }

            [Benchmark]
            public void WithReflection()
            {
                object service = ReflectionEx.GetDelegate(typeof(Qux1))();
            }

            [Benchmark]
            public void WithDirect()
            {
               object service = DirectEx.GetDelegate();

            }
        }

            public static class DirectEx
            {
                public static Func<object> GetDelegate()
                {
                    return () => new Qux1();
                }    
            }

        public static class ReflectionEx
        {
            public static Func<object> GetDelegate(Type qux)
            {
                ConstructorInfo constructor = qux.GetConstructor(Type.EmptyTypes);
                object service = constructor.Invoke(null);
                return () => service;
            }
        }

        public static class ExpressionEx
        {
            public static Func<object> GetDelegate(Type qux)
            {
                // Компилируем в делегат
                return Expression.Lambda<Func<object>>(Expression.New(qux)).Compile();;
            }
        }

        public interface IQux1
        {
        }

        public class Qux1 : IQux1
        {

        }
        