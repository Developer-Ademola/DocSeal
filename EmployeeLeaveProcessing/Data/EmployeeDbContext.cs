using EmployeeLeaveProcessing.Model;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveProcessing.Data
{
    public class EmployeeDbContext : DbContext
    {
        public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeLeave> Leaves { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Leaves)
                .WithOne(l => l.Employee)
                .HasForeignKey(l => l.EmployeeId);

            modelBuilder.Entity<Employee>()
           .Property(e => e.LastLeaveBalanceUpdate)
           .IsRequired(); // Ensure the LastLeaveBalanceUpdate is not nullable

            base.OnModelCreating(modelBuilder);
        }

    }
}
