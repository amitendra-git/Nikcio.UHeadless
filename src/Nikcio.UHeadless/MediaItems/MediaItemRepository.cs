using Microsoft.Extensions.Logging;
using Nikcio.UHeadless.Media;
using Nikcio.UHeadless.Reflection;
using Umbraco.Cms.Core.PublishedCache;

namespace Nikcio.UHeadless.MediaItems;

/// <summary>
/// A repository to get media from Umbraco
/// </summary>
public interface IMediaItemRepository<out TMediaItem>
    where TMediaItem : MediaItemBase
{
    /// <summary>
    /// Gets the media model based on the media item from Umbraco
    /// </summary>
    /// <param name="command">The create command for a media item.</param>
    /// <returns></returns>
    TMediaItem? GetMediaItem(MediaItemBase.CreateCommand command);

    /// <summary>
    /// Gets the media cache.
    /// </summary>
    /// <returns></returns>
    IPublishedMediaCache? GetCache();
}

internal class MediaItemRepository<TMediaItem> : IMediaItemRepository<TMediaItem>
    where TMediaItem : MediaItemBase
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

    private readonly ILogger<MediaItemRepository<TMediaItem>> _logger;

    private readonly IDependencyReflectorFactory _dependencyReflectorFactory;

    public MediaItemRepository(IPublishedSnapshotAccessor publishedSnapshotAccessor, ILogger<MediaItemRepository<TMediaItem>> logger, IDependencyReflectorFactory dependencyReflectorFactory)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _logger = logger;
        _dependencyReflectorFactory = dependencyReflectorFactory;
    }

    public TMediaItem? GetMediaItem(MediaItemBase.CreateCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return MediaItemBase.CreateMediaItem<TMediaItem>(command, _dependencyReflectorFactory);
    }

    public IPublishedMediaCache? GetCache()
    {
        if (!_publishedSnapshotAccessor.TryGetPublishedSnapshot(out IPublishedSnapshot? publishedSnapshot) || publishedSnapshot == null)
        {
            _logger.LogError("Unable to get publishedSnapShot");
            return default;
        }

        return publishedSnapshot.Media;
    }
}
