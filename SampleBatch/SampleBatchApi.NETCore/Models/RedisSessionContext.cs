using Newtonsoft.Json;
using SampleBatch.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web;

namespace SampleBatchApi.Models
{
    [Export("Redis", typeof(ISessionContext))]
    public class RedisSessionContext : ISessionContext
    {
        
        string redisConnString = null;

        IDatabase cache = null;
        Lazy<ConnectionMultiplexer> lazyConnection;

        [ImportingConstructor]
        public RedisSessionContext([Import("RedisConnString")] string connString)
        {
            this.redisConnString = connString;
            lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                string cacheConnection = redisConnString;
                return ConnectionMultiplexer.Connect(cacheConnection);
            });
            cache = lazyConnection.Value.GetDatabase();

        }

        public bool CloseSession(string sessionId)
        {
            bool closed = false;
            Session session = FindSession(sessionId);
            if(session != null)
            {
                this.cache.KeyDelete(sessionId);
                closed = true;
            }
            return closed;
        }

        public Session FindSession(string sessionId)
        {
            Session session = null;
            var redisSession = this.cache.StringGet(sessionId);
            if(redisSession.HasValue && !redisSession.IsNullOrEmpty)
            {
                session = JsonConvert.DeserializeObject<Session>(redisSession.ToString());
 
            }
            return session;
        }

        public Session RefreshSession(string sessionId, DateTime newExpireDt)
        {
            throw new NotImplementedException();
        }

        public string RegisterSession(Session session)
        {
            string newId = Guid.NewGuid().ToString();
            session.SessionId = newId;
            session.IsActive = true;

            var isAdded = this.cache.StringSet(newId, JsonConvert.SerializeObject(session));
            if(!isAdded)
            {
                newId = null;
            }
            return newId;
        }

        public ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }
}