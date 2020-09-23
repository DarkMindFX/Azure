using SampleBatch.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

namespace SampleBatchApi.Models
{
    [Export("Local", typeof(ISessionContext))]
    public class LocalSessionContext : ISessionContext
    {
        Dictionary<string, Session> sessions = new Dictionary<string, Session>();

        public bool CloseSession(string sessionId)
        {
            Session session = FindSession(sessionId);
            if(session != null)
            {
                session.ExpiresDt = DateTime.Now;
                session.IsActive = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public Session FindSession(string sessionId)
        {
            Session session = null;
            sessions.TryGetValue(sessionId, out session);

            return session;
        }

        public Session RefreshSession(string sessionId, DateTime newExpireDt)
        {
            Session session = FindSession(sessionId);
            if(session != null && session.IsActive)
            {
                session.ExpiresDt = newExpireDt;
            }
            else
            {
                session = null;
            }
            return session;
        }

        public string RegisterSession(Session session)
        {
            string newId = Guid.NewGuid().ToString();
            session.SessionId = newId;
            session.IsActive = true;

            // checking if session for given user exists - and closing it
            Session oldSession = sessions.Values.FirstOrDefault(s => s.UserId == session.UserId && s.IsActive);
            if(oldSession != null)
            {
                CloseSession(oldSession.SessionId);
            }

            sessions.Add(newId, session);

            return newId;
            
        }
    }
}