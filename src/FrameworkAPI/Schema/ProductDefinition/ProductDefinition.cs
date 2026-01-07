using System;
using HotChocolate.Types;

namespace FrameworkAPI.Schema.ProductDefinition;

/// <summary>
/// Generic interface for product definition entities of all machine families.
/// A product definition is used to group similar products and describe them in a generic 'RUBY language'.
/// Like this, the different production runs of a product can be analyzed.
/// </summary>
[InterfaceType]
public abstract class ProductDefinition(string id, string name)
{

    /// <summary>
    /// Unique identifier of the product definition.
    /// [Source: Department-specific ProductRecognizer]
    /// </summary>
    public string Id { get; set; } = id;

    /// <summary>
    /// Friendly name for the product definition.
    /// [Source: Department-specific ProductRecognizer]
    /// </summary>
    public string Name { get; set; } = name;

    internal static ProductDefinition CreateInstance(int businessUnit, string id, string name)
    {
        return businessUnit switch
        {
            3 => new ExtrusionProductDefinition(id, name),
            _ => throw new ArgumentException($"Creating a product definition is not supported for the business unit '{businessUnit}'.")
        };
    }
}