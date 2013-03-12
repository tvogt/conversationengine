
var TurnTowardsPlayer : boolean = true;
var StopMoving : boolean = true;
var ReversedModel : boolean = false; // some models (e.g. Dexsoft) have their axes the wrong way around


// StartConversationActions() is called by the conversation engine when a conversation begins.
// It is called via BroadcastMessage(), so you can add as many actions as necessary. These are just examples
function StartConversationActions() {
	if (TurnTowardsPlayer) {
		// look at player - not up or down, to prevent odd arrangement on floor
		Player = GameObject.FindWithTag("Player");
		transform.LookAt(new Vector3(Player.transform.position.x, transform.position.y, Player.transform.position.z));
		if (ReversedModel) transform.Rotate(0, 180, 0);
	}
	
	if (StopMoving) {
		// pause the model's movement, e.g. wandering around, for the duration of the conversation
	}
	
}

// StopConversationActions() is called when the conversation is over, similar to StartConversationActions() above
function StopConversationActions() {
	if (StopMoving) {
		// start the model's movement up again, e.g. wandering around, for the duration of the conversation
	}
	
}