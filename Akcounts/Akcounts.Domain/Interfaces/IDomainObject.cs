using System.Xml.Linq;

namespace Akcounts.Domain.Interfaces
{
    public interface IDomainObject 
    {
        XElement EmitXml();
    }
}