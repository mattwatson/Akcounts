using System;
using Akcounts.Domain.Objects;
using NUnit.Framework;

namespace Akcounts.Domain.Tests
{
    [TestFixture]
    public class Journal_spec
    {
        readonly Account _bankAccount = new Account(1, "Bank", AccountType.Asset);
        readonly Account _expenseAccount = new Account(2, "Groceries", AccountType.Expense);
        readonly Account _creditCard = new Account(3, "Credit Card", AccountType.Asset);
        readonly Account _groceries = new Account(4, "Groceries", AccountType.Expense);
        readonly Account _toiletries = new Account(5, "Toiletries", AccountType.Expense);
        readonly DateTime _today = DateTime.Today;

        [Test]
        public void can_create_new_Journal()
        {
            var journal = new Journal(_today);

            Assert.AreEqual(_today, journal.Date);
            Assert.AreEqual("", journal.Description);
            Assert.AreEqual(0, journal.Transactions.Count);
        }

        [Test]
        public void can_create_new_Journal_with_description()
        {
            var journal = new Journal(_today, "BoE Payslip");

            Assert.AreEqual("BoE Payslip", journal.Description);
        }

        [Test]
        public void cannot_create_new_Journal_with_default_date()
        {
            Assert.Throws<ArgumentException>(() => new Journal(default(DateTime)));
        }

        [Test]
        public void cannot_set_date_to_default_date()
        {
            var journal = new Journal(_today);
            Assert.Throws<ArgumentException>(() => journal.Date = default(DateTime));
        }

        [Test]
        public void cannot_create_new_Journal_with_null_description()
        {
            Assert.Throws<ArgumentNullException>(() => new Journal(_today, null));
        }

        [Test]
        public void cannot_set_description_to_null()
        {
            var journal = new Journal(_today);
            Assert.Throws<ArgumentNullException>(() => journal.Description = null);
        }

        [Test]
        public void can_set_a_description()
        {
            var journal = new Journal(_today) {Description = "Wagamama"};

            Assert.AreEqual("Wagamama", journal.Description);
        }

        [Test]
        public void journal_keeps_track_of_any_transaction_that_are_created_with_it()
        {
            var journal = new Journal(_today, "Morrisons");

            var t1 = new Transaction(journal, TransactionDirection.Out, amount: 10, account: _bankAccount);
            var t2 = new Transaction(journal, TransactionDirection.In, amount: 10, account: _expenseAccount);
           
            Assert.AreEqual(2, journal.Transactions.Count);
            Assert.IsTrue(journal.Transactions.Contains(t1));
            Assert.IsTrue(journal.Transactions.Contains(t2));
            Assert.AreEqual(journal, t1.Journal);
            Assert.AreEqual(journal, t2.Journal);
        }

        [Test]
        public void can_remove_a_transaction_from_a_Journal()
        {
            var journal = new Journal(_today, "Morrisons");

            var t1 = new Transaction(journal, TransactionDirection.Out, amount: 10, account: _bankAccount);
            journal.DeleteTransaction(t1);

            Assert.AreEqual(0, journal.Transactions.Count);
            Assert.IsFalse(journal.Transactions.Contains(t1));
        }

        [Test]
        public void journal_is_valid_when_in_and_out_transactions_balance()
        {
            var journal = new Journal(_today, "Morrisons");

            var t1 = new Transaction(journal, TransactionDirection.Out, amount: 10M, account: _creditCard);
            var t2 = new Transaction(journal, TransactionDirection.In, amount: 8.56M, account: _groceries);
            var t3 = new Transaction(journal, TransactionDirection.In, amount: 1.44M, account: _toiletries);
           
            Assert.AreEqual(3, journal.Transactions.Count);
            Assert.IsTrue(journal.Transactions.Contains(t1));
            Assert.IsTrue(journal.Transactions.Contains(t2));
            Assert.IsTrue(journal.Transactions.Contains(t3));
            Assert.AreEqual(journal, t1.Journal);
            Assert.AreEqual(journal, t2.Journal);
            Assert.AreEqual(journal, t3.Journal);

            Assert.IsTrue(journal.IsValid);
        }

        [Test]
        public void journal_is_not_valid_when_in_and_out_transactions_do_not_balance()
        {
            var journal = new Journal(_today, "Morrisons");

            var t1 = new Transaction(journal, TransactionDirection.Out, amount: 9.99M, account: _creditCard);
            var t2 = new Transaction(journal, TransactionDirection.In, amount: 8.56M, account: _groceries);
            var t3 = new Transaction(journal, TransactionDirection.In, amount: 1.44M, account: _toiletries);

            Assert.AreEqual(3, journal.Transactions.Count);
            Assert.IsTrue(journal.Transactions.Contains(t1));
            Assert.IsTrue(journal.Transactions.Contains(t2));
            Assert.IsTrue(journal.Transactions.Contains(t3));
            Assert.AreEqual(journal, t1.Journal);
            Assert.AreEqual(journal, t2.Journal);
            Assert.AreEqual(journal, t3.Journal);

            Assert.IsFalse(journal.IsValid);
        }

        [Test]
        public void journal_is_not_valid_when_it_has_an_invalid_transaction_null_account()
        {
            var journal = new Journal(_today, "Morrisons");

            var t1 = new Transaction(journal, TransactionDirection.Out, amount: 9.99M);
            new Transaction(journal, TransactionDirection.In, amount: 9.99M, account: _groceries);

            Assert.IsFalse(journal.IsValid);
            Assert.IsFalse(t1.IsValid);
        }

        [Test]
        public void journal_is_not_valid_when_it_has_an_invalid_transaction_zero_balance()
        {
            var journal = new Journal(_today, "Morrisons");

            var t1 = new Transaction(journal, TransactionDirection.Out, _groceries);
            var t2 = new Transaction(journal, TransactionDirection.In, _groceries);

            Assert.IsFalse(journal.IsValid);
            Assert.IsFalse(t1.IsValid);
            Assert.IsFalse(t2.IsValid);
        }


        [Test]
        public void Journal_with_no_transactions_is_not_valid()
        {
            var journal = new Journal(_today, "Hotel Jezzera");

            Assert.IsFalse(journal.IsValid);
        }

        [Test]
        public void can_be_marked_as_verified_if_IsValid_is_true()
        {
            var journal = new Journal(_today, "Morrisons");

            new Transaction(journal, TransactionDirection.Out, amount: 9.99M, account: _creditCard);
            new Transaction(journal, TransactionDirection.In, amount: 8.545M, account: _groceries);
            new Transaction(journal, TransactionDirection.In, amount: 1.445M, account: _toiletries);

            journal.IsLocked = true;

            Assert.IsTrue(journal.IsValid);
            Assert.IsTrue(journal.IsLocked);
        }

        [Test]
        public void cannot_be_marked_as_verified_if_IsValid_is_false()
        {
            var journal = new Journal(_today, "Hotel Jezzera");
            Assert.IsFalse(journal.IsValid);

            Assert.Throws<InValidJournalCannotBeVerifiedException>(() => journal.IsLocked = true);
        }

        [Test]
        public void cannot_change_attribtes_when_journal_has_been_verified()
        {
            var journal = new Journal(_today, "Morrisons");

            var t1 = new Transaction(journal, TransactionDirection.Out, amount: 9.99M, account: _creditCard);
            var t2 = new Transaction(journal, TransactionDirection.In, amount: 8.545M, account: _groceries);
            new Transaction(journal, TransactionDirection.In, amount: 1.445M, account: _toiletries);

            journal.IsLocked = true;

            Assert.Throws<VerifiedJournalCannotBeModifiedException>(() => journal.Date = DateTime.Now);
            Assert.AreEqual(_today, journal.Date);

            Assert.Throws<VerifiedJournalCannotBeModifiedException>(() => journal.Description = "Morrisons 2");
            Assert.AreEqual("Morrisons", journal.Description);

            Assert.Throws<VerifiedJournalCannotBeModifiedException>(() => journal.DeleteTransaction(t1));
            Assert.IsTrue(journal.Transactions.Contains(t1));

            Assert.Throws<VerifiedJournalCannotBeModifiedException>(() => new Transaction(journal, TransactionDirection.In));
            Assert.AreEqual(3, journal.Transactions.Count);

            Assert.Throws<TransactionIsLockedAndCannotBeModifiedException>(() => t1.Amount = 2M);
            Assert.AreEqual(9.99M, t1.Amount);

            Assert.Throws<TransactionIsLockedAndCannotBeModifiedException>(() => t1.Account = _toiletries);
            Assert.AreEqual(_creditCard, t1.Account);

            Assert.Throws<TransactionIsLockedAndCannotBeModifiedException>(() => t1.Note = "New note");
            Assert.AreEqual("", t1.Note);

            Assert.Throws<TransactionIsLockedAndCannotBeModifiedException>(() => t2.Direction = TransactionDirection.Out);
            Assert.AreEqual(TransactionDirection.In, t2.Direction);
        }

        [Test]
        public void can_emit_xml_describing_Journal()
        {
            var date = new DateTime(2011, 5, 24);
            var journal = new Journal(3, date, "Morrisons");
            new Transaction(journal, TransactionDirection.In, amount: 10, account: _groceries);
            new Transaction(journal, TransactionDirection.Out, amount: 10, account: _bankAccount) {IsVerified = true};
            journal.IsLocked = true;

            const string expected = @"<journal id=""3"" date=""2011-05-24T00:00:00"" description=""Morrisons"" isVerified=""true"">
  <transactions>
    <transaction direction=""1"" account=""4"" amount=""10"" note="""" isVerified=""false"" />
    <transaction direction=""2"" account=""1"" amount=""10"" note="""" isVerified=""true"" />
  </transactions>
</journal>";
            var actual = journal.EmitXml().ToString();
            Assert.AreEqual(expected, actual);
        }

    }
}
