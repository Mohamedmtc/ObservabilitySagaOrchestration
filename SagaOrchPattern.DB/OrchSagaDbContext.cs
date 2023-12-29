using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchestrator.Presistance
{
    public class OrchSagaDbContext : DbContext
    {
        public DbSet<OrderStateData> OrderStateData { get; set; }
        public OrchSagaDbContext()
        {
        }

        public OrchSagaDbContext(DbContextOptions<OrchSagaDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.;Database=OrchestrationDB;integrated security=true;MultipleActiveResultSets=True;TrustServerCertificate=True;");
        }
    }
}
