using Lapo.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Lapo.Integration.Tests;

[TestClass]
public class ConfigurationServiceTests
{
    const string Path = "appsettings.test";
    const string PathWithExtension = $"{Path}.json";
    const string Section = "Test";

    [TestInitialize]
    public async Task SetUp()
    {
        if (File.Exists(PathWithExtension)) File.Delete(PathWithExtension);
        await File.WriteAllTextAsync(PathWithExtension, "{}");
    }

    [TestCleanup]
    public void TearDown()
    {
        if (File.Exists(Path)) File.Delete(Path);
    }

    [TestMethod]
    public void Constructor_ShouldThrowFileNotFoundExceptionIfFileDoesNotExist() =>
        ThrowsExactly<FileNotFoundException>(() => _ = new ConfigurationService("nonexistent.json", "Section"));

    [TestMethod]
    public async Task UpsertAsync_ShouldAddNewValue()
    {
        var sut = new ConfigurationService(Path, Section);
        await sut.UpsertAsync("NewKey", "NewValue");

        const string expected = """
                                {
                                  "Test": {
                                    "NewKey": "NewValue"
                                  }
                                }
                                """;

        var actual = await GetActualAsync();
        AreEqual(expected.Trim(), actual.Trim());
    }
    
    [TestMethod]
    public async Task UpsertAsync_ShouldAddNewEnumerableValueWithOneElements()
    {
        var sut = new ConfigurationService(Path, Section);
        await sut.UpsertAsync<List<string>>("NewKey", ["Value"]);

        const string expected = """
                                {
                                  "Test": {
                                    "NewKey": [
                                      "Value"
                                    ]
                                  }
                                }
                                """;

        var actual = await GetActualAsync();
        AreEqual(expected.Trim(), actual.Trim());
    }
    
    [TestMethod]
    public async Task UpsertAsync_ShouldAddNewEnumerableValueWithTwoElements()
    {
        var sut = new ConfigurationService(Path, Section);
        await sut.UpsertAsync<List<string>>("NewKey", ["Value1", "Value2"]);

        const string expected = """
                                {
                                  "Test": {
                                    "NewKey": [
                                      "Value1",
                                      "Value2"
                                    ]
                                  }
                                }
                                """;

        var actual = await GetActualAsync();
        AreEqual(expected.Trim(), actual.Trim());
    }

    [TestMethod]
    public async Task UpsertAsync_ShouldUpdateExistingValue()
    {
        var sut = new ConfigurationService(Path, Section);
        await sut.UpsertAsync("ExistingKey", "InitialValue");
        await sut.UpsertAsync("ExistingKey", "UpdatedValue");

        const string expected = """
                                {
                                  "Test": {
                                    "ExistingKey": "UpdatedValue"
                                  }
                                }
                                """;

        var actual = await GetActualAsync();
        AreEqual(expected.Trim(), actual.Trim());
    }

    [TestMethod]
    public async Task RemoveAsync_ShouldRemoveExistingValue()
    {
        var sut = new ConfigurationService(Path, Section);
        await sut.UpsertAsync("Key", "Value");
        await sut.RemoveAsync("Key");

        const string expected = """
                                {
                                  "Test": {}
                                }
                                """;

        var actual = await GetActualAsync();
        AreEqual(expected.Trim(), actual.Trim());
    }

    [TestMethod]
    public async Task RemoveAsync_ShouldRemoveNestedKey()
    {
        var sut = new ConfigurationService(Path, Section);
        await sut.UpsertAsync("SubSection:SubSubSection:Key", "NestedValue");
        await sut.RemoveAsync("SubSection:SubSubSection:Key");

        const string expected = """
                                {
                                  "Test": {
                                    "SubSection": {
                                      "SubSubSection": {}
                                    }
                                  }
                                }
                                """;

        var actual = await GetActualAsync();
        AreEqual(expected.Trim(), actual.Trim());
    }

    [TestMethod]
    public async Task RemoveAsync_ShouldRemoveParentSectionIfEmpty()
    {
        var sut = new ConfigurationService(Path, Section);
        await sut.UpsertAsync("SubSection:SubSubSection:Key", "Value");
        await sut.RemoveAsync("SubSection:SubSubSection:Key");

        const string expected = """
                                {
                                  "Test": {
                                    "SubSection": {
                                      "SubSubSection": {}
                                    }
                                  }
                                }
                                """;

        var actual = await GetActualAsync();
        AreEqual(expected.Trim(), actual.Trim());
    }

    [TestMethod]
    public async Task RemoveAsync_ShouldDoNothingIfKeyDoesNotExist()
    {
        var sut = new ConfigurationService(Path, Section);
        await sut.UpsertAsync("Key", "Value");
        await sut.RemoveAsync("NonExistentKey");

        const string expected = """
                                {
                                  "Test": {
                                    "Key": "Value"
                                  }
                                }
                                """;

        var actual = await GetActualAsync();
        AreEqual(expected.Trim(), actual.Trim());
    }

    static Task<string> GetActualAsync() => File.ReadAllTextAsync(PathWithExtension);
}