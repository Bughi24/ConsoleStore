using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ConsoleStore.Models;

namespace ConsoleStore.Data;

public class ConsoleStoreContext : IdentityDbContext<IdentityUser>
{
    public ConsoleStoreContext(DbContextOptions<ConsoleStoreContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ShoppingCartItem> ShoppingCartItem => Set<ShoppingCartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>(); 
    public DbSet<FavoriteItem> FavoriteItems => Set<FavoriteItem>();
}
