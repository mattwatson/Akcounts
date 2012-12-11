using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Akcounts.Domain.Interfaces;
using Akcounts.Domain.Objects;

namespace Akcounts.DataAccess.Repositories
{
    public abstract class Repository<TEntity>
        where TEntity : EntityIdentifiedByInt<TEntity>, IDomainObject
    {
        protected readonly IDictionary<int, TEntity> Entities = new Dictionary<int, TEntity>();

        public TEntity GetById(int id)
        {
            TEntity e;
            if (Entities.TryGetValue(id, out e)) return e;
            
            throw new EntityNotFoundException();
        }

        public IEnumerable<TEntity> GetAll()
        {
            return from accountTag in Entities.Values
                   select accountTag;
        }

        public void Save(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException();
            if (!Entities.ContainsKey(entity.Id)) Add(entity);
            OnRepositoryModified();
        }

        protected virtual void Add(TEntity e)
        {
            if (e.Id == 0)
            {
                if (Entities.Count > 0)
                {
                    int i = Entities.Keys.Max() + 1;
                    e.Id = i;
                }
                else
                {
                    e.Id = 1;
                }
            }

            Entities.Add(e.Id, e);
        }

        public void Remove(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException();
            if (Entities.ContainsKey(entity.Id))
            {
                Entities.Remove(entity.Id);
                OnRepositoryModified();
            }
            else throw new EntityNotFoundException();
        }

        public abstract string EntityNames
        {
            get;
        }

        public XStreamingElement EmitXml()
        {
            return new XStreamingElement(EntityNames,
                from entity in Entities.Values
                select entity.EmitXml());
        }

        //TODO Could possibly do some testing - particularly rainy day cases
        public void WriteXmlFile(string dataPath)
        {
            XStreamingElement accountXml = EmitXml();
            accountXml.Save(dataPath);
        }

        public event EventHandler RepositoryModified;

        protected virtual void OnRepositoryModified()
        {
            EventHandler handler = RepositoryModified;
            if (handler != null)
                handler(this, null);
        }

        protected abstract void Initialise(XElement xElement);

        public void InitialiseRepository(string dataPath)
        {
            using (Stream stream = new FileStream(dataPath, FileMode.Open))
            using (XmlReader xmlReader = new XmlTextReader(stream))
            {
                var xElement = XElement.Load(xmlReader);
                Initialise(xElement);
            }
        }
    }
}
