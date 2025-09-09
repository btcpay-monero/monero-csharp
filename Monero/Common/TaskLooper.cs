namespace Monero.Common;

public class TaskLooper
{
    private readonly Action _task;
    private ulong _periodInMs;
    private CancellationTokenSource _cts;
    private Task _loopTask;
    private readonly object _lock = new();

    public TaskLooper(Action task)
    {
        _task = task ?? throw new ArgumentNullException(nameof(task));
    }

    public bool IsStarted
    {
        get
        {
            lock (_lock)
            {
                return _cts != null && !_cts.IsCancellationRequested;
            }
        }
    }

    public TaskLooper Start(ulong periodInMs, bool targetFixedPeriod = false)
    {
        if (periodInMs <= 0)
            throw new ArgumentException("Period must be greater than 0 ms", nameof(periodInMs));

        lock (_lock)
        {
            if (IsStarted) return this;

            _periodInMs = periodInMs;
            _cts = new CancellationTokenSource();

            _loopTask = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    var startTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    try
                    {
                        _task();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Task execution error: {ex}");
                    }

                    if (_cts.Token.IsCancellationRequested) break;

                    ulong delay = _periodInMs;
                    if (targetFixedPeriod)
                    {
                        var elapsed = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime;
                        delay = Math.Max(0, _periodInMs - elapsed);
                    }

                    try
                    {
                        await Task.Delay((int)delay, _cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }, _cts.Token);
        }

        return this;
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }
    }

    public void SetPeriodInMs(ulong periodInMs)
    {
        if (periodInMs <= 0)
            throw new ArgumentException("Period must be greater than 0 ms");

        lock (_lock)
        {
            _periodInMs = periodInMs;
        }
    }
}