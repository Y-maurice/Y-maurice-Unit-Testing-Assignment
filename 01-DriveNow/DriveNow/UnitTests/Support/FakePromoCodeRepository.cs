using DriveNow.Domain.Abstractions;
using DriveNow.Domain.Entities;

namespace DriveNow.UnitTests.Support;

/// <summary>
/// A fake (a working but simple implementation): the promo codes live in a list the test controls.
/// The lookup is case-insensitive, like the real repository (BR-36).
/// </summary>
public class FakePromoCodeRepository : IPromoCodeRepository
{
    readonly List<PromoCode> _promos = new();

    public FakePromoCodeRepository(params PromoCode[] promos) => _promos.AddRange(promos);

    public IReadOnlyList<PromoCode> Promos => _promos;

    public PromoCode? GetByCode(string code) =>
        _promos.FirstOrDefault(promo =>
            string.Equals(promo.Code, code, StringComparison.OrdinalIgnoreCase));

    public void Update(PromoCode promo)
    {
        var index = _promos.FindIndex(item =>
            string.Equals(item.Code, promo.Code, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
            _promos[index] = promo;
    }
}