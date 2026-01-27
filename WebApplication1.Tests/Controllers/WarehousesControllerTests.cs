using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Controllers;
using WebApplication1.Data;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests
{
    public class WarehousesControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "WarehousesTestDb")
                .Options;

            return new ApplicationDbContext(options);
        }

        private WarehousesController GetController(ApplicationDbContext context, int? userId = 1)
        {
            var controller = new WarehousesController(context);

            // Mock HttpContext session
            var httpContext = new DefaultHttpContext();
            if (userId.HasValue)
                httpContext.Session.SetInt32("UserId", userId.Value);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        [Fact]
        public async Task Index_Should_Redirect_To_Login_If_Not_Authenticated()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = GetController(context, null); // not authenticated

            // Act
            var result = await controller.Index() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }

        [Fact]
        public async Task Index_Should_Return_View_With_Warehouses_When_Authenticated()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Warehouses.Add(new Warehouse { Id = 1, Name = "Test Warehouse", Address = "123 St" });
            await context.SaveChangesAsync();

            var controller = GetController(context);

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<System.Collections.Generic.List<Warehouse>>(result.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Create_Post_Should_Add_Warehouse_When_Model_Valid()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = GetController(context);

            var warehouse = new Warehouse { Name = "New Warehouse", Address = "456 St" };

            // Act
            var result = await controller.Create(warehouse) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            var created = await context.Warehouses.FirstOrDefaultAsync(w => w.Name == "New Warehouse");
            Assert.NotNull(created);
        }

        [Fact]
        public async Task Edit_Post_Should_Update_Warehouse_When_Model_Valid()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var warehouse = new Warehouse { Id = 1, Name = "Old Name", Address = "Old Address" };
            context.Warehouses.Add(warehouse);
            await context.SaveChangesAsync();

            var controller = GetController(context);
            warehouse.Name = "Updated Name";
            warehouse.Address = "Updated Address";

            // Act
            var result = await controller.Edit(1, warehouse) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            var updated = await context.Warehouses.FindAsync(1);
            Assert.Equal("Updated Name", updated.Name);
            Assert.Equal("Updated Address", updated.Address);
        }

        [Fact]
        public async Task DeleteConfirmed_Should_Remove_Warehouse()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var warehouse = new Warehouse { Id = 1, Name = "Delete Me", Address = "St" };
            context.Warehouses.Add(warehouse);
            await context.SaveChangesAsync();

            var controller = GetController(context);

            // Act
            var result = await controller.DeleteConfirmed(1) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            var deleted = await context.Warehouses.FindAsync(1);
            Assert.Null(deleted);
        }
    }
}
