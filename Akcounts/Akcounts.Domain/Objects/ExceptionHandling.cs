using System;

namespace Akcounts.Domain.Objects
{
    public class EntityAlreadyExistsException : ApplicationException { }
    public class EntityNotFoundException : ApplicationException { }
    
    public class OrphanTransactionException : Exception { }

    public class InValidJournalCannotBeVerifiedException : Exception { }
    public class VerifiedJournalCannotBeModifiedException : Exception { }
    public class TransactionIsLockedAndCannotBeModifiedException : Exception { }
    public class InValidTransactionCannotBeVerifiedException : Exception { }
}
