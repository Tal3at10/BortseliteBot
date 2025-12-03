using CustomsBot.Models;
using System.Collections.Concurrent;

namespace CustomsBot.Services
{
    public class SessionManager
    {
        private static readonly ConcurrentDictionary<string, UserSession> _sessions = new();
        private static readonly TimeSpan SessionTimeout = TimeSpan.FromMinutes(30);

        public UserSession GetOrCreateSession(string phoneNumber)
        {
            CleanupExpiredSessions();

            return _sessions.GetOrAdd(phoneNumber, _ => new UserSession
            {
                PhoneNumber = phoneNumber,
                LastActivity = DateTime.UtcNow
            });
        }

        public void UpdateSession(UserSession session)
        {
            session.LastActivity = DateTime.UtcNow;
            _sessions[session.PhoneNumber] = session;
        }

        public void ResetSession(string phoneNumber)
        {
            _sessions.TryRemove(phoneNumber, out _);
        }

        private void CleanupExpiredSessions()
        {
            var expiredSessions = _sessions
                .Where(s => DateTime.UtcNow - s.Value.LastActivity > SessionTimeout)
                .Select(s => s.Key)
                .ToList();

            foreach (var key in expiredSessions)
            {
                _sessions.TryRemove(key, out _);
            }
        }
    }
}
