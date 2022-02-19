using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeXiecheng.API.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace FakeXiecheng.API.Database
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
              : base(options)
        {

        }

        public DbSet<TouristRoute> TouristRoutes { get; set; }
        public DbSet<TouristRoutePicture> TouristRoutePictures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*            modelBuilder.Entity<TouristRoute>().HasData(new TouristRoute() {
                            Id = Guid.NewGuid(),
                            Title = "ceshititle",
                            Description = "shuoming",
                            OriginalPrice = 0,
                            CreateTime = DateTime.UtcNow
                        });*/
            var touristRouteJsonData = File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"/Database/touristRoutesMockData.json");
            IList<TouristRoute> touristRoutes = JsonConvert.DeserializeObject<IList<TouristRoute>>(touristRouteJsonData);
            modelBuilder.Entity<TouristRoute>().HasData(touristRoutes);

            var touristRoutePictureJsonData = File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"/Database/touristRoutePicturesMockData.json");
            IList<TouristRoutePicture> touristRoutePictures = JsonConvert.DeserializeObject<IList<TouristRoutePicture>>(touristRoutePictureJsonData);
            modelBuilder.Entity<TouristRoutePicture>().HasData(touristRoutePictures);

            //Initial user and role "seed" data
            // 1. Update user and role foreign ken
            modelBuilder.Entity<ApplicationUser>(u =>
            u.HasMany(x => x.UserRoles)
            .WithOne().HasForeignKey(ur => ur.UserId).IsRequired()
            );
            // 2. Add admin role
            var adminRoleId = "39996f34-013c-4fc6-b1b3-0c1036c47110";
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole()
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "Admin".ToUpper()
                }
                );
            // 3. Add user
            var adminUserId = "79996f34-013c-4fc6-b1b3-0c1036c47118";
            ApplicationUser adminUser = new ApplicationUser
            {
                Id = adminUserId,
                UserName = "admin@fakexicheng.com",
                NormalizedUserName = "admin@fakexiecheng.com".ToUpper(),
                Email = "admin@fakexiecheng.com",
                NormalizedEmail = "admin@fakexiecheng.com".ToUpper(),
                TwoFactorEnabled = false,
                EmailConfirmed = true,
                PhoneNumber = "123456789",
                PhoneNumberConfirmed = false
            };
            var ph = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = ph.HashPassword(adminUser, "Fake123$");
            modelBuilder.Entity<ApplicationUser>().HasData(adminUser);
            // 4. Grant user Admin role
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>()
                {
                    RoleId = adminRoleId,
                    UserId = adminUserId
                });

            base.OnModelCreating(modelBuilder);
        }

    }
}
