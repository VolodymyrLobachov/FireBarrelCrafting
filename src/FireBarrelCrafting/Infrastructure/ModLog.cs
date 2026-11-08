using MelonLoader;

namespace FireBarrelCrafting.Infrastructure;

/// <summary>Thin logger that adds an opt-in debug level on top of MelonLoader's logger.</summary>
/// <param name="logger">MelonLoader logger of this mod.</param>
/// <param name="isVerbose">Returns whether <see cref="Debug"/> messages should be written.</param>
/// <exception cref="ArgumentNullException">A parameter is <see langword="null"/>.</exception>
internal sealed class ModLog(MelonLogger.Instance logger, Func<bool> isVerbose)
{
    private readonly MelonLogger.Instance _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Func<bool> _isVerbose = isVerbose ?? throw new ArgumentNullException(nameof(isVerbose));

    /// <summary>Writes an informational message.</summary>
    /// <param name="message">Text to write.</param>
    public void Info(string message) => _logger.Msg(message);

    /// <summary>Writes a diagnostic message, only when verbose logging is enabled.</summary>
    /// <param name="message">Text to write.</param>
    public void Debug(string message)
    {
        if (_isVerbose())
        {
            _logger.Msg(message);
        }
    }

    /// <summary>Writes a warning.</summary>
    /// <param name="message">Text to write.</param>
    public void Warning(string message) => _logger.Warning(message);

    /// <summary>Writes an error, optionally with the exception that caused it.</summary>
    /// <param name="message">What failed.</param>
    /// <param name="exception">The cause, if any.</param>
    public void Error(string message, Exception? exception = null) =>
        _logger.Error(exception is null ? message : $"{message}: {exception}");
}
