using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebApplication1.Controllers;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using Xunit;

namespace WebApplication1.Tests
{
    public class AccountControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            var context = new ApplicationDbContext(options);
            return context;
        }

        private AccountController GetController(ApplicationDbContext context)
        {
            var controller = new AccountController(context);

            // Mock HttpContext for session
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        [Fact]
        public async Task Register_Should_Create_User_When_Model_Is_Valid()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = GetController(context);

            var model = new RegisterViewModel
            {
                FirstName = "Felicia",
                LastName = "Mayeyane",
                Email = "felicia@test.com",
                Password = "Password123",
                ContactNumber = "1234567890",
                Address = "123 Street"
            };

            // Act
            var result = await controller.Register(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);

            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "felicia@test.com");
            Assert.NotNull(user);
            Assert.Equal("Felicia", user.FirstName);
        }

        [Fact]
        public async Task Register_Should_Return_ModelError_When_Email_Exists()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Users.Add(new User { Email = "felicia@test.com" });
            await context.SaveChangesAsync();

            var controller = GetController(context);
            var model = new RegisterViewModel { Email = "felicia@test.com" };

            // Act
            var result = await controller.Register(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey("Email"));
        }

        [Fact]
        public async Task Login_Should_RedirectToDashboard_When_Credentials_Are_Valid()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new User { Id = 1, FirstName = "Felicia", LastName = "Mayeyane", Email = "test@test.com", Password = "Password123" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = GetController(context);
            var model = new LoginViewModel { Email = "test@test.com", Password = "Password123" };

            // Act
            var result = await controller.Login(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Dashboard", result.ControllerName);
            Assert.Equal(1, controller.HttpContext.Session.GetInt32("UserId"));
        }

        [Fact]
        public async Task Login_Should_Return_ModelError_When_Credentials_Invalid()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = GetController(context);
            var model = new LoginViewModel { Email = "wrong@test.com", Password = "badpassword" };

            // Act
            var result = await controller.Login(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(""));
        }

        [Fact]
        public void Logout_Should_Clear_Session_And_RedirectToLogin()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = GetController(context);

            controller.HttpContext.Session.SetInt32("UserId", 1);

            // Act
            var result = controller.Logout() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Null(controller.HttpContext.Session.GetInt32("UserId"));
        }
    }
}
