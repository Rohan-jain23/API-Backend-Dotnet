using System;
using System.Collections.Generic;
using System.Linq;
using StrawberryShake;
using WuH.Ruby.FrameworkAPI.Client.GraphQL;
using WuH.Ruby.FrameworkAPI.Client;

namespace FrameworkAPI.Client.Test;

// ###################################################################################### Raw Material Consumption By TimeSpan ######################################################################################
public class MockRawMaterialConsumptionByMaterialValueByTimeSpan(double value, string unit) : IGenerateRawMaterialConsumptionByTimeSpan_MachineTimeSpan_RawMaterialConsumptionByMaterial_Value
{
    public double? Value { get; set; } = value;
    public string? Unit { get; set; } = unit;
}

public class MockRawMaterialConsumptionByMaterialByTimeSpan(string materialName, double value, string unit) : IGenerateRawMaterialConsumptionByTimeSpan_MachineTimeSpan_RawMaterialConsumptionByMaterial
{
    public string Key { get; set; } = materialName;
    public IGenerateRawMaterialConsumptionByTimeSpan_MachineTimeSpan_RawMaterialConsumptionByMaterial_Value Value { get; set; } = new MockRawMaterialConsumptionByMaterialValueByTimeSpan(value, unit);
}

public class MockExtrusionMachineTimeSpan(RawMaterialConsumptionByMaterial? rawMaterialConsumptionByMaterial) : IGenerateRawMaterialConsumptionByTimeSpan_MachineTimeSpan_ExtrusionMachineTimeSpan
{
    public string __typename { get; set; } = "";
    public IReadOnlyList<IGenerateRawMaterialConsumptionByTimeSpan_MachineTimeSpan_RawMaterialConsumptionByMaterial>? RawMaterialConsumptionByMaterial { get; set; } = rawMaterialConsumptionByMaterial?
        .Select(kv => new MockRawMaterialConsumptionByMaterialByTimeSpan(kv.Key, kv.Value.Consumption, kv.Value.Unit)).ToList();
}

public class MockGenerateRawMaterialConsumptionByTimeSpanResult(RawMaterialConsumptionByMaterial? rawMaterialConsumptionByTimeSpanResult) : IGenerateRawMaterialConsumptionByTimeSpanResult
{
    public IGenerateRawMaterialConsumptionByTimeSpan_MachineTimeSpan MachineTimeSpan { get; set; } = new MockExtrusionMachineTimeSpan(rawMaterialConsumptionByTimeSpanResult);
}
// ###################################################################################### Raw Material Consumption By Job ######################################################################################
public class MockRawMaterialConsumptionByMaterialValueJob(double value, string unit) : IGenerateRawMaterialConsumptionByJob_ProducedJob_RawMaterialConsumptionByMaterial_Value
{
    public double? Value { get; set; } = value;
    public string? Unit { get; set; } = unit;
}

public class MockRawMaterialConsumptionByMaterialByJob(string materialName, double value, string unit) : IGenerateRawMaterialConsumptionByJob_ProducedJob_RawMaterialConsumptionByMaterial
{
    public string Key { get; set; } = materialName;
    public IGenerateRawMaterialConsumptionByJob_ProducedJob_RawMaterialConsumptionByMaterial_Value Value { get; set; } = new MockRawMaterialConsumptionByMaterialValueJob(value, unit);
}

public class MockProducedJob(RawMaterialConsumptionByMaterial? rawMaterialConsumptionByMaterial) : IGenerateRawMaterialConsumptionByJob_ProducedJob_ExtrusionProducedJob
{
    public IReadOnlyList<IGenerateRawMaterialConsumptionByJob_ProducedJob_RawMaterialConsumptionByMaterial>? RawMaterialConsumptionByMaterial { get; set; } = rawMaterialConsumptionByMaterial?
        .Select(kv => new MockRawMaterialConsumptionByMaterialByJob(kv.Key, kv.Value.Consumption, kv.Value.Unit)).ToList();
}

public class MockIGenerateRawMaterialConsumptionByJobResult(RawMaterialConsumptionByMaterial? rawMaterialConsumptionByTimeSpanResult) : IGenerateRawMaterialConsumptionByJobResult
{
    public IGenerateRawMaterialConsumptionByJob_ProducedJob ProducedJob { get; set; } = new MockProducedJob(rawMaterialConsumptionByTimeSpanResult);
}
// ###################################################################################### General ######################################################################################
public class MockClientError(
    string message,
    string? code = null,
    IReadOnlyList<object>? path = null,
    IReadOnlyList<Location>? locations = null,
    Exception? exception = null,
    IReadOnlyDictionary<string, object?>? extensions = null) : IClientError
{
    public string Message { get; set; } = message;
    public string? Code { get; set; } = code;
    public IReadOnlyList<object>? Path { get; set; } = path;
    public IReadOnlyList<Location>? Locations { get; set; } = locations;
    public Exception? Exception { get; set; } = exception;
    public IReadOnlyDictionary<string, object?>? Extensions { get; set; } = extensions;
}