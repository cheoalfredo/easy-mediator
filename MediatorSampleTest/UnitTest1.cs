using MediatorSample.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace MediatorSampleTest
{
    public class UnitTest1
    {
        public record EchoRequest(string _data) : IRequest<string> { }

        public class MyRequestHandler : IRequestHandler<EchoRequest, string>
        {
            public Task<string> HandleAsync(EchoRequest request, CancellationToken token)
            {
                return Task.FromResult($"hello {request._data}");
            }
        }


        [Fact]
        public async Task MediatorDoesHandlerResolutionOk()
        {
            IRequest<string> cmd = new EchoRequest("Test");
            var services = new ServiceCollection()
                .AddSingleton<IMediator, Mediator>()
                .AddTransient<IRequestHandler<EchoRequest, string>, MyRequestHandler>()
                .BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var result = await mediator.Send(cmd, CancellationToken.None);
            Assert.Equal("hello Test", result);
        }

        [Fact]
        public async Task MediatorFailsToResolveHandler()
        {
            IRequest<string> cmd = new EchoRequest("Test");
            var services = new ServiceCollection()
                .AddSingleton<IMediator, MediatorSample.Mediator.Mediator>()
                .BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await mediator.Send(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task MediatorFailsWorkBecauseOfCancellationToken()
        {
            var cancelTokenSource = new CancellationTokenSource();
            cancelTokenSource.Cancel();

            IRequest<string> cmd = new EchoRequest("Test");
            var services = new ServiceCollection()
                .AddSingleton<IMediator, MediatorSample.Mediator.Mediator>()
                .AddTransient<IRequestHandler<EchoRequest, string>, MyRequestHandler>()
                .BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();

            await Assert.ThrowsAsync<OperationCanceledException>(
                async () => await mediator.Send(cmd, cancelTokenSource.Token));
        }

        [Fact]
        public async Task MediatorThrowsOnNullRequest()
        {
            var services = new ServiceCollection()
                .AddSingleton<IMediator, MediatorSample.Mediator.Mediator>()
                .AddTransient<IRequestHandler<EchoRequest, string>, MyRequestHandler>()
                .BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();

            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await mediator.Send<string>(null!, CancellationToken.None));
        }

        [Fact]
        public async Task MediatorWorksWithDefaultCancellationToken()
        {
            IRequest<string> cmd = new EchoRequest("DefaultToken");
            var services = new ServiceCollection()
                .AddSingleton<IMediator, Mediator>()
                .AddTransient<IRequestHandler<EchoRequest, string>, MyRequestHandler>()
                .BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var result = await mediator.Send(cmd);
            Assert.Equal("hello DefaultToken", result);
        }
    }
}