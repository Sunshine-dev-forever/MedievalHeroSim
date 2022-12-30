using Godot;
using System.Collections.Generic;
using Pawn.Controller;

namespace Pawn.Action {
	public interface IAction {

		int CooldownMilliseconds {get; }
		//TODO implement tags for actions
		List<ActionTags> Tags {get;}
		string Name {get;}
		float MaxRange {get;}

		void execute(object argsStruct, VisualController visualController);
	}

	public enum ActionTags {COMBAT}
}