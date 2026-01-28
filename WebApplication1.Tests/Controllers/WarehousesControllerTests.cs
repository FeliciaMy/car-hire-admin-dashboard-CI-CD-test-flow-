using System.Threading.Tasks;
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
        [Fact]
        public async Task Index_Should_Redirect_To_Login_If_Not_Authenticated()
        {
            var context = TestHelper.CreateDbContext("Db_Warehouse_Unauth");
            var controller = TestHelper.CreateController<WarehousesController>(context);
            controller.HttpContext.Session.Clear(); // simulate unauthenticated

            var result = await controller.Index() as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Account", result.ControllerName);
        }

        [Fact]
        public async Task Index_Should_Return_View_With_Warehouses_When_Authenticated()
        {
            var context = TestHelper.CreateDbContext("Db_Warehouse_Valid");
            context.Warehouses.Add(new Warehouse { Id = 1, Name = "Test Warehouse", Address = "123 St" });
            await context.SaveChangesAsync();

            var controller = TestHelper.CreateController<WarehousesController>(context);

            var result = await controller.Index() as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<System.Collections.Generic.List<Warehouse>>(result.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Create_Post_Should_Add_Warehouse_When_Model_Valid()
        {
            var context = TestHelper.CreateDbContext("Db_Warehouse_Create");
            var controller = TestHelper.CreateController<WarehousesController>(context);

            var warehouse = new Warehouse { Name = "New Warehouse", Address = "456 St" };

            var result = await controller.Create(warehouse) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            var created = await context.Warehouses.FirstOrDefaultAsync(w => w.Name == "New Warehouse");
            Assert.NotNull(created);
        }

        [Fact]
        public async Task Edit_Post_Should_Update_Warehouse_When_Model_Valid()
        {
            var context = TestHelper.CreateDbContext("Db_Warehouse_Edit");
            var warehouse = new Warehouse { Id = 1, Name = "Old Name", Address = "Old Address" };
            context.Warehouses.Add(warehouse);
            await context.SaveChangesAsync();

            var controller = TestHelper.CreateController<WarehousesController>(context);
            warehouse.Name = "Updated Name";
            warehouse.Address = "Updated Address";

            var result = await controller.Edit(1, warehouse) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            var updated = await context.Warehouses.FindAsync(1);
            Assert.Equal("Updated Name", updated.Name);
            Assert.Equal("Updated Address", updated.Address);
        }

        [Fact]
        public async Task DeleteConfirmed_Should_Remove_Warehouse()
        {
            var context = TestHelper.CreateDbContext("Db_Warehouse_Delete");
            var warehouse = new Warehouse { Id = 1, Name = "Delete Me", Address = "St" };
            context.Warehouses.Add(warehouse);
            await context.SaveChangesAsync();

            var controller = TestHelper.CreateController<WarehousesController>(context);

            var result = await controller.DeleteConfirmed(1) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            var deleted = await context.Warehouses.FindAsync(1);
            Assert.Null(deleted);
        }
    }
}


