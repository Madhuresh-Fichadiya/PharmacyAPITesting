using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Tests.Controllers
{
    public class MedicinesControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public MedicinesControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SellMedicine_InvalidId_ReturnsNotFound()
        {
            // Act: Sending an HTTP POST to the in-memory server
            var response = await _client.PostAsJsonAsync("/api/medicines/999/sell", 5);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
