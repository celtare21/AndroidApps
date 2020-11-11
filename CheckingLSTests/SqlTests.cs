using CheckinLS.API;
using CheckinLS.InterfacesAndClasses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;

namespace CheckingLSTests
{
    [TestFixture]
    public class SqlTests
    {
        private async Task<Elements> CreateTaskAsync()
        {
            var dateInterface = Substitute.For<IGetDate>();
            dateInterface.GetCurrentDate().Returns(DateTime.Parse("2020-01-01"));
            
            var accounts = new List<Accounts> { new Accounts("test", "1111") };
            var userInterface = Substitute.For<IUsers>();
            userInterface.DeserializeCache().Returns(accounts);

            (MainSql sqlClass, _) = await MainSql.CreateAsync("1111", userInterface);

            return await Elements.CreateAsync(sqlClass, dateInterface).ConfigureAwait(false);

        }

        [TestCase("1111")]
        public async Task CreateAsync_CorrectUser_ReturnsClassAsync(string pin)
        {
            var dateInterface = Substitute.For<IGetDate>();
            dateInterface.GetCurrentDate().Returns(DateTime.Parse("2020-01-01"));

            var accounts = new List<Accounts> { new Accounts("test", "1111") };
            var userInterface = Substitute.For<IUsers>();
            userInterface.DeserializeCache().Returns(accounts);

            (MainSql sqlClass, int returnCode) = await MainSql.CreateAsync(pin, userInterface);

            Assert.IsNotNull(sqlClass);
            Assert.AreEqual(returnCode, 0);
        }

        [TestCase("Alex")]
        [TestCase("123")]
        [TestCase("!@#")]
        public async Task CreateAsync_IncorrectUser_ReturnsNullAsync(string pin)
        {
            var dateInterface = Substitute.For<IGetDate>();
            dateInterface.GetCurrentDate().Returns(DateTime.Parse("2020-01-01"));

            var accounts = new List<Accounts> { new Accounts("test", "1111") };
            var userInterface = Substitute.For<IUsers>();
            userInterface.DeserializeCache().Returns(accounts);

            (MainSql sqlClass, int returnCode) = await MainSql.CreateAsync(pin, userInterface);

            Assert.IsNull(sqlClass);
            Assert.AreEqual(returnCode, -2);
        }

        [Test]
        public async Task AddNewEntryAsync_AllFalse_ReturnsException()
        {
            var elements = await CreateTaskAsync();

            Assert.CatchAsync(() => elements.AddNewEntryAsync(null, false, false, false), "All parameters are false!");
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryAsync_CursMultipleObservations_ReturnsCursAndObservation(string obs)
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(obs, true, false, false);

            var index = elements.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(elements.Entries[index].Observatii, "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(elements.Entries[index].Observatii, obs);
                    break;
            }
            Assert.AreEqual(elements.Entries[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(elements.Entries[index].OraIncepere, TimeSpan.Parse("10:00"));
            Assert.AreEqual(elements.Entries[index].OraFinal, TimeSpan.Parse("11:30"));
            Assert.AreEqual(elements.Entries[index].CursAlocat, TimeSpan.Parse("01:30"));
            Assert.AreEqual(elements.Entries[index].PregatireAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(elements.Entries[index].RecuperareAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(elements.Entries[index].Total, TimeSpan.Parse("01:30"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryAsync_PregatireMultipleObservations_ReturnsPregatireAndObservation(string obs)
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(obs, false, true, false);

            var index = elements.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(elements.Entries[index].Observatii, "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(elements.Entries[index].Observatii, obs);
                    break;
            }
            Assert.AreEqual(elements.Entries[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(elements.Entries[index].OraIncepere, TimeSpan.Parse("10:00"));
            Assert.AreEqual(elements.Entries[index].OraFinal, TimeSpan.Parse("10:30"));
            Assert.AreEqual(elements.Entries[index].CursAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(elements.Entries[index].PregatireAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(elements.Entries[index].RecuperareAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(elements.Entries[index].Total, TimeSpan.Parse("00:30"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryAsync_RecuperareMultipleObservations_ReturnsRecuperareAndObservation(string obs)
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(obs, false, false, true);

            var index = elements.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(elements.Entries[index].Observatii, "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(elements.Entries[index].Observatii, obs);
                    break;
            }
            Assert.AreEqual(elements.Entries[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(elements.Entries[index].OraIncepere, TimeSpan.Parse("10:00"));
            Assert.AreEqual(elements.Entries[index].OraFinal, TimeSpan.Parse("10:30"));
            Assert.AreEqual(elements.Entries[index].CursAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(elements.Entries[index].PregatireAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(elements.Entries[index].RecuperareAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(elements.Entries[index].Total, TimeSpan.Parse("00:30"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryAsync_CursPregatireMultipleObservations_ReturnsCursPregatireandObservation(string obs)
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(obs, true, true, false);

            var index = elements.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(elements.Entries[index].Observatii, "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(elements.Entries[index].Observatii, obs);
                    break;
            }
            Assert.AreEqual(elements.Entries[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(elements.Entries[index].OraIncepere, TimeSpan.Parse("10:00"));
            Assert.AreEqual(elements.Entries[index].OraFinal, TimeSpan.Parse("12:00"));
            Assert.AreEqual(elements.Entries[index].CursAlocat, TimeSpan.Parse("01:30"));
            Assert.AreEqual(elements.Entries[index].PregatireAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(elements.Entries[index].RecuperareAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(elements.Entries[index].Total, TimeSpan.Parse("02:00"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryAsync_CursRecuperareMultipleObservations_ReturnsCursRecuperareandObservation(string obs)
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(obs, true, false, true);

            var index = elements.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(elements.Entries[index].Observatii, "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(elements.Entries[index].Observatii, obs);
                    break;
            }
            Assert.AreEqual(elements.Entries[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(elements.Entries[index].OraIncepere, TimeSpan.Parse("10:00"));
            Assert.AreEqual(elements.Entries[index].OraFinal, TimeSpan.Parse("12:00"));
            Assert.AreEqual(elements.Entries[index].CursAlocat, TimeSpan.Parse("01:30"));
            Assert.AreEqual(elements.Entries[index].PregatireAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(elements.Entries[index].RecuperareAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(elements.Entries[index].Total, TimeSpan.Parse("02:00"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryAsync_PregatireRecuperareMultipleObservations_ReturnsPregatireRecuperareandObservation(string obs)
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(obs, false, true, true);

            var index = elements.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(elements.Entries[index].Observatii, "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(elements.Entries[index].Observatii, obs);
                    break;
            }
            Assert.AreEqual(elements.Entries[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(elements.Entries[index].OraIncepere, TimeSpan.Parse("10:00"));
            Assert.AreEqual(elements.Entries[index].OraFinal, TimeSpan.Parse("11:00"));
            Assert.AreEqual(elements.Entries[index].CursAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(elements.Entries[index].PregatireAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(elements.Entries[index].RecuperareAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(elements.Entries[index].Total, TimeSpan.Parse("01:00"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryAsync_CursRecuperarePregatireMultipleObservations_ReturnsCursPregatireRecuperareandObservation(string obs)
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(obs, true, true, true);

            var index = elements.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(elements.Entries[index].Observatii, "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(elements.Entries[index].Observatii, obs);
                    break;
            }
            Assert.AreEqual(elements.Entries[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(elements.Entries[index].OraIncepere, TimeSpan.Parse("10:00"));
            Assert.AreEqual(elements.Entries[index].OraFinal, TimeSpan.Parse("12:30"));
            Assert.AreEqual(elements.Entries[index].CursAlocat, TimeSpan.Parse("01:30"));
            Assert.AreEqual(elements.Entries[index].PregatireAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(elements.Entries[index].RecuperareAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(elements.Entries[index].Total, TimeSpan.Parse("02:30"));
        }

        [Test]
        public async Task AddNewEntryAsync_AllTillOverflow_ReturnsException()
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(null, true, true, true);
            await elements.AddNewEntryAsync(null, true, true, true);
            await elements.AddNewEntryAsync(null, true, true, true);
            await elements.AddNewEntryAsync(null, true, true, true);
            await elements.AddNewEntryAsync(null, true, true, true);

            Assert.CatchAsync(() => elements.AddNewEntryAsync(null, true, true, true), "Hours out of bounds!");
        }

        [Test]
        public async Task DeleteEntryAsync_NoArguments_ReturnsException()
        {
            var elements = await CreateTaskAsync();

            Assert.CatchAsync(() => elements.DeleteEntryAsync(), "All parameters are false!");
        }

        [Test]
        public async Task DeleteEntryAsync_ById_ReturnSmallerMax()
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(null, false, false, true);

            var max = elements.MaxElement();

            await elements.DeleteEntryAsync(elements.Entries[elements.MaxElement() - 1].Id);

            Assert.AreNotEqual(elements.MaxElement(), max);
        }

        [TearDown]
        public async Task CleanupAsync()
        {
            var dateInterface = Substitute.For<IGetDate>();
            dateInterface.GetCurrentDate().Returns(DateTime.Parse("2020-01-01"));

            var accounts = new List<Accounts> { new Accounts("test", "1111") };
            var userInterface = Substitute.For<IUsers>();
            userInterface.DeserializeCache().Returns(accounts);

            (MainSql sqlClass, _) = await MainSql.CreateAsync("1111", userInterface);

            await sqlClass.DeleteFromDbAsync(date: "2020-01-01").ConfigureAwait(false);
        }
    }
}
