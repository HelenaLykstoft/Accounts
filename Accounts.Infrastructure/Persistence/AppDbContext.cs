using Microsoft.EntityFrameworkCore;
using Accounts.Domain.Entities;

namespace Accounts.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ContactInfo> ContactInfos { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<LoginInformation> LoginInformations { get; set; }
        public DbSet<UserType> UserTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Defines table names
            modelBuilder.Entity<User>().ToTable("app_user");
            modelBuilder.Entity<ContactInfo>().ToTable("contact_info");
            modelBuilder.Entity<Address>().ToTable("address");
            modelBuilder.Entity<City>().ToTable("city");
            modelBuilder.Entity<LoginInformation>().ToTable("login_information");
            modelBuilder.Entity<UserType>().ToTable("user_type");

            // Defines primary keys
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<ContactInfo>().HasKey(c => c.Id);
            modelBuilder.Entity<Address>().HasKey(a => a.Id);
            modelBuilder.Entity<City>().HasKey(c => c.PostalCode);
            modelBuilder.Entity<LoginInformation>().HasKey(l => l.Username);
            modelBuilder.Entity<UserType>().HasKey(u => u.Id);

            // Map properties to columns
            modelBuilder.Entity<User>()
                .ToTable("app_user")
                .Property(u => u.Id).HasColumnName("id");
            modelBuilder.Entity<User>()
                .Property(u => u.FirstName).HasColumnName("first_name");
            modelBuilder.Entity<User>()
                .Property(u => u.LastName).HasColumnName("last_name");
            modelBuilder.Entity<User>()
                .Property(u => u.Username).HasColumnName("username");
            modelBuilder.Entity<User>()
                .Property(u => u.UserTypeId).HasColumnName("user_type");
            modelBuilder.Entity<User>()
                .Property(u => u.ContactInfoId).HasColumnName("contact_info");

            // ContactInfo entity
            modelBuilder.Entity<ContactInfo>()
                .ToTable("contact_info")
                .Property(c => c.Id).HasColumnName("id");
            modelBuilder.Entity<ContactInfo>()
                .Property(c => c.Email).HasColumnName("email");
            modelBuilder.Entity<ContactInfo>()
                .Property(c => c.PhoneNumber).HasColumnName("phone_number");
            modelBuilder.Entity<ContactInfo>()
                .Property(c => c.AddressId).HasColumnName("address");

            // Address entity
            modelBuilder.Entity<Address>()
                .ToTable("address")
                .Property(a => a.Id).HasColumnName("id");
            modelBuilder.Entity<Address>()
                .Property(a => a.StreetNumber).HasColumnName("street_number");
            modelBuilder.Entity<Address>()
                .Property(a => a.StreetName).HasColumnName("street_name");
            modelBuilder.Entity<Address>()
                .Property(a => a.CityPostalCode).HasColumnName("city");

            // City entity
            modelBuilder.Entity<City>()
                .ToTable("city")
                .Property(c => c.PostalCode).HasColumnName("postal_code");
            modelBuilder.Entity<City>()
                .Property(c => c.Name).HasColumnName("name");

            // UserType entity
            modelBuilder.Entity<UserType>()
                .ToTable("user_type")
                .Property(u => u.Id).HasColumnName("id");
            modelBuilder.Entity<UserType>()
                .Property(u => u.Type).HasColumnName("type");

            // LoginInformation entity
            modelBuilder.Entity<LoginInformation>()
                .ToTable("login_information")
                .Property(l => l.Username).HasColumnName("username");
            modelBuilder.Entity<LoginInformation>()
                .Property(l => l.Password).HasColumnName("password");

            // Defines foreign key relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.ContactInfo)
                .WithMany()
                .HasForeignKey(u => u.ContactInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContactInfo>()
                .HasOne(ci => ci.Address) 
                .WithMany(a => a.ContactInfos) 
                .HasForeignKey(ci => ci.AddressId);

            modelBuilder.Entity<Address>()
                .HasOne(a => a.City)
                .WithMany(c => c.Addresses)
                .HasForeignKey(a => a.CityPostalCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserType) 
                .WithMany(ut => ut.Users)
                .HasForeignKey(u => u.UserTypeId) 
                .OnDelete(DeleteBehavior.Restrict); 


            modelBuilder.Entity<UserType>().HasData(
                new UserType { Id = 1, Type = "user" },
                new UserType { Id = 2, Type = "deliveryAgent" },
                new UserType { Id = 3, Type = "admin" }
            );

            base.OnModelCreating(modelBuilder);
        }

        public async Task AddDefaultUserTypesAsync()
        {
            if (!UserTypes.Any())
            {
                UserTypes.AddRange(
                    new UserType { Id = 1, Type = "user" },
                    new UserType { Id = 2, Type = "deliveryAgent" },
                    new UserType { Id = 3, Type = "admin" }
                );
                await SaveChangesAsync();
            }
        }
    }
}