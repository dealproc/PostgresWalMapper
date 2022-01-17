namespace PostgresWalMapper.Tests {
    using System.Threading.Tasks;

    using Shouldly;

    using Xunit;

    public class ActionHandlerTests {
        [Fact(Timeout = 1000)]
        public async Task An_action_handler_can_be_executed() {
            var x = new TestClass();
            object passed = null;
            var tsc = new TaskCompletionSource();
            var h = new ActionHandler((o) => {
                passed = o;
                tsc.SetResult();
            });

            h.CanHandle(x).ShouldBeTrue();

            h.Handle(x);
            await tsc.Task;

            x.ShouldBeSameAs(passed);
        }

        [Fact(Timeout = 1000)]
        public async Task A_generic_action_handler_can_be_executed() {
            var x = new TestClass();
            object passed = null;
            var tsc = new TaskCompletionSource();
            var h = new ActionHandler<TestClass>((c) => {
                passed = c;
                tsc.SetResult();
            });
            
            h.CanHandle(x).ShouldBeTrue();
            
            h.Handle(x);
            await tsc.Task;
            
            x.ShouldBeSameAs(passed);
        }

        private class TestClass { }
    }
}