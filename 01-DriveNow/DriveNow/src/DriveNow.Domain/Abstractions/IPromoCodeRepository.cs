using DriveNow.Domain.Entities;

namespace DriveNow.Domain.Abstractions;

public interface IPromoCodeRepository
{
    PromoCode? GetByCode(string code);

    void Update(PromoCode promo);
}