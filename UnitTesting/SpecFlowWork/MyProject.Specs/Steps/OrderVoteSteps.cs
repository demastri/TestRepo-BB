using System;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace MyProject.Specs
{
    [Binding]
    public class OrderVoteSteps
    {
        UpVoter thisVoter = new UpVoter();

        [Given(@"there is a question ""(.*)"" with the answers\?")]
        public void GivenThereIsAQuestionWithTheAnswers(string p0, Table table)
        {
            foreach (TableRow r in table.Rows)
                thisVoter.Add(r[0], Convert.ToInt32(r[1]) );
        }
        
        [When(@"you upvote answer ""(.*)""")]
        public void WhenYouUpvoteAnswer(string p0)
        {
            thisVoter.UpVote(p0);
        }
        
        [Then(@"the answer ""(.*)"" should be on top")]
        public void ThenTheAnswerShouldBeOnTop(string p0)
        {
            Assert.AreEqual(p0, thisVoter.GetOption(0));
        }
    }
}
