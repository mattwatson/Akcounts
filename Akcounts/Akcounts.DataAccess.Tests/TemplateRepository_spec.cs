using System.IO;
using System.Xml;
using System.Xml.Linq;
using Akcounts.DataAccess.Repositories;
using Akcounts.Domain.Objects;
using Akcounts.Domain.RepositoryInterfaces;
using NMock2;
using NUnit.Framework;

namespace Akcounts.DataAccess.Tests
{
    [TestFixture]
    public class TemplateRepository_spec
    {
        private ITemplateRepository _repository;

        const string TestData = @"<templates>
  <template>
    <journal id=""0"" date=""2011-06-09T00:00:00"" description=""Rent"" isVerified=""false"">
      <transactions>
        <transaction direction=""2"" account=""15"" amount=""433.33"" note="""" isVerified=""true"" />
        <transaction direction=""1"" account=""90"" amount=""433.33"" note="""" isVerified=""false"" />
      </transactions>
    </journal>
    <journal id=""0"" date=""2011-10-09T00:00:00"" description=""Electricity bill"" isVerified=""false"">
      <transactions>
        <transaction direction=""2"" account=""15"" amount=""15"" note="""" isVerified=""false"" />
        <transaction direction=""1"" account=""19"" amount=""15"" note="""" isVerified=""false"" />
      </transactions>
    </journal>
  </template>
</templates>";

        private Account _account15;
        private Account _account90;
        private Account _account19;

        private Mockery _mocks;
        private IAccountRepository _mockAccountRepository;

        [SetUp]
        public void CreateInitialData()
        {
            _mocks = new Mockery();
            _mockAccountRepository = _mocks.NewMock<IAccountRepository>();

            _account15 = new Account(15, "Bank", AccountType.Asset);
            _account90 = new Account(90, "Rent", AccountType.Expense);
            _account19 = new Account(19, "Electricity", AccountType.Expense);

            Expect.Once.On(_mockAccountRepository).Method("GetById").With(15).Will(Return.Value(_account15));
            Expect.Once.On(_mockAccountRepository).Method("GetById").With(90).Will(Return.Value(_account90));
            Expect.Once.On(_mockAccountRepository).Method("GetById").With(15).Will(Return.Value(_account15));
            Expect.Once.On(_mockAccountRepository).Method("GetById").With(19).Will(Return.Value(_account19));

            using (var sr = new StringReader(TestData))
            using (XmlReader xml = new XmlTextReader(sr))
            {
                XElement journals = XElement.Load(xml);
                _repository = new TemplateRepository(journals, _mockAccountRepository);
            }
        }

        [Test]
        public void can_output_the_repository_as_Xml()
        {
            var output = _repository.EmitXml().ToString();
            
            Assert.AreEqual(TestData, output);
            _mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}