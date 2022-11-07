using System;
using System.Threading.Tasks;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.Core.Tests.DomainModel.Users;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Inforigami.Regalo.SqlServer.Tests.Unit
{
    [TestFixture]
    public class ConnectionTests
    {
        private ILogger _logger;
        private IConfiguration _configuration;

        [SetUp]
        public async Task SetUp()
        {
            _configuration =
                new ConfigurationBuilder().AddJsonFile("appsettings.json")
                                          .Build();

            _logger = new ConsoleLogger();

            Resolver.Configure(type =>
            {
                if (type == typeof(ILogger))
                {
                    return _logger;
                }
                throw new InvalidOperationException(string.Format("No type of {0} registered.", type));
            },
            type => null,
            o => { });

            await DatabaseInstaller.Install();
        }

        [TearDown]
        public void TearDown()
        {
            Resolver.Reset();
        }

        [Test]
        public void Connecting_to_SqlClient_does_not_throw_exception()
        {
            var store = new SqlServerEventStore(_configuration.GetConnectionString("RegaloConnection"), _logger);
            var user = new User();
            user.Register();
            user.ChangePassword("password");
            Assert.DoesNotThrow(
                () => store.Save<User>(user.Id.ToString(), EntityVersion.New, user.GetUncommittedEvents()));
        }
    }
}