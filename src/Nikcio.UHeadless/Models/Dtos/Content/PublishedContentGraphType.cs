﻿using Nikcio.UHeadless.Models.Dtos.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Nikcio.UHeadless.Models.Dtos.Content
{
    public class PublishedContentGraphType : PublishedElementGraphType, IPublishedContentGraphType
    {
        public int? TemplateId { get; set; }

        public PublishedContentGraphType Parent { get; set; }

        public PublishedItemType ItemType { get; set; }

        public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures { get; set; }

        public DateTime UpdateDate { get; set; }

        public int WriterId { get; set; }

        public DateTime CreateDate { get; set; }

        public int CreatorId { get; set; }

        public IEnumerable<PublishedContentGraphType> ChildrenForAllCultures => Mapper.Map<IEnumerable<PublishedContentGraphType>>(Content.ChildrenForAllCultures).Select(item => SetInitalValues(item, propertyFactory, Culture, Mapper) as PublishedContentGraphType);

        public string Path { get; set; }

        public int Level { get; set; }

        public int SortOrder { get; set; }

        public string UrlSegment { get; set; }

        public string Name { get; set; }

        public int Id { get; set; }

        public IEnumerable<PublishedContentGraphType> Children => Mapper.Map<IEnumerable<PublishedContentGraphType>>(Content.Children).Select(item => SetInitalValues(item, propertyFactory, Culture, Mapper) as PublishedContentGraphType);
    }
}