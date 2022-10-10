using Godot;
using System.Collections.Generic;

namespace Pawn.Actions {
	public interface IAction {

		int CooldownMilliseconds {get; }
		//TODO implement tags for actions
		List<ActionTags> Tags {get;}
		string Name {get;}
		float MaxRange {get;}

		void execute(object argsStruct);
	}

	public enum ActionTags {COMBAT}
}