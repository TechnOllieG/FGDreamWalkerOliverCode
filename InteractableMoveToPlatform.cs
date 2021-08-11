using System.Collections;
using GP2_Team7.Objects.Characters;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace GP2_Team7.Objects
{
	using Libraries;
	
	[InteractOn(Interaction.KeyDown), DefaultExecutionOrder(1)]
	public class InteractableMoveToPlatform : Interactable
	{
        public static System.Action<InteractableMoveToPlatform, MoveToPlatformData> onInteracted;
        
        [Header("Platform specific settings")]
        
        [Tooltip("The speed curve of the players movement")]
        public AnimationCurve speedCurve;

        public Vector3 destinationOffset;

        [Tooltip("")] public Vector3 playerBezierTangent;
        public Vector3 platformBezierTangent;

        [Header("Debug")]
        public bool showMovementArc;
        public Color movementArcColor = Color.red;
        
        #if UNITY_EDITOR
        [HideInInspector] public Tool oldTool;
        [HideInInspector] public bool toolHasBeenSet;
        #endif
        
        private static bool isCurrentlyMoving = false;
        private PlayerCharacter _characterScript;
        private Transform _transform;

        public Vector3 TargetPosition
		{
			get
			{
				if (_transform == null)
					_transform = transform;
				
				return _transform.position + destinationOffset;
			}
		}
		
		protected override void Awake()
		{
			base.Awake();
			_transform = transform;
			
			_characterScript = player.GetComponent<PlayerCharacter>();

			if (speedCurve.length == 0)
			{
				speedCurve = AnimationCurve.EaseInOut(0f, 0f, 2f, 1.01f);
				Debug.LogWarning("Animation curve is not set explicitly in this move to platform, reverting to default placeholder animation", gameObject);
			}
		}

		public override void Interact()
		{
			if (isCurrentlyMoving)
				return;

			isCurrentlyMoving = true;
			MoveToPlatformData moveToPlatformData = new MoveToPlatformData(transform, destinationOffset, speedCurve, playerBezierTangent, platformBezierTangent, StoppedMoving);
            onInteracted(this, moveToPlatformData);
		}

		private void StoppedMoving() => isCurrentlyMoving = false;

		public override bool IsCurrentlyInteractable()
		{
			if (base.IsCurrentlyInteractable())
			{
				if (isCurrentlyMoving)
					return false;

				return true;
			}
			return false;
		}
	}
}