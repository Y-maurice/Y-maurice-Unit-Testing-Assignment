using DriveNow.Domain.Abstractions;
using DriveNow.Domain.Entities;
using DriveNow.Infrastructure.Persistence;

namespace DriveNow.Infrastructure.Repositories;

public class PromoCodeRepository : IPromoCodeRepository
{
    readonly InMemoryDatabase _database;

    public PromoCodeRepository(InMemoryDatabase database) => _database = database;

    public PromoCode? GetByCode(string code) =>
        _database.PromoCodes.FirstOrDefault(promo =>
            string.Equals(promo.Code, code, StringComparison.OrdinalIgnoreCase));

    public void Update(PromoCode promo)
    {
        var index = _database.PromoCodes.FindIndex(item =>
            string.Equals(item.Code, promo.Code, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
            _database.PromoCodes[index] = promo;
    }
}