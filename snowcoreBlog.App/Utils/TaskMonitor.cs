using System.Text;

namespace snowcoreBlog.App.Utils;

/// <summary>
/// Watches a task returning a result and calls callbacks when the task completes.
/// </summary>
public interface ITaskMonitor<out TResult> : ITaskMonitor
{
    /// <summary>
    /// Gets the result of the task. Returns the "default result" value specified in the constructor if the task has not yet completed successfully.
    /// </summary>
    TResult Result { get; }
}

/// <summary>
/// Watches a task and raises property-changed notifications when the task completes.
/// </summary>
public interface ITaskMonitor
{
    /// <summary>
    /// Gets the task being watched. This property never changes and is never <c>null</c>.
    /// </summary>
    Task Task { get; }

    /// <summary>
    /// Gets a task that completes successfully when <see cref="Task"/> completes (successfully, faulted, or canceled). This property never changes and is never <c>null</c>.
    /// </summary>
    Task TaskCompleted { get; }

    /// <summary>
    /// Gets the current task status.
    /// </summary>
    TaskStatus Status { get; }

    /// <summary>
    /// Gets whether the task has started.
    /// </summary>
    bool IsNotStarted { get; }

    /// <summary>
    /// Gets whether the task has completed.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets whether the task is busy (not completed).
    /// </summary>
    bool IsNotCompleted { get; }

    /// <summary>
    /// Gets whether the task has completed successfully.
    /// </summary>
    bool IsSuccessfullyCompleted { get; }

    /// <summary>
    /// Gets whether the task has been canceled.
    /// </summary>
    bool IsCanceled { get; }

    /// <summary>
    /// Gets whether the task has faulted.
    /// </summary>
    bool IsFaulted { get; }

    /// <summary>
    /// The name given to the task by the user.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Whether or not the user gave a name to this monitor.
    /// </summary>
    bool HasName { get; }

    /// <summary>
    /// Gets the wrapped faulting exception for the task. Returns <c>null</c> if the task is not faulted.
    /// </summary>
    AggregateException Exception { get; }

    /// <summary>
    /// Gets the original faulting exception for the task. Returns <c>null</c> if the task is not faulted.
    /// </summary>
    Exception InnerException { get; }

    /// <summary>
    /// Gets the error message for the original faulting exception for the task. Returns <c>null</c> if the task is not faulted.
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    /// In case of a cold task, we start it manually.
    /// </summary>
    void Start();

    /// <summary>
    /// Cancels the callbacks: the task will execute till the end, but none of the callbacks will be invoked.
    /// </summary>
    void CancelCallbacks();
}

/// <summary>
/// Watches a task and calls callbacks when the task completes.
/// </summary>
public abstract partial class TaskMonitorBase : ITaskMonitor
{
    /// <summary>
    /// Instance logger.
    /// </summary>
    protected readonly Action<ITaskMonitor, string, Exception> ErrorHandler;

    /// <summary>
    /// If true we monitor the task in the constructor to start it.
    /// </summary>
    private readonly bool _isHot;

    /// <summary>
    /// If true wrap the task in a new Task.
    /// </summary>
    private readonly bool _inNewTask;

    private readonly bool? _considerCanceledAsFaulted;

    /// <summary>
    /// Callback called when the task has been canceled.
    /// </summary>
    private readonly Action<ITaskMonitor> _whenCanceled;

    /// <summary>
    /// Callback called when the task is faulted.
    /// </summary>
    private readonly Action<ITaskMonitor> _whenFaulted;

    /// <summary>
    /// Callback called when the task completed (successfully or not).
    /// </summary>
    private readonly Action<ITaskMonitor> _whenCompleted;

    private bool _areCallbacksCancelled;

    /// <summary>
    /// Initializes a task notifier watching the specified task.
    /// </summary>
    protected TaskMonitorBase(
        Task task = null,
        Func<Task> taskSource = null,
        Action<ITaskMonitor> whenCanceled = null,
        Action<ITaskMonitor> whenFaulted = null,
        Action<ITaskMonitor> whenCompleted = null,
        string name = null,
        bool inNewTask = false,
        bool isHot = false,
        bool? considerCanceledAsFaulted = null,
        Action<ITaskMonitor, string, Exception> errorHandler = null)
    {
        if (task == null && taskSource == null)
        {
            throw new ArgumentException("You have to set either the task or the taskSource parameter");
        }

        if (task != null && taskSource != null)
        {
            throw new ArgumentException("You cannot set both the task and taskSource parameters at the same time");
        }

        Task = task;
        TaskSource = taskSource;
        _whenCanceled = whenCanceled;
        _whenFaulted = whenFaulted;
        _whenCompleted = whenCompleted;
        _inNewTask = inNewTask;
        _isHot = isHot;
        _considerCanceledAsFaulted = considerCanceledAsFaulted;
        Name = name;
        ErrorHandler = errorHandler;
    }

    /// <inheritdoc />
    public Task Task { get; protected set; }

    /// <inheritdoc />
    public Task TaskCompleted { get; protected set; }

    /// <inheritdoc />
    public TaskStatus Status => Task?.Status ?? TaskStatus.Created;

    /// <inheritdoc />
    public bool IsCompleted => Task?.IsCompleted ?? false;

    /// <inheritdoc />
    public bool IsNotStarted => Status == TaskStatus.Created;

    /// <inheritdoc />
    public bool IsNotCompleted => !IsCompleted;

    /// <inheritdoc />
    public bool IsSuccessfullyCompleted => Status == TaskStatus.RanToCompletion;

    /// <inheritdoc />
    public bool IsCanceled => Task?.IsCanceled ?? false;

    /// <inheritdoc />
    public bool IsFaulted => (Task?.IsFaulted ?? false) || (ConsiderCanceledAsFaulted && IsCanceled);

    /// <inheritdoc />
    public string Name { get; }

    public bool ConsiderCanceledAsFaulted =>
        _considerCanceledAsFaulted.HasValue && _considerCanceledAsFaulted.Value;

    public bool HasName => Name != null;

    /// <inheritdoc />
    public AggregateException Exception => Task?.Exception;

    /// <inheritdoc />
    public Exception InnerException => Exception?.InnerException;

    /// <inheritdoc />
    public string ErrorMessage => InnerException?.Message;

    protected virtual bool HasCallbacks => _whenCanceled != null || _whenCompleted != null || _whenFaulted != null;

    protected Func<Task> TaskSource { get; }

    /// <inheritdoc />
    public void Start()
    {
        if (!_isHot)
        {
            if (TaskSource != null)
            {
                TaskCompleted = MonitorTaskAsync();
            }
        }
    }

    public void CancelCallbacks()
    {
        _areCallbacksCancelled = true;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        if (HasName)
        {
            builder.Append(Name);
            builder.Append(" => ");
        }

        builder.Append("Status: ");
        if (IsNotStarted)
        {
            builder.Append(nameof(IsNotStarted));
        }
        else if (IsNotCompleted)
        {
            builder.Append(nameof(IsNotCompleted));
        }
        else if (IsSuccessfullyCompleted)
        {
            builder.Append(nameof(IsSuccessfullyCompleted));
        }
        else if (IsCanceled)
        {
            builder.Append(nameof(IsCanceled));
        }
        else
        {
            builder.Append(nameof(IsFaulted));
        }

        return builder.ToString();
    }

    protected async Task MonitorTaskAsync()
    {
        try
        {
            if (TaskSource != null)
            {
                Task = TaskSource();
            }

            if (_inNewTask)
            {
                await Task.Run(async () => await Task);
            }
            else
            {
                await Task;
            }
        }
        catch (TaskCanceledException canceledException)
        {
            Task ??= Task.FromException(canceledException);

            ErrorHandler?.Invoke(this, "Task has been canceled", canceledException);
        }
        catch (Exception exception)
        {
            Task ??= Task.FromException(exception);

            ErrorHandler?.Invoke(this, "Error in wrapped task", exception);
        }
        finally
        {
            OnTaskCompleted();
        }
    }

    protected abstract void OnSuccessfullyCompleted();

    private void OnTaskCompleted()
    {
        if (_areCallbacksCancelled || !HasCallbacks)
        {
            return;
        }

        try
        {
            _whenCompleted?.Invoke(this);
        }
        catch (Exception exception)
        {
            ErrorHandler?.Invoke(this, "Error while calling the WhenCompleted callback", exception);
        }

        if (IsCanceled && !ConsiderCanceledAsFaulted)
        {
            try
            {
                _whenCanceled?.Invoke(this);
            }
            catch (Exception exception)
            {
                ErrorHandler?.Invoke(this, "Error while calling the WhenCanceled callback", exception);
            }
        }
        else if (IsFaulted)
        {
            try
            {
                _whenFaulted?.Invoke(this);
            }
            catch (Exception exception)
            {
                ErrorHandler?.Invoke(this, "Error while calling the WhenFaulted callback", exception);
            }
        }
        else
        {
            OnSuccessfullyCompleted();
        }
    }
}

public partial class TaskMonitor : TaskMonitorBase
{
    public static readonly ITaskMonitor NotStartedTask = new NotStartedTask();

    /// <summary>
    /// Callback called when the task successfully completed.
    /// </summary>
    private readonly Action<ITaskMonitor> _whenSuccessfullyCompleted;

    /// <inheritdoc />
    internal TaskMonitor(
        Task task = null,
        Func<Task> taskSource = null,
        Action<ITaskMonitor> whenCanceled = null,
        Action<ITaskMonitor> whenFaulted = null,
        Action<ITaskMonitor> whenCompleted = null,
        Action<ITaskMonitor> whenSuccessfullyCompleted = null,
        string name = null,
        bool inNewTask = false,
        bool isHot = false,
        bool? considerCanceledAsFaulted = null,
        Action<ITaskMonitor, string, Exception> errorHandler = null)
        : base(task, taskSource, whenCanceled, whenFaulted, whenCompleted, name, inNewTask, isHot, considerCanceledAsFaulted, errorHandler)
    {
        _whenSuccessfullyCompleted = whenSuccessfullyCompleted;

        if (isHot)
        {
            TaskCompleted = MonitorTaskAsync();
        }
    }

    protected override bool HasCallbacks => base.HasCallbacks || _whenSuccessfullyCompleted != null;

    /// <summary>
    /// Creates a new task monitor watching the specified task.
    /// </summary>
    public static TaskMonitor Create(
        Task task,
        Action<ITaskMonitor> whenCompleted = null,
        Action<ITaskMonitor> whenFaulted = null,
        Action<ITaskMonitor> whenSuccessfullyCompleted = null,
        bool isHot = true,
        string name = null,
        bool inNewTask = false)
    {
        return new (
            task,
            whenCompleted: whenCompleted,
            whenFaulted: whenFaulted,
            whenSuccessfullyCompleted: whenSuccessfullyCompleted,
            name: name,
            isHot: isHot,
            inNewTask: inNewTask);
    }

    /// <summary>
    /// Creates a new task monitor watching the specified task.
    /// </summary>
    public static TaskMonitor Create(
        Func<Task> taskSource,
        Action<ITaskMonitor> whenCompleted = null,
        Action<ITaskMonitor> whenFaulted = null,
        Action<ITaskMonitor> whenSuccessfullyCompleted = null,
        bool isHot = true,
        string name = null,
        bool inNewTask = false)
    {
        return new (
            taskSource: taskSource,
            whenCompleted: whenCompleted,
            whenFaulted: whenFaulted,
            whenSuccessfullyCompleted: whenSuccessfullyCompleted,
            name: name,
            isHot: isHot,
            inNewTask: inNewTask);
    }

    protected override void OnSuccessfullyCompleted()
    {
        try
        {
            _whenSuccessfullyCompleted?.Invoke(this);
        }
        catch (Exception exception)
        {
            ErrorHandler?.Invoke(this, "Error while calling the WhenSuccessfullyCompleted callback", exception);
        }
    }
}

public class NotStartedTask : ITaskMonitor
{
    public Task Task { get; }

    public Task TaskCompleted { get; }

    public TaskStatus Status => TaskStatus.Created;

    public bool IsNotStarted => true;

    public bool IsCompleted { get; }

    public bool IsNotCompleted => true;

    public bool IsSuccessfullyCompleted { get; }

    public bool IsCanceled { get; }

    public bool IsFaulted { get; }

    public string Name { get; }

    public bool HasName { get; }

    public AggregateException Exception { get; }

    public Exception InnerException { get; }

    public string ErrorMessage { get; }

    public void Start()
    {
        throw new NotSupportedException();
    }

    public void CancelCallbacks()
    {
        throw new NotSupportedException();
    }
}