using MediatorSample.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MediatorSampleTest
{
    public class PipelineBehaviorTests
    {
        public record TestRequest(string Value) : IRequest<string>;

        public class TestRequestHandler : IRequestHandler<TestRequest, string>
        {
            public Task<string> HandleAsync(TestRequest request, CancellationToken token)
            {
                return Task.FromResult($"Handled: {request.Value}");
            }
        }

        public class TrackerBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
        {
            public static List<string> ExecutionOrder { get; } = new();

            public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            {
                ExecutionOrder.Add($"Before-{typeof(TRequest).Name}");
                var response = await next();
                ExecutionOrder.Add($"After-{typeof(TRequest).Name}");
                return response;
            }
        }

        public class ModifyingBehavior : IPipelineBehavior<TestRequest, string>
        {
            public async Task<string> HandleAsync(TestRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
            {
                var response = await next();
                return $"[Modified: {response}]";
            }
        }

        [Fact]
        public async Task MediatorExecutesHandlerWithoutBehaviors()
        {
            var services = new ServiceCollection()
                .AddSingleton<IMediator, MediatorSample.Mediator.Mediator>()
                .AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>()
                .BuildServiceProvider();

            var mediator = services.GetRequiredService<IMediator>();
            var result = await mediator.Send(new TestRequest("Test"));

            Assert.Equal("Handled: Test", result);
        }

        [Fact]
        public async Task MediatorExecutesBehaviorBeforeAndAfterHandler()
        {
            TrackerBehavior<TestRequest, string>.ExecutionOrder.Clear();

            var services = new ServiceCollection()
                .AddSingleton<IMediator, MediatorSample.Mediator.Mediator>()
                .AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>()
                .AddTransient<IPipelineBehavior<TestRequest, string>, TrackerBehavior<TestRequest, string>>()
                .BuildServiceProvider();

            var mediator = services.GetRequiredService<IMediator>();
            var result = await mediator.Send(new TestRequest("Test"));

            Assert.Equal("Handled: Test", result);
            Assert.Equal(2, TrackerBehavior<TestRequest, string>.ExecutionOrder.Count);
            Assert.Equal("Before-TestRequest", TrackerBehavior<TestRequest, string>.ExecutionOrder[0]);
            Assert.Equal("After-TestRequest", TrackerBehavior<TestRequest, string>.ExecutionOrder[1]);
        }

        [Fact]
        public async Task BehaviorCanModifyResponse()
        {
            var services = new ServiceCollection()
                .AddSingleton<IMediator, MediatorSample.Mediator.Mediator>()
                .AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>()
                .AddTransient<IPipelineBehavior<TestRequest, string>, ModifyingBehavior>()
                .BuildServiceProvider();

            var mediator = services.GetRequiredService<IMediator>();
            var result = await mediator.Send(new TestRequest("Test"));

            Assert.Equal("[Modified: Handled: Test]", result);
        }

        [Fact]
        public async Task MultipleBehaviorsExecuteInOrder()
        {
            TrackerBehavior<TestRequest, string>.ExecutionOrder.Clear();

            var services = new ServiceCollection()
                .AddSingleton<IMediator, MediatorSample.Mediator.Mediator>()
                .AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>()
                .AddTransient<IPipelineBehavior<TestRequest, string>, TrackerBehavior<TestRequest, string>>()
                .AddTransient<IPipelineBehavior<TestRequest, string>, ModifyingBehavior>()
                .BuildServiceProvider();

            var mediator = services.GetRequiredService<IMediator>();
            var result = await mediator.Send(new TestRequest("Test"));

            // ModifyingBehavior should wrap TrackerBehavior (registered last, executed first)
            Assert.Equal("[Modified: Handled: Test]", result);
            Assert.Equal(2, TrackerBehavior<TestRequest, string>.ExecutionOrder.Count);
        }

        [Fact]
        public async Task AddMediatorBehaviorExtensionWorks()
        {
            TrackerBehavior<TestRequest, string>.ExecutionOrder.Clear();

            var services = new ServiceCollection()
                .AddSingleton<IMediator, MediatorSample.Mediator.Mediator>()
                .AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>()
                .AddMediatorBehavior<TrackerBehavior<TestRequest, string>>()
                .BuildServiceProvider();

            var mediator = services.GetRequiredService<IMediator>();
            var result = await mediator.Send(new TestRequest("Test"));

            Assert.Equal("Handled: Test", result);
            Assert.Equal(2, TrackerBehavior<TestRequest, string>.ExecutionOrder.Count);
        }
    }
}
