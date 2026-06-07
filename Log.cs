using System;

namespace XeXtractor
{
    public class Log
    {
        private static Log instance;
        private string logText = "";

        private Log()
        {
        }

        public event EventHandler LogChanged;

        public static Log getInstance()
        {
            if (instance == null)
                instance = new Log();
            return instance;
        }

        public void AddEntry(string entry)
        {
            logText += entry + "\r\n";
            OnLogChanged();
        }

        public void Clear()
        {
            logText = "";
            OnLogChanged();
        }

        public void AddSeperator()
        {
            logText += "-----------------------------------\r\n";
            OnLogChanged();
        }

        public string getLog() => logText;

        private void OnLogChanged()
        {
            if (LogChanged != null)
                LogChanged(this, EventArgs.Empty);
        }
    }
}
