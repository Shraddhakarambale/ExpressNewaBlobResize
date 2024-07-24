using Microsoft.Extensions.Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((hostContext,services) =>
    {
        services.AddAzureClients(ClientBuilder =>
        {
            ClientBuilder.AddBlobServiceClient(hostContext.Configuration.GetSection("AzureWebJobsStorage"))
            .WithName("copyBlob");

        });
 
    })
    .Build();

host.Run();
