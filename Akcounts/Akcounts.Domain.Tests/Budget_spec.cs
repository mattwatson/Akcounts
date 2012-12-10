using System;
using Akcounts.Domain.Objects;
using NUnit.Framework;

namespace Akcounts.Domain.Tests
{
    [TestFixture]
    public class Template_spec
    {
        readonly DateTime _today = DateTime.Today;

        [Test]
        public void can_create_new_Template()
        {
            var template = new Template();

            Assert.AreEqual(0, template.Journals.Count);
        }
        
        [Test]
        public void can_add_a_journal_to_a_template()
        {
            var template = new Template();
            var journal1 = new Journal(_today, "Rent");
            var journal2 = new Journal(_today, "Credit Card Bill");
            
            template.AddJournal(journal1);
            template.AddJournal(journal2);

            Assert.AreEqual(2, template.Journals.Count);
            Assert.IsTrue(template.Journals.Contains(journal1));
            Assert.IsTrue(template.Journals.Contains(journal2));
        }

        [Test]
        public void can_remove_a_journal_from_a_Template()
        {
            var template = new Template();
            var journal = new Journal(_today, "Rent");
            template.AddJournal(journal);

            template.RemoveJournal(journal);

            Assert.AreEqual(0, template.Journals.Count);
            Assert.IsFalse(template.Journals.Contains(journal));
        }
        
        [Test]
        public void can_emit_xml_describing_Template()
        {
            var date = new DateTime(2011, 5, 24);
            var journal = new Journal(3, date, "Rent");

            var template = new Template();
            template.AddJournal(journal);

            const string expected = @"<template>
  <journal id=""3"" date=""2011-05-24T00:00:00"" description=""Rent"" isVerified=""false"">
    <transactions />
  </journal>
</template>";
            var actual = template.EmitXml().ToString();
            Assert.AreEqual(expected, actual);
        }
    }
}
