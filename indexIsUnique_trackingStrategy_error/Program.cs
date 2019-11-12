using System;
using Microsoft.EntityFrameworkCore;

namespace indexIsUnique_trackingStrategy_error
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new CoreContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var product = new Product
                {
                    Name = "Product 1",
                };
                db.Products.Add(product);
                db.SaveChanges();
                System.Threading.Thread.Sleep(5000);
                db.Products.Remove(product);
                db.SaveChanges();
            }
        }

        public class CoreContext : DbContext
        {
            public DbSet<Product> Products { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=testApp;Trusted_Connection=True;");
                optionsBuilder.ConfigureWarnings(b =>
                {
                    b.Default(WarningBehavior.Throw);
                    b.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.SensitiveDataLoggingEnabledWarning);
                });
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.UseLoggerFactory(new Microsoft.Extensions.Logging.LoggerFactory(new[] { new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider() }));
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
                modelBuilder.Entity<Product>(entity =>
                {
                    entity.HasKey(e => e.ProductId);
                    entity.HasIndex(e => e.BarCode).IsUnique();
                    entity.Property(e => e.ProductId).ValueGeneratedOnAdd();
                });
            }
        }

        public class Product : INotifyEntity
        {
            private Guid _productId;
            private string _name;
            private int _barCode;

            public Guid ProductId
            {
                get => _productId;
                set => SetWithNotify(ref _productId, value);
            }

            public string Name
            {
                get => _name;
                set => SetWithNotify(ref _name, value);
            }

            public int BarCode
            {
                get => _barCode;
                set => SetWithNotify(ref _barCode, value);
            }
        }
    }
}
