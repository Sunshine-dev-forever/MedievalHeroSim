using Godot;
using System;
using System.Collections.Generic;
using Serilog;
using Pawn.Actions;
using System.Threading.Tasks;

//TODO: I need more examples on this
[System.Diagnostics.CodeAnalysis.SuppressMessage("Non-nullable", "CA8618:must contain non-null value exiting constructor", Justification = "Not production code.")]
public class AdhocTest : Node
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	// Called when the node enters the scene tree for the first time.

	private KdTreeController kdTreeController; 
	public override void _Ready()
	{
		kdTreeController = GetNode<KdTreeController>("/root/KdTreeController");
	}
	
	public override void _Input(InputEvent input) {
		 if(input.IsActionPressed("mouse_left_click")) {
			Adhoc();
		 } else if(input.IsActionPressed("ui_left")) {
			Adhoc2();
		 }
	}

	private void Adhoc(){
		PackedScene pawnScene = GD.Load<PackedScene>("res://Objects/pawn.tscn");
		Spatial pawn = pawnScene.Instance<Spatial>();
		this.AddChild(pawn);
		pawn.GlobalTransform = new Transform(pawn.GlobalTransform.basis, new Vector3(0,5,0));
		//kdTreeController.AddPawnToAllPawnList(pawn);

		Log.Information("the current position is: " + pawn.GlobalTransform.origin);
	}

	private void Adhoc2(){
		PawnController child = this.GetChild<Spatial>(0).GetChild<PawnController>(0);
		Log.Information("Childs name is: " + child.Name);
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//	  
//  }
}