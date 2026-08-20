using Application.Dtos;

namespace Application.Interfaces;

/// <summary>
/// Renders an <see cref="OrderDto"/> into the semantic text used as the
/// row-level RAG document for that order (embedded and stored for
/// similarity search).
/// </summary>
public interface IOrderDocumentTextBuilder
{
    /// <summary>Builds the semantic document text for <paramref name="order"/>.</summary>
    string Build(OrderDto order);
}
