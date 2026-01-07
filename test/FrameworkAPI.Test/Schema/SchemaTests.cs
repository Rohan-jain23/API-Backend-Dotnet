using System.Threading.Tasks;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter.Xunit;
using Xunit;

namespace FrameworkAPI.Test.Schema;

public class SchemaTests
{
    [Fact]
    public async Task SchemaChangeTest()
    {
        var schema = await new ServiceCollection()
            .AddGraphQlServices()
            .BuildSchemaAsync();

        Snapshot.Match(schema.ToString(), "schema.graphql");
    }
}