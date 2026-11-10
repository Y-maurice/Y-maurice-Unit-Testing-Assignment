using DriveNow.Domain.Abstractions;

namespace DriveNow.Infrastructure.Logging;

public class AuditLog : IAuditLog
{
    readonly TimeProvider _time;

    public AuditLog(TimeProvider time) => _time = time;

    public void Write(string message)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "audit.log");
        // Formatting the DateTimeOffset with "O" prints the same text as DateTime.Now:O
        // (local time with the offset), so the audit.log format does not change.
        File.AppendAllText(path, $"[{_time.GetLocalNow():O}] {message}{Environment.NewLine}");
    }
}