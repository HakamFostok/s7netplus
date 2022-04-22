namespace S7.Net.Internal;

internal class TaskQueue
{
    private static readonly object Sentinel = new();

    private Task prev = Task.FromResult(Sentinel);

    public async Task<T> Enqueue<T>(Func<Task<T>> action)
    {
        TaskCompletionSource<object>? tcs = new();
        await Interlocked.Exchange(ref prev, tcs.Task).ConfigureAwait(false);

        try
        {
            return await action.Invoke().ConfigureAwait(false);
        }
        finally
        {
            tcs.SetResult(Sentinel);
        }
    }
}