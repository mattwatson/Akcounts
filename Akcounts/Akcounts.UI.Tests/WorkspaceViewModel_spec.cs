using NUnit.Framework;
using Akcounts.UI.ViewModel;

namespace Akcounts.UI.Tests
{
    [TestFixture]
    public class WorkSpaceViewModel_spec
    {
        [Test]
        public void Can_subscribe_to_close_event_which_is_triggered_by_close_command()
        {
            WorkspaceViewModel vm = new TestWorkspaceViewModel();

            bool isClosed = false;
            vm.RequestClose += (sender, args) => isClosed = true;

            vm.CloseCommand.Execute(null);

            Assert.IsTrue(isClosed);
        }
    }

    public class TestWorkspaceViewModel : WorkspaceViewModel
    { }

}
