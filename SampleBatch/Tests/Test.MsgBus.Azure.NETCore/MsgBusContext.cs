using Microsoft.Extensions.Configuration;
using MsgBus.Azure;
using Newtonsoft.Json;
using NUnit.Framework;
using SampleBatch.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Test.MsgBus.Azure.NETCore
{
    class MsgBusContextTest
    {
        class MsgBusConfig
        {
            [JsonProperty("StorageAccountName")]
            public string StorageAccountName { get; set; }

            [JsonProperty("StorageAccountKey")]
            public string StorageAccountKey { get; set; }

            [JsonProperty("MessageQueue")]
            public string MessageQueue { get; set; }

        }
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void PrepareParams_Success()
        {
            IMsgBusContextParams ctxParams = GetContextParams();

            Assert.IsNotNull(ctxParams);
            Assert.IsNotNull(ctxParams.Parameters);
            Assert.IsNotEmpty(ctxParams.Parameters);
            Assert.IsTrue(ctxParams.Parameters.Count == 3);
            Assert.IsTrue(ctxParams.Parameters.ContainsKey("StorageAccountName"));
            Assert.IsTrue(ctxParams.Parameters.ContainsKey("StorageAccountKey"));
            Assert.IsTrue(ctxParams.Parameters.ContainsKey("MessageQueue"));
        }

        [Test]
        public void Init_Success()
        {
            IConfiguration config = GetConfiguration();
            var msgBusConfig = config.GetSection("MsgBusConfig").Get<MsgBusConfig>();

            IMsgBusContextParams busParams = GetContextParams();
            busParams.Parameters["MessageQueue"] = msgBusConfig.MessageQueue;
            busParams.Parameters["StorageAccountKey"] = msgBusConfig.StorageAccountKey;
            busParams.Parameters["StorageAccountName"] = msgBusConfig.StorageAccountName;

            IMsgBusContext ctx = new MsgBusContext();
            ctx.Init(busParams);
        }

        [Test]
        public void Init_Fail_InvalidAccount()
        {
            try
            {
                IConfiguration config = GetConfiguration();
                var msgBusConfig = config.GetSection("MsgBusConfig_InvalidAccount").Get<MsgBusConfig>();

                IMsgBusContextParams busParams = GetContextParams();
                busParams.Parameters["MessageQueue"] = msgBusConfig.MessageQueue;
                busParams.Parameters["StorageAccountKey"] = msgBusConfig.StorageAccountKey;
                busParams.Parameters["StorageAccountName"] = msgBusConfig.StorageAccountName;

                IMsgBusContext ctx = new MsgBusContext();
                ctx.Init(busParams);

                Assert.Fail("Initialized with invalid Account");
            }
            catch(UriFormatException exUriFormat)
            {
                Assert.Pass(); // OK - exception expected
            }

        }

        [Test]
        public void Init_Fail_InvalidKey()
        {
            try
            {
                IConfiguration config = GetConfiguration();
                var msgBusConfig = config.GetSection("MsgBusConfig_InvalidKey").Get<MsgBusConfig>();

                IMsgBusContextParams busParams = GetContextParams();
                busParams.Parameters["MessageQueue"] = msgBusConfig.MessageQueue;
                busParams.Parameters["StorageAccountKey"] = msgBusConfig.StorageAccountKey;
                busParams.Parameters["StorageAccountName"] = msgBusConfig.StorageAccountName;

                IMsgBusContext ctx = new MsgBusContext();
                ctx.Init(busParams);

                Assert.Fail("Initialized with invalid Key");
            }
            catch (FormatException exFormat)
            {
                Assert.Pass(); // OK - exception expected
            }

        }

        private IConfiguration GetConfiguration()
        {
            var codebase = Assembly.GetExecutingAssembly().GetName().CodeBase;
            var path = Path.Combine( Path.GetDirectoryName(codebase), "appconfig.json").Substring(6);
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(path, optional: false, reloadOnChange: true)
                .Build();

            return config;

        }

        private IMsgBusContextParams GetContextParams()
        {
            MsgBusContext ctx = new MsgBusContext();
            return ctx.PrepareParams();
        }
    }
}
