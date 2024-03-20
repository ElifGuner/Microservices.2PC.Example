using Microsoft.EntityFrameworkCore;

namespace Coordinator.Models.Contexts
{
    public class TwoPhaseCommitContext : DbContext
    {
        public TwoPhaseCommitContext(DbContextOptions options) : base(options)
        { 
        
        }
        public DbSet<Node> Nodes {get; set;}
        public DbSet<NodeState> NodeStates { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Node>().HasData(
                new Node() { Id = 1 ,Name = "Order.API" },
                new Node() { Id = 2, Name = "Stock.API" },
                new Node() { Id = 3, Name = "Payment.API" }
            );
        }

    }
}
