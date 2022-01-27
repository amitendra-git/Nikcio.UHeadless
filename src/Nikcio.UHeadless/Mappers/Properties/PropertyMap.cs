﻿using Nikcio.UHeadless.Mappers.Bases;
using Nikcio.UHeadless.Models.Dtos.Propreties.PropertyValues;
using Nikcio.UHeadless.Models.Properties.BlockList;
using Nikcio.UHeadless.Models.Properties.NestedContent;
using System.Collections.Generic;
using Umbraco.Cms.Core;

namespace Nikcio.UHeadless.Mappers.Properties
{
    public class PropertyMap : BaseMap, IPropertyMap
    {
        private readonly Dictionary<string, string> editorPropertyMap = new();
        private readonly Dictionary<string, string> aliasPropertyMap = new();

        public PropertyMap()
        {
            AddPropertyMapDefaults();
        }

        private void AddPropertyMapDefaults()
        {
            if (!ContainsEditor(UHeadlessConstants.Constants.PropertyConstants.DefaultKey))
            {
                AddEditorMapping<PropertyValueBasicGraphType>(UHeadlessConstants.Constants.PropertyConstants.DefaultKey);
            }
            if (!ContainsEditor(Constants.PropertyEditors.Aliases.BlockList))
            {
                AddEditorMapping<BlockListModelGraphType<BlockListItemGraphType>>(Constants.PropertyEditors.Aliases.BlockList);
            }
            if (!ContainsEditor(Constants.PropertyEditors.Aliases.NestedContent))
            {
                AddEditorMapping<NestedContentGraphType<NestedContentElementGraphType>>(Constants.PropertyEditors.Aliases.NestedContent);
            }
        }

        /// <inheritdoc/>
        public void AddEditorMapping<TType>(string editorName) where TType : PropertyValueBaseGraphType
        {
            AddMapping<TType>(editorName, editorPropertyMap);
        }

        /// <inheritdoc/>
        public void AddAliasMapping<TType>(string contentTypeAlias, string propertyTypeAlias) where TType : PropertyValueBaseGraphType
        {
            AddMapping<TType>(contentTypeAlias + propertyTypeAlias, aliasPropertyMap);
        }

        /// <inheritdoc/>
        public bool ContainsEditor(string editorName)
        {
            return editorPropertyMap.ContainsKey(editorName.ToLowerInvariant());
        }

        /// <inheritdoc/>
        public bool ContainsAlias(string contentTypeAlias, string propertyTypeAlias)
        {
            return aliasPropertyMap.ContainsKey((contentTypeAlias + propertyTypeAlias).ToLowerInvariant());
        }

        /// <inheritdoc/>
        public string GetEditorValue(string key)
        {
            return editorPropertyMap[key.ToLowerInvariant()];
        }


        /// <inheritdoc/>
        public string GetAliasValue(string contentTypeAlias, string propertyAlias)
        {
            return aliasPropertyMap[(contentTypeAlias + propertyAlias).ToLowerInvariant()];
        }
    }
}
