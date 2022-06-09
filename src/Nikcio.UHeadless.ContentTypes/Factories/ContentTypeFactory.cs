﻿using Nikcio.UHeadless.ContentTypes.Models;

namespace Nikcio.UHeadless.ContentTypes.Factories {
    /// <inheritdoc/>
    public class ContentTypeFactory<TContentType> : IContentTypeFactory<TContentType>
        where TContentType : IContentType {
        /// <summary>
        /// A factory that can create objects with DI
        /// </summary>
        protected readonly IDependencyReflectorFactory dependencyReflectorFactory;

        /// <inheritdoc/>
        public ContentTypeFactory(IDependencyReflectorFactory dependencyReflectorFactory) {
            this.dependencyReflectorFactory = dependencyReflectorFactory;
        }

        /// <inheritdoc/>
        public virtual TContentType? CreateContentType(IPublishedContentType publishedContentType) {
            var createContentTypeCommand = new CreateContentType(publishedContentType);

            var createdContentType = dependencyReflectorFactory.GetReflectedType<IContentType>(typeof(TContentType), new object[] { createContentTypeCommand });

            if (createdContentType == null) {
                return default;
            }

            return (TContentType) createdContentType;
        }
    }
}