using System.Linq.Expressions;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context
{
    public class ApplicationDbContext
        : IdentityDbContext<
            User,
            Role,
            string,
            UserClaim,
            UserRole,
            IdentityUserLogin<string>,
            RoleClaim,
            IdentityUserToken<string>
        >
    {
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<UserSetting> UserSettings { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ProductImage> ProductImage { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }
        public DbSet<Review> Review { get; set; }
        public DbSet<WishList> WishList { get; set; }
        public DbSet<Cart> Cart { get; set; }


        // public DbSet<AuditLog> AuditLogs { get; set; }
        // public DbSet<AppSetting> AppSettings { get; set; }
        // public DbSet<AppNotification> Notifications { get; set; }
        // public DbSet<UserSetting> UserSettings { get; set; }
        // public DbSet<Subscription> Subscriptions { get; set; }
        // public DbSet<Company> Companies { get; set; }
        // public DbSet<CompanyBranch> CompanyBranches { get; set; }
        // public DbSet<CompanyStaff> CompanyStaffs { get; set; }
        //public DbSet<AdminRole> AdminRoles { get; set; }
        // public DbSet<AdminRoleClaim> AdminRoleClaims { get; set; }
        // public DbSet<CompanyRole> CompanyRoles { get; set; }
        // public DbSet<CompanyRoleClaim> CompanyRoleClaims { get; set; }
        // public DbSet<LoanOffer> LoanOffers { get; set; }
        // public DbSet<LoanRequest> LoanRequests { get; set; }
        // public DbSet<PayRoll> PayRolls { get; set; }
        // public DbSet<Transaction> Transactions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(c =>  c.Email)
                .IsUnique();
            
            modelBuilder.Entity<Product>()
                .Property(p => p.RowVersion)
                .IsRowVersion(); 


            //modelBuilder.UseEncryption(_provider);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IBase).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var isDeletedProperty = Expression.Property(parameter, nameof(IBase.IsDeleted));
                    var condition = Expression.Equal(isDeletedProperty, Expression.Constant(false));
                    var lambda = Expression.Lambda(condition, parameter);
                    entityType.SetQueryFilter(lambda);
                }
            }
            foreach (
                var relationship in modelBuilder
                    .Model.GetEntityTypes()
                    .SelectMany(e => e.GetForeignKeys())
            )
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        public override int SaveChanges()
        {
            UpdateAuditEntities();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateAuditEntities();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            UpdateAuditEntities();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void UpdateAuditEntities()
        {
            var modifiedEntries = ChangeTracker
                .Entries()
                .Where(x =>
                    x.Entity is IBase
                    && (x.State == EntityState.Added || x.State == EntityState.Modified)
                );

            foreach (var entry in modifiedEntries)
            {
                var entity = (IBase)entry.Entity;
                DateTime now = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedDate = now;
                }
                else
                {
                    base.Entry(entity).Property(x => x.CreatedDate).IsModified = false;
                }

                entity.UpdatedDate = now;
            }
        }
    }
}
