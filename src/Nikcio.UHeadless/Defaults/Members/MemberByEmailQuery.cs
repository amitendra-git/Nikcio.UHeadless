using System.Diagnostics.CodeAnalysis;
using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nikcio.UHeadless.MemberItems;
using Nikcio.UHeadless.Members;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;

namespace Nikcio.UHeadless.Defaults.Members;

/// <summary>
/// Implements the <see cref="MemberByEmail"/> query
/// </summary>
[ExtendObjectType(typeof(HotChocolateQueryObject))]
public class MemberByEmailQuery : IGraphQLQuery
{
    public const string PolicyName = "MemberByEmailQuery";

    public const string ClaimValue = "member.by.email.query";

    public virtual void ApplyConfiguration(UHeadlessOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.UmbracoBuilder.Services.AddAuthorization(configure =>
        {
            configure.AddPolicy(PolicyName, policy =>
            {
                if (options.DisableAuthorization)
                {
                    policy.AddRequirements(new AlwaysAllowAuthoriaztionRequirement());
                    return;
                }

                policy.RequireAuthenticatedUser();

                policy.RequireClaim(DefaultClaims.UHeadlessScope, ClaimValue, DefaultClaimValues.GlobalMemberRead);
            });
        });
    }

    /// <summary>
    /// Gets a member by email
    /// </summary>
    [Authorize(Policy = PolicyName)]
    [GraphQLDescription("Gets a member by email.")]
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Marking as static will remove this query from GraphQL")]
    public MemberItem? MemberByEmail(
        IResolverContext resolverContext,
        [GraphQLDescription("The email to fetch.")] string email)
    {
        ArgumentNullException.ThrowIfNull(resolverContext);
        ArgumentException.ThrowIfNullOrEmpty(email);

        IMemberItemRepository<MemberItem> memberItemRepository = resolverContext.Service<IMemberItemRepository<MemberItem>>();

        IPublishedMemberCache? memberCache = memberItemRepository.GetCache();

        if (memberCache == null)
        {
            throw new InvalidOperationException("The content cache is not available");
        }

        IMemberService memberService = resolverContext.Service<IMemberService>();

        IMember? member = memberService.GetByEmail(email);

        if (member == null)
        {
            ILogger<MemberByEmailQuery> logger = resolverContext.Service<ILogger<MemberByEmailQuery>>();
            logger.LogInformation("Member not found with given email.");
            return default;
        }

        IPublishedContent? memberItem = memberCache.Get(member);

        return memberItemRepository.GetMemberItem(new MemberItemBase.CreateCommand()
        {
            PublishedContent = memberItem,
            ResolverContext = resolverContext,
        });
    }
}
