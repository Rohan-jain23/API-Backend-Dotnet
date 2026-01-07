using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCollectionOrderer(
    ordererTypeName: "FrameworkAPI.E2E.Test.Helper.OrderTestCollectionByAlphabet",
    ordererAssemblyName: "FrameworkAPI.E2E.Test")]