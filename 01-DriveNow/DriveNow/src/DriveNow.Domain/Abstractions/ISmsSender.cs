namespace DriveNow.Domain.Abstractions;

public interface ISmsSender
{
    void Send(string phone, string text);
}