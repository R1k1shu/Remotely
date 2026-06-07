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

        _watcherTask = Task.Run(
            async () => await WatchClipboard(_dispatcher.ApplicationExitingToken),
            _dispatcher.ApplicationExitingToken);
    }

    public async Task SetText(string clipboardText)
    {
        try
        {
            var clipboard = _dispatcher.Clipboard;
            if (clipboard is null)
            {
                _logger.LogWarning("Clipboard is null.");
                return;
            }
        
            await _clipboardLock.WaitAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(clipboardText))
                    await clipboard.ClearAsync();
                else
                    await clipboard.SetTextAsync(clipboardText);
            }
            finally
            {
                _clipboardLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while setting text.");
        }
    }

    private bool _windowCreated = false;
    private readonly SemaphoreSlim _clipboardLock = new(1, 1);
    
    private async Task WatchClipboard(CancellationToken cancelToken)
    {
        while (
            !cancelToken.IsCancellationRequested &&
            !Environment.HasShutdownStarted)
        {
            try
            {
                var clipboard = _dispatcher.Clipboard;
                if (clipboard is null)
                {
                    if (!_windowCreated)
                    {
                        _windowCreated = true;
                        _logger.LogInformation("ClipboardService: waiting for UI thread...");

                        // Ждём пока Avalonia запустится
                        var waited = 0;
                        while (_dispatcher.CurrentApp is null && waited < 10000)
                        {
                            await Task.Delay(200, cancelToken);
                            waited += 200;
                        }

                        _logger.LogInformation("ClipboardService: creating window, app={app}", _dispatcher.CurrentApp is null ? "null" : "ok");
                        try
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                _logger.LogInformation("ClipboardService: inside UIThread");
                                var window = new Window
                                {
                                    Width = 1,
                                    Height = 1,
                                    ShowInTaskbar = false,
                                    SystemDecorations = Avalonia.Controls.SystemDecorations.None,
                                    Opacity = 0
                                };
                                _dispatcher.ShowMainWindow(window);
                                _logger.LogInformation("ClipboardService: window created, clipboard={clip}", _dispatcher.Clipboard is null ? "null" : "ok");
                             });
                         }
                         catch (Exception winEx)
                         {
                            _logger.LogError(winEx, "ClipboardService: failed to create window");
                         }
                        }
                    else
                    {
                        _logger.LogWarning("Clipboard is null.");
                    }
                    await Task.Delay(500, cancelToken);
                    continue;
                }

               await _clipboardLock.WaitAsync(cancelToken);
               try
               {
                    var currentText = await clipboard.GetTextAsync();
                    if (!string.IsNullOrEmpty(currentText) && currentText != ClipboardText)
                    {
                        ClipboardText = currentText;
                        ClipboardTextChanged?.Invoke(this, ClipboardText);
                    }
               }
               finally
               {
                    _clipboardLock.Release();
               }

                await Task.Delay(500, cancelToken);
            }
                catch (TaskCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while watching clipboard.");
                    await Task.Delay(500, cancelToken);
                }
        }
    }
}
