using ShareBook.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Repository;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly EntityCrud<User> _crud = new EntityCrud<User>(context);

    public IQueryable<User> Get() => _crud.Get();

    public async Task<User> UpdatePasswordAsync(User user)
    {
        _context.Users.Update(user);
        _context.Entry(user).Property(x => x.Password).IsModified = true;
        _context.Entry(user).Property(x => x.PasswordSalt).IsModified = true;
        await _context.SaveChangesAsync();

        return user;
    }
}
