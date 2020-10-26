var incomeHandler = (state, evt) => { state.balance += evt.data.sum; };
var outcomeHandler = (state, evt) => { state.balance -= evt.data.sum; };

fromStream('card-75d7486b-fbbe-4b08-8aa9-bbbbbbbbbbbb')
	.when({
		$init: () => ({ balance: 0 }),
		Income: incomeHandler,
		Outcome: outcomeHandler
	});
