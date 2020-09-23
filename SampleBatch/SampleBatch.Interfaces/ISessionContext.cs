using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleBatch.Interfaces
{
    public interface ISessionContext
    {
        /// <summary>
        /// Registers new session
        /// </summary>
        /// <param name="session"></param>
        /// <returns>New session Id</returns>
        string RegisterSession(Session session);

        /// <summary>
        /// Finds session with given Id
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns>Session object or null if no session with given id exists</returns>
        Session FindSession(string sessionId);

        /// <summary>
        /// Closes session with given id
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns>True if session was closed or false is no session was found</returns>
        bool CloseSession(string sessionId);

        /// <summary>
        /// Prolongues session expiration time
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns>Updated session object or null if no such session found</returns>
        Session RefreshSession(string sessionId, DateTime newExpireDt);

    }
}
