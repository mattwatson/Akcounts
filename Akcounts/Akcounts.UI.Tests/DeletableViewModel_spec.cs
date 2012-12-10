using Akcounts.UI.Util;
using NUnit.Framework;

namespace Akcounts.UI.Tests
{
    [TestFixture]
    public class DeletableViewModel_spec
    {
        [Test]
        public void can_execute_remove_command_that_fires_event_which_can_be_subscribed_to_by_another_object()
        {
            var vm = new DeletableTest();
            var eventTriggered = false;
            vm.RequestDelete += (o, a) => eventTriggered = true;
            
            vm.DeleteCommand.Execute(null);

            Assert.IsTrue(eventTriggered);
        }
    }

    class DeletableTest : DeletableViewModel { }
}


      