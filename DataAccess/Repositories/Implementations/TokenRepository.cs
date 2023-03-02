using DataAccess.DataContext;
using DataAccess.Repositories.Interfaces;

using Models.Entities;

namespace DataAccess.Repositories.Implementations;
public class TokenRepository : Repository<RefreshToken>, ITokenRepository
{
    public TokenRepository(AppDbContext appContext) : base(appContext)
    {
    }
}