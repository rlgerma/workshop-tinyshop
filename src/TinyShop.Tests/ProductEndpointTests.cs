using System.Net;
using System.Net.Http.Json;
using DataEntities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Products.Data;
using Products.Endpoints;

namespace TinyShop.Tests;

[TestClass]
public class ProductEndpointTests
{
    [TestMethod]
    public async Task Post_CreatesProductWithGeneratedIdAndRetrievableLocation()
    {
        await using var api = await TestApi.StartAsync();
        var product = new Product { Id = 123, Name = "Tent", Description = "Lightweight", Price = 49.99m, ImageUrl = "tent.png" };

        var response = await api.Client.PostAsJsonAsync("/api/Product", product);

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Product>();
        Assert.IsNotNull(created);
        Assert.AreNotEqual(0, created.Id);
        Assert.AreNotEqual(product.Id, created.Id);
        Assert.AreEqual(product.Name, created.Name);
        Assert.AreEqual(product.Description, created.Description);
        Assert.AreEqual(product.Price, created.Price);
        Assert.AreEqual(product.ImageUrl, created.ImageUrl);
        Assert.IsNotNull(response.Headers.Location);

        var retrieved = await api.Client.GetFromJsonAsync<Product>(response.Headers.Location);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(created.Id, retrieved.Id);
        Assert.AreEqual(created.Name, retrieved.Name);
        var products = await api.Client.GetFromJsonAsync<List<Product>>("/api/Product");
        Assert.IsNotNull(products);
        Assert.AreEqual(1, products.Count);
        Assert.AreEqual(created.Id, products[0].Id);
    }

    [TestMethod]
    public async Task Put_ReplacesFieldsOfProductIdentifiedByRoute()
    {
        await using var api = await TestApi.StartAsync();
        var createdResponse = await api.Client.PostAsJsonAsync("/api/Product", new Product { Name = "Old", Price = 1m });
        var created = await createdResponse.Content.ReadFromJsonAsync<Product>();
        Assert.IsNotNull(created);
        var replacement = new Product { Id = 999, Name = "New", Description = "Updated", Price = 12.50m, ImageUrl = "new.png" };

        var response = await api.Client.PutAsJsonAsync($"/api/Product/{created.Id}", replacement);

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        var updated = await api.Client.GetFromJsonAsync<Product>($"/api/Product/{created.Id}");
        Assert.IsNotNull(updated);
        Assert.AreEqual(created.Id, updated.Id);
        Assert.AreEqual(replacement.Name, updated.Name);
        Assert.AreEqual(replacement.Description, updated.Description);
        Assert.AreEqual(replacement.Price, updated.Price);
        Assert.AreEqual(replacement.ImageUrl, updated.ImageUrl);
        Assert.AreEqual(HttpStatusCode.NotFound, (await api.Client.GetAsync("/api/Product/999")).StatusCode);
    }

    [TestMethod]
    public async Task Delete_RemovesProductAndMissingOperationsReturnNotFound()
    {
        await using var api = await TestApi.StartAsync();
        var createdResponse = await api.Client.PostAsJsonAsync("/api/Product", new Product { Name = "Tent" });
        var created = await createdResponse.Content.ReadFromJsonAsync<Product>();
        Assert.IsNotNull(created);
        var path = $"/api/Product/{created.Id}";

        Assert.AreEqual(HttpStatusCode.NoContent, (await api.Client.DeleteAsync(path)).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await api.Client.GetAsync(path)).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await api.Client.DeleteAsync(path)).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await api.Client.PutAsJsonAsync(path, new Product { Name = "Missing" })).StatusCode);
        var products = await api.Client.GetFromJsonAsync<List<Product>>("/api/Product");
        Assert.IsNotNull(products);
        Assert.AreEqual(0, products.Count);
    }

    private sealed class TestApi(WebApplication app, HttpClient client) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;

        public static async Task<TestApi> StartAsync()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            var databaseName = Guid.NewGuid().ToString();
            builder.Services.AddDbContext<ProductDataContext>(options => options.UseInMemoryDatabase(databaseName));
            var app = builder.Build();
            app.MapProductEndpoints();
            await app.StartAsync();
            var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
            var client = new HttpClient { BaseAddress = new Uri(addresses!.Addresses.Single()) };
            return new TestApi(app, client);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.DisposeAsync();
        }
    }
}