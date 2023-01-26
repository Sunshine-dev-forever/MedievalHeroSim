using System.Threading;
using System;
using Serilog;
using System.Collections.Generic;
using Pawn.Tasks;
using Pawn.Action;
using Pawn.Controller;
using Pawn.Item;
using Pawn.Targeting;
namespace Pawn.Goal {
	public class HealGoal : IPawnGoal
	{
		public ITask GetTask(PawnController pawnController, SensesStruct sensesStruct) {
			IItem? currentItem = null;
			foreach( IItem item in pawnController.PawnInventory.inventory) {
				if(item is Consumable) {
					currentItem = item;
					break;
				}
			}

			if(currentItem == null) {
				//if we have no consumables, then we early exit
				return new InvalidTask();
			}
			if(pawnController.PawnInformation.Health > 50) {
				//we are not hurt, no reason to use a potion
				return new InvalidTask();
			}

			Consumable potion = (Consumable) currentItem;
			System.Action executable = () => {
				pawnController.PawnInventory.inventory.Remove(potion);
				//TODO: TakeDamage should be called 'change health'
				pawnController.TakeDamage(potion.Healing * (-1));
			};
			//we know it is only health potions
			IAction action = ActionBuilder.Start(pawnController, executable)
										.Animation(AnimationName.Drink)
										.HeldItem(potion)
										.Finish();
			ITargeting targeting = new InteractableTargeting(pawnController);
			return new Task(targeting, action);
		}
	}
}