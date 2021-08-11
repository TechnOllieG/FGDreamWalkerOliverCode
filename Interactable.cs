using System;
using GP2_Team7.Objects.Characters;
using UnityEngine;

namespace GP2_Team7.Objects
{
	using Managers;
	
	[RequireComponent(typeof(Collider))]
	public abstract class Interactable : MonoBehaviour
	{
		public new bool enabled = true;
		
		[Tooltip("The gameobject that should be enabled when player is looking at the object " +
		         "(and disabled when player looks away), could be a canvas or just an object for example")]
		public GameObject hint;

		public bool playInteractSound;

		[Tooltip("The game object that should be outlined when looking at this interactable")]
		public Transform outlineObject;

		[Tooltip("These materials will be applied to the mesh renderer on this object or it's children (first that is found) when the interactable is \"Disabled\"")]
		public Material[] disabledMaterials = new Material[0];

		[Tooltip("These materials will be applied to the mesh renderer on this object or it's children (first that is found) when the interactable is \"Disabled\"")]
		public Material[] enabledMaterials = new Material[0];
		
		[Tooltip("This is the mesh renderer that will be used with the disabled/enabled materials")]
		public MeshRenderer meshRendererToApplyMaterialsTo;
		
		[Tooltip("If the player is within this distance of the destination platform it will not be interactable")]
		public float nearDistance = 0.5f;
		
		[Tooltip("The player can only interact with this platform if it is within this range")]
		public float playerRange = 50f;

		[NonSerialized]
		public int id = 0; // Only used when the interactable is connected to a puzzle manager (doesn't have to be)

		[Header("Base class Debug")]
		public bool nearDistanceGizmo = false;
		public bool playerRangeGizmo = false;
		public bool gizmoLineBetweenPlayerAndPlatform = false;
		public Color playerRangeColor = Color.green;
		public Color nearDistanceColor = Color.red;
		public Color gizmoLineColor = Color.cyan;
		
		
		// Event that is primarily used to switch the state of this interactable's boolean in the boolean array in a puzzle manager or to trigger a interactable event system
		[NonSerialized] public Action<int> eventToTrigger;

		private bool _materialState = false; // This will signify when the material should show off/on respectively
		protected GameObject player;
		protected PlayerCharacter playerScript;

#if UNITY_EDITOR
		protected virtual void OnDrawGizmos()
		{
			if (player == null)
				player = GameManager.Player;
			
			Vector3 targetPos = transform.position;
			
			if (nearDistanceGizmo)
			{
				Gizmos.color = nearDistanceColor;
				Gizmos.DrawWireSphere(targetPos, nearDistance);
			}

			if (playerRangeGizmo)
			{
				Gizmos.color = playerRangeColor;
				Gizmos.DrawWireSphere(targetPos, playerRange);
			}

			if (gizmoLineBetweenPlayerAndPlatform)
			{
				Gizmos.color = gizmoLineColor;
				Gizmos.DrawLine(player.transform.position, targetPos);
			}
			
			Gizmos.color = Color.white;
		}
#endif

		protected virtual void Awake()
		{
			player = GameManager.Player;
			playerScript = player.GetComponent<PlayerCharacter>();

			if(hint != null)
				hint.SetActive(false);
			
			if(meshRendererToApplyMaterialsTo == null)
				meshRendererToApplyMaterialsTo = GetComponentInChildren<MeshRenderer>();

			if ((disabledMaterials.Length == 0 || Array.TrueForAll(disabledMaterials, a => a == null)) && meshRendererToApplyMaterialsTo != null)
			{
				disabledMaterials = meshRendererToApplyMaterialsTo.materials;
			}
			
			if (meshRendererToApplyMaterialsTo != null && disabledMaterials.Length > 0 && enabledMaterials.Length > 0)
			{
				Material[] materials = meshRendererToApplyMaterialsTo.materials;
				for (int i = 0; i < disabledMaterials.Length && i < meshRendererToApplyMaterialsTo.materials.Length; i++)
				{
					if (disabledMaterials[i] != null)
					{
						materials[i] = disabledMaterials[i];
					}
				}
				meshRendererToApplyMaterialsTo.materials = materials;
			}
		}

		public virtual void Interact()
		{
			if (playInteractSound)
			{
				playerScript.PlayInteractSound();
			}
			
			eventToTrigger?.Invoke(id);
			if (meshRendererToApplyMaterialsTo != null && disabledMaterials.Length > 0 && enabledMaterials.Length > 0)
			{
				Material[] materials = meshRendererToApplyMaterialsTo.materials;
				if (_materialState)
				{
					for (int i = 0; i < disabledMaterials.Length && i < meshRendererToApplyMaterialsTo.materials.Length; i++)
					{
						if(disabledMaterials[i] != null)
							materials[i] = disabledMaterials[i];
					}
				} 
				
				if (!_materialState)
				{
					for (int i = 0; i < enabledMaterials.Length && i < meshRendererToApplyMaterialsTo.materials.Length; i++)
					{
						if(enabledMaterials[i] != null)
							materials[i] = enabledMaterials[i];
					}
				}
				meshRendererToApplyMaterialsTo.materials = materials;
				_materialState = !_materialState;
			}
		}

		public virtual bool IsCurrentlyInteractable()
		{
			if (enabled)
			{
				float sqrDistanceToPlayer = (GameManager.Player.transform.position - transform.position).sqrMagnitude;
				
				return sqrDistanceToPlayer > nearDistance * nearDistance && sqrDistanceToPlayer < playerRange * playerRange;
			}
			return false;
		}

		[AttributeUsage(AttributeTargets.Class)]
		public class InteractOn : Attribute
		{
			private Interaction[] modes;

			/// <summary>
			/// Specifies when this class's Interact method should be called based on the input interaction mode
			/// </summary>
			public InteractOn(Interaction mode)
			{
				modes = new[] {mode};
			}

			/// <summary>
			/// Specifies when this class's Interact method should be called based on the two input interaction modes
			/// </summary>
			public InteractOn(Interaction mode1, Interaction mode2)
			{
				modes = new[] {mode1, mode2};
			}

			public Interaction[] GetModes()
			{
				return modes;
			}
		}
	}
	
	public enum Interaction
	{
		KeyDown,
		KeyUp
	}
}
