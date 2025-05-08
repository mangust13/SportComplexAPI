namespace SportComplexAPI.Services
{
    public static class LogService
    {
        private static readonly string logFilePath = "Log.txt"; // можна винести в конфіг

        public static void LogAction(string userName, string roleName, string actionDescription)
        {
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | User: {userName} | Role: {roleName} | Action: {actionDescription}";
            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }
    }
}
