﻿using System.Net.Http.Json;

namespace Nikcio.UHeadless.IntegrationTests.Sqlite;

public partial class ApiTests
{
    private const string _contentByTagSnapshotPath = $"{SnapshotConstants.BasePath}/ContentByTag";

    [Theory]
    [InlineData(0, null)]
    [InlineData(1, null)]
    [InlineData(5, null)]
    [InlineData(10, null)]
    [InlineData(0, "en-us")]
    [InlineData(1, "en-us")]
    [InlineData(10, "en-us")]
    [InlineData(5, "da")]
    public async Task ContentByTag_Can_Get_FirstNodes_Async(int firstCount, string? culture)
    {
        var snapshotProvider = new SnapshotProvider($"{_contentByTagSnapshotPath}/FirstNodes");
        var client = _factory.CreateClient();

        var request = JsonContent.Create(new
        {
            query = """
                query GetFirstNodesContentByTag($firstCount: Int!, $culture: String) {
                    contentByTag(tag: "normal", first: $firstCount, order: { id: ASC}, culture: $culture) { 
                        nodes {
                            id 
                        }
                        pageInfo {
                          hasNextPage
                        }
                    }
                }
                """,
            variables = new
            {
                firstCount,
                culture
            }
        });

        var response = await client.PostAsync("/graphql", request);

        var responseContent = await response.Content.ReadAsStringAsync();

        var snapshotName = $"ContentByTag_GetFirstNodes_{firstCount}_{culture}";

        await snapshotProvider.AssertIsSnapshotEqualAsync(snapshotName, responseContent);
        Assert.True(response.IsSuccessStatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("en-us")]
    [InlineData("da")]
    public async Task ContentByTag_Can_Get_General_Async(string? culture)
    {
        var snapshotProvider = new SnapshotProvider($"{_contentByTagSnapshotPath}/GetGeneral");
        var client = _factory.CreateClient();

        var request = JsonContent.Create(new
        {
            query = """
                query GetGeneralContentByTag($culture: String) {
                  contentByTag(tag: "normal", first: 50, culture: $culture) {
                    nodes {
                      id
                      key
                      path
                      name
                      creatorId
                      writerId
                      properties {
                        alias
                      }
                      itemType
                      level
                      parent {
                        name
                      }
                      redirect {
                        redirectUrl
                      }
                      sortOrder
                      templateId
                      url
                      urlSegment
                      absoluteUrl
                      children {
                        name
                        creatorId
                        writerId
                        properties {
                          alias
                        }
                        itemType
                        level
                        parent {
                          name
                        }
                        redirect {
                          redirectUrl
                        }
                        sortOrder
                        templateId
                        url
                        urlSegment
                        absoluteUrl
                      }
                    }
                  }
                }
                """,
            variables = new
            {
                culture
            }
        });

        var response = await client.PostAsync("/graphql", request);

        var responseContent = await response.Content.ReadAsStringAsync();

        var snapshotName = $"ContentByTag_GetGeneral_{culture}";

        await snapshotProvider.AssertIsSnapshotEqualAsync(snapshotName, responseContent);
        Assert.True(response.IsSuccessStatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("en-us")]
    [InlineData("da")]
    public async Task ContentByTag_Can_Get_NodeId_Async(string? culture)
    {
        var snapshotProvider = new SnapshotProvider($"{_contentByTagSnapshotPath}/GetNodeId");
        var client = _factory.CreateClient();

        var request = JsonContent.Create(new
        {
            query = """
                query GetNodeIdContentByTag($culture: String) {
                    contentByTag(tag: "normal", first: 50, culture: $culture) { 
                        nodes {
                            id 
                        }
                    }
                }
                """,
            variables = new
            {
                culture
            }
        });

        var response = await client.PostAsync("/graphql", request);

        var responseContent = await response.Content.ReadAsStringAsync();

        var snapshotName = $"ContentByTag_GetNodeId_{culture}";

        await snapshotProvider.AssertIsSnapshotEqualAsync(snapshotName, responseContent);
        Assert.True(response.IsSuccessStatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("en-us")]
    [InlineData("da")]
    public async Task ContentByTag_Can_Get_Properties_Async(string? culture)
    {
        var snapshotProvider = new SnapshotProvider($"{_contentByTagSnapshotPath}/GetProperties");
        var client = _factory.CreateClient();

        var request = JsonContent.Create(new
        {
            query = """
                query GetPropertiesContentByTag($culture: String) {
                  contentByTag(tag: "normal", first: 50, culture: $culture) {
                    nodes {
                      properties {
                        alias
                        value {
                          ...propertyValues
                        }
                        editorAlias
                      }
                      children {
                        properties {
                          alias
                          value {
                            ...propertyValues
                          }
                        }
                      }
                    }
                  }
                }
                """ + Fragments.PropertyValues,
            variables = new
            {
                culture
            }
        });

        var response = await client.PostAsync("/graphql", request);

        var responseContent = await response.Content.ReadAsStringAsync();

        var snapshotName = $"ContentByTag_GetProperties_{culture}";

        await snapshotProvider.AssertIsSnapshotEqualAsync(snapshotName, responseContent);
        Assert.True(response.IsSuccessStatusCode);
    }
}
