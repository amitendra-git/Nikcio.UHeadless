using System.Net.Http.Json;

namespace Nikcio.UHeadless.IntegrationTests.Defaults;

public partial class ApiTests
{
    private const string _contentByGuidSnapshotPath = $"{SnapshotConstants.BasePath}/ContentByGuid";

    [Theory]
    [InlineData("eadd5be4-456c-4a7d-8c4a-2f7ead9c8ecf", "en-us", false, null, true)]
    [InlineData("eadd5be4-456c-4a7d-8c4a-2f7ead9c8ecf", "da", false, null, true)]
    [InlineData("eadd5be4-456c-4a7d-8c4a-2f7ead9c8ecf", null, false, null, true)]
    [InlineData("f894ebb3-7353-4439-b84c-04ed7d283202", "en-us", true, null, true)]
    [InlineData("d0351e46-e0bf-4eaa-ad2b-57a45451097e", "da", null, null, true)]
    [InlineData("08f23909-4751-47e9-b2e8-88ca07598947", null, null, null, true)]
    [InlineData("24099846-1261-4ee7-b2cf-223424767bdb", null, null, null, true)]
    [InlineData("ff3ef402-0e01-4043-b40f-85f71cc4e702", null, null, null, true)]
    [InlineData("abc", "en-us", false, null, false)]
    public async Task ContentByGuidQuery_Snaps_Async(
        string key,
        string? culture,
        bool? includePreview,
        string? segment,
        bool expectSuccess)
    {
        var snapshotProvider = new SnapshotProvider($"{_contentByGuidSnapshotPath}/Snaps");
        HttpClient client = _factory.CreateClient();

        using var request = JsonContent.Create(new
        {
            query = ContentByGuidQueries.GetItems,
            variables = new
            {
                key,
                culture,
                includePreview,
                segment
            }
        });

        HttpResponseMessage response = await client.PostAsync("/graphql", request).ConfigureAwait(true);

        string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

        string snapshotName = $"ContentByGuid_Snaps_{key}_{culture}_{includePreview}_{segment}";

        await snapshotProvider.AssertIsSnapshotEqualAsync(snapshotName, responseContent).ConfigureAwait(true);
        Assert.Equal(expectSuccess, response.IsSuccessStatusCode);
    }
}

public static class ContentByGuidQueries
{
    public const string GetItems = """
        query ContentByGuidQuery(
          $key: UUID!
          $culture: String
          $includePreview: Boolean
          $fallbacks: [PropertyFallback!]
          $segment: String
        ) {
          contentByGuid(
            id: $key
            inContext: {
              culture: $culture
              includePreview: $includePreview
              fallbacks: $fallbacks
              segment: $segment
            }
          ) {
            url(urlMode: ABSOLUTE)
            redirect {
              redirectUrl
              isPermanent
            }
            statusCode
            properties {
              ...typedProperties
              __typename
            }
            urlSegment
            name
            id
            key
            templateId
            parent {
              url(urlMode: ABSOLUTE)
              properties {
                ...typedProperties
                __typename
              }
              urlSegment
              name
              id
              key
              templateId
            }
            __typename
          }
        }
        """ + Fragments.TypedProperties;
}
