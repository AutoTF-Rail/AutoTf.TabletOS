using System.Threading.Tasks;

namespace AutoTf.TabletOS.Avalonia.ViewModels.Base;

public class DialogViewModelBase : ViewModelBase
{
    private TaskCompletionSource<int>? _tcs;

    internal Task<int> ShowAsync()
    {
        _tcs = new TaskCompletionSource<int>();
        return _tcs.Task;
    }

    protected void Close(int result)
    {
        _tcs?.TrySetResult(result);
    }
}