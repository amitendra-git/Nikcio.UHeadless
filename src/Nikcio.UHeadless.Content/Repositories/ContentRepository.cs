﻿using Nikcio.UHeadless.Content.Factories;
using Nikcio.UHeadless.Content.Models;

namespace Nikcio.UHeadless.Content.Repositories {
    /// <inheritdoc/>
    public class ContentRepository<TContent, TProperty> : IContentRepository<TContent, TProperty>
        where TContent : IContent<TProperty>
        where TProperty : IProperty {
        /// <summary>
        /// An accessor to the published shapshot
        /// </summary>
        protected readonly IPublishedSnapshotAccessor publishedSnapshotAccessor;

        /// <summary>
        /// A factory for creating content
        /// </summary>
        protected readonly IContentFactory<TContent, TProperty> contentFactory;

        /// <inheritdoc/>
        public ContentRepository(IPublishedSnapshotAccessor publishedSnapshotAccessor, IUmbracoContextFactory umbracoContextFactory, IContentFactory<TContent, TProperty> contentFactory) {
            umbracoContextFactory.EnsureUmbracoContext();
            this.publishedSnapshotAccessor = publishedSnapshotAccessor;
            this.contentFactory = contentFactory;
        }

        /// <inheritdoc/>
        public virtual TContent? GetContent(Func<IPublishedContentCache?, IPublishedContent?> fetch, string? culture) {
            if (publishedSnapshotAccessor.TryGetPublishedSnapshot(out var publishedSnapshot)) {
                var content = fetch(publishedSnapshot?.Content);
                if (content != null && culture == null || content != null) {
                    return GetConvertedContent(content, culture);
                }
            }

            return default;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<TContent?> GetContentList(Func<IPublishedContentCache?, IEnumerable<IPublishedContent>?> fetch, string? culture) {
            if (publishedSnapshotAccessor.TryGetPublishedSnapshot(out var publishedSnapshot)) {
                var contentList = fetch(publishedSnapshot?.Content);
                if (contentList != null) {
                    return contentList.Select(content => GetConvertedContent(content, culture));
                }
            }

            return new List<TContent>();
        }

        /// <inheritdoc/>
        public virtual TContent? GetConvertedContent(IPublishedContent? content, string? culture) {
            return contentFactory.CreateContent(content, culture);
        }
    }
}