using DriveNow.Domain.Abstractions;

namespace DriveNow.Infrastructure.Generators;

/// <summary>
/// The real generator used by the console: the same alphabet and length
/// that BookingService used before (BR-25).
/// </summary>
public class RandomReferenceGenerator : IReferenceGenerator
{
    const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    readonly Random _random = new();

    public string Generate(DateTime pickupAt)
    {
        var chars = new char[4];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = Alphabet[_random.Next(Alphabet.Length)];

        return "DN-" + pickupAt.ToString("yyyyMMdd") + "-" + new string(chars);
    }
}