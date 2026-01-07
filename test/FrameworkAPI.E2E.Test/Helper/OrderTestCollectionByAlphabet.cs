using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace FrameworkAPI.E2E.Test.Helper;

public class OrderTestCollectionByAlphabet : ITestCollectionOrderer
{
    public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
    {
        return testCollections.OrderBy(testCollection => testCollection.DisplayName);
    }
}