using System;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Inforigami.Regalo.Core;
using Inforigami.Regalo.Core.Tests.DomainModel.Users;
using Inforigami.Regalo.EventSourcing;
using Inforigami.Regalo.Testing;
using NUnit.Framework;

namespace Inforigami.Regalo.SqlServer.Tests.Unit
{
    [TestFixture]
    public class ConnectionTests
    {
        private ILogger _logger;

        [SetUp]
        public async Task SetUp()
        {
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
        public void Connecting_to_undefined_database_name_throws_exception()
        {
            var store = new SqlServerEventStore("InvalidConnnectionName", _logger);
            var user = new User();
            user.Register();
            user.ChangePassword("password");
            Assert.Throws<InvalidOperationException>(
                () => store.Save<User>(user.Id.ToString(), EntityVersion.New, user.GetUncommittedEvents()));
        }

        [Test]
        public void Connecting_to_non_SqlClient_throws_exception()
        {
            var store = new SqlServerEventStore("NonSqlClientConnection", _logger);
            var user = new User();
            user.Register();
            user.ChangePassword("password");
            Assert.Throws<InvalidOperationException>(
                () => store.Save<User>(user.Id.ToString(), EntityVersion.New, user.GetUncommittedEvents()));
        }

        [Test]
        public void Connecting_to_SqlClient_does_not_throw_exception()
        {
            var store = new SqlServerEventStore("SqlClientConnection", _logger);
            var user = new User();
            user.Register();
            user.ChangePassword("password");
            Assert.DoesNotThrow(
                () => store.Save<User>(user.Id.ToString(), EntityVersion.New, user.GetUncommittedEvents()));
        }
    }
}