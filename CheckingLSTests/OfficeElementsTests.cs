using CheckinLS.API.Misc;
using CheckinLS.API.Office;
using CheckinLS.InterfacesAndClasses.Date;
using CheckinLS.InterfacesAndClasses.Users;
using NUnit.Framework;
using NSubstitute;
using System;
using System.Threading.Tasks;
using CheckinLS.InterfacesAndClasses.Internet;
using MainSql = CheckinLS.API.Sql.MainSql;

namespace CheckingLSTests
{
    [TestFixture]
    public class OfficeElementsTests
    {
        private static async Task<OfficeElements> CreateTaskAsync()
        {
            var dateInterface = Substitute.For<IGetDate>();
            dateInterface.GetCurrentDate().Returns(DateTime.Parse("2020-01-01"));

            MainSql.CreateConnection();
            await MainSql.CkeckConnectionAsync();
            await MainSql.CreateAsync(new TestUserHelpers(), new TestInternetAccess(), "1111");

            return await OfficeElements.CreateAsync(dateInterface).ConfigureAwait(false);
        }

        [Test]
        public async Task AddNewEntryAsync_EqualTime_ReturnsException()
        {
            var officeElements = await CreateTaskAsync();

            Task AsyncTestDelegate() => officeElements.AddNewEntryAsync(TimeSpan.FromHours(8), TimeSpan.FromHours(8), null);

            Assert.CatchAsync<HoursCantBeEqual>(AsyncTestDelegate);
        }

        [Test]
        public async Task AddNewEntryAsync_StartBiggerThanFinish_ReturnsException()
        {
            var officeElements = await CreateTaskAsync();

            Task AsyncTestDelegate() => officeElements.AddNewEntryAsync(TimeSpan.FromHours(9), TimeSpan.FromHours(8), null);

            Assert.CatchAsync<StartCantBeBigger>(AsyncTestDelegate);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryAsync_CorrectTimeMultipleObservations_ReturnNewEntry(string observatii)
        {
            var officeElements = await CreateTaskAsync();

            await officeElements.AddNewEntryAsync(TimeSpan.FromHours(8), TimeSpan.FromHours(9), observatii);

            var index = officeElements.MaxElement();

            switch (observatii)
            {
                case null:
                case "":
                    Assert.AreEqual(officeElements.Entries[index].Observatii, "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(officeElements.Entries[index].Observatii, observatii);
                    break;
            }

            Assert.AreEqual(officeElements.Entries[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(officeElements.Entries[index].OraIncepere, TimeSpan.FromHours(8));
            Assert.AreEqual(officeElements.Entries[index].OraFinal, TimeSpan.FromHours(9));
            Assert.AreEqual(officeElements.Entries[index].Total, TimeSpan.Parse("01:00"));
        }

        [Test]
        public async Task DeleteEntryAsync_ById_ReturnSmallerMax()
        {
            var officeElements = await CreateTaskAsync();

            await officeElements.AddNewEntryAsync(TimeSpan.FromHours(8), TimeSpan.FromHours(9), null);

            var max = officeElements.MaxElement();

            await officeElements.DeleteEntryAsync(officeElements.Entries[officeElements.MaxElement()].Id);

            Assert.AreNotEqual(officeElements.MaxElement(), max);
        }

        [TearDown]
        public async Task CleanupAsync()
        {
            MainSql.SetNullConnection();

            var dateInterface = Substitute.For<IGetDate>();
            dateInterface.GetCurrentDate().Returns(DateTime.Parse("2020-01-01"));

            MainSql.CreateConnection();
            await MainSql.CkeckConnectionAsync();
            await MainSql.CreateAsync(new TestUserHelpers(), new TestInternetAccess(), "1111");

            await MainSql.DeleteFromDbAsync(true, "2020-01-01").ConfigureAwait(false);

            MainSql.SetNullConnection();
        }

        private sealed class TestUserHelpers : UserHelpers
        {
            public override Task CreateLoggedUserAsync(string user) =>
                Task.CompletedTask;
        }

        private sealed class TestInternetAccess : InternetAccess
        {
            public override Task<bool> CheckInternetAsync() =>
                Task.FromResult(true);
        }
    }
}
