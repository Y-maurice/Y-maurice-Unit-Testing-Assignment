namespace DriveNow.Domain.Abstractions;

/// <summary>
/// Creates the booking reference of BR-25: "DN-" + pickup date (yyyyMMdd) + "-" + 4 letters or digits.
/// Hiding the random part behind this interface lets a test use a fixed reference.
/// </summary>
public interface IReferenceGenerator
{
    string Generate(DateTime pickupAt);
}