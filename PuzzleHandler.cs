using System;
using System.Linq;
using UnityEngine;

namespace GP2_Team7.Managers
{
	using Objects;
	
	[DefaultExecutionOrder(-1), RequireComponent(typeof(InteractableEventSystem))]
	public class PuzzleHandler : MonoBehaviour
	{
		[Tooltip("Assign any interactable object(s) in here that must be interacted with to clear the puzzle")]
		public Interactable[] inputs;

		[Header("Debug")]
		
		[Tooltip("The array of booleans that all have to be true for the puzzle to be marked as completed (one boolean per interactable)")]
		public bool[] puzzleConditions;
		
		[SerializeReference] public Action<PuzzleHandlerTriggerMode> outputAction;

		private bool _enabledOutputObjects = false;

		protected virtual void Awake()
		{
			int id = 0;
			
			foreach (Interactable inter in inputs)
			{
				inter.id = id;
				inter.eventToTrigger += Interacted;
				id++;
			}

			puzzleConditions = new bool[inputs.Length];
		}

		private void OnDisable()
		{
			foreach (Interactable inter in inputs)
			{
				inter.eventToTrigger -= Interacted;
			}
		}

		public void Interacted(int id)
		{
			puzzleConditions[id] = !puzzleConditions[id];

			if (puzzleConditions.All(a => a))
			{
				if (!_enabledOutputObjects)
				{
					outputAction?.Invoke(PuzzleHandlerTriggerMode.OnEnableOutput);
					_enabledOutputObjects = true;
				}
			}
			else if(_enabledOutputObjects)
			{
				outputAction?.Invoke(PuzzleHandlerTriggerMode.OnDisableOutput);
				_enabledOutputObjects = false;
			}
		}
	}
}
