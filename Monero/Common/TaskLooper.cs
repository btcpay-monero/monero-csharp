namespace Monero.Common;

public class TaskLooper
{
    private readonly object _lock = new();
    private readonly Func<Task> _task;
    private CancellationTokenSource? _cts;
    private ulong _periodInMs;

    public TaskLooper(Func<Task> task)
    {
        _task = task ?? throw new ArgumentNullException(nameof(task));
    }

    private bool IsStarted
    {
        get
        {
            lock (_lock)
            {
                return _cts != null && !_cts.IsCancellationRequested;
            }
        }
    }

    public void Start(ulong periodInMs)
    {
        if (periodInMs <= 0)
        {
            throw new ArgumentException("Period must be greater than 0 ms", nameof(periodInMs));
        }

        lock (_lock)
        {
            if (IsStarted)
            {
                return;
            }

            _periodInMs = periodInMs;
            _cts = new CancellationTokenSource();

            _ = RunLoopAsync(false, _cts.Token);
        }
    }

    private async Task RunLoopAsync(bool targetFixedPeriod, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            ulong startTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            try
            {
                await _task();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Task execution error: {ex}");
            }

            if (token.IsCancellationRequested)
            {
                break;
            }

            ulong delay = _periodInMs;
            if (targetFixedPeriod)
            {
                ulong elapsed = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime;
                delay = elapsed >= _periodInMs ? 0 : _periodInMs - elapsed;
            }

            try
            {
                await Task.Delay((int)delay, token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
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

    public void SetPeriodInMs(ulong? periodInMs)
    {
        if (periodInMs == null)
        {
            throw new ArgumentException("Period must be greater than 0 ms");
        }

        lock (_lock)
        {
            _periodInMs = (ulong)periodInMs;
        }
    }
}