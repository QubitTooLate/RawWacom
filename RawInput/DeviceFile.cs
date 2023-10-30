using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace RawInput;

internal class DeviceFile : IDisposable
{
    protected readonly HANDLE _fileHandle;
    protected readonly HANDLE _eventHandle;

    protected DeviceFile(HANDLE fileHandle, HANDLE eventHandle) 
    {
        _fileHandle = fileHandle;
        _eventHandle = eventHandle;
    }

    protected unsafe bool TryRead(byte* buffer, int* length)
    {
        const int ERROR_IO_PENDING = 997;

        var overlapped = default(NativeOverlapped);
        overlapped.EventHandle = _eventHandle;
        if (!Native.ReadFile(
            _fileHandle, 
            buffer, 
            (uint)*length, 
            (uint*)length, 
            &overlapped
        ))
        {
            var lastError = Marshal.GetLastWin32Error();
            if (lastError is ERROR_IO_PENDING)
            {
                var waitResult = Native.WaitForSingleObject((HANDLE)overlapped.EventHandle, uint.MaxValue);
                if (waitResult is WAIT_EVENT.WAIT_OBJECT_0)
                {
                    if (Native.GetOverlappedResult(_fileHandle, &overlapped, (uint*)length, true))
                    {
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
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        Native.CloseHandle(_fileHandle);
        Native.CloseHandle(_eventHandle);
    }
}
