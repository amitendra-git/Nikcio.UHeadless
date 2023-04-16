using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using Nikcio.UHeadless.Basics.Properties.Models;
using Nikcio.UHeadless.Content.Basics.Models;
using Nikcio.UHeadless.Content.Queries;
using Nikcio.UHeadless.Content.Repositories;
using Nikcio.UHeadless.Core.GraphQL.Queries;

namespace Nikcio.UHeadless.Content.Basics.Queries;

/// <summary>
/// The default query implementation of the ContentById query
/// </summary>
[ExtendObjectType(typeof(Query))]
public class AuthContentByIdQuery : ContentByIdQuery<BasicContent, BasicProperty>
{
    /// <inheritdoc />
    [Authorize]
    public override BasicContent? ContentById(
        [Service] IContentRepository<BasicContent, BasicProperty> contentRepository,
        [GraphQLDescription("The id to fetch.")] int id,
        [GraphQLDescription("The culture to fetch.")] string? culture = null,
        [GraphQLDescription("Fetch preview values. Preview will show unpublished items.")] bool preview = false)
    {
        return base.ContentById(contentRepository, id, culture, preview);
    }
}
