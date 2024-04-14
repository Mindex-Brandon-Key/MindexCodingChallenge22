using System;
using System.Net.Http;
using System.Threading.Tasks;
using CodeChallenge.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCodeChallenge.Tests.Integration.Helpers
{
    public class TestServer : IDisposable, IAsyncDisposable
    {
        private WebApplicationFactory<Program> applicationFactory;

        public TestServer()
        {
            applicationFactory = new WebApplicationFactory<Program>();
            ResetDatabase();
        }

        public HttpClient NewClient()
        {
            return applicationFactory.CreateClient();
        }

        private void ResetDatabase()
        {
            using (var scope = applicationFactory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EmployeeContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                new EmployeeDataSeeder(db).Seed().Wait();
            }
        }

        public ValueTask DisposeAsync()
        {
            return ((IAsyncDisposable)applicationFactory).DisposeAsync();
        }

        public void Dispose()
        {
            ((IDisposable)applicationFactory).Dispose();
        }
    }
}
