using Core.Interfaces;
using Core.Models;
using Microsoft.EntityFrameworkCore;


namespace TaskManager.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IOrderRepository _orders;
        private readonly Guid _loggedInUserId = Guid.Empty; 
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            _orders = new OrderRepository(_context);
        }

        public IOrderRepository Orders => _orders;
        public async Task SaveChangesAsync()
        {

            var entries = _context.ChangeTracker.Entries()
                    .Where(e => (e.Entity is AuditEntity) && (
                      e.State == EntityState.Added
                               || e.State == EntityState.Modified)).ToList();

            foreach (var entityEntry in entries)
            {
                var tableName = entityEntry.Entity.GetType().Name.Replace("Proxy", "");

                if (entityEntry.State == EntityState.Modified)
                {
                    ((AuditEntity)entityEntry.Entity).UpdatedOn = DateTime.Now;
                    ((AuditEntity)entityEntry.Entity).UpdatedBy = _loggedInUserId;
                    entityEntry.Property("CreatedOn").IsModified = false;
                    entityEntry.Property("CreatedBy").IsModified = false;

                }

                if (entityEntry.State == EntityState.Added)
                {
                    ((AuditEntity)entityEntry.Entity).CreatedOn = DateTime.Now;
                    ((AuditEntity)entityEntry.Entity).CreatedBy = _loggedInUserId;

                 
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
