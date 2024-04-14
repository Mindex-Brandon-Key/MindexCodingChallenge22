using CodeChallenge.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeChallenge.Data
{
    public class EmployeeContext : DbContext
    {
        public EmployeeContext(DbContextOptions<EmployeeContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Compensation>()
                .HasOne(c => c.Employee)
                .WithMany(e => e.Compensations)
                .HasForeignKey(c => c.EmployeeId);
        }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Compensation> Compensations { get; set; }
    }
}
