using System.Windows.Threading;

namespace SportVAR;

public class WpfDispatcher(Dispatcher dispatcher) : IDispatcher
{
    public void Invoke(Action action)
    {
        dispatcher.Invoke(action);
    }

    public Task InvokeAsync(Action action)
    {
        return dispatcher.InvokeAsync(action).Task;
    }
}
