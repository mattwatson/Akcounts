using System;
using System.Collections.Generic;
using Akcounts.Domain.Objects;

namespace Akcounts.Domain.RepositoryInterfaces
{
    public interface ITemplateRepository : IXmlRepository<Template>
    {
        IList<Journal> GetTemplateJournals();

        IList<Journal> GetImaginaryJournals(DateTime startDate, DateTime endDate);
    }
}
