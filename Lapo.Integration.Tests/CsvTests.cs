using System.Dynamic;
using Lapo.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Lapo.Integration.Tests;

[TestClass]
public class CsvServiceTests
{
    const string Path = "test";
    const string PathWithExtension = $"{Path}.csv";

    [TestInitialize]
    public Task SetUp() => File.WriteAllTextAsync(PathWithExtension, string.Empty);

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(PathWithExtension)) File.Delete(PathWithExtension);
    }

    [TestMethod]
    public void Constructor_ShouldThrowFileNotFoundException_IfFileDoesNotExist() =>
        ThrowsExactly<FileNotFoundException>(() => _ = new CsvService("nonexistent.csv"));

    [TestMethod]
    public async Task Write_ShouldWriteCsvWithSingleRecord()
    {
        var records = new List<dynamic>();

        dynamic jane = new ExpandoObject();
        jane.GivenName = "Jane";
        jane.FamilyName = "Doe";
        jane.Age = 25;

        records.Add(jane);

        var sut = new CsvService(Path);
        sut.Write(records);

        const string expected = "GivenName,FamilyName,Age\r\nJane,Doe,25\r\n";
        var actual = await GetActualAsync();

        AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task Write_ShouldWriteCsvWithValidRecords()
    {
        var records = new List<dynamic>();

        dynamic john = new ExpandoObject();
        john.GivenName = "John";
        john.FamilyName = "Doe";
        john.Age = 30;

        dynamic jane = new ExpandoObject();
        jane.GivenName = "Jane";
        jane.FamilyName = "Doe";
        jane.Age = 25;

        records.Add(john);
        records.Add(jane);

        var sut = new CsvService(Path);
        sut.Write(records);

        const string expected = "GivenName,FamilyName,Age\r\nJohn,Doe,30\r\nJane,Doe,25\r\n";
        var actual = await GetActualAsync();

        AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task Write_ShouldNotWriteCsv_WhenRecordsAreEmpty()
    {
        var records = new List<dynamic>();

        var sut = new CsvService(Path);
        sut.Write(records);

        var actual = await GetActualAsync();
        AreEqual(string.Empty, actual);
    }

    static Task<string> GetActualAsync() => File.ReadAllTextAsync(PathWithExtension);
}