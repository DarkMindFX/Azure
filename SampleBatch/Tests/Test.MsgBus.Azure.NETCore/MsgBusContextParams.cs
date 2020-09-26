using Microsoft.Extensions.Configuration;
using MsgBus.Azure;
using NUnit.Framework;
using SampleBatch.Interfaces;

namespace Test.MsgBus.Azure.NETCore
{
    public class MsgBusContextParamsTest
    {
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

        

        private IMsgBusContextParams GetContextParams()
        {
            MsgBusContext ctx = new MsgBusContext();
            return ctx.PrepareParams();
        }
    }
}