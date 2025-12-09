
using Microsoft.EntityFrameworkCore;
using OneInteg.Server.Domain.Repositories;
using System.Linq.Expressions;

namespace OneInteg.Server.DataAccess
{
    public partial class SubscriptionRepository : BaseRepository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(OneIntegDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Subscription>> Find(Expression<Func<Subscription, bool>> predicate)
        {
            return await _context.Set<Subscription>().Where(predicate).AsNoTracking().ToListAsync();
        }
    }
}
