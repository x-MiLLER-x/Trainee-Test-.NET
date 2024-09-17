using Microsoft.EntityFrameworkCore;
using Trainee_Test_.NET.Models;

namespace Trainee_Test_.NET.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Contact> Contacts { get; set; }
    }
}
