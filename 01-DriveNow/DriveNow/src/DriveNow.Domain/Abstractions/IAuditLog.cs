namespace DriveNow.Domain.Abstractions;

public interface IAuditLog
{
    void Write(string message);
}