using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;

namespace ResultHandler.AspNetCore.Extensions;

public static partial class AspNetCoreResultExtensions
{
    /// <summary>Awaits <paramref name="resultTask"/> and maps it like <see cref="ToResult(IOperationResult, HttpContext?)"/>, so an endpoint can return a service call directly: <c>(Guid id, OrderService orders) =&gt; orders.CancelAsync(id).ToResultAsync()</c>. Overloads for concrete result tasks exist because <see cref="Task{TResult}"/> is invariant.</summary>
    public static async Task<IResult> ToResultAsync(this Task<IOperationResult> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToResult();
    }

    /// <inheritdoc cref="ToResultAsync(Task{IOperationResult})"/>
    public static async Task<IResult> ToResultAsync(this Task<OperationResult> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToResult();
    }

    /// <summary>Awaits <paramref name="resultTask"/> and maps it like <see cref="ToResult{T}(IOperationResult{T}, HttpContext?)"/>: the raw data on success, Problem Details on failure.</summary>
    public static async Task<IResult> ToResultAsync<T>(this Task<IOperationResult<T>> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToResult();
    }

    /// <inheritdoc cref="ToResultAsync{T}(Task{IOperationResult{T}})"/>
    public static async Task<IResult> ToResultAsync<T>(this Task<OperationDataResult<T>> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToResult();
    }

    /// <summary>Awaits <paramref name="resultTask"/> and maps it like <see cref="ToEnvelopedResult(IOperationResult, HttpContext?)"/>: the whole result on success, Problem Details on failure.</summary>
    public static async Task<IResult> ToEnvelopedResultAsync(this Task<IOperationResult> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToEnvelopedResult();
    }

    /// <inheritdoc cref="ToEnvelopedResultAsync(Task{IOperationResult})"/>
    public static async Task<IResult> ToEnvelopedResultAsync(this Task<OperationResult> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToEnvelopedResult();
    }

    /// <inheritdoc cref="ToEnvelopedResultAsync(Task{IOperationResult})"/>
    public static async Task<IResult> ToEnvelopedResultAsync<T>(this Task<IOperationResult<T>> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToEnvelopedResult();
    }

    /// <inheritdoc cref="ToEnvelopedResultAsync(Task{IOperationResult})"/>
    public static async Task<IResult> ToEnvelopedResultAsync<T>(this Task<OperationDataResult<T>> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToEnvelopedResult();
    }

    /// <summary>Awaits <paramref name="resultTask"/> and maps it like <see cref="ToActionResult(IOperationResult, HttpContext?)"/>, for MVC actions: <c>public Task&lt;IActionResult&gt; Cancel(Guid id) =&gt; orders.CancelAsync(id).ToActionResultAsync();</c>.</summary>
    public static async Task<IActionResult> ToActionResultAsync(this Task<IOperationResult> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToActionResult();
    }

    /// <inheritdoc cref="ToActionResultAsync(Task{IOperationResult})"/>
    public static async Task<IActionResult> ToActionResultAsync(this Task<OperationResult> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToActionResult();
    }

    /// <summary>Awaits <paramref name="resultTask"/> and maps it like <see cref="ToActionResult{T}(IOperationResult{T}, HttpContext?)"/>: the raw data on success, Problem Details on failure.</summary>
    public static async Task<IActionResult> ToActionResultAsync<T>(this Task<IOperationResult<T>> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToActionResult();
    }

    /// <inheritdoc cref="ToActionResultAsync{T}(Task{IOperationResult{T}})"/>
    public static async Task<IActionResult> ToActionResultAsync<T>(this Task<OperationDataResult<T>> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToActionResult();
    }

    /// <summary>Awaits <paramref name="resultTask"/> and maps it like <see cref="ToEnvelopedActionResult(IOperationResult, HttpContext?)"/>: the whole result on success, Problem Details on failure.</summary>
    public static async Task<IActionResult> ToEnvelopedActionResultAsync(this Task<IOperationResult> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToEnvelopedActionResult();
    }

    /// <inheritdoc cref="ToEnvelopedActionResultAsync(Task{IOperationResult})"/>
    public static async Task<IActionResult> ToEnvelopedActionResultAsync(this Task<OperationResult> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToEnvelopedActionResult();
    }

    /// <inheritdoc cref="ToEnvelopedActionResultAsync(Task{IOperationResult})"/>
    public static async Task<IActionResult> ToEnvelopedActionResultAsync<T>(this Task<IOperationResult<T>> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToEnvelopedActionResult();
    }

    /// <inheritdoc cref="ToEnvelopedActionResultAsync(Task{IOperationResult})"/>
    public static async Task<IActionResult> ToEnvelopedActionResultAsync<T>(this Task<OperationDataResult<T>> resultTask)
    {
        ArgumentNullException.ThrowIfNull(resultTask);
        return (await resultTask.ConfigureAwait(false)).ToEnvelopedActionResult();
    }
}
