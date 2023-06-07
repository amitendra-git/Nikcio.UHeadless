using HotChocolate.Types;
using Nikcio.UHeadless.Base.Basics.Models;
using Nikcio.UHeadless.Core.GraphQL.Queries;
using Nikcio.UHeadless.Media.Basics.Models;
using Nikcio.UHeadless.Media.Queries;

namespace Nikcio.UHeadless.Media.Basics.Queries;

/// <summary>
/// The default query implementation of the MediaById query
/// </summary>
[ExtendObjectType(typeof(Query))]
public class BasicMediaByIdQuery : MediaByIdQuery<BasicMedia, BasicProperty>
{
}