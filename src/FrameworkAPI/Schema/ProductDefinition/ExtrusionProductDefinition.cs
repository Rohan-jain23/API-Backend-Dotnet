namespace FrameworkAPI.Schema.ProductDefinition;
/// <summary>
/// Product definition entity of extrusion machines.
/// A product definition is used to group similar products and describe them in a generic 'RUBY language'.
/// Like this, the different production runs of a product can be analyzed.
/// The extrusion produced product is currently recognized from the machines process data (mainly the material mix).
/// </summary>
public class ExtrusionProductDefinition(string id, string name) : ProductDefinition(id, name)
{
}