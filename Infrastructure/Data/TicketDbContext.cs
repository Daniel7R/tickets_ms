using Microsoft.EntityFrameworkCore;
using TicketsMS.Domain.Entities;

namespace TicketsMS.Infrastructure.Data
{
    public class TicketDbContext: DbContext 
    {
        public DbSet<Tickets> Tickets { get; set; }
        public DbSet<TicketSales> TicketSales { get; set; }

        public TicketDbContext(DbContextOptions<TicketDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tickets>().ToTable("tickets");
            modelBuilder.Entity<TicketSales>().ToTable("ticket_sales");

            modelBuilder.Entity<Tickets>().Property(t => t.Id).HasColumnName("id").ValueGeneratedOnAdd();
            modelBuilder.Entity<Tickets>().Property(t => t.IdTournament).HasColumnName("id_tournament");
            modelBuilder.Entity<Tickets>().Property(t => t.IdMatch).HasColumnName("id_match");
            modelBuilder.Entity<Tickets>().Property(t => t.Code).HasColumnName("code");
            modelBuilder.Entity<Tickets>().Property(t => t.Type).HasColumnName("type").HasConversion<string>();
            modelBuilder.Entity<Tickets>().Property(t => t.Price).HasColumnName("price");
            modelBuilder.Entity<Tickets>().Property(t => t.Status).HasColumnName("status").HasConversion<string>();

            modelBuilder.Entity<TicketSales>().Property(ts => ts.Id).HasColumnName("id").ValueGeneratedOnAdd();
            modelBuilder.Entity<TicketSales>().Property(ts => ts.IdTransaction).HasColumnName("id_transaction");
            modelBuilder.Entity<TicketSales>().Property(ts => ts.IdTicket).HasColumnName("id_ticket");

            modelBuilder.Entity<TicketSales>()
                .HasOne(ts => ts.Ticket)
                .WithOne(t => t.TicketSales)
                .HasForeignKey<TicketSales>(ts => ts.IdTicket)
                .IsRequired();

        }
    }
}
