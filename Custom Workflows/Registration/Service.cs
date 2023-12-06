using Elsa.Persistence.EntityFramework.Core;
using Microsoft.EntityFrameworkCore;


namespace ElsaGuides.ContentApproval.Web.Entity
{
    public class MyDbContext : ElsaContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public DbSet<Register> MyEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
