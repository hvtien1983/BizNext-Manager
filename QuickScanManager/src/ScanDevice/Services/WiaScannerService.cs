using System.Runtime.InteropServices;
using System.Globalization;
using ScanDevice.Models;

namespace ScanDevice.Services;

public sealed class WiaScannerService : IScannerService
{
    private const int ScannerDeviceType = 1;
    private const int DocumentHandlingCapabilitiesPropertyId = 3086;
    private const int DocumentHandlingStatusPropertyId = 3087;
    private const int DocumentHandlingSelectPropertyId = 3088;
    private const int FeederCapability = 1;
    private const int FeederReady = 1;
    private const int FeederSelection = 1;
    private const int PaperEmptyError = unchecked((int)0x80210003);
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

    public Task<bool> SupportsAutomaticDocumentFeederAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        EnsureWindows();

        return RunOnStaThread(() =>
        {
            var manager = CreateDeviceManager();
            object? device = null;
            try
            {
                device = ConnectToScanner(deviceId, manager);
                var capabilities = Convert.ToInt32(
                    GetDevicePropertyValue(device, DocumentHandlingCapabilitiesPropertyId),
                    CultureInfo.InvariantCulture);
                return (capabilities & FeederCapability) != 0;
            }
            finally
            {
                Release(device);
                Release(manager);
            }
        }, cancellationToken);
    }

    public Task<int> ScanBatchToImagesAsync(
        string deviceId,
        string outputDirectory,
        ManualResetEventSlim continueSignal,
        Action<string> onImageScanned,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(continueSignal);
        ArgumentNullException.ThrowIfNull(onImageScanned);
        EnsureWindows();

        return RunOnStaThread(
            () => ScanBatchToImages(
                deviceId,
                outputDirectory,
                continueSignal,
                onImageScanned,
                cancellationToken),
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
        object? device = null;
        object? items = null;
        object? item = null;
        object? image = null;

        try
        {
            device = ConnectToScanner(deviceId, manager);
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
            Release(manager);
        }
    }

    private static int ScanBatchToImages(
        string deviceId,
        string outputDirectory,
        ManualResetEventSlim continueSignal,
        Action<string> onImageScanned,
        CancellationToken cancellationToken)
    {
        var manager = CreateDeviceManager();
        object? device = null;
        object? items = null;
        object? item = null;
        object? selectionProperty = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            device = ConnectToScanner(deviceId, manager);
            var capabilities = Convert.ToInt32(
                GetDevicePropertyValue(device, DocumentHandlingCapabilitiesPropertyId),
                CultureInfo.InvariantCulture);
            if ((capabilities & FeederCapability) == 0)
            {
                throw new InvalidOperationException(
                    "Máy scan hiện tại không hỗ trợ khay nạp tài liệu tự động (ADF).");
            }

            selectionProperty = GetDeviceProperty(device, DocumentHandlingSelectPropertyId);
            dynamic selectedProperty = selectionProperty;
            selectedProperty.Value = FeederSelection;

            dynamic connectedDevice = device;
            items = connectedDevice.Items;
            dynamic deviceItems = items;
            item = deviceItems.Item(1);
            dynamic scanItem = item;
            Directory.CreateDirectory(outputDirectory);

            var scannedCount = 0;
            while (true)
            {
                continueSignal.Wait(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                var status = Convert.ToInt32(
                    GetDevicePropertyValue(device, DocumentHandlingStatusPropertyId),
                    CultureInfo.InvariantCulture);
                if ((status & FeederReady) == 0)
                {
                    return scannedCount;
                }

                object? image = null;
                try
                {
                    image = scanItem.Transfer(BitmapFormatId);
                }
                catch (COMException exception) when (exception.ErrorCode == PaperEmptyError)
                {
                    return scannedCount;
                }

                var outputPath = Path.Combine(outputDirectory, $"{Guid.NewGuid():N}.bmp");
                try
                {
                    dynamic transferredImage = image;
                    transferredImage.SaveFile(outputPath);
                    onImageScanned(outputPath);
                    scannedCount++;
                }
                catch
                {
                    if (File.Exists(outputPath))
                    {
                        File.Delete(outputPath);
                    }

                    throw;
                }
                finally
                {
                    Release(image);
                }
            }
        }
        finally
        {
            Release(selectionProperty);
            Release(item);
            Release(items);
            Release(device);
            Release(manager);
        }
    }

    private static object ConnectToScanner(string deviceId, object manager)
    {
        object? deviceInfos = null;
        object? deviceInfo = null;
        object? device = null;
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
            return device;
        }
        catch
        {
            Release(device);
            throw;
        }
        finally
        {
            Release(deviceInfo);
            Release(deviceInfos);
        }
    }

    private static object GetDeviceProperty(object device, int propertyId)
    {
        object? properties = null;
        try
        {
            dynamic connectedDevice = device;
            properties = connectedDevice.Properties;
            dynamic deviceProperties = properties;
            return deviceProperties.Item(propertyId.ToString(CultureInfo.InvariantCulture));
        }
        finally
        {
            Release(properties);
        }
    }

    private static object? GetDevicePropertyValue(object device, int propertyId)
    {
        object? properties = null;
        object? property = null;
        try
        {
            dynamic connectedDevice = device;
            properties = connectedDevice.Properties;
            dynamic deviceProperties = properties;
            property = deviceProperties.Item(propertyId.ToString(CultureInfo.InvariantCulture));
            dynamic scannerProperty = property;
            return scannerProperty.Value;
        }
        finally
        {
            Release(property);
            Release(properties);
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
