using System;
using NUnit.Framework;
using Akcounts.UI.Util;

namespace Akcounts.UI.Tests
{
    [TestFixture]
    public class ViewModelBase_spec
    {
        [Test]
        public void PropertyChanged_is_raised_correctly_when_a_property_is_changed()
        {
            var target = new TestViewModel();
            var eventWasRaised = false;
            target.PropertyChanged += (sender, e) => eventWasRaised = e.PropertyName == "GoodProperty";
            
            target.GoodProperty = "Some new value...";
            
            Assert.IsTrue(eventWasRaised);
        }

        [Test]
        public void exception_is_thrown_on_invalid_Property_name()
        {
            var target = new TestViewModel();

            Assert.Throws<Exception> (() => target.BadProperty = "Some new value...");
        }

        [Test]
        public void test_dispose_does_not_do_anything()
        {
            var target = new TestViewModel();
            target.Dispose();
        }

        private class TestViewModel : ViewModelBase
        {
            // ReSharper disable UnusedMember.Local
            // ReSharper disable ValueParameterNotUsed
            public string GoodProperty
            {
                get { return null; }
                set { base.OnPropertyChanged("GoodProperty"); }
            }

            public string BadProperty
            {
                get { return null; }
                set { base.OnPropertyChanged("ThisIsAnInvalidPropertyName!"); }
            }
            // ReSharper restore ValueParameterNotUsed
            // ReSharper restore UnusedMember.Local
        }
    }
}

