Feature: OrderVote

@OrderVote
Scenario: The order with the highest vote gets to the top
	Given there is a question "What is your fovorite color?" with the answers?
		| Answer | Vote |
		| Red    | 3    |
		| Green  | 3    |
		| Blue   | 2    |

	When you upvote answer "Green"
	Then the answer "Green" should be on top
