using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository.Mapping;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Repository;

public class ApplicationDbContext : DbContext
{
    // Opcional: fora de um request HTTP (ex.: jobs em background, tooling de design-time do
    // `dotnet ef`) não há usuário autenticado pra registrar como autor da mudança no EFLog.
    private readonly ICurrentUserAccessor? _currentUserAccessor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserAccessor? currentUserAccessor = null) : base(options)
    {
        _currentUserAccessor = currentUserAccessor;
    }
    public ApplicationDbContext() { }

    public DbSet<Book> Books { get; set; }
    public DbSet<BookDownloadEvent> BookDownloadEvents { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<EFLog> EFLogs { get; set; }
    public DbSet<BookUser> BookUser { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<JobHistory> JobHistories { get; set; }
    public DbSet<AccessHistory> AccessHistories { get; set; }
    public DbSet<Meetup> Meetups { get; set; }
    public DbSet<MeetupParticipant> MeetupParticipants { get; set; }

    public DbSet<MailBounce> MailBounces { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("unaccent");

        //O Contexto procura pelas classes que implementam IEntityTypeConfiguration adicionando o mapeamento de forma automática.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        this.SetUtcOnDatabase(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        await this.LogChanges(_currentUserAccessor?.UserId);
        return await base.SaveChangesAsync(cancellationToken);
    }
}
