using System;
using System.Linq;
using System.Xml.Linq;
using Akcounts.DataAccess.Repositories;
using Akcounts.Domain.Interfaces;
using Akcounts.Domain.Objects;
using NUnit.Framework;

namespace Akcounts.DataAccess.Tests
{
    [TestFixture]
    public class Repository_spec
    {
        private readonly Repository<TestEntity> _repository = new TestEntityRepository();

        private readonly TestEntity _testEntity1 = new TestEntity(1);
        private readonly TestEntity _testEntity2 = new TestEntity(2);
        private readonly TestEntity _testEntity3 = new TestEntity(3);

        [SetUp]
        public void SetUp()
        {
            _repository.Save(_testEntity1);
            _repository.Save(_testEntity2);
            _repository.Save(_testEntity3);
        }

        [Test]
        public void can_get_existing_Entity_by_id()
        {
            var entity = _repository.GetById(1);

            Assert.IsNotNull(entity);
            Assert.AreEqual(1, entity.Id);
        }

        [Test]
        public void trying_to_get_a_non_existent_entity_by_id_causes_EntityNotFoundException()
        {
            Assert.Throws<EntityNotFoundException>(() => _repository.GetById(10000));
        }

        [Test]
        public void can_save_new_Entity()
        {
            var newEntity = new TestEntity(4);
            _repository.Save(newEntity);
            var retrievedEntity = _repository.GetById(newEntity.Id);

            Assert.AreEqual(newEntity.Id, retrievedEntity.Id);
        }

        [Test]
        public void saving_a_new_entity_without_an_id_gives_it_an_id_one_higher_than_maximum()
        {
            int maxId = _repository.GetAll().Select(n => n.Id).Max();

            var entity = new TestEntity(0);
            _repository.Save(entity);

            Assert.AreEqual(maxId + 1, entity.Id);
        }

        [Test]
        public void saving_a_new_entity_to_empty_repository_gives_it_an_id_of_one()
        {
            foreach (var e in _repository.GetAll().ToList())
                _repository.Remove(e);

            var entity = new TestEntity(0);
            _repository.Save(entity);

            Assert.AreEqual(1, entity.Id);
        }

        [Test]
        public void can_save_multiple_new_entities()
        {
            var testEntity4 = new TestEntity(4);
            var testEntity5 = new TestEntity(5);

            _repository.Save(testEntity4);
            _repository.Save(testEntity5);
            var retrieved4 = _repository.GetById(testEntity4.Id);
            var retrieved5 = _repository.GetById(testEntity5.Id);

            Assert.AreEqual(testEntity4.Id, retrieved4.Id);
            Assert.AreEqual(testEntity5.Id, retrieved5.Id);
        }

        [Test]
        public void inserting_same_Entity_twice_has_no_side_affect()
        {
            var newEntity = new TestEntity(4);
            _repository.Save(newEntity);
            var entityCount = _repository.GetAll().Count();

            _repository.Save(newEntity);

            Assert.AreEqual(entityCount, _repository.GetAll().Count());
        }
        
        [Test]
        public void Can_delete_existing_Entity()
        {
            var entity = _repository.GetById(2);
            _repository.Remove(entity);

            Assert.Throws<EntityNotFoundException>(() => _repository.GetById(2));
        }

        [Test]
        public void deleting_entity_that_does_not_exist_causes_EntityNotFoundException()
        {
            var entity = _repository.GetById(5);
            _repository.Remove(entity);

            Assert.Throws<EntityNotFoundException>(() => _repository.Remove(entity));
        }

        [Test]
        public void trying_to_add_a_null_entity_results_in_an_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _repository.Save(null));
        }

        [Test]
        public void trying_to_remove_a_null_entity_results_in_an_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _repository.Remove(null));
        }

        [Test]
        public void Can_get_all()
        {
            var entities = _repository.GetAll().ToList();

            Assert.AreEqual(3, entities.Count);
            Assert.IsTrue(entities.Contains(_testEntity1));
            Assert.IsTrue(entities.Contains(_testEntity2));
            Assert.IsTrue(entities.Contains(_testEntity3));
        }

        [Test]
        public void can_output_the_repository_as_Xml()
        {
            var output = _repository.EmitXml().ToString();

            Assert.AreEqual(@"<testEntities>
  <testEntity Id=""1"" />
  <testEntity Id=""2"" />
  <testEntity Id=""3"" />
</testEntities>", output);
        }

        [Test]
        public void repository_raises_an_event_when_an_item_is_added()
        {
            int eventRaisedCount = 0;
            _repository.RepositoryModified += (sender, e) => eventRaisedCount++;

            _repository.Save(_testEntity1);

            Assert.AreEqual(1, eventRaisedCount);
        }

        [Test]
        public void repository_raises_an_event_when_an_item_is_removed()
        {
            int eventRaisedCount = 0;
            _repository.RepositoryModified += (sender, e) => eventRaisedCount++;
            
            var entity = _repository.GetById(1);
            _repository.Remove(entity);

            Assert.AreEqual(1, eventRaisedCount);
        }

        [Test]
        public void repository_raises_an_event_when_an_item_is_updated()
        {
            int eventRaisedCount = 0;
            _repository.RepositoryModified += (sender, e) => eventRaisedCount++;

            var entity = _repository.GetById(1);
            _repository.Save(entity);

            Assert.AreEqual(1, eventRaisedCount);
        }
    }

    sealed class TestEntity : EntityIdentifiedByInt<TestEntity>, IDomainObject
    {
        public TestEntity(int id)
        {
            Id = id;
        }

        public XElement EmitXml()
        {
            return new XElement("testEntity", 
                new XAttribute("Id", Id)
                );
        }
    }

    class TestEntityRepository : Repository<TestEntity>
    {
        protected override void Initialise(XElement xElement) { }

        protected override string EntityNames
        {
            get { return "testEntities"; }
        }
    }
}
