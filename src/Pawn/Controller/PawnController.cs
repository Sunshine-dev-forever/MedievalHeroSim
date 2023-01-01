using System.Globalization;
using Godot;
using System;
using Serilog;
using Pawn.Tasks;
using System.Collections.Generic;

//HAVE TO CALL Setup() before this node will function!!!
namespace Pawn.Controller
{
	public class PawnController : Node
	{
		//TODO: This might be bad design, time will tell
		public Transform GlobalTransform
		{
			get { return rigidBody.GlobalTransform; }
			set { rigidBody.GlobalTransform = value; }
		}

		//TODO should all be in a pawnInfomration class
		private float health = 100;
		private float maxHealth = 100;
		private string faction = "none";
		public string pawnName = "Testy Mc Testerson";
		public ActionController actionController {get;}
		public PawnBrainController PawnBrain {get;}
		private SensesStruct sensesStruct;

		//ALL OF THE BELOW VARIABLES ARE CREATED IN Setup() or _Ready
		private SensesController sensesController = null!;
		private HealthBar3D healthBar = null!;
		private VisualController visualController = null!;
		private CollisionShape collisionShape = null!;
		private RayCast downwardRayCast = null!;
		private NavigationAgent navigationAgent = null!;
		private RigidBody rigidBody = null!;
		public MovementController MovementController {get; private set;} = null!;


		private ITask currentTask = new InvalidTask();

		//TODO: implement something like the below:
		//private List<IPawnTrait> pawnTraits;

		//When created by instancing a scene, the default constructor is called.
		public PawnController() {
			actionController = new ActionController();
			sensesStruct = new SensesStruct();
			PawnBrain = new PawnBrainController(actionController);
		}
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			//so paths are *ONLY* reference here so I think I will just leave it hard coded
			healthBar = GetNode<HealthBar3D>("RigidBody/HealthBar");
			visualController = GetNode<VisualController>("RigidBody/VisualController");
			collisionShape = GetNode<CollisionShape>("RigidBody/CollisionShape");
			downwardRayCast = GetNode<RayCast>("RigidBody/DownwardRayCast");
			navigationAgent = GetNode<NavigationAgent>("RigidBody/NavigationAgent");
			rigidBody = GetNode<RigidBody>("RigidBody");

			MovementController = new MovementController(rigidBody,
														visualController,
														navigationAgent,
														downwardRayCast);
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(float delta)
		{
			sensesStruct = sensesController.UpdatePawnSenses(sensesStruct);
			currentTask = PawnBrain.updateCurrentTask(currentTask, sensesStruct, this);
			visualController.ProcessTask(currentTask);
		}

		public override void _PhysicsProcess(float delta)
		{
			actionController.HandleTask(currentTask, MovementController, visualController);
		}

		public void Setup(KdTreeController kdTreeController)
		{
			sensesController = new SensesController(kdTreeController, this);
		}


		public void TakeDamage(float damage)
		{
			health = health - damage;
			healthBar.SetHealthPercent(health / maxHealth);
			if (health <= 0)
			{
				Die();
			}
		}

		private void Die()
		{
			this.QueueFree();
		}

		public void Adhoc()
		{
			foreach(Node node in this.GetChildren()) {
				dfs(node);
			}
		}

		public void dfs(Node node) {
			Log.Information("found node: " + node.Name);
			foreach( Node othernode in node.GetChildren()) {
				dfs(othernode);
			}
		}

		//only used by UI elements 
		//NEVER MUTATE THE TASK!!!
		public ITask GetTask()
		{
			return currentTask;
		}
	}
}
