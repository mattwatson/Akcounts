using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Akcounts.Domain.RepositoryInterfaces
{
    public interface IXmlRepository<T>
    {
        T GetById(int id);
        IEnumerable<T> GetAll();

        string EntityNames { get; }

        void Save(T entity);
        void Remove(T entity);

        XStreamingElement EmitXml();

        void WriteXmlFile(string accountDataPath);

        event EventHandler RepositoryModified;
    }
}
