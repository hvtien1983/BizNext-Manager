using System.Runtime.InteropServices;
using ScanDevice.Models;

namespace ScanDevice.Services;

public sealed class WiaScannerService : IScannerService
{
    private const int ScannerDeviceType = 1;
    private const string BitmapFormatId = "{B96B3CAF-0728-11D3-9D7B-0000F81EF32E}";

    public Task<IReadOnlyList<ScannerDevice>> GetAvailableScannersAsync(
        CancellationToken cancellationToken = default)
    {
        EnsureWindows();
        return RunOnStaThread(EnumerateScanners, cancellationToken);
    }

    public Task<string> ScanToImageAsync(
        string deviceId,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        EnsureWindows();

        return RunOnStaThread(
            () => ScanToImage(deviceId, outputDirectory),
            cancellationToken);
    }

    private static IReadOnlyList<ScannerDevice> EnumerateScanners()
    {
        var manager = CreateDeviceManager();
        object? deviceInfos = null;
        try
        {
            dynamic wiaManager = manager;
            deviceInfos = wiaManager.DeviceInfos;
            dynamic infos = deviceInfos;
            var scanners = new List<ScannerDevice>();
            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var index = 1; index <= (int)infos.Count; index++)
            {
                object? deviceInfo = null;
                try
                {
                    deviceInfo = infos.Item(index);
                    dynamic info = deviceInfo;
                    if ((int)info.Type != ScannerDeviceType)
                    {
                        continue;
                    }

                    var deviceId = (string)info.DeviceID;
                    if (!seenIds.Add(deviceId))
                    {
                        continue;
                    }

                    scanners.Add(new ScannerDevice(deviceId, GetDeviceName(info, deviceId)));
                }
                finally
                {
                    Release(deviceInfo);
                }
            }

            return scanners;
        }
        finally
        {
            Release(deviceInfos);
            Release(manager);
        }
    }

    private static string GetDeviceName(dynamic deviceInfo, string deviceId)
    {
        object? properties = null;
        object? nameProperty = null;
        try
        {
            properties = deviceInfo.Properties;
            dynamic deviceProperties = properties;
            nameProperty = deviceProperties.Item("Name");
            dynamic property = nameProperty;
            var name = (string?)property.Value;
            return string.IsNullOrWhiteSpace(name) ? GetFallbackName(deviceId) : name.Trim();
        }
        catch (COMException)
        {
            return GetFallbackName(deviceId);
        }
        finally
        {
            Release(nameProperty);
            Release(properties);
        }
    }

    private static string GetFallbackName(string deviceId)
    {
        var suffix = deviceId.Length > 8 ? deviceId[^8..] : deviceId;
        return $"WIA Scanner ({suffix})";
    }

    private static string ScanToImage(string deviceId, string outputDirectory)
    {
        var manager = CreateDeviceManager();
        object? deviceInfos = null;
        object? deviceInfo = null;
        object? device = null;
        object? items = null;
        object? item = null;
        object? image = null;

        try
        {
            dynamic wiaManager = manager;
            deviceInfos = wiaManager.DeviceInfos;
            dynamic infos = deviceInfos;

            for (var index = 1; index <= (int)infos.Count; index++)
            {
                object? candidate = null;
                try
                {
                    candidate = infos.Item(index);
                    dynamic candidateInfo = candidate;
                    if (string.Equals(
                            (string)candidateInfo.DeviceID,
                            deviceId,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        deviceInfo = candidate;
                        candidate = null;
                        break;
                    }
                }
                finally
                {
                    Release(candidate);
                }
            }

            if (deviceInfo is null)
            {
                throw new InvalidOperationException($"Scanner '{deviceId}' is no longer available.");
            }

            dynamic selectedInfo = deviceInfo;
            device = selectedInfo.Connect();
            dynamic connectedDevice = device;
            items = connectedDevice.Items;
            dynamic deviceItems = items;
            item = deviceItems.Item(1);
            dynamic scanItem = item;
            image = scanItem.Transfer(BitmapFormatId);

            Directory.CreateDirectory(outputDirectory);
            var outputPath = Path.Combine(outputDirectory, $"{Guid.NewGuid():N}.bmp");
            dynamic transferredImage = image;
            transferredImage.SaveFile(outputPath);
            return outputPath;
        }
        finally
        {
            Release(image);
            Release(item);
            Release(items);
            Release(device);
            Release(deviceInfo);
            Release(deviceInfos);
            Release(manager);
        }
    }

    private static object CreateDeviceManager()
    {
        var managerType = Type.GetTypeFromProgID("WIA.DeviceManager");
        if (managerType is null)
        {
            throw new InvalidOperationException(
                "Windows Image Acquisition (WIA) is not available on this computer.");
        }

        return Activator.CreateInstance(managerType)
               ?? throw new InvalidOperationException("Could not initialize the WIA device manager.");
    }

    private static Task<T> RunOnStaThread<T>(
        Func<T> operation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var completion = new TaskCompletionSource<T>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                completion.TrySetResult(operation());
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                completion.TrySetCanceled(cancellationToken);
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }
        })
        {
            IsBackground = true,
            Name = "QuickScanManager WIA"
        };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task;
    }

    private static void EnsureWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("WIA scanning is only supported on Windows.");
        }
    }

    private static void Release(object? comObject)
    {
        if (comObject is not null && Marshal.IsComObject(comObject))
        {
            Marshal.FinalReleaseComObject(comObject);
        }
    }
}
