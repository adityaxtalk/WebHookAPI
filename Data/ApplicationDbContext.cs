using Microsoft.EntityFrameworkCore;
using WebhookReceiver.Models;

namespace WebhookReceiver.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions) : base(dbContextOptions) { }

        public DbSet<Alert> Alerts { get; set; }
    }
}
