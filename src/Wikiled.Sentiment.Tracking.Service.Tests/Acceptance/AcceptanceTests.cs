using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikiled.Common.Net.Client;
using Wikiled.Sentiment.Tracking.Api.Service;
using Wikiled.Sentiment.Tracking.Logic;
using Wikiled.Sentiment.Tracking.Service.Sample;
using Wikiled.Server.Core.Testing.Server;

namespace Wikiled.Sentiment.Tracking.Service.Tests.Acceptance
{
    [TestFixture]
    public class AcceptanceTests
    {
        private ServerWrapper wrapper;

        private SentimentTracking analysis;

        [OneTimeSetUp]
        public void SetUp()
        {
            wrapper = ServerWrapper.Create<Startup>(TestContext.CurrentContext.TestDirectory, services => { });
            analysis = new SentimentTracking(
                new ApiClientFactory(wrapper.Client, wrapper.Client.BaseAddress));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            wrapper.Dispose();
        }

        [Test]
        public async Task Version()
        {
            ServiceResponse<RawResponse<string>> response = await wrapper.ApiClient.GetRequest<RawResponse<string>>("api/monitor/version", CancellationToken.None).ConfigureAwait(false);
            Assert.IsTrue(response.IsSuccess);
        }

        [Test]
        public async Task GetTrackingResults()
        {
            var result = await analysis.GetTrackingResults(new Api.Request.SentimentRequest("AMD", "TSLA"), CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result["AMD"].Length);
            Assert.AreEqual(24, result["AMD"][0].Hours);
            Assert.AreEqual(1, result["AMD"][0].TotalMessages);
            Assert.AreEqual(1, result["AMD"][0].Average);
            Assert.AreEqual(0, result["TSLA"][0].TotalMessages);
            Assert.IsNull(result["TSLA"][0].Average);
        }

        [Test]
        public async Task GetTrackingHistory()
        {
            IDictionary<string, RatingRecord[]> result = await analysis.GetTrackingHistory(new Api.Request.SentimentRequest("AMD"), CancellationToken.None).ConfigureAwait(false);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result["AMD"].Length);
        }
    }
}
