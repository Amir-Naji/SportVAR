namespace SportVAR;

public interface IDispatcher
{
    void Invoke(Action action);
    Task InvokeAsync(Action action);
}
