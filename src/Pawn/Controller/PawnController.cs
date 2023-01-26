using System.Globalization;
using Godot;
using System;
using Serilog;
using Pawn.Tasks;
using System.Collections.Generic;
using Pawn.Item;

//HAVE TO CALL Setup() before this node will function!!!
namespace Pawn.Controller
{
	public class PawnController : Node, IInteractable
	{
		//TODO: This might be bad design, time will tell
		public Transform GlobalTransform
		{
			get { return rigidBody.GlobalTransform; }
			set { rigidBody.GlobalTransform = value; }
		}
		public PawnInformation PawnInformation {get;}
		public PawnInventory PawnInventory {get;}

		public PawnBrainController PawnBrain {get;}
		private SensesStruct sensesStruct;

		//ALL OF THE BELOW VARIABLES ARE CREATED IN Setup() or _Ready
		private SensesController sensesController = null!;
		private HealthBar3D healthBar = null!;
		public TaskExecutor ActionController {get; private set;} = null!;
		public VisualController VisualController {get; private set;} = null!;
		private CollisionShape collisionShape = null!;
		private RayCast downwardRayCast = null!;
		private NavigationAgent navigationAgent = null!;
		private RigidBody rigidBody = null!;
		public MovementController MovementController {get; private set;} = null!;
		private ITask currentTask = new InvalidTask();
		
		//if death has been started, then this pawn is in the process of Dying
		public bool IsDying {get { return startedDeath != DateTime.MaxValue;}}
		private DateTime startedDeath = DateTime.MaxValue;

		//TODO: implement something like the below:
		//private List<IPawnTrait> pawnTraits;

		//When created by instancing a scene, the default constructor is called.
		public PawnController() {
			sensesStruct = new SensesStruct();
			PawnBrain = new PawnBrainController();
			PawnInventory = new PawnInventory();
			PawnInformation = new PawnInformation();
		}
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			//so paths are *ONLY* reference here so I think I will just leave it hard coded
			healthBar = GetNode<HealthBar3D>("RigidBody/HealthBar");
			VisualController = GetNode<VisualController>("RigidBody/VisualController");
			collisionShape = GetNode<CollisionShape>("RigidBody/CollisionShape");
			downwardRayCast = GetNode<RayCast>("RigidBody/DownwardRayCast");
			navigationAgent = GetNode<NavigationAgent>("RigidBody/NavigationAgent");
			rigidBody = GetNode<RigidBody>("RigidBody");

			MovementController = new MovementController(rigidBody,
														VisualController,
														navigationAgent,
														downwardRayCast);
			ActionController = new TaskExecutor(MovementController, VisualController, PawnInformation);
		}

		//Setup HAS to be called for a pawn to work
		//For a pawn to function: the constructor is called,
		//then _Ready is called
		//Then Setup has to be called
		//Setup basically fills in for an actual constructor since I cannot seem to call a real constructor with args
		//for the PackedScene.Instance function
		public void Setup(KdTreeController kdTreeController)
		{
			sensesController = new SensesController(kdTreeController, this);
			VisualController.ForceVisualUpdate(PawnInventory);
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(float delta)
		{
			if(IsDying) {
				HandleDying();
			} else {
				//only living pawns get to think
				sensesStruct = sensesController.UpdatePawnSenses(sensesStruct);
				currentTask = PawnBrain.updateCurrentTask(currentTask, sensesStruct, this);
				VisualController.ProcessTask(currentTask, PawnInventory);
			}
		}

		public override void _PhysicsProcess(float delta)
		{
			if(!IsDying) {
				//Placed in physics process since Handle task may call some functions
				//in movement controller which **I Think** means this function has to be placed in 
				//_PhysicsProcess
				ActionController.HandleTask(currentTask);
			}
		}

		public void HandleDying() {
			int TIME_TO_WAIT_AFTER_DEATH = 5;
			if( (DateTime.Now - startedDeath).TotalSeconds > TIME_TO_WAIT_AFTER_DEATH) {
				//Corpse has gone cold for 5 seconds, we finally pass
				Die();
			}
		}
		//This function begins the death process
		public void StartDying() {
			//I need to start playing the death animation
			startedDeath = DateTime.Now;
			VisualController.SetAnimation(AnimationName.Death);
			//TODO: cannot figure out how to 'turn off' the rigid body
			//This basically 'turns off' the rigid body
			//rigidBody.CollisionLayer = 0;
			//uint MASK_JUST_FLOOR = 1;
			//rigidBody.CollisionMask = MASK_JUST_FLOOR;
			//rigidBody.Sleeping = true;
		}

		//Gets the total damage that this pawn is able to produce.
		//TODO: needs to be replaced with actual pawn stats
		public double GetDamage() {
			Equipment? weapon = PawnInventory.GetWornEquipment(EquipmentType.HELD);
			if(weapon != null) {
				return PawnInformation.BaseDamage + weapon.Damage;
			} else {
				return PawnInformation.BaseDamage;
			}
		}

		public void SetWeapon(Equipment _weapon) {
			PawnInventory.WearEquipment(_weapon);
		}


		public void TakeDamage(double damage)
		{
			PawnInformation.Health = PawnInformation.Health - damage;
			if (PawnInformation.Health <= 0)
			{
				healthBar.SetHealthPercent(0);
				StartDying();
				return;
			}
			if(PawnInformation.Health > PawnInformation.MaxHealth) {
				PawnInformation.Health = PawnInformation.MaxHealth;
			}
			healthBar.SetHealthPercent(PawnInformation.Health / PawnInformation.MaxHealth);
		}

		private void Die()
		{
			PawnInventory.QueueFree();
			this.QueueFree();
		}

		public void Adhoc()
		{
		}

		//only used by UI elements 
		//NEVER MUTATE THE TASK!!!
		public ITask GetTask()
		{
			return currentTask;
		}

		public bool IsInstanceValid() {
			return IsInstanceValid(this);
		}
	}
}
