using Remotely.Desktop.Shared.Abstractions;
using Microsoft.Extensions.Logging;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Input.Platform;

namespace Remotely.Desktop.UI.Services;

public class ClipboardService : IClipboardService
{
    private readonly IUiDispatcher _dispatcher;
    private readonly ILogger<ClipboardService> _logger;
    private Task? _watcherTask;
    private Window? _clipboardWindow;
    private IClipboard? _windowClipboard;

    public event EventHandler<string>? ClipboardTextChanged;

    public ClipboardService(
        IUiDispatcher dispatcher,
        ILogger<ClipboardService> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    private string ClipboardText { get; set; } = string.Empty;

    public void BeginWatching()
    {
        if (_watcherTask?.Status == TaskStatus.Running)
        {
            return;
        }

        if (_dispatcher.Clipboard is null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _clipboardWindow = new Window
                {
                    Width = 1,
                    Height = 1,
                    ShowInTaskbar = false,
                    SystemDecorations = Avalonia.Controls.SystemDecorations.None,
                    Opacity = 0
                };
                _clipboardWindow.Show();
                _dispatcher.ShowMainWindow(_clipboardWindow);
                _windowClipboard = TopLevel.GetTopLevel(_clipboardWindow)?.Clipboard;
            });
        }

        _watcherTask = Task.Run(
            async () => await WatchClipboard(_dispatcher.ApplicationExitingToken),
            _dispatcher.ApplicationExitingToken);
    }

    public async Task SetText(string clipboardText)
    {
        try
        {
            var clipboard = _dispatcher.Clipboard ?? _windowClipboard;
            if (clipboard is null)
            {
                _logger.LogWarning("Clipboard is null.");
                return;
            }
            if (string.IsNullOrWhiteSpace(clipboardText))
            {
                await clipboard.ClearAsync();
            }
            else
            {
                await clipboard.SetTextAsync(clipboardText);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while setting text.");
        }
    }

    private async Task WatchClipboard(CancellationToken cancelToken)
    {
        while (
            !cancelToken.IsCancellationRequested &&
            !Environment.HasShutdownStarted)
        {
            try
            {
                var clipboard = _dispatcher.Clipboard ?? _windowClipboard;
                if (clipboard is null)
                {
                    await Task.Delay(500, cancelToken);
                    continue;
                }

                var currentText = await Dispatcher.UIThread.InvokeAsync(
                    async () => await clipboard.GetTextAsync());

                if (!string.IsNullOrEmpty(currentText) && currentText != ClipboardText)
                {
                    ClipboardText = currentText;
                    ClipboardTextChanged?.Invoke(this, ClipboardText);
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while watching clipboard.");
            }
            finally
            {
                await Task.Delay(500, cancelToken).ConfigureAwait(false);
            }
        }
    }
}
