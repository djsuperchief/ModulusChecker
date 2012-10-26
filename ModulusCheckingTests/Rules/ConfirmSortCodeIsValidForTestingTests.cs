using System.Collections.Generic;
using ModulusChecking.Loaders;
using ModulusChecking.Models;
using ModulusChecking.Steps;
using Moq;
using NUnit.Framework;

namespace ModulusCheckingTests.Rules
{
    public class ConfirmSortCodeIsValidForTestingTests
    {
        private Mock<IModulusWeightTable> _mockModulusWeightTable;
        private Mock<FirstModulusCalculatorStep> _firstModulusCalculatorStep;
        private ConfirmSortCodeIsValidForModulusCheck _ruleStep;

        [SetUp]
        public void Before()
        {
            var mappingSource = new Mock<IRuleMappingSource>();
            mappingSource.Setup(ms => ms.GetModulusWeightMappings())
                .Returns(new List<IModulusWeightMapping>
                             {
                                 new ModulusWeightMapping(
                                     "010004 010006 MOD10 2 1 2 1 2  1 2 1 2 1 2 1 2 1"),
                                 new ModulusWeightMapping(
                                     "010004 010006 DBLAL 2 1 2 1 2  1 2 1 2 1 2 1 2 1"),
                                 new ModulusWeightMapping(
                                     "010007 010010 DBLAL  2 1 2 1 2  1 2 1 2 1 2 1 2 1"),
                                 new ModulusWeightMapping(
                                     "010011 010013 MOD11    2 1 2 1 2  1 2 1 2 1 2 1 2 1"),
                                 new ModulusWeightMapping(
                                     "010014 010014 MOD11    2 1 2 1 2  1 2 1 2 1 2 1 2 1 5")
                             });
            _mockModulusWeightTable = new Mock<IModulusWeightTable>();
            _mockModulusWeightTable.Setup(mwt => mwt.GetRuleMappings(new SortCode("010004"))).Returns(
                new List<IModulusWeightMapping>
                    {
                        new ModulusWeightMapping(
                            "010004 010006 MOD10 2 1 2 1 2  1 2 1 2 1 2 1 2 1"),
                        new ModulusWeightMapping(
                            "010004 010006 DBLAL 2 1 2 1 2  1 2 1 2 1 2 1 2 1"),
                    });
            _mockModulusWeightTable.Setup(mwt => mwt.GetRuleMappings(new SortCode("010009"))).Returns(
                new List<IModulusWeightMapping>
                    {
                        new ModulusWeightMapping(
                            "010007 010010 DBLAL  2 1 2 1 2  1 2 1 2 1 2 1 2 1")
                    });
            _firstModulusCalculatorStep = new Mock<FirstModulusCalculatorStep>();
            _firstModulusCalculatorStep.Setup(fmc => fmc.Process(It.IsAny<BankAccountDetails>(), _mockModulusWeightTable.Object)).
                Returns(false);
            _ruleStep = new ConfirmSortCodeIsValidForModulusCheck(_firstModulusCalculatorStep.Object);
        }

        [Test]
        public void UnknownSortCodeIsValid()
        {
            const string sortCode = "123456";
            var accountDetails = new BankAccountDetails(sortCode, "12345678");
            var result = _ruleStep.Process(accountDetails, _mockModulusWeightTable.Object);
            Assert.IsTrue(result);
            _firstModulusCalculatorStep.Verify(fmc => fmc.Process(It.IsAny<BankAccountDetails>(), _mockModulusWeightTable.Object), Times.Never());
        }

        [Test]
        [TestCase("010004", "12345678",TestName = "CanTestAtStartOfRange")]
        [TestCase("010009", "12345678", TestName = "CanTestAtEndOfRange")]
        public void KnownSortCodeIsTested(string sc, string an)
        {
            var accountDetails = new BankAccountDetails(sc, an);
            _ruleStep.Process(accountDetails, _mockModulusWeightTable.Object);
            _firstModulusCalculatorStep.Verify(nr => nr.Process(accountDetails, _mockModulusWeightTable.Object));
        }
    }
}