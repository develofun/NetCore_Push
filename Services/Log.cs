using System;
using NLog;

namespace NetCore_PushServer
{
    public interface ILog
    {
        void Info(string message);
        void Warning(string message);
        void Debug(string message);
        void Error(string message);
    }

    public class Log : ILog
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public Log() { }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Warning(string message)
        {
            _logger.Warn(message);
        }
    }
}