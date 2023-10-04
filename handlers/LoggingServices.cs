using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace dcnet.Handlers
{
    public class LoggingService
    {
        private readonly string logFilePath;

        public LoggingService(DiscordSocketClient client, CommandService command)
        {
            client.Log += LogAsync;
            command.Log += LogAsync;

            string currentDate = DateTime.Now.ToString("dd-MM-yyyy");
            string logDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            
            if (logDirectoryPath != null)
            {
                logFilePath = Path.Combine(logDirectoryPath, $"{currentDate}--logs.log");
                Directory.CreateDirectory(logDirectoryPath);
            } 
            else
            {
                throw new InvalidOperationException("Log directory path cannot be found!");
            }
        }

        private async Task LogAsync(LogMessage message)
        {
            string formattedDate = $"[{DateTime.Now:dd.MM.yyyy}]";
            string formattedSeverity = $"[{message.Severity}]";
            string logText = $"{formattedDate} {formattedSeverity} {message}";

            Console.WriteLine(logText);

            await File.AppendAllTextAsync(logFilePath, logText + Environment.NewLine);
        }
    }
}
