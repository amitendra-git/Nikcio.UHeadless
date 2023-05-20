﻿using Nikcio.UHeadless.Base.Elements.Factories;
using Nikcio.UHeadless.Base.Properties.Models;
using Nikcio.UHeadless.Media.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Nikcio.UHeadless.Media.Factories;

/// <summary>
/// A factory for creating media
/// </summary>
/// <typeparam name="TMedia"></typeparam>
/// <typeparam name="TProperty"></typeparam>
public interface IMediaFactory<TMedia, TProperty> : IElementFactory<TMedia, TProperty>
    where TMedia : IMedia<TProperty>
    where TProperty : IProperty
{
    /// <summary>
    /// Creates media
    /// </summary>
    /// <param name="media"></param>
    /// <returns></returns>
    TMedia? CreateMedia(IPublishedContent media);
}