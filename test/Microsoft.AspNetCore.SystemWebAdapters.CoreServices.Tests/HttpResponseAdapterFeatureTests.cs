// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters.CoreServices.Tests;

public class HttpResponseAdapterFeatureTests
{
    [Fact]
    public async Task CompleteAsyncSuppressesNullReferenceForAbortedRequest()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var responseBodyFeature = CreateResponseBodyFeature();
        responseBodyFeature.Setup(f => f.CompleteAsync()).ThrowsAsync(new TestNullReferenceException());

        await using var feature = new HttpResponseAdapterFeature(responseBodyFeature.Object, cts.Token);

        // Act
        await ((IHttpResponseEndFeature)feature).EndAsync();

        // Assert
        responseBodyFeature.Verify(f => f.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task CompleteAsyncPropagatesNullReferenceForActiveRequest()
    {
        // Arrange
        var responseBodyFeature = CreateResponseBodyFeature();
        responseBodyFeature.Setup(f => f.CompleteAsync()).ThrowsAsync(new TestNullReferenceException());

        await using var feature = new HttpResponseAdapterFeature(responseBodyFeature.Object, CancellationToken.None);

        // Act / Assert
        await Assert.ThrowsAnyAsync<NullReferenceException>(() => ((IHttpResponseEndFeature)feature).EndAsync());
        responseBodyFeature.Verify(f => f.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task CompleteAsyncSkipsResponseBodyCompletionWhenContentIsSuppressed()
    {
        // Arrange
        var responseBodyFeature = CreateResponseBodyFeature();
        responseBodyFeature.Setup(f => f.CompleteAsync()).ThrowsAsync(new TestNullReferenceException());

        await using var feature = new HttpResponseAdapterFeature(responseBodyFeature.Object, CancellationToken.None);
        ((IHttpResponseBufferingFeature)feature).EnableBuffering(null, null);
        feature.SuppressContent = true;

        // Act
        await ((IHttpResponseEndFeature)feature).EndAsync();

        // Assert
        responseBodyFeature.Verify(f => f.CompleteAsync(), Times.Never);
    }

    private static Mock<IHttpResponseBodyFeature> CreateResponseBodyFeature()
    {
        var responseBodyFeature = new Mock<IHttpResponseBodyFeature>(MockBehavior.Strict);
        responseBodyFeature.SetupGet(f => f.Stream).Returns(Stream.Null);
        responseBodyFeature.SetupGet(f => f.Writer).Returns(PipeWriter.Create(Stream.Null, new StreamPipeWriterOptions(leaveOpen: true)));
        responseBodyFeature.Setup(f => f.DisableBuffering());
        responseBodyFeature.Setup(f => f.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        return responseBodyFeature;
    }

    private sealed class TestNullReferenceException : NullReferenceException;
}
