using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.UI.Input;

sealed unsafe record class RawInputDevice(
    HANDLE Handle,
    RID_DEVICE_INFO_TYPE Type,
    string DeviceName
) : IDisposable
{
    private HANDLE _fileHandle;
    private HANDLE _eventHandle;

    private void GetFileOfDevice()
    {
        const uint GENERIC_READ = 0x80000000;
        _fileHandle = Native.CreateFile(
            DeviceName,
            GENERIC_READ,
            FILE_SHARE_MODE.FILE_SHARE_NONE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_DEVICE | FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_OVERLAPPED,
            HANDLE.Null
        );

        _eventHandle = Native.CreateEvent(
            null,
            true,
            false,
            default(PCWSTR)
        );
    }

    public bool TryRead(Span<byte> buffer, out int length)
    {
        const int ERROR_IO_PENDING = 997;

        if (_fileHandle == HANDLE.Null)
        {
            GetFileOfDevice();
        }

        var inLength = 0;
        var overlapped = default(NativeOverlapped);
        overlapped.EventHandle = _eventHandle;
        if (!Native.ReadFile(_fileHandle, buffer, (uint*)&inLength, &overlapped))
        {
            var lastError = Marshal.GetLastWin32Error();
            if (lastError is ERROR_IO_PENDING)
            {
                var waitResult = Native.WaitForSingleObject((HANDLE)overlapped.EventHandle, uint.MaxValue);
                if (waitResult is WAIT_EVENT.WAIT_OBJECT_0)
                {
                    if (Native.GetOverlappedResult(_fileHandle, &overlapped, (uint*)&inLength, true))
                    {
                        length = inLength;
                        return true;
                    }

                    Console.WriteLine($"Uncatched overlappedResult error {Marshal.GetLastWin32Error()}");
                }
                else
                {
                    Console.WriteLine($"Uncatched wait result {waitResult}");
                }
            }
            else
            {
                Console.WriteLine($"Uncatched last error: {lastError}");
            }
        }
        else
        {
            length = inLength;
            return true;
        }

        length = 0;
        return false;
    }

    public void Dispose()
    {
        Native.CloseHandle(_fileHandle);
        Native.CloseHandle(_eventHandle);
    }
}
