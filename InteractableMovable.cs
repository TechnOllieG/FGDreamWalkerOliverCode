using System;
using System.Collections;
using UnityEngine;

namespace GP2_Team7.Objects
{
	public enum MoveOrder
	{
		XZY,
		ZXY
	}
	
	[InteractOn(Interaction.KeyDown)]
	public class InteractableMovable : Interactable
	{
		[Header("Movable specific")] 
		
		public SoundEmitter emitter;

		public string nameOfObjectWithEvent = "Move";
		[Tooltip("All possible positions this movable can have, will first try to move to the state with index 0, if occupied it will try index 1 etc.")]
		public MovableState[] possiblePositions = new MovableState[0];
		[Tooltip("When moving this object, in what order of axes should it move in")]
		public MoveOrder moveOrder;
		[Tooltip("This value will be added to the interpolator * Time.deltaTime (higher value = faster)")]
		public float moveSpeed = 0.1f;
		[Tooltip("The object will make sure to move to a y level which is above the current objects scaled mesh bounds (so it will be above other objects using the same mesh when moving), this padding will further add to that y level if extra space is preferred")]
		public float yPadding = 0.5f;
		[Tooltip("Whether the object should start in one of the possible positions (will teleport to the closest state at start of game)")]
		public bool startInValidState;
		[Tooltip("The state the object should move to that does not trigger the event (will only be used when Start in valid state is true)")]
		public Transform baseState;
		
		public bool onlyMoveIfRaycastUpFails = false;
		public float raycastLength = 2f;

		[Tooltip("This refers to the index (of the possible positions array) of the correct movable state for this interactable to send a success event to the puzzle manager, set to a negative value to never send an event to any puzzle managers")]
		public int correctState = 0;

		[Header("Debug")]
		
		public bool debugRaycast = false;

		public Color raycastColor = Color.cyan;

		private MeshFilter _meshFilter;
		private Mesh _mesh;
		private Vector3 _originalPosition;
		private Transform _transform;
		private MoveOrder _currentMoveOrder;
		private bool _currentlyMoving = false;
		private bool _currentlyInCorrectPosition = false;

		private MovableState _currentState = null;

		private const float MaximumDistance = 0.001f; // The maximum distance this object can be from the target for the interpolation to be overridden altogether

		private void OnValidate()
		{
			if (correctState > possiblePositions.Length - 1)
				correctState = possiblePositions.Length - 1;
		}
		
		#if UNITY_EDITOR
		protected override void OnDrawGizmos()
		{
			base.OnDrawGizmos();
			
			Vector3 pos = transform.position;
			Gizmos.color = raycastColor;
			if (debugRaycast && onlyMoveIfRaycastUpFails)
			{
				Gizmos.DrawLine(pos, pos + raycastLength * Vector3.up);
			}
			Gizmos.color = Color.white;
		}
		#endif

		protected override void Awake()
		{
			base.Awake();
			_transform = transform;
			_meshFilter = GetComponentInChildren<MeshFilter>();
			_mesh = _meshFilter.sharedMesh;
		}

		private void Start()
		{
			if (startInValidState)
			{
				if (possiblePositions.Length == 0)
				{
					Debug.LogError("No possible points have been assigned in array", transform);
					return;
				}

				_originalPosition = baseState.position;
				Vector3 currentPosition = _transform.position;

				float closestDistance = float.MaxValue;
				int indexOfClosestState = -1;
				for (int i = 0; i < possiblePositions.Length; i++)
				{
					float distance = (possiblePositions[i].transform.position - currentPosition).sqrMagnitude;
					if (distance < closestDistance && !possiblePositions[i].pointOccupied)
					{
						closestDistance = distance;
						indexOfClosestState = i;
					}
				}

				if (indexOfClosestState == -1)
					throw new Exception("This shouldn't happen, why did it?");
				
				_currentState = possiblePositions[indexOfClosestState];
				_currentState.pointOccupied = true;
				_transform.position = _currentState.transform.position;
				
				if(correctState >= 0 && indexOfClosestState == correctState)
				{
					base.Interact();
					_currentlyInCorrectPosition = true;
				}
			}
			else
			{
				_originalPosition = _transform.position;
			}
		}

		public override bool IsCurrentlyInteractable()
		{
			if (base.IsCurrentlyInteractable())
			{
				if (onlyMoveIfRaycastUpFails)
					return !Physics.Raycast(transform.position, Vector3.up, raycastLength);

				return true;
			}
			return false;
		}

		public override void Interact()
		{
			if (_currentlyMoving)
				return;

			Vector3 targetPos;
			if (_currentState == null)
			{
				_currentMoveOrder = moveOrder;
				MovableState targetPoint = null;
				for (int i = 0; i < possiblePositions.Length; i++)
				{
					if (possiblePositions[i].pointOccupied)
						continue;

					targetPoint = possiblePositions[i];
					break;
				}

				if (targetPoint == null)
				{
					Debug.LogWarning($"No valid position to move to, please assign GameObjects with the script {possiblePositions.GetType()} in {this}");
					return;
				}

				targetPoint.pointOccupied = true;
				targetPos = targetPoint.transform.position;
				_currentState = targetPoint;
			}
			else
			{
				_currentMoveOrder = moveOrder == MoveOrder.XZY ? MoveOrder.ZXY : MoveOrder.XZY;
				_currentState.pointOccupied = false;
				_currentState = null;
				targetPos = _originalPosition;
			}
			
			_currentlyMoving = true;
			StartCoroutine(Move(targetPos));
		}

		private IEnumerator Move(Vector3 position)
		{
			if(emitter != null)
				emitter.Play(nameOfObjectWithEvent);
			
			float interpolator = 0f;
			Vector3 originPosition = _transform.position;
			
			float highY = CalculateYHighPoint();
			float originY = originPosition.y;
			while (MoveByComponent(originY, highY, out float currentComponent))
			{
				_transform.position = new Vector3(originPosition.x, currentComponent, originPosition.z);
				yield return null;
			}
			
			switch(_currentMoveOrder)
			{
				case MoveOrder.XZY:
				{
					yield return StartCoroutine(MoveX());
					yield return StartCoroutine(MoveZ());
					break;
				}
				case MoveOrder.ZXY:
				{
					yield return StartCoroutine(MoveZ());
					yield return StartCoroutine(MoveX());
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}

			yield return StartCoroutine(MoveY());
			_currentlyMoving = false;
			if(emitter != null)
				emitter.TriggerCue(nameOfObjectWithEvent);
			
			if ((correctState >= 0 && Array.FindIndex(possiblePositions, a => a == _currentState) == correctState) || _currentlyInCorrectPosition)
			{
				base.Interact();
				_currentlyInCorrectPosition = !_currentlyInCorrectPosition;
			}

			IEnumerator MoveX()
			{
				originPosition = _transform.position;
				bool keepGoing = true;
				while (keepGoing)
				{
					keepGoing = MoveByComponent(originPosition.x, position.x, out float currentComponent);
					_transform.position = new Vector3(currentComponent, originPosition.y, originPosition.z);
					yield return null;
				}
			}

			IEnumerator MoveY()
			{
				originPosition = _transform.position;
				bool keepGoing = true;
				while (keepGoing)
				{
					keepGoing = MoveByComponent(originPosition.y, position.y, out float currentComponent);
					_transform.position = new Vector3(originPosition.x, currentComponent, originPosition.z);
					yield return null;
				}
			}

			IEnumerator MoveZ()
			{
				originPosition = _transform.position;
				bool keepGoing = true;
				while (keepGoing)
				{
					keepGoing = MoveByComponent(originPosition.z, position.z, out float currentComponent);
					_transform.position = new Vector3(originPosition.x, originPosition.y, currentComponent);
					yield return null;
				}
			}

			float CalculateYHighPoint()
			{
				float meshExtentsYWorld = _mesh.bounds.extents.y * _transform.localScale.y;
				float highPoint;

				if (_transform.position.y < position.y)
					highPoint = position.y;
				else
					highPoint = _transform.position.y;

				return highPoint + meshExtentsYWorld + yPadding;
			}
			
			bool MoveByComponent(float originComponent,  float destinationComponent, out float currentComponent)
			{
				currentComponent = Mathf.Lerp(originComponent, destinationComponent, interpolator);
				if (Mathf.Abs(currentComponent - destinationComponent) > MaximumDistance)
				{
					interpolator += moveSpeed * Time.deltaTime;
					return true;
				}

				currentComponent = destinationComponent;
				interpolator = 0f;
				return false;
			}
		}
	}
}