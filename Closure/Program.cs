// See https://aka.ms/new-console-template for more information

using Closure.Classes;

var engine = new ServiceProviderEngine();
var firstFunc = engine.RealizeService(new ServiceCallSite{Name = "план-схема объекта"});
firstFunc(new ServiceProviderEngineScope{Name = "Первый скоуп"});
firstFunc(new ServiceProviderEngineScope{Name = "Второй скоуп"});




