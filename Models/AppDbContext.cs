using App.Models.Contacts;
using App.Models.Product;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            base.OnConfiguring(builder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            //Mỗi Entity<> đại diện cho 1 bảng để thiết lập các tính chất cho bảng đó

            modelBuilder.Entity<ProductModel>(entity =>
            {
                entity.HasIndex(p => p.Slug)
                        .IsUnique();
            });

            modelBuilder.Entity<CategoryProduct>(entity =>
            {
                entity.HasIndex(c => c.Slug)
                        .IsUnique();
            });

            modelBuilder.Entity<Product.ProductCategoryProduct>(entity =>
            {
                entity.HasKey(pc => new { pc.ProductId, pc.CategoryProductId }); //thiết lập khóa chính cho bảng
            });

            modelBuilder.Entity<OrderProduct>(entity => 
            {
                entity.HasKey(op => new {op.ProductId, op.OrderId}); //thiết lập khóa chính cho bảng OrderProduct
            });

            modelBuilder.Entity<ProductAttribute>(entity => {
                entity.HasIndex(pa => pa.AttributeName)
                        .IsUnique();
                
            });

            modelBuilder.Entity<ProductAttributeValue>(entity =>
            {
                entity.HasKey(ptp => new {ptp.ProductId, ptp.AttributeId});
            });

            modelBuilder.Entity<Voucher>(entity => 
            {
                entity.HasOne(v => v.User)
                    .WithMany(p => p.Vouchers)
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Voucher_AppUser");

                entity.HasOne(v => v.Order)
                        .WithMany(o => o.Vouchers)
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("FK_Voucher_Order");
            });

            modelBuilder.Entity<Order>(entity => 
            {
                entity.HasOne(o => o.user) 
                        .WithMany(u => u.Orders)
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("FK_Order_User");
            });

        }

        public DbSet<ContactModel> Contacts { set; get; }

        public DbSet<ProductModel> Products { set; get; }

        public DbSet<CategoryProduct> CategoryProducts { set; get; }

        public DbSet<ProductCategoryProduct> ProductCategoryProducts { set; get; }

        public DbSet<ProductAttribute> ProductAttributes {set; get;}

        public DbSet<ProductAttributeValue> ProductAttributeValues {set; get;}

        public DbSet<ProductPhoto> ProductPhotos {set; get;}

        public DbSet<Order> Orders {set; get;}

        public DbSet<Voucher> Vouchers {set; get;}

        public DbSet<OrderProduct> OrderProducts {set; get;}
    }
}