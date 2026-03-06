using Application.Interfaces;
using Common.Constants;
using Domain.Constant;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Persistence.Context;
using Persistence.Extension;

namespace WebApi.Initializer
{
    public class DatabaseInitializer
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            DatabaseInitializer initializer = new DatabaseInitializer();
            await initializer.SeedEverything(context);
        }

        private async Task SeedEverything(ApplicationDbContext context)
        {
            //SeedRoles(context);
            await SeedUsers(context);
        }

        //private void SeedRoles(ApplicationDbContext context)
        //{
        //    var roles = RoleNames.AllRoles;
        //    foreach (var role in roles)
        //    {
        //        if (!context.Roles.Any(p => p.Name == role))
        //        {
        //            context.Roles.Add(new Role() { Name = role, NormalizedName = role.ToUpper() });
        //        }
        //    }

        //    context.SaveChanges();
        //}

        private async Task SeedUsers(ApplicationDbContext context)
        {
            if (context.Users.Any())
            {
                return;
            }

            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();

            // Administrator
            //var admin = new User()
            //{
            //    Name = AppConstant.AdminName,
            //    UserName = AppConstant.AdminUserName,
            //    Status = "At The Office",
            //};
            //admin.PasswordHash = passwordHasher.HashPassword(admin, AppConstant.AdminPassword);

            //var User1 = new User()
            //{
            //    ChatId = Guid.NewGuid().ToString(),
            //    Name = "user1",
            //    UserName = "user1",
            //    Status = "Available",
            //};
            //User1.PasswordHash = passwordHasher.HashPassword(User1, "password");

            //var User2 = new User()
            //{
            //    ChatId = Guid.NewGuid().ToString(),
            //    Name = "user2",
            //    UserName = "user2",
            //    Status = "Available",
            //};
            //User2.PasswordHash = passwordHasher.HashPassword(User2, "password");
            
            //var User3 = new User()
            //{
            //    ChatId = Guid.NewGuid().ToString(),
            //    Name = "user3",
            //    UserName = "user3",
            //    Status = "Available",
            //};
            //User1.PasswordHash = passwordHasher.HashPassword(User3, "password");

            //await context.User.AddRangeAsync(User1, User2, User3);

            //await context.SaveChangesAsync();
        }
    }
}
