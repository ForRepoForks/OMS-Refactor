using System;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.API.Data;

namespace OrderManagementSystem.Tests.TestHelpers
{
    public static class DbContextTestHelper
    {
        /// <summary>
        /// Returns DbContextOptions configured for a real PostgreSQL database using Npgsql.
        /// This does NOT use an in-memory database. The connection string is taken from the
        /// OMS_TEST_DB environment variable if set, otherwise a default is used.
        /// </summary>
        public static DbContextOptions<OrderManagementContext> GetTestDbOptions()
        {
            // Prefer config/environment variable in real CI; fallback to constant for local/dev
            var connectionString = Environment.GetEnvironmentVariable("OMS_TEST_DB") ?? "Host=localhost;Port=5432;Database=omsdb;Username=omsuser;Password=omspassword";
            return new DbContextOptionsBuilder<OrderManagementContext>()
                .UseNpgsql(connectionString)
                .Options;
        }
    }
}
