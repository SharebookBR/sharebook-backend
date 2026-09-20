using ShareBook.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly EntityCrud<User> _crud;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
        _crud = new EntityCrud<User>(context);
    }

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
