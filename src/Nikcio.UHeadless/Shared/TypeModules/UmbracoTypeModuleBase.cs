using HotChocolate.Execution.Configuration;
using HotChocolate.Resolvers;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Descriptors.Definitions;
using Microsoft.Extensions.Logging;
using Nikcio.UHeadless.Shared.Properties;
using Nikcio.UHeadless.Shared.Reflection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Nikcio.UHeadless.Shared.TypeModules;

/// <summary>
/// Represents the base for creating type modules for the Umbraco types like ContentType and MediaType
/// </summary>
internal abstract class UmbracoTypeModuleBase<TContentType> : ITypeModule
    where TContentType : IContentTypeComposition
{
    /// <summary>
    /// Represents the property map
    /// </summary>
    private readonly IPropertyMap _propertyMap;

    /// <inheritdoc/>
    public event EventHandler<EventArgs>? TypesChanged;

    /// <inheritdoc/>
    protected UmbracoTypeModuleBase(IPropertyMap propertyMap)
    {
        _propertyMap = propertyMap;
    }

    /// <summary>
    /// Gets the content types to register in the GraphQL schema
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerable<TContentType> GetContentTypes();

    /// <summary>
    /// Call this when the types have changed
    /// </summary>
    /// <param name="eventArgs"></param>
    public void OnTypesChanged(EventArgs eventArgs)
    {
        TypesChanged?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Gets the interface type name for the interface type definition
    /// </summary>
    /// <param name="contentTypeAlias"></param>
    /// <returns></returns>
    protected static string GetInterfaceTypeName(string contentTypeAlias)
    {
        return string.Concat("I", GetObjectTypeName(contentTypeAlias));
    }

    /// <summary>
    /// Gets the object type name for the object type definition
    /// </summary>
    /// <param name="contentTypeAlias"></param>
    /// <returns></returns>
    protected static string GetObjectTypeName(string contentTypeAlias)
    {
        return contentTypeAlias.FirstCharToUpper();
    }

    /// <inheritdoc/>
    public ValueTask<IReadOnlyCollection<ITypeSystemMember>> CreateTypesAsync(IDescriptorContext context, CancellationToken cancellationToken)
    {
        GenerateTypes(out List<ITypeSystemMember> types, out List<ObjectType> objectTypes);

        AddTypedPropertyUnion<TypedProperties>(types, objectTypes, ResolveScopedValueAsObjectType<IPublishedContent>(
            objectTypes: objectTypes,
            scopedValueKey: ContextDataKeys.PublishedContent,
            getContentTypeAlias: publishedContent => publishedContent?.ContentType?.Alias));

        AddTypedPropertyUnion<TypedBlockListContentProperties>(types, objectTypes, ResolveScopedValueAsObjectType<BlockListItem>(
            objectTypes: objectTypes,
            scopedValueKey: ContextDataKeys.BlockListItemContent,
            getContentTypeAlias: blockListItem => blockListItem?.Content?.ContentType?.Alias));

        AddTypedPropertyUnion<TypedBlockListSettingsProperties>(types, objectTypes, ResolveScopedValueAsObjectType<BlockListItem>(
            objectTypes: objectTypes,
            scopedValueKey: ContextDataKeys.BlockListItemSettings,
            getContentTypeAlias: blockListItem => blockListItem?.Settings?.ContentType?.Alias));

        AddTypedPropertyUnion<TypedBlockGridContentProperties>(types, objectTypes, ResolveScopedValueAsObjectType<BlockGridItem>(
            objectTypes: objectTypes,
            scopedValueKey: ContextDataKeys.BlockGridItemContent,
            getContentTypeAlias: blockGridItem => blockGridItem?.Content?.ContentType?.Alias));

        AddTypedPropertyUnion<TypedBlockGridSettingsProperties>(types, objectTypes, ResolveScopedValueAsObjectType<BlockGridItem>(
            objectTypes: objectTypes,
            scopedValueKey: ContextDataKeys.BlockGridItemSettings,
            getContentTypeAlias: blockGridItem => blockGridItem?.Settings?.ContentType?.Alias));

        return new ValueTask<IReadOnlyCollection<ITypeSystemMember>>(types);
    }

    private void GenerateTypes(out List<ITypeSystemMember> types, out List<ObjectType> objectTypes)
    {
        types = new List<ITypeSystemMember>();
        objectTypes = new List<ObjectType>();
        AddEmptyPropertyType(objectTypes);

        var contentTypes = GetContentTypes().ToList();

        foreach (TContentType? contentType in contentTypes)
        {
            InterfaceTypeDefinition interfaceTypeDefinition = CreateInterfaceTypeDefinition(contentType);

            if (interfaceTypeDefinition.Fields.Count == 0)
            {
                continue;
            }

            types.Add(InterfaceType.CreateUnsafe(interfaceTypeDefinition));

            ObjectTypeDefinition objectTypeDefinition = CreateObjectTypeDefinition(contentType);

            if (objectTypeDefinition.Fields.Count == 0)
            {
                continue;
            }

            var objectType = ObjectType.CreateUnsafe(objectTypeDefinition);

            objectTypes.Add(objectType);

            types.Add(objectType);
        }
    }

    private static void AddTypedPropertyUnion<TTypedPropertyUnion>(List<ITypeSystemMember> types, List<ObjectType> objectTypes, ResolveAbstractType resolver)
        where TTypedPropertyUnion : class
    {
        var typedPropertiesUnion = new UnionType<TTypedPropertyUnion>(descriptor =>
        {
            descriptor.ResolveAbstractType(resolver);

            foreach (ObjectType objectType in objectTypes)
            {
                descriptor.Type(objectType);
            }
        });

        types.Add(typedPropertiesUnion);
    }

    private static ResolveAbstractType ResolveScopedValueAsObjectType<TScopedValue>(List<ObjectType> objectTypes, string scopedValueKey, Func<TScopedValue, string?> getContentTypeAlias)
    {
        return (context, result) =>
        {
            TScopedValue? scopeValue = context.GetScopedState<TScopedValue?>(scopedValueKey);

            if (scopeValue == null)
            {
                ILogger<UmbracoTypeModuleBase<TContentType>> logger = context.Service<ILogger<UmbracoTypeModuleBase<TContentType>>>();
                logger.LogWarning("Scope value is not available in scoped data. Scoped value key: {ScopedValueKey}", scopedValueKey);
                return default;
            }

            if (scopeValue == null)
            {
                return default;
            }

            string? contentTypeAlias = getContentTypeAlias.Invoke(scopeValue);

            if (string.IsNullOrWhiteSpace(contentTypeAlias))
            {
                return objectTypes[0];
            }

            return objectTypes.Find(type => type.Name == GetObjectTypeName(contentTypeAlias)) ?? objectTypes[0];
        };
    }

    /// <summary>
    /// Adds a placeholder empty type that is used when the matching content type doesn't have a type in the schema.
    /// </summary>
    /// <param name="objectTypes"></param>
    private static void AddEmptyPropertyType(List<ObjectType> objectTypes)
    {
        var emptyNamedProperty = new ObjectTypeDefinition($"EmptyPropertyType", "Represents a content type that doesn't have any properties and therefore needs a placeholder");
        emptyNamedProperty.Fields.Add(new ObjectFieldDefinition("Empty_Field", "Placeholder field. Will never hold a value.", type: TypeReference.Parse("String!"), pureResolver: _ => string.Empty));
        objectTypes.Add(ObjectType.CreateUnsafe(emptyNamedProperty));
    }

    private InterfaceTypeDefinition CreateInterfaceTypeDefinition(TContentType contentType)
    {
        var interfaceTypeDefinition = new InterfaceTypeDefinition(GetInterfaceTypeName(contentType.Alias), contentType.Description);

        foreach (IPropertyType property in contentType.CompositionPropertyTypes)
        {
            string propertyTypeName = _propertyMap.GetPropertyTypeName(contentType.Alias, property.Alias, property.PropertyEditorAlias);

            var propertyType = Type.GetType(propertyTypeName);

            if (propertyType == null)
            {
                continue;
            }

            interfaceTypeDefinition.Fields.Add(new InterfaceFieldDefinition(property.Alias, property.Description, TypeReference.Parse(propertyType.Name)));
        }

        return interfaceTypeDefinition;
    }

    private ObjectTypeDefinition CreateObjectTypeDefinition(TContentType contentType)
    {
        var typeDefinition = new ObjectTypeDefinition(GetObjectTypeName(contentType.Alias), contentType.Description);

        foreach (IPropertyType property in contentType.CompositionPropertyTypes)
        {
            string propertyTypeName = _propertyMap.GetPropertyTypeName(contentType.Alias, property.Alias, property.PropertyEditorAlias);

            var propertyType = Type.GetType(propertyTypeName);

            if (propertyType == null)
            {
                continue;
            }

            typeDefinition.Fields.Add(new ObjectFieldDefinition(property.Alias, property.Description, TypeReference.Parse(propertyType.Name), resolver: ResolvePropertyValueAsync));
        }

        foreach (string composite in contentType.CompositionAliases())
        {
            typeDefinition.Interfaces.Add(TypeReference.Parse(GetInterfaceTypeName(composite)));
        }

        typeDefinition.Interfaces.Add(TypeReference.Parse(GetInterfaceTypeName(typeDefinition.Name)));

        return typeDefinition;
    }

    private static async ValueTask<object?> ResolvePropertyValueAsync(IResolverContext context)
    {
        object? resolver;

        string pathParent = ((NamePathSegment?) context.Path.Parent)?.Name ?? "$$"; // Make sure we don't match null when looking for the correct resolver
        if (string.Equals(context.GetScopedStateOrDefault<string>(ContextDataKeys.BlockListItemContentPropertyName), pathParent, StringComparison.OrdinalIgnoreCase))
        {
            resolver = await ResolveScopedValueAsPropertyValueAsync<BlockListItem>(
                context: context,
                scopedDataKey: ContextDataKeys.BlockListItemContent,
                getProperty: (blockItem, propertyAlias) => blockItem.Content.GetProperty(propertyAlias),
                getContentTypeAlias: (blockItem) => blockItem?.Content?.ContentType?.Alias).ConfigureAwait(false);
        }
        else if (string.Equals(context.GetScopedStateOrDefault<string>(ContextDataKeys.BlockListItemSettingsPropertyName), pathParent, StringComparison.OrdinalIgnoreCase))
        {
            resolver = await ResolveScopedValueAsPropertyValueAsync<BlockListItem>(
                context: context,
                scopedDataKey: ContextDataKeys.BlockListItemSettings,
                getProperty: (blockItem, propertyAlias) => blockItem.Settings.GetProperty(propertyAlias),
                getContentTypeAlias: (blockItem) => blockItem?.Settings?.ContentType?.Alias).ConfigureAwait(false);
        }
        else if (string.Equals(context.GetScopedStateOrDefault<string>(ContextDataKeys.BlockGridItemContentPropertyName), pathParent, StringComparison.OrdinalIgnoreCase))
        {
            resolver = await ResolveScopedValueAsPropertyValueAsync<BlockGridItem>(
                context: context,
                scopedDataKey: ContextDataKeys.BlockGridItemContent,
                getProperty: (blockItem, propertyAlias) => blockItem.Content.GetProperty(propertyAlias),
                getContentTypeAlias: (blockItem) => blockItem?.Content?.ContentType?.Alias).ConfigureAwait(false);
        }
        else if (string.Equals(context.GetScopedStateOrDefault<string>(ContextDataKeys.BlockGridItemSettingsPropertyName), pathParent, StringComparison.OrdinalIgnoreCase))
        {
            resolver = await ResolveScopedValueAsPropertyValueAsync<BlockGridItem>(
                context: context,
                scopedDataKey: ContextDataKeys.BlockGridItemSettings,
                getProperty: (blockItem, propertyAlias) => blockItem.Settings.GetProperty(propertyAlias),
                getContentTypeAlias: (blockItem) => blockItem?.Settings?.ContentType?.Alias).ConfigureAwait(false);
        }
        else
        {
            resolver = await ResolveScopedValueAsPropertyValueAsync<IPublishedContent>(
                context: context,
                scopedDataKey: ContextDataKeys.PublishedContent,
                getProperty: (publishedContent, propertyAlias) => publishedContent.GetProperty(propertyAlias),
                getContentTypeAlias: publishedContent => publishedContent?.ContentType?.Alias).ConfigureAwait(false);
        }

        return resolver;
    }

    private static ValueTask<object?> ResolveScopedValueAsPropertyValueAsync<TScopedValue>(
        IResolverContext context,
        string scopedDataKey,
        Func<TScopedValue, string, IPublishedProperty?> getProperty,
        Func<TScopedValue, string?> getContentTypeAlias)
    {
        TScopedValue? scopedData = context.GetScopedStateOrDefault<TScopedValue>(scopedDataKey);

        if (scopedData == null)
        {
            return default;
        }

        IPublishedProperty? publishedProperty = getProperty(scopedData, context.Selection.ResponseName);

        if (publishedProperty == null)
        {
            ILogger<UmbracoTypeModuleBase<TContentType>> logger = context.Service<ILogger<UmbracoTypeModuleBase<TContentType>>>();
            logger.LogWarning("Property {PropertyName} not found on content type {ContentTypeAlias}.", context.Selection.ResponseName, getContentTypeAlias(scopedData));
            return default;
        }

        var command = new PropertyValue.CreateCommand()
        {
            PublishedProperty = publishedProperty,
            PublishedValueFallback = context.Service<IPublishedValueFallback>(),
            ResolverContext = context
        };

        return ValueTask.FromResult((object?) PropertyValue.CreatePropertyValue(command, context.Service<IPropertyMap>(), context.Service<IDependencyReflectorFactory>()));
    }
}
