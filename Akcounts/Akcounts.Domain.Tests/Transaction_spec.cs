using System;
using Akcounts.Domain.Objects;
using NUnit.Framework;

namespace Akcounts.Domain.Tests
{
    [TestFixture]
    public class Transaction_spec
    {
        private readonly Journal _journal = new Journal(2, DateTime.Today);
        private readonly Account _account = new Account(1, "Bank", AccountType.Asset);
        private readonly Account _account2 = new Account(2, "Halifax", AccountType.Asset);

        [Test]
        public void can_create_new_Transaction()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, _account, amount: 10.58M, note:"Test Transaction");

            Assert.AreEqual(_journal, transaction.Journal);
            Assert.AreEqual(TransactionDirection.In, transaction.Direction);
            Assert.AreEqual(_account, transaction.Account);
            Assert.AreEqual(10.58M, transaction.Amount);
            Assert.AreEqual("Test Transaction", transaction.Note);
            Assert.IsTrue(transaction.IsValid);
        }

        [Test]
        public void can_create_new_Transaction_without_optional_parameters()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, _account);

            Assert.AreEqual(_journal, transaction.Journal);
            Assert.AreEqual(TransactionDirection.In, transaction.Direction);
            Assert.AreEqual(_account, transaction.Account);
            Assert.AreEqual(0M, transaction.Amount);
            Assert.AreEqual("", transaction.Note);
            Assert.IsFalse(transaction.IsValid);
        }

        [Test]
        public void can_set_a_note()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In) {Note = "Overtime Rate 1"};
            Assert.AreEqual("Overtime Rate 1", transaction.Note);
        }

        [Test]
        public void cannot_create_new_Transaction_with_null_note()
        {
            Assert.Throws<ArgumentNullException>(() => new Transaction(_journal, TransactionDirection.Out, note: null));
        }

        [Test]
        public void cannot_set_transaction_Note_to_null()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In);

            Assert.Throws<ArgumentNullException>(() => transaction.Note = null);
        }

        [Test]
        public void IsValid_is_false_if_account_is_not_specified()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, amount: 10M);
            Assert.IsFalse(transaction.IsValid);

            transaction.Account = _account2;
            Assert.IsTrue(transaction.IsValid);

            transaction.Account = null;
            Assert.IsFalse(transaction.IsValid);
        }

        [Test]
        public void IsValid_is_false_if_amount_is_zero()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out, _account2);
            Assert.IsFalse(transaction.IsValid);

            transaction.Amount = 123.45653M;
            Assert.IsTrue(transaction.IsValid);

            transaction.Amount = 0M;
            Assert.IsFalse(transaction.IsValid);
        }

        [Test]
        public void cannot_create_transaction_with_invalid_enum_value_for_direction()
        {
            Assert.Throws<ArgumentException>(() => new Transaction(_journal, (TransactionDirection)35));
        }

        [Test]
        public void cannot_set_direction_to_an_invalid_enum_value()
        {
            var transaction = new Transaction(_journal, TransactionDirection.Out);
            Assert.Throws<ArgumentException>(() => transaction.Direction = (TransactionDirection)4000);
        }

        //TODO questionable?
        [Test]
        public void Transaction_throws_an_orphaned_transaction_exception_if_accessed_after_it_has_been_removed_from_its_journal()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In);
            _journal.DeleteTransaction(transaction);

            object x = null;
            Assert.Throws<OrphanTransactionException>(() => { x = transaction.IsValid; });
            Assert.Throws<OrphanTransactionException>(() => { x = transaction.Journal; });
            Assert.Throws<OrphanTransactionException>(() => { x = transaction.Note; });
            Assert.Throws<OrphanTransactionException>(() => { x = transaction.Amount; });
            Assert.Throws<OrphanTransactionException>(() => { x = transaction.Account; });
            Assert.IsNull(x);
        }

        [Test]
        public void cannot_update_or_edit_transaction_if_it_belongs_to_a_locked_Journal()
        {
            var journal2 = new Journal(3, DateTime.Today);
            var tran1 = new Transaction(journal2, TransactionDirection.In, _account, 10M, "Stylophone");
            new Transaction(journal2, TransactionDirection.Out, _account, 10M);
            journal2.IsLocked = true;

            Assert.Throws<TransactionIsLockedAndCannotBeModifiedException>(() => tran1.Direction = TransactionDirection.Out);
            Assert.Throws<TransactionIsLockedAndCannotBeModifiedException>(() => tran1.Account = _account2);
            Assert.Throws<TransactionIsLockedAndCannotBeModifiedException>(() => tran1.Amount = 100.99M);
            Assert.Throws<TransactionIsLockedAndCannotBeModifiedException>(() => tran1.Note = "");
        }

        [Test]
        public void can_emit_xml_describing_Transaction()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In, _account, 10.99M, "Stylophone & Stylodrum");

            const string expectedResult = @"<transaction direction=""1"" account=""1"" amount=""10.99"" note=""Stylophone &amp; Stylodrum"" isVerified=""false"" />";

            Assert.AreEqual(expectedResult, transaction.EmitXml().ToString());
        }

        [Test]
        public void Transaction_emit_xml_does_handles_missing_attributes()
        {
            var transaction = new Transaction(_journal, TransactionDirection.In);

            const string expectedResult = @"<transaction direction=""1"" amount=""0"" note="""" isVerified=""false"" />";

            Assert.AreEqual(expectedResult, transaction.EmitXml().ToString());
        }

    }
}
