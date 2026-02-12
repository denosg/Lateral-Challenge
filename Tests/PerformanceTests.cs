using Database;
using Database.Models;
using Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class SimplePerformanceTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly AppDbContext _db;
        private readonly string _connectionString =
            "Server=localhost,1433;Database=ShipmentAPI_PerfTests;User Id=sa;Password=LateralInterview!1;TrustServerCertificate=True;";

        public SimplePerformanceTests(ITestOutputHelper output)
        {
            _output = output;

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(_connectionString)
                .Options;

            _db = new AppDbContext(options);

            _db.Database.EnsureDeleted();
            _db.Database.EnsureCreated();
        }

        [Fact]
        public async Task GetById_ShouldBeFast()
        {
            var tenant = "test";
            var shipmentId = await SeedTestData(100_000, tenant);

            var sw = Stopwatch.StartNew();
            var shipment = await _db.Shipments.FindAsync(shipmentId);
            sw.Stop();

            _output.WriteLine($"Time to find 1 record from 100,000: {sw.ElapsedMilliseconds}ms");
            Assert.NotNull(shipment);
            Assert.True(sw.ElapsedMilliseconds < 500, $"Too slow: {sw.ElapsedMilliseconds}ms"); //Cold Start
        }

        [Fact]
        public async Task GetByStatus_ShouldBeFast()
        {
            var tenant = "test";
            await SeedTestData(100_000, tenant);

            var sw = Stopwatch.StartNew();
            var shipments = await _db.Shipments
                .Where(s => s.Status == ShipmentStatus.InTransit && s.Tenant.Equals(tenant) && !s.IsDeleted)
                .ToListAsync();
            sw.Stop();

            _output.WriteLine($"Time to query by status from 100,000 records: {sw.ElapsedMilliseconds}ms");
            _output.WriteLine($"Found {shipments.Count} shipments");
            Assert.True(sw.ElapsedMilliseconds < 1000, $"Too slow: {sw.ElapsedMilliseconds}ms");
        }

        [Fact]
        public async Task GetPaginated_ShouldBeFast()
        {
            var tenant = "test";
            await SeedTestData(100_000, tenant);

            var sw = Stopwatch.StartNew();
            var shipments = await _db.Shipments
                .Where(s => !s.IsDeleted && s.Tenant.Equals(tenant))
                .OrderByDescending(s => s.CreatedAt)
                .Skip(0)
                .Take(50)
                .ToListAsync();
            sw.Stop();

            _output.WriteLine($"Time to get page 1 from 100,000 records: {sw.ElapsedMilliseconds}ms");
            Assert.Equal(50, shipments.Count);
            Assert.True(sw.ElapsedMilliseconds < 800, $"Too slow: {sw.ElapsedMilliseconds}ms");
        }

        [Fact]
        public async Task Create1000Shipments_ShouldComplete()
        {
            var tenant = "test";

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
            {
                var shipment = new Shipment
                {
                    ID = NewId.NextSequentialGuid(),
                    TrackingNumber = $"TEST-{Guid.NewGuid()}",
                    RecipientName = $"Recipient {i}",
                    Status = ShipmentStatus.Created,
                    CreatedAt = DateTime.UtcNow,
                    Version = NewId.NextSequentialGuid(),
                    Tenant = tenant,
                    IsDeleted = false
                };

                _db.Shipments.Add(shipment);

                if ((i + 1) % 100 == 0)
                {
                    await _db.SaveChangesAsync();
                    _db.ChangeTracker.Clear();
                }
            }

            await _db.SaveChangesAsync();
            sw.Stop();

            _output.WriteLine($"Time to create 1,000 shipments: {sw.ElapsedMilliseconds}ms");
            Assert.True(sw.ElapsedMilliseconds < 30000, $"Too slow: {sw.ElapsedMilliseconds}ms");
        }

        [Fact]
        public async Task ConcurrentReads_ShouldWork()
        {
            var tenant = "test";
            var shipmentIds = await SeedTestDataMultiple(100_000, tenant);

            var tasks = new List<Task>();
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < 50; i++)
            {
                var id = shipmentIds[i % shipmentIds.Count];
                tasks.Add(Task.Run(async () =>
                {
                    var options = new DbContextOptionsBuilder<AppDbContext>()
                        .UseSqlServer(_connectionString)
                        .Options;
                    using var db = new AppDbContext(options);
                    var shipment = await db.Shipments.FindAsync(id);
                }));
            }

            await Task.WhenAll(tasks);
            sw.Stop();

            _output.WriteLine($"Time for 50 concurrent reads: {sw.ElapsedMilliseconds}ms");
            Assert.True(sw.ElapsedMilliseconds < 2000, $"Too slow: {sw.ElapsedMilliseconds}ms");
        }

        private async Task<Guid> SeedTestData(int count, string tenant)
        {
            var ids = await SeedTestDataMultiple(count, tenant);
            return ids[count / 2];
        }

        private async Task<List<Guid>> SeedTestDataMultiple(int count, string tenant)
        {
            var ids = new List<Guid>();
            var statuses = new[] { ShipmentStatus.Created, ShipmentStatus.InTransit };
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                var id = NewId.NextSequentialGuid();
                var shipment = new Shipment
                {
                    ID = id,
                    TrackingNumber = $"PERF-{i:D10}",
                    RecipientName = $"Recipient {i}",
                    Status = statuses[random.Next(statuses.Length)],
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30)),
                    Version = NewId.NextSequentialGuid(),
                    Tenant = tenant,
                    IsDeleted = false
                };

                _db.Shipments.Add(shipment);
                ids.Add(id);

                if ((i + 1) % 1000 == 0)
                {
                    await _db.SaveChangesAsync();
                    _db.ChangeTracker.Clear();
                }
            }

            await _db.SaveChangesAsync();
            _db.ChangeTracker.Clear();

            return ids;
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }
    }
}
