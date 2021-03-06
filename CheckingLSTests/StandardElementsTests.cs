﻿using CheckinLS.API.Misc;
using CheckinLS.API.Standard;
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
    public class StandardElementsTests
    {
        private static async Task<StandardElements> CreateTaskAsync()
        {
            var dateInterface = Substitute.For<IGetDate>();
            dateInterface.GetCurrentDate().Returns(DateTime.Parse("2020-01-01"));

            MainSql.CreateConnection();
            await MainSql.CkeckConnectionAsync();
            await MainSql.CreateAsync(new TestUserHelpers(), new TestInternetAccess(), "1111");

            return await StandardElements.CreateAsync(dateInterface).ConfigureAwait(false);
        }

        [TestCase("1111")]
        public async Task CreateAsync_CorrectUser_ReturnsClassAsync(string pin)
        {
            var dateInterface = Substitute.For<IGetDate>();
            dateInterface.GetCurrentDate().Returns(DateTime.Parse("2020-01-01"));

            MainSql.CreateConnection();
            await MainSql.CkeckConnectionAsync();

            Task AsyncTestDelegate() => MainSql.CreateAsync(new TestUserHelpers(), new TestInternetAccess(), pin);

            Assert.DoesNotThrowAsync(AsyncTestDelegate);
        }

        [TestCase("Alex")]
        [TestCase("123")]
        [TestCase("!@#")]
        public async Task CreateAsync_IncorrectUser_ReturnsNullAsync(string pin)
        {
            var dateInterface = Substitute.For<IGetDate>();
            dateInterface.GetCurrentDate().Returns(DateTime.Parse("2020-01-01"));

            MainSql.CreateConnection();
            await MainSql.CkeckConnectionAsync();

            Task AsyncTestDelegate() => MainSql.CreateAsync(new TestUserHelpers(), new TestInternetAccess(), pin);

            Assert.CatchAsync<NoUserFound>(AsyncTestDelegate);
        }

        [Test]
        public async Task AddNewEntryAsync_AllFalse_ReturnsException()
        {
            var elements = await CreateTaskAsync();

            Task AsyncTestDelegate() => elements.AddNewEntryAsync(null, false, false, false, null, null);

            Assert.CatchAsync<AllParametersFalse>(AsyncTestDelegate);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryAsync_CursMultipleObservations_ReturnsCursAndObservation(string obs)
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(obs, true, false, false, null, null);

            var index = elements.MaxElement();

            switch (obs)
            {
                case null:
                case "":
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
        [TestCase("")]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryAsync_PregatireMultipleObservations_ReturnsPregatireAndObservation(string obs)
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(obs, false, true, false, null, null);

            var index = elements.MaxElement();

            switch (obs)
            {
                case null:
                case "":
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
        [TestCase("")]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryAsync_RecuperareMultipleObservations_ReturnsRecuperareAndObservation(string obs)
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(obs, false, false, true, null, null);

            var index = elements.MaxElement();

            switch (obs)
            {
                case null:
                case "":
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
        [TestCase("")]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryAsync_CursPregatireMultipleObservations_ReturnsCursPregatireandObservation(string obs)
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(obs, true, true, false, null, null);

            var index = elements.MaxElement();

            switch (obs)
            {
                case null:
                case "":
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
        [TestCase("")]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryAsync_CursRecuperareMultipleObservations_ReturnsCursRecuperareandObservation(string obs)
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(obs, true, false, true, null, null);

            var index = elements.MaxElement();

            switch (obs)
            {
                case null:
                case "":
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
        [TestCase("")]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryAsync_PregatireRecuperareMultipleObservations_ReturnsPregatireRecuperareandObservation(string obs)
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(obs, false, true, true, null, null);

            var index = elements.MaxElement();

            switch (obs)
            {
                case null:
                case "":
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
        [TestCase("")]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryAsync_CursRecuperarePregatireMultipleObservations_ReturnsCursPregatireRecuperareandObservation(string obs)
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(obs, true, true, true, null, null);

            var index = elements.MaxElement();

            switch (obs)
            {
                case null:
                case "":
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

            await elements.AddNewEntryAsync(null, true, true, true, null, null);
            await elements.AddNewEntryAsync(null, true, true, true, null, null);
            await elements.AddNewEntryAsync(null, true, true, true, null, null);
            await elements.AddNewEntryAsync(null, true, true, true, null, null);
            await elements.AddNewEntryAsync(null, true, true, true, null, null);

            Task AsyncTestDelegate() => elements.AddNewEntryAsync(null, true, true, true, null, null);

            Assert.CatchAsync<HoursOutOfBounds>(AsyncTestDelegate);
        }

        [Test]
        public async Task DeleteEntryAsync_ById_ReturnSmallerMax()
        {
            var elements = await CreateTaskAsync();

            await elements.AddNewEntryAsync(null, false, false, true, null, null);

            var max = elements.MaxElement();

            await elements.DeleteEntryAsync(elements.Entries[elements.MaxElement()].Id);

            Assert.AreNotEqual(elements.MaxElement(), max);
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

            await MainSql.DeleteFromDbAsync(false, null, "2020-01-01").ConfigureAwait(false);

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
