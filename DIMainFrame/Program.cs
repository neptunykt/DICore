// See https://aka.ms/new-console-template for more information

using DIMainFrame.Classes;


var serviceProvider = new ServiceProvider();

var scopes = new List<ServiceProviderEngineScope>();
var objects = new List<object>();
for (var i = 0; i < 50; i++)
{
   
    scopes.Add(new ServiceProviderEngineScope(serviceProvider));
    
}

foreach (var scope in scopes)
{
    var item = scope.GetService(new ServiceIdentifier { ServiceType = typeof(ICloset) });
    objects.Add(item);
}

var result = objects.ToArray();


