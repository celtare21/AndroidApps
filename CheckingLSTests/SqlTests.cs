using CheckinLS.API;
using CheckinLS.InterfacesAndClasses;
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;

namespace CheckingLSTests
{
    [TestFixture]
    public class SqlTests
    {
        private IGetDate _dateInterface;

        [SetUp]
        public void SetInterfaces()
        {
            _dateInterface = Substitute.For<IGetDate>();
            _dateInterface.GetCurrentDate().Returns(DateTime.Parse("2020-01-01"));
        }

        [TestCase("1111")]
        public async Task CreateAsync_CorrectUser_ReturnsClassAsync(string pin)
        {
            (MainSql sqlClass, int returnCode) = await MainSql.CreateAsync(pin, _dateInterface);

            Assert.IsNotNull(sqlClass);
            Assert.AreEqual(returnCode, 0);
        }

        [TestCase("Alex")]
        [TestCase("123")]
        [TestCase("!@#")]
        public async Task CreateAsync_IncorrectUser_ReturnsNullAsync(string pin)
        {
            (MainSql sqlClass, int returnCode) = await MainSql.CreateAsync(pin, _dateInterface);

            Assert.IsNull(sqlClass);
            Assert.AreEqual(returnCode, -2);
        }

        [Test]
        public async Task AddNewEntryInDbAsync_AllFalse_ReturnsException()
        {
            (MainSql sqlClass, _) = await MainSql.CreateAsync("1111", _dateInterface);

            Assert.CatchAsync(() => sqlClass.AddNewEntryInDbAsync(null, false, false, false), "All parameters are false!");
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryInDbAsync_CursMultipleObservations_ReturnsCursAndObservation(string obs)
        {
            (MainSql sqlClass, _) = await MainSql.CreateAsync("1111", _dateInterface);

            await sqlClass.AddNewEntryInDbAsync(obs, true, false, false);

            var index = sqlClass.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(sqlClass.Elements[index].Observatii, "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(sqlClass.Elements[index].Observatii, obs);
                    break;
            }
            Assert.AreEqual(sqlClass.Elements[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(sqlClass.Elements[index].OraIncepere, TimeSpan.Parse("10:00"));
            Assert.AreEqual(sqlClass.Elements[index].OraFinal, TimeSpan.Parse("11:30"));
            Assert.AreEqual(sqlClass.Elements[index].CursAlocat, TimeSpan.Parse("01:30"));
            Assert.AreEqual(sqlClass.Elements[index].PregatireAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(sqlClass.Elements[index].RecuperareAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(sqlClass.Elements[index].Total, TimeSpan.Parse("01:30"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryInDbAsync_PregatireMultipleObservations_ReturnsPregatireAndObservation(string obs)
        {
            (MainSql sqlClass, _) = await MainSql.CreateAsync("1111", _dateInterface);

            await sqlClass.AddNewEntryInDbAsync(obs, false, true, false);

            var index = sqlClass.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(sqlClass.Elements[index].Observatii, "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(sqlClass.Elements[index].Observatii, obs);
                    break;
            }
            Assert.AreEqual(sqlClass.Elements[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(sqlClass.Elements[index].OraIncepere, TimeSpan.Parse("10:00"));
            Assert.AreEqual(sqlClass.Elements[index].OraFinal, TimeSpan.Parse("10:30"));
            Assert.AreEqual(sqlClass.Elements[index].CursAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(sqlClass.Elements[index].PregatireAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(sqlClass.Elements[index].RecuperareAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(sqlClass.Elements[index].Total, TimeSpan.Parse("00:30"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryInDbAsync_RecuperareMultipleObservations_ReturnsRecuperareAndObservation(string obs)
        {
            (MainSql sqlClass, _) = await MainSql.CreateAsync("1111", _dateInterface);

            await sqlClass.AddNewEntryInDbAsync(obs, false, false, true);

            var index = sqlClass.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(sqlClass.Elements[index].Observatii, "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(sqlClass.Elements[index].Observatii, obs);
                    break;
            }
            Assert.AreEqual(sqlClass.Elements[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(sqlClass.Elements[index].OraIncepere, TimeSpan.Parse("10:00"));
            Assert.AreEqual(sqlClass.Elements[index].OraFinal, TimeSpan.Parse("10:30"));
            Assert.AreEqual(sqlClass.Elements[index].CursAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(sqlClass.Elements[index].PregatireAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(sqlClass.Elements[index].RecuperareAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(sqlClass.Elements[index].Total, TimeSpan.Parse("00:30"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryInDbAsync_CursPregatireMultipleObservations_ReturnsCursPregatireandObservation(string obs)
        {
            (MainSql sqlClass, _) = await MainSql.CreateAsync("1111", _dateInterface);

            await sqlClass.AddNewEntryInDbAsync(obs, true, true, false);

            var index = sqlClass.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(sqlClass.Elements[index].Observatii, "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(sqlClass.Elements[index].Observatii, obs);
                    break;
            }
            Assert.AreEqual(sqlClass.Elements[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(sqlClass.Elements[index].OraIncepere, TimeSpan.Parse("10:00"));
            Assert.AreEqual(sqlClass.Elements[index].OraFinal, TimeSpan.Parse("12:00"));
            Assert.AreEqual(sqlClass.Elements[index].CursAlocat, TimeSpan.Parse("01:30"));
            Assert.AreEqual(sqlClass.Elements[index].PregatireAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(sqlClass.Elements[index].RecuperareAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(sqlClass.Elements[index].Total, TimeSpan.Parse("02:00"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryInDbAsync_CursRecuperareMultipleObservations_ReturnsCursRecuperareandObservation(string obs)
        {
            (MainSql sqlClass, _) = await MainSql.CreateAsync("1111", _dateInterface);

            await sqlClass.AddNewEntryInDbAsync(obs, true, false, true);

            var index = sqlClass.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(sqlClass.Elements[index].Observatii, "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(sqlClass.Elements[index].Observatii, obs);
                    break;
            }
            Assert.AreEqual(sqlClass.Elements[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(sqlClass.Elements[index].OraIncepere, TimeSpan.Parse("10:00"));
            Assert.AreEqual(sqlClass.Elements[index].OraFinal, TimeSpan.Parse("12:00"));
            Assert.AreEqual(sqlClass.Elements[index].CursAlocat, TimeSpan.Parse("01:30"));
            Assert.AreEqual(sqlClass.Elements[index].PregatireAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(sqlClass.Elements[index].RecuperareAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(sqlClass.Elements[index].Total, TimeSpan.Parse("02:00"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryInDbAsync_PregatireRecuperareMultipleObservations_ReturnsPregatireRecuperareandObservation(string obs)
        {
            (MainSql sqlClass, _) = await MainSql.CreateAsync("1111", _dateInterface);

            await sqlClass.AddNewEntryInDbAsync(obs, false, true, true);

            var index = sqlClass.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(sqlClass.Elements[index].Observatii, "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(sqlClass.Elements[index].Observatii, obs);
                    break;
            }
            Assert.AreEqual(sqlClass.Elements[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(sqlClass.Elements[index].OraIncepere, TimeSpan.Parse("10:00"));
            Assert.AreEqual(sqlClass.Elements[index].OraFinal, TimeSpan.Parse("11:00"));
            Assert.AreEqual(sqlClass.Elements[index].CursAlocat, TimeSpan.Parse("00:00"));
            Assert.AreEqual(sqlClass.Elements[index].PregatireAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(sqlClass.Elements[index].RecuperareAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(sqlClass.Elements[index].Total, TimeSpan.Parse("01:00"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryInDbAsync_CursRecuperarePregatireMultipleObservations_ReturnsCursPregatireRecuperareandObservation(string obs)
        {
            (MainSql sqlClass, _) = await MainSql.CreateAsync("1111", _dateInterface);

            await sqlClass.AddNewEntryInDbAsync(obs, true, true, true);

            var index = sqlClass.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(sqlClass.Elements[index].Observatii, "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(sqlClass.Elements[index].Observatii, obs);
                    break;
            }
            Assert.AreEqual(sqlClass.Elements[index].Date, DateTime.Parse("2020-01-01"));
            Assert.AreEqual(sqlClass.Elements[index].OraIncepere, TimeSpan.Parse("10:00"));
            Assert.AreEqual(sqlClass.Elements[index].OraFinal, TimeSpan.Parse("12:30"));
            Assert.AreEqual(sqlClass.Elements[index].CursAlocat, TimeSpan.Parse("01:30"));
            Assert.AreEqual(sqlClass.Elements[index].PregatireAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(sqlClass.Elements[index].RecuperareAlocat, TimeSpan.Parse("00:30"));
            Assert.AreEqual(sqlClass.Elements[index].Total, TimeSpan.Parse("02:30"));
        }

        [Test]
        public async Task AddNewEntryInDbAsync_AllTillOverflow_ReturnsException()
        {
            (MainSql sqlClass, _) = await MainSql.CreateAsync("1111", _dateInterface);

            await sqlClass.AddNewEntryInDbAsync(null, true, true, true);
            await sqlClass.AddNewEntryInDbAsync(null, true, true, true);
            await sqlClass.AddNewEntryInDbAsync(null, true, true, true);
            await sqlClass.AddNewEntryInDbAsync(null, true, true, true);
            await sqlClass.AddNewEntryInDbAsync(null, true, true, true);

            Assert.CatchAsync(() => sqlClass.AddNewEntryInDbAsync(null, true, true, true), "Hours out of bounds!");
        }

        [Test]
        public async Task DeleteFromDbAsync_NoArguments_ReturnsException()
        {
            (MainSql sqlClass, _) = await MainSql.CreateAsync("1111", _dateInterface);

            Assert.CatchAsync(() => sqlClass.DeleteFromDbAsync(), "All parameters are false!");
        }

        [Test]
        public async Task DeleteFromDbAsync_ById_ReturnSmallerMax()
        {
            (MainSql sqlClass, _) = await MainSql.CreateAsync("1111", _dateInterface);

            await sqlClass.AddNewEntryInDbAsync(null, false, false, true);

            var max = sqlClass.MaxElement();

            await sqlClass.DeleteFromDbAsync(sqlClass.Elements[sqlClass.MaxElement() - 1].Id);

            Assert.AreNotEqual(sqlClass.MaxElement(), max);
        }

        [TearDown]
        public async Task CleanupAsync()
        {
            (MainSql sqlClass, _) = await MainSql.CreateAsync("1111", _dateInterface);

            await sqlClass.DeleteFromDbAsync(date: "2020-01-01").ConfigureAwait(false);
        }
    }
}
