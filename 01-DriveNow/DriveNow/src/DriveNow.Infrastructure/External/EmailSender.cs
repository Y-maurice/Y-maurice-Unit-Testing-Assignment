using DriveNow.Domain.Abstractions;

namespace DriveNow.Infrastructure.External;

public class EmailSender : IEmailSender
{
    readonly TimeProvider _time;

    public EmailSender(TimeProvider time) => _time = time;

    public void Send(string to, string subject, string body)
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "outbox");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, $"{_time.GetLocalNow():yyyyMMdd_HHmmssfff}_{to}.txt");
        var content = $"To: {to}{Environment.NewLine}Subject: {subject}{Environment.NewLine}{Environment.NewLine}{body}";
        File.WriteAllText(path, content);
    }
}