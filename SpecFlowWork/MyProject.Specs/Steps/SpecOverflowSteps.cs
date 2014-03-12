using System;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace MyProject.Specs
{
    [Binding]
    public class SpecOverflowSteps
    {
        bool caughtException = false;
        SpecFlowCalculator thisCalc = new SpecFlowCalculator();

        [Given(@"I have entered (.*) into the calculator")]
        public void GivenIHaveEnteredIntoTheCalculator(int p0)
        {
            thisCalc.EnterOperand(p0);
        }

        [When(@"I press add")]
        public void WhenIPressAdd()
        {
            caughtException = false;
            try
            {
                thisCalc.TakeAction("Add");
            }
            catch (SpecFlowCalculator.InsufficientOperands e)
            {
                caughtException = true;
            }
        }

        [Then(@"the result should be (.*) on the screen")]
        public void ThenTheResultShouldBeOnTheScreen(int p0)
        {
            Assert.AreEqual(p0, thisCalc.Peek());
        }
        [Then(@"the result should be an OperandsException")]
        public void ThenTheResultShouldBeAnOperandsException()
        {
            Assert.AreEqual(true, caughtException);
        }
    }
}
