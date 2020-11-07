using System;
using CheckinLS.API;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CheckingLSTests
{
    [TestFixture]
    public class SqlTests
    {
        [TestCase("1111")]
        public async Task CreateAsync_CorrectUser_ReturnsClassAsync(string pin)
        {
            var result = await MainSql.CreateAsync(pin, true);

            Assert.IsNotNull(result);
        }

        [TestCase("Alex")]
        [TestCase("123")]
        [TestCase("!@#")]
        public async Task CreateAsync_IncorrectUser_ReturnsNullAsync(string pin)
        {
            var result = await MainSql.CreateAsync(pin, true);

            Assert.IsNull(result);
        }

        [Test]
        public async Task AddNewEntryInDbAsync_AllFalse_ReturnsException()
        {
            var main = await MainSql.CreateAsync("1111", true);

            Assert.CatchAsync(() => main.AddNewEntryInDbAsync(null, false, false, false), "All parameters are false!");
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryInDbAsync_CursMultipleObservations_ReturnsCursAndObservation(string obs)
        {
            var main = await MainSql.CreateAsync("1111", true);

            await main.AddNewEntryInDbAsync(obs, true, false, false);

            var index = main.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(main.Elements["observatii"][index], "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(main.Elements["observatii"][index], obs);
                    break;
            }
            Assert.AreEqual(main.Elements["date"][index], DateTime.Parse("2020-01-01"));
            Assert.AreEqual(main.Elements["ora_incepere"][index], TimeSpan.Parse("10:00"));
            Assert.AreEqual(main.Elements["ora_final"][index], TimeSpan.Parse("11:30"));
            Assert.AreEqual(main.Elements["curs_alocat"][index], TimeSpan.Parse("01:30"));
            Assert.AreEqual(main.Elements["pregatire_alocat"][index], TimeSpan.Parse("00:00"));
            Assert.AreEqual(main.Elements["recuperare_alocat"][index], TimeSpan.Parse("00:00"));
            Assert.AreEqual(main.Elements["total"][index], TimeSpan.Parse("01:30"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryInDbAsync_PregatireMultipleObservations_ReturnsPregatireAndObservation(string obs)
        {
            var main = await MainSql.CreateAsync("1111", true);

            await main.AddNewEntryInDbAsync(obs, false, true, false);

            var index = main.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(main.Elements["observatii"][index], "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(main.Elements["observatii"][index], obs);
                    break;
            }
            Assert.AreEqual(main.Elements["date"][index], DateTime.Parse("2020-01-01"));
            Assert.AreEqual(main.Elements["ora_incepere"][index], TimeSpan.Parse("10:00"));
            Assert.AreEqual(main.Elements["ora_final"][index], TimeSpan.Parse("10:30"));
            Assert.AreEqual(main.Elements["curs_alocat"][index], TimeSpan.Parse("00:00"));
            Assert.AreEqual(main.Elements["pregatire_alocat"][index], TimeSpan.Parse("00:30"));
            Assert.AreEqual(main.Elements["recuperare_alocat"][index], TimeSpan.Parse("00:00"));
            Assert.AreEqual(main.Elements["total"][index], TimeSpan.Parse("00:30"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryInDbAsync_RecuperareMultipleObservations_ReturnsRecuperareAndObservation(string obs)
        {
            var main = await MainSql.CreateAsync("1111", true);

            await main.AddNewEntryInDbAsync(obs, false, false, true);

            var index = main.MaxElement() - 1;

            Console.WriteLine(main.Elements["ora_final"][index].ToString());

            switch (obs)
            {
                case null:
                    Assert.AreEqual(main.Elements["observatii"][index], "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(main.Elements["observatii"][index], obs);
                    break;
            }
            Assert.AreEqual(main.Elements["date"][index], DateTime.Parse("2020-01-01"));
            Assert.AreEqual(main.Elements["ora_incepere"][index], TimeSpan.Parse("10:00"));
            Assert.AreEqual(main.Elements["ora_final"][index], TimeSpan.Parse("10:30"));
            Assert.AreEqual(main.Elements["curs_alocat"][index], TimeSpan.Parse("00:00"));
            Assert.AreEqual(main.Elements["pregatire_alocat"][index], TimeSpan.Parse("00:00"));
            Assert.AreEqual(main.Elements["recuperare_alocat"][index], TimeSpan.Parse("00:30"));
            Assert.AreEqual(main.Elements["total"][index], TimeSpan.Parse("00:30"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryInDbAsync_CursPregatireMultipleObservations_ReturnsCursPregatireandObservation(string obs)
        {
            var main = await MainSql.CreateAsync("1111", true);

            await main.AddNewEntryInDbAsync(obs, true, true, false);

            var index = main.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(main.Elements["observatii"][index], "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(main.Elements["observatii"][index], obs);
                    break;
            }
            Assert.AreEqual(main.Elements["date"][index], DateTime.Parse("2020-01-01"));
            Assert.AreEqual(main.Elements["ora_incepere"][index], TimeSpan.Parse("10:00"));
            Assert.AreEqual(main.Elements["ora_final"][index], TimeSpan.Parse("12:00"));
            Assert.AreEqual(main.Elements["curs_alocat"][index], TimeSpan.Parse("01:30"));
            Assert.AreEqual(main.Elements["pregatire_alocat"][index], TimeSpan.Parse("00:30"));
            Assert.AreEqual(main.Elements["recuperare_alocat"][index], TimeSpan.Parse("00:00"));
            Assert.AreEqual(main.Elements["total"][index], TimeSpan.Parse("02:00"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryInDbAsync_CursRecuperareMultipleObservations_ReturnsCursRecuperareandObservation(string obs)
        {
            var main = await MainSql.CreateAsync("1111", true);

            await main.AddNewEntryInDbAsync(obs, true, false, true);

            var index = main.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(main.Elements["observatii"][index], "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(main.Elements["observatii"][index], obs);
                    break;
            }
            Assert.AreEqual(main.Elements["date"][index], DateTime.Parse("2020-01-01"));
            Assert.AreEqual(main.Elements["ora_incepere"][index], TimeSpan.Parse("10:00"));
            Assert.AreEqual(main.Elements["ora_final"][index], TimeSpan.Parse("12:00"));
            Assert.AreEqual(main.Elements["curs_alocat"][index], TimeSpan.Parse("01:30"));
            Assert.AreEqual(main.Elements["pregatire_alocat"][index], TimeSpan.Parse("00:00"));
            Assert.AreEqual(main.Elements["recuperare_alocat"][index], TimeSpan.Parse("00:30"));
            Assert.AreEqual(main.Elements["total"][index], TimeSpan.Parse("02:00"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryInDbAsync_PregatireRecuperareMultipleObservations_ReturnsPregatireRecuperareandObservation(string obs)
        {
            var main = await MainSql.CreateAsync("1111", true);

            await main.AddNewEntryInDbAsync(obs, false, true, true);

            var index = main.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(main.Elements["observatii"][index], "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(main.Elements["observatii"][index], obs);
                    break;
            }
            Assert.AreEqual(main.Elements["date"][index], DateTime.Parse("2020-01-01"));
            Assert.AreEqual(main.Elements["ora_incepere"][index], TimeSpan.Parse("10:00"));
            Assert.AreEqual(main.Elements["ora_final"][index], TimeSpan.Parse("11:00"));
            Assert.AreEqual(main.Elements["curs_alocat"][index], TimeSpan.Parse("00:00"));
            Assert.AreEqual(main.Elements["pregatire_alocat"][index], TimeSpan.Parse("00:30"));
            Assert.AreEqual(main.Elements["recuperare_alocat"][index], TimeSpan.Parse("00:30"));
            Assert.AreEqual(main.Elements["total"][index], TimeSpan.Parse("01:00"));
        }

        [TestCase(null)]
        [TestCase("test")]
        [TestCase("this is a test")]
        [TestCase("this is an even longer test")]
        public async Task AddNewEntryInDbAsync_CursRecuperarePregatireMultipleObservations_ReturnsCursPregatireRecuperareandObservation(string obs)
        {
            var main = await MainSql.CreateAsync("1111", true);

            await main.AddNewEntryInDbAsync(obs, true, true, true);

            var index = main.MaxElement() - 1;

            switch (obs)
            {
                case null:
                    Assert.AreEqual(main.Elements["observatii"][index], "None");
                    break;
                case "test":
                case "this is a test":
                case "this is an even longer test":
                    Assert.AreEqual(main.Elements["observatii"][index], obs);
                    break;
            }
            Assert.AreEqual(main.Elements["date"][index], DateTime.Parse("2020-01-01"));
            Assert.AreEqual(main.Elements["ora_incepere"][index], TimeSpan.Parse("10:00"));
            Assert.AreEqual(main.Elements["ora_final"][index], TimeSpan.Parse("12:30"));
            Assert.AreEqual(main.Elements["curs_alocat"][index], TimeSpan.Parse("01:30"));
            Assert.AreEqual(main.Elements["pregatire_alocat"][index], TimeSpan.Parse("00:30"));
            Assert.AreEqual(main.Elements["recuperare_alocat"][index], TimeSpan.Parse("00:30"));
            Assert.AreEqual(main.Elements["total"][index], TimeSpan.Parse("02:30"));
        }

        [Test]
        public async Task AddNewEntryInDbAsync_AllTillOverflow_ReturnsException()
        {
            var main = await MainSql.CreateAsync("1111", true);

            await main.AddNewEntryInDbAsync(null, true, true, true);
            await main.AddNewEntryInDbAsync(null, true, true, true);
            await main.AddNewEntryInDbAsync(null, true, true, true);
            await main.AddNewEntryInDbAsync(null, true, true, true);
            await main.AddNewEntryInDbAsync(null, true, true, true);

            Assert.CatchAsync(() => main.AddNewEntryInDbAsync(null, true, true, true), "Hours out of bounds!");
        }

        [Test]
        public async Task DeleteFromDbAsync_NoArguments_ReturnsException()
        {
            var main = await MainSql.CreateAsync("1111", true);

            Assert.CatchAsync(() => main.DeleteFromDbAsync(), "All parameters are false!");
        }

        [Test]
        public async Task DeleteFromDbAsync_ById_ReturnSmallerMax()
        {
            var main = await MainSql.CreateAsync("1111", true);

            await main.AddNewEntryInDbAsync(null, false, false, true);

            var max = main.MaxElement();

            await main.DeleteFromDbAsync((int)main.Elements["id"][main.MaxElement() - 1]);

            Assert.AreNotEqual(main.MaxElement(), max);
        }

        [TearDown]
        public async Task CleanupAsync()
        {
            var main = await MainSql.CreateAsync("1111", true);

            await main.DeleteFromDbAsync(date: "2020-01-01").ConfigureAwait(false);
        }
    }
}
