using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inforigami.Regalo.Core
{
    public class ConsoleLogger : ILogger
    {
        private readonly object _syncLock = new object();
        private DateTime _lastLogDateTime;

        public bool ShowDebugMessages { get; set; }
        public TimeSpan QuietTime { get; set; }

        public ConsoleLogger()
        {
            ShowDebugMessages = false;
            QuietTime = TimeSpan.FromMinutes(1);
        }

        public void Debug(object sender, string format, params object[] args)
        {
            if (!ShowDebugMessages) return;

            Write(sender, Severity.Debug, string.Format(format, args));
        }

        public void Info(object sender, string format, params object[] args)
        {
            Write(sender, Severity.Information, string.Format(format, args));
        }

        public void Warn(object sender, string format, params object[] args)
        {
            Write(sender, Severity.Warning, string.Format(format, args));
        }

        public void Error(object sender, Exception exception, string format, params object[] args)
        {
            var message = string.Format(format, args);
            var exceptionText = GetExceptionReport(exception);
            message += "\r\n" + exceptionText;

            Write(sender, Severity.Exception, message);
        }

        public void Error(object sender, string format, params object[] args)
        {
            Write(sender, Severity.Error, string.Format(format, args));
        }

        private string GetExceptionReport(Exception exception)
        {
            var report = new StringBuilder("*** Exception Report ***").AppendLine();

            var exceptions = GetExceptions(exception).Reverse().ToArray();
            foreach (var ex in exceptions)
            {
                report.AppendLine(FormatException(ex));

                if (ex != exceptions.Last())
                {
                    report.AppendLine(" - Inner Exception of:").AppendLine();
                }
                else if(exceptions.Length > 1)
                {
                    report.AppendLine(" (Top-most exception)");
                }

            }

            return report.ToString();
        }

        private IEnumerable<Exception> GetExceptions(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            do
            {
                yield return exception;
            } while ((exception = exception.InnerException) != null);
        }

        private string FormatException(Exception ex)
        {
            var result = new StringBuilder();

            result.AppendFormat("   Type: \"{0}\"", ex.GetType().Name).AppendLine();
            result.AppendFormat("   Message: \"{0}\"", ex.Message).AppendLine();
            result.AppendFormat("{0}", ex.StackTrace).AppendLine();

            return result.ToString();
        }

        private void Write(object sender, Severity severity, string message)
        {
            lock (_syncLock)
            {
                var timeSinceLastWrite = TimeSinceLastWrite();
                if (timeSinceLastWrite > QuietTime)
                {
                    WriteQuietTimeMessage(timeSinceLastWrite);
                }

                WriteTimestamp();
                WriteDivider();
                WriteSeverity(severity);
                WriteDivider();
                WriteSender(sender);
                WriteDivider();
                WriteMessage(message);

                Console.WriteLine();

                _lastLogDateTime = DateTime.Now;
            }
        }

        private void WriteQuietTimeMessage(TimeSpan timeSinceLastWrite)
        {
            WriteText("<<<");
            Console.WriteLine();
            WriteTimeSinceLastWrite(timeSinceLastWrite);
            Console.WriteLine();
            WriteText(">>>");
            Console.WriteLine();
            Console.WriteLine();
        }

        private void WriteMessage(string message)
        {
            WriteText(message);
        }

        private void WriteSender(object sender)
        {
            var senderName = $"\"{sender.GetType().Name}\"";
            WriteText(senderName);
        }

        private void WriteTimeSinceLastWrite(TimeSpan since)
        {
            WriteText($"Quiet period of {since}");
        }

        private void WriteTimestamp()
        {
            WriteText(DateTimeOffset.Now.ToString("s").Replace("T", " "), ConsoleColor.White, null);
        }

        private void WriteSeverity(Severity severity)
        {
            ConsoleColor foreground = Console.ForegroundColor;
            ConsoleColor background = Console.BackgroundColor;
            string severityName = "";

            switch (severity)
            {
                case Severity.Debug:
                    foreground = ConsoleColor.Black;
                    background = ConsoleColor.Cyan;
                    severityName = "DEBUG";
                    break;
                case Severity.Information:
                    foreground = ConsoleColor.White;
                    background = ConsoleColor.DarkGreen;
                    severityName = "INFO";
                    break;
                case Severity.Warning:
                    foreground = ConsoleColor.White;
                    background = ConsoleColor.DarkYellow;
                    severityName = "WARN";
                    break;
                case Severity.Error:
                    foreground = ConsoleColor.White;
                    background = ConsoleColor.Red;
                    severityName = "ERROR";
                    break;
                case Severity.Exception:
                    foreground = ConsoleColor.White;
                    background = ConsoleColor.Red;
                    severityName = "PANIC";
                    break;
            }

            severityName = severityName.PadRight(5, ' ');

            WriteText(severityName, foreground, background);
        }

        private void WriteDivider()
        {
            Console.Write('\t');
        }

        private void WriteText(string text, ConsoleColor? foreground, ConsoleColor? background)
        {
            var prevForeground = Console.ForegroundColor;
            var prevBackground = Console.BackgroundColor;
            try
            {
                Console.ForegroundColor = foreground ?? prevForeground;
                Console.BackgroundColor = background ?? prevBackground;

                WriteText(text);
            }
            finally
            {
                Console.ForegroundColor = prevForeground;
                Console.BackgroundColor = prevBackground;
            }
        }

        private void WriteText(string text)
        {
            Console.Write(text);
        }

        private TimeSpan TimeSinceLastWrite()
        {
            if (_lastLogDateTime > DateTime.MinValue)
            {
                return DateTime.Now - _lastLogDateTime;
            }

            return TimeSpan.Zero;
        }

        private enum Severity
        {
            Debug,
            Information,
            Warning,
            Error,
            Exception
        }
    }
}
