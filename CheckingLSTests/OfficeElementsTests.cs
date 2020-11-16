using CheckinLS.API.Misc;
using CheckinLS.API.Office;
using CheckinLS.InterfacesAndClasses.Date;
using CheckinLS.InterfacesAndClasses.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;
using MainSql = CheckinLS.API.Sql.MainSql;

namespace CheckingLSTests
{
    [TestFixture]
    public class OfficeElementsTests
    {
        private async Task<OfficeElements> CreateTaskAsync()
        {
            var dateInterface = Substitute.For<IGetDate>();
            dateInterface.GetCurrentDate().Returns(DateTime.Parse("2020-01-01"));
            
            var accounts = new List<Accounts> { new Accounts("test", "1111") };
            var userInterface = Substitute.For<IUsers>();
            userInterface.DeserializeCache().Returns(accounts);

            MainSql.CreateConnection();
            await MainSql.CkeckConnectionAsync();
            _ = await MainSql.CreateAsync("1111", userInterface);

            return await OfficeElements.CreateAsync(dateInterface).ConfigureAwait(false);
        }

        [Test]
        public async Task AddNewEntryAsync_EqualTime_ReturnsException()
        {
            var officeElements = await CreateTaskAsync();

            Assert.CatchAsync(() => officeElements.AddNewEntryAsync(TimeSpan.FromHours(8), TimeSpan.FromHours(8)), "All parameters are false!");
        }

        [Test]
        public async Task AddNewEntryAsync_StartBiggerThanFinish_ReturnsException()
        {
            var officeElements = await CreateTaskAsync();

            Assert.CatchAsync(() => officeElements.AddNewEntryAsync(TimeSpan.FromHours(9), TimeSpan.FromHours(8)), "All parameters are false!");
        }

        [Test]
        public async Task AddNewEntryAsync_CursMultipleObservations_ReturnNewEntry()
        {
            var officeElements = await CreateTaskAsync();

            await officeElements.AddNewEntryAsync(TimeSpan.FromHours(8), TimeSpan.FromHours(9));

            var index = officeElements.MaxElement() - 1;

            Assert.AreEqual(officeElements.Entries[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(officeElements.Entries[index].OraIncepere, TimeSpan.FromHours(8));
            Assert.AreEqual(officeElements.Entries[index].OraFinal, TimeSpan.FromHours(9));
            Assert.AreEqual(officeElements.Entries[index].Total, TimeSpan.Parse("01:00"));
        }

        [Test]
        public async Task DeleteEntryAsync_ById_ReturnSmallerMax()
        {
            var officeElements = await CreateTaskAsync();

            await officeElements.AddNewEntryAsync(TimeSpan.FromHours(8), TimeSpan.FromHours(9));

            var max = officeElements.MaxElement();

            await officeElements.DeleteEntryAsync(officeElements.Entries[officeElements.MaxElement() - 1].Id);

            Assert.AreNotEqual(officeElements.MaxElement(), max);
        }

        [TearDown]
        public async Task CleanupAsync()
        {
            MainSql.SetNullConnection();

            var dateInterface = Substitute.For<IGetDate>();
            dateInterface.GetCurrentDate().Returns(DateTime.Parse("2020-01-01"));

            var accounts = new List<Accounts> { new Accounts("test", "1111") };
            var userInterface = Substitute.For<IUsers>();
            userInterface.DeserializeCache().Returns(accounts);

            MainSql.CreateConnection();
            await MainSql.CkeckConnectionAsync();
            _ = await MainSql.CreateAsync("1111", userInterface);

            await MainSql.DeleteFromDbAsync(true, date: "2020-01-01").ConfigureAwait(false);

            MainSql.SetNullConnection();
        }
    }
}
