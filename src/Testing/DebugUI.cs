using Godot;
using System;
using System.Collections.Generic;
using Serilog;
using Pawn.Actions;
using System.Threading.Tasks;

public class DebugUI : Spatial
{
	private Camera camera;
	private int RAY_LENGTH = 10000;
	public override void _Ready()
	{
		camera = GetNode<Camera>("Camera");	
	}
	
	public override void _Input(InputEvent input) {
		 if(input.IsActionPressed("mouse_left_click") && input is InputEventMouseButton) {
			CastRayFromCamera((InputEventMouseButton) input);
		 } else if(input.IsActionPressed("ui_left")) {
			//pass
		 }
	}

	private void CastRayFromCamera(InputEventMouseButton input) {
		//just a query should be fine to call outside of physics_process
		PhysicsDirectSpaceState spaceState = GetWorld().DirectSpaceState;
		Vector3 from = camera.ProjectRayOrigin(input.Position);
		Vector3 to = from + camera.ProjectRayNormal(input.Position) * RAY_LENGTH;
		Godot.Collections.Dictionary result = spaceState.IntersectRay(from, to);
		//we really dont want to be using Godot dictionaries
		if(result.Count == 0) {
			//we hit nothing
			return;
		}
		if(result["collider"] is PawnRigidBody) {
			PawnController pawnController = ((PawnRigidBody) result["collider"]).GetPawnController();
			Log.Information("success!, the faction of this pawn is " + pawnController.Name);
		}

	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//	  
//  }
}