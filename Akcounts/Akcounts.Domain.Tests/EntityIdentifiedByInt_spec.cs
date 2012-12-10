using Akcounts.Domain.Objects;
using NUnit.Framework;

namespace Akcounts.Domain.Tests
{
    [TestFixture]
    public class EntityIdentifiedByInt_spec
    {
        [Test]
        public void can_get_and_set_id_property_of_an_Entity()
        {
            var a = new TestEntity {Id = 1000};
            Assert.AreEqual(1000, a.Id);
        }

        [Test]
        public void two_entities_are_the_same_if_their_ids_are_the_same_and_non_zero()
        {
            var a = new TestEntity {Id = 1};
            var b = new TestEntity {Id = 1};

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [Test]
        public void two_separate_entities_are_not_the_same_if_their_ids_are_both_zero()
        {
            var a = new TestEntity {Id = 0};
            var b = new TestEntity {Id = 0};
            
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);
        }

        [Test]
        public void two_references_to_the_same_entity_are_equal_even_if_their_ids_are_both_zero()
        {
            var a = new TestEntity {Id = 0};
            TestEntity b = a;

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [Test]
        public void an_entity_is_not_equal_to_null()
        {
            var a = new TestEntity {Id = 1000};
            Assert.IsFalse(a.Equals(null));
            Assert.IsFalse(a == null);
            Assert.IsTrue(a != null);
        }

        [Test]
        public void entity_hashcode_is_id_hashcode_if_id_is_non_zero()
        {
            var a = new TestEntity {Id = 10};
            Assert.AreEqual(10, a.GetHashCode());
        }

        [Test]
        public void entity_hashcode_is_remembered_if_id_is_zero()
        {
            var a = new TestEntity {Id = 0};
            int first = a.GetHashCode();
            int second = a.GetHashCode();
            Assert.IsTrue(first == second);
        }
    }

    public class TestEntity : EntityIdentifiedByInt<TestEntity> { }
}
