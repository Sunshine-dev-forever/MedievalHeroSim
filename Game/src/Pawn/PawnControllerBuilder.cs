using System;
using Pawn.Goal;
using Godot;
using Item;
using Pawn.Action.Ability;
using Util;
using System.Collections.Generic;

namespace Pawn
{
	//builder class for a pawn
	public class PawnControllerBuilder
	{
		KdTreeController kdTreeController;
		//this is the pawn that is under construction
		PawnController pawn;

		private static string PAWN_RIG_RESOURCE_FILE_DEFAULT = ResourcePaths.WARRIOR_MODEL;
		private string pawnRigResourceFile = PAWN_RIG_RESOURCE_FILE_DEFAULT;

		//creates a pawn with no AI
		public static PawnController CreateTrainingDummy(Vector3 location, 
														Node parent, 
														KdTreeController _kdTreeController, 
														NavigationRegion3D navigation) {
			return PawnControllerBuilder.Start(parent, _kdTreeController, navigation)
										.Location(location)
										.AddGoal(new DebugGoal())
										.SetName("Training Dummy")
										.Finish();
		}

		public PawnControllerBuilder(Node parent, KdTreeController _kdTreeController, NavigationRegion3D navigation){
			//TODO: There should be some default here in case this fails to load
			//Using the custom resource loader here is complicated because the Pawn_Scene contains much more information than just a mesh
			//Thus I cannot create a reasonable pawn_scene to default to, and so the CustomResourceLoader has no utility
			pawn = ResourceLoader.Load<PackedScene>(ResourcePaths.PAWN_SCENE).Instantiate<PawnController>();
			parent.AddChild(pawn);

			kdTreeController = _kdTreeController;
			kdTreeController.AddInteractable(pawn);

			pawn.PawnMovement.SetNavigation(navigation);
		}
		public static PawnControllerBuilder Start(Node parent, KdTreeController _kdTreeController, NavigationRegion3D navigation) {
			return new PawnControllerBuilder(parent, _kdTreeController, navigation);
		}

		public PawnControllerBuilder Location(Vector3 spawnLocation) {
			pawn.GlobalTransform = new Transform3D(pawn.GlobalTransform.Basis, spawnLocation);
			return this;
		}

		//Appends to the list of goals, so most important goals should be added first
		public PawnControllerBuilder AddGoal(IPawnGoal goal) {
			pawn.PawnBrain.AddGoal(goal);
			return this;
		}

		public PawnControllerBuilder WearEquipment(Equipment equipment){
			pawn.PawnInventory.WearEquipment(equipment);
			return this;
		}

		public PawnController Finish() {
			pawn.PawnVisuals.Setup(pawnRigResourceFile);
			pawn.Setup(kdTreeController);
			return pawn;
		}

		public PawnControllerBuilder SetName(string name) {
			pawn.PawnInformation.Name = name;
			return this;
		}

		public PawnControllerBuilder AddItem(IItem item) {
			pawn.PawnInventory.AddItem(item);
			return this;
		}

		public PawnControllerBuilder DealDamage(double damage) {
			pawn.TakeDamage(damage);
			return this;
		}

		public PawnControllerBuilder Faction(string faction) {
			pawn.PawnInformation.Faction = faction;
			return this;
		}

		//sets the resource file for the pawnMesh
		public PawnControllerBuilder SetPawnRig(string filename) {
			pawnRigResourceFile = filename;
			return this;
		}

		public PawnControllerBuilder AddAbility(string abilityName) {
			switch (abilityName) {
				case AbilityDefinitions.STAB_ABILITY:
					pawn.PawnInformation.AddAbility(AbilityDefinitions.CreateStabAbility(pawn));
				break;
				case AbilityDefinitions.THROW_ABILITY:
					pawn.PawnInformation.AddAbility(AbilityDefinitions.CreateThrowAbility(pawn));
				break;
				default:
					throw new Exception("abilty name not recognized");
			}
			return this;
		}
	}
}