using Godot;
using System;
using System.Collections.Generic;
using Serilog;
using Pawn.Action;
using System.Threading.Tasks;
using Pawn;
using Pawn.Goal;
using Item;
using Pawn.Action.Ability;
using Util;
using Pawn.Targeting;
using Interactable;

namespace Worlds.MemLeakTest 
{
	//A small class created for testing memory leaks in C#
	//the result is the result I got last time I ran the test
	public partial class MemLeakTestRunner : Node
	{
		/* Current conclusions:
		 * The C# garbage collector does not interact with nodes, and classes that inherit from Nodes will not have the finalizer called
		 * This leaves me in a bind for how to manage memory with Godot C#
		 * Nodes that exist in the scene tree will be cleaned up fine
		 * If I lose a reference to a node that exists only in C# (outside the scene tree) than that is a memory leak
		 *
		 *
		 * Future Ideas:
		 * If I can figure out a way to automatically call the finalizer on classes than inherit from godot types, memory management will get a lot easier
		 *
		 * Another option is to make none of my C# classes inherit from the node class, and instead contain a reference to a node class. 
		 * the above solution will require more refactoring that I would like to think about, but I can also implement that incrementally
		 */
		Label testStatus = null!;
		Panel Panel = null!;
		public override void _Ready()
		{
			testStatus = GetNode<Label>("Label");
			Panel = GetNode<Panel>("Panel");
		}
		
		public override void _Input(InputEvent input) {
			if(input.IsActionPressed("ui_left")) {
				FinalizerTestNormalCSharpClass();
				GC.Collect();
				GC.WaitForPendingFinalizers();
				testStatus.Text = "Done!";
			} 
		}

		private void nullTest() {
			//does not do anything
			//result: no memleaks reported
		}

		private void CreateNodeLoseReference() {
			Node node = new Node();
			//result: ObjectDB instances leaked at exit
		}

		Node nodeReference = null!;
		private void CreateNodeKeepReference() {
			Node node = new Node();
			nodeReference = node;
			//result: ObjectDB instances leaked at exit
		}

		private void CreateNodeFreeNode() {
			Node node = new Node();
			node.QueueFree();
			//result: no leaks reported
		}

		private void CreateNodeAddNodeAsChild() {
			Node node = new Node();
			this.AddChild(node);
			//result: no leaks reported
		}

		//tests if the Deconstructor is called if a node is referenced only by the scene tree
		private void FinalizerTest() {
			Node node = new TestNodeWrapper();
			this.AddChild(node);
			//result: no leaks reported, and the finalizer is not called
		}

		private void FinalizerTestLoseReference() {
			Node node = new TestNodeWrapper();
			//result: no leaks reported, and the !!!FINALIZER IS NOT CALLED!!!
			//TODO: how do I call the finalizer here?
		}

		private void FinalizerTestNormalCSharpClass() {
			PureCSharpClassTest test = new PureCSharpClassTest();
			//result: no leaks reported, and the finalizer is called
		}
	}
}
