using DeltaLake.Table;

namespace DeltaLake.Tests.Table;
public class DeleteTests
{
    public static IEnumerable<object[]> BaseCases()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        yield return [1, default(string?), 0];
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        yield return [1, "test < CAST(0 AS INT)", 1];
        yield return [1, "test >= CAST(0 AS INT)", 0];
        yield return [1, "second == 'test'", 1];
        yield return [1, "second == '0'", 0];
        yield return [1, "third < 1", 0];
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        yield return [2, default(string?), 0];
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        yield return [2, "test < CAST(0 AS INT)", 2];
        yield return [2, "test >= CAST(0 AS INT)", 0];
        yield return [2, "second == 'test'", 2];
        yield return [2, "second == '0'", 1];
        yield return [2, "third < 1", 1];
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        yield return [10, default(string?), 0];
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        yield return [10, "test < CAST(0 AS INT)", 10];
        yield return [10, "test >= CAST(0 AS INT)", 0];
        yield return [10, "second == 'test'", 10];
        yield return [10, "second == '0'", 9];
        yield return [10, "third < 1", 9];
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        yield return [100, default(string?), 0];
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        yield return [100, "test < CAST(0 AS INT)", 100];
        yield return [100, "test >= CAST(0 AS INT)", 0];
        yield return [100, "second == 'test'", 100];
        yield return [100, "second == '0'", 99];
        yield return [100, "third < 1", 99];
    }

    [Theory]
    [MemberData(nameof(BaseCases))]
    public async Task Memory_Insert_Variable_Record_Delete_Test(
        int length,
        string? predicate,
        int expectedRecords)
    {
        await BaseDeleteTest($"memory://{Guid.NewGuid():N}", length, predicate, expectedRecords);
    }

    [Theory]
    [MemberData(nameof(BaseCases))]
    public async Task File_System_Insert_Variable_Record_Delete_Test(
        int length,
        string? predicate,
        int expectedRecords)
    {
        var info = Directory.CreateTempSubdirectory();
        try
        {
            await BaseDeleteTest($"file://{info.FullName}", length, predicate, expectedRecords);
        }
        finally
        {
            info.Delete(true);
        }
    }

    private async Task BaseDeleteTest(
        string path,
        int length,
        string? predicate,
        int expectedRecords)
    {
        var data = await TableHelpers.SetupTable(path, length);
        using var runtime = data.runtime;
        using var table = data.table;
        if (predicate == null)
        {
            await table.DeleteAsync(CancellationToken.None);
        }
        else
        {
            await table.DeleteAsync(predicate, CancellationToken.None);
        }

        var queryResult = table.QueryAsync(new SelectQuery("select * from test")
        {
            TableAlias = "test",
        },
        CancellationToken.None).ToBlockingEnumerable().ToList();

        var totalRecords = queryResult.Select(s => s.Length).Sum();
        Assert.Equal(expectedRecords, totalRecords);
    }
}