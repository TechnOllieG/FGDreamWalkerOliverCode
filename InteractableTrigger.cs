using GP2_Team7.Objects.Characters;
using UnityEngine;

namespace GP2_Team7.Objects
{
	using Objects;

	public class InteractableTrigger : Interactable
	{
		public bool onlyDetectPlayer = false;
		
        [SerializeField]
        private Conditionizer _conditionizer;

        public override void Interact()
		{
			
		}

		private void OnTriggerEnter(Collider other)
		{
			PlayerCharacter character = other.GetComponent<PlayerCharacter>();

			if (character == null && onlyDetectPlayer)
				return;
			
			if(enabled && (_conditionizer == null || _conditionizer.AllTrue()))
				base.Interact();
		}

		public override bool IsCurrentlyInteractable()
		{
			return false;
		}

#if UNITY_EDITOR
        [ContextMenu("Add Condition")]
        public void AddCondition()
        {
            _conditionizer.AddCondition();
        }
#endif
    }
}