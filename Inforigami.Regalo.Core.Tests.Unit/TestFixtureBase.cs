using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NUnit.Framework;
using Inforigami.Regalo.Core.Tests.DomainModel.Users;
using Inforigami.Regalo.Testing;

namespace Inforigami.Regalo.Core.Tests.Unit
{
    public abstract class TestFixtureBase
    {
        [SetUp]
        public void SetUp()
        {
            var _nullLogger = new NullLogger();

            Resolver.Configure(
                type =>
                {
                    if (type == typeof(ILogger))
                    {
                        return _nullLogger;
                    }

                    throw new NotSupportedException(string.Format("TestFixtureBase::SetUp - Nothing registered for {0}", type));
                },
                type => null,
                o => { });
        }

        [TearDown]
        public void TearDown()
        {
            Resolver.Reset();
        }

        protected void CollectionAssertAreJsonEqual(IEnumerable<object> expected, IEnumerable<object> actual)
        {
            var expectedJson = GetJsonList(expected);
            var actualJson   = GetJsonList(actual);

            CollectionAssert.AreEqual(expectedJson, actualJson);
        }

        protected void AssertAreJsonEqual(object expected, object actual)
        {
            var expectedJson = JsonConvert.SerializeObject(expected, Formatting.Indented);
            var actualJson   = JsonConvert.SerializeObject(actual, Formatting.Indented);

            Assert.AreEqual(expectedJson, actualJson);
        }

        private IEnumerable<string> GetJsonList(IEnumerable<object> list)
        {
            return list.Select(x => JsonConvert.SerializeObject(x, Formatting.Indented))
                .Select(FixGuids)
                .ToArray();
        }

        private string FixGuids(string json)
        {
            var result = json;

            result = Regex.Replace(
                    result,
                    @"""Id""\s*:\s*""(?i:[a-f\d]{8}-[a-f\d]{4}-[a-f\d]{4}-[a-f\d]{4}-[a-f\d]{12})""",
                    @"""Id"" : ""00000000-0000-0000-0000-000000000000""");

            result = Regex.Replace(
                    result,
                    @"""BaseVersion""\s*:\s*""(?i:[a-f\d]{8}-[a-f\d]{4}-[a-f\d]{4}-[a-f\d]{4}-[a-f\d]{12})""",
                    @"""BaseVersion"" : ""00000000-0000-0000-0000-000000000000""");

            result =
                Regex.Replace(
                    result,
                    @"""ParentVersion""\s*:\s*""(?i:[a-f\d]{8}-[a-f\d]{4}-[a-f\d]{4}-[a-f\d]{4}-[a-f\d]{12})""",
                    @"""ParentVersion"" : ""00000000-0000-0000-0000-000000000000""");

            result =
                Regex.Replace(
                    result,
                    @"""CausationId""\s*:\s*""(?i:[a-f\d]{8}-[a-f\d]{4}-[a-f\d]{4}-[a-f\d]{4}-[a-f\d]{12})""",
                    @"""CausationId"" : ""00000000-0000-0000-0000-000000000000""");

            result =
                Regex.Replace(
                    result,
                    @"""CorrelationId""\s*:\s*""(?i:[a-f\d]{8}-[a-f\d]{4}-[a-f\d]{4}-[a-f\d]{4}-[a-f\d]{12})""",
                    @"""CorrelationId"" : ""00000000-0000-0000-0000-000000000000""");

            return result;
        }
    }
}
