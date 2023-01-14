using System;
using Serilog;
using System.Collections.Generic;
using Pawn.Tasks;
using Pawn.Action;
using Pawn.Action.Ability;
using Pawn.Controller;
using Pawn.Item;
using Pawn.Targeting;

namespace Pawn.Goal {
	public class DefendSelfGoal : IPawnGoal
	{
		public ITask GetTask(PawnController pawnController, SensesStruct sensesStruct) {

			if(sensesStruct.nearbyPawns.Count == 0) {
				return new InvalidTask();
			}
			PawnController otherPawnController = sensesStruct.nearbyPawns[0];
			List<ActionTags> requestedTags = new List<ActionTags>();
			requestedTags.Add(ActionTags.COMBAT);
			List<IAbility> validAbilities = pawnController.ActionController.GetAllActionsWithTags(requestedTags, false);
			//no matter what we are targeting the other pawn
			ITargeting targeting = new InteractableTargeting(otherPawnController);
			//The only valid action in combat is stabbing
			if (validAbilities.Count == 0)
			{
				//returning an invalid action here could cause the brain to move on to the next goal
				//Which is not what we want
				//Therefore the pawn will wait until an ability is usable
				//if not actions are vaild, then we have to wait
				int waitTimeMilliseconds = 100;
				IAction waitAction = ActionBuilder.Start(pawnController, () => {})
										.Animation(AnimationName.Idle)
										.AnimationPlayLength(waitTimeMilliseconds)
										.Finish();
				//TODO: pawnController.Weapon.Mesh should default to a spatial node. even if Weapon is null
				waitAction.HeldItem = pawnController.PawnInventory.GetWornEquipment(EquipmentType.HELD);
				return new Task(targeting, waitAction);
			} else {
				//This action has to be a stab action for now
				IAbility ability = validAbilities[0].Duplicate(pawnController, otherPawnController);
				IAction action = ActionBuilder.Start(ability, pawnController).Finish();
				return new Task(targeting, action);
			}
		}
	}
}
