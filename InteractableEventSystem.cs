using System;
using System.Collections;
using FMODUnity;
using GP2_Team7.Objects;
using GP2_Team7.Objects.Cameras;
using GP2_Team7.Objects.Characters;
using GP2_Team7.Objects.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GP2_Team7.Managers
{
	public enum InteractionEventType
	{
		EnableDisableObjects,
		EnableDisableScripts,
		EnableDisablePortal,
		SetAnimationProperty,
		DeleteObjects,
		DeleteScripts,
		SpawnObjects,
		MoveObjects,
		Log,
		TriggerCameraCutscene,
		ChangeMaterial,
		EnableDisableMouseLock,
		EnableDisablePlayerMovement,
		OpenDiary,
		ShowSubtitle,
		EnableFlickerLights,
		ChangeRespawnPoint,
		SwitchScene,
		EnableDisableCrosshair,
		StopAllMusic,
		ClearInventory
	}

	public enum EnableDisableMode
	{
		Enable,
		Disable,
		Toggle
	}

	public enum PuzzleHandlerTriggerMode
	{
		OnEnableOutput,
		OnDisableOutput,
		Both,
		OnReset
	}

	public enum AnimationPropertyType
	{
		SetTrigger,
		SetBool,
		SetFloat,
		SetInteger,
		Play
	}

	[Serializable]
	public class InteractionEvent
	{
		public string eventName;
		public InteractionEventType eventType;
		public EnableDisableMode mode;
		public Object[] relevantObjects;
		public bool delayed;
		public float delayInSecs;

		public Vector3 position;
		public Vector3 rotation;
		public float lerpSpeed;

		public Animator targetAnimation;
		public AnimationPropertyType animPropertyType;
		[FormerlySerializedAs("animationPropertyName")] public string contextualName;
		public bool animationBoolToSet;
		public float animationFloatToSet;
		public int animationIntToSet;
		public string message;

		public CamFixedViewSettings camSettings;
		public float duration;

		public Material[] materialsToChangeTo;
		public bool lerpMaterial;
		public bool parentCameraToPlayer;
		public bool changePlayerState;
		public Color subtitleColor = Color.white;
		public Transform respawnPoint;
		
		#if UNITY_EDITOR
		public bool foldout;
		#endif
	}

	public class InteractableEventSystem : MonoBehaviour
	{
		public Interactable interactable;
		public bool disableAfterRunning = false;
		public InteractionEvent[] events = new InteractionEvent[0];
		public Button button;

		public bool mainFoldoutBool = false;
		public bool[] eventFoldouts = new bool[0];
		public bool[] eventObjectFoldouts = new bool[0];
		public bool[] eventMaterialFoldouts = new bool[0];
		
		public PuzzleHandler puzzleHandler;
		public bool runEventsOnEnable;

		private bool _hasRun = false;
		private PuzzleHandlerTriggerMode triggerMode = PuzzleHandlerTriggerMode.OnEnableOutput;

		private void OnEnable()
		{
			if (runEventsOnEnable)
			{
				TriggerEvents();
				return;
			}

			if (puzzleHandler != null)
			{
				puzzleHandler.outputAction += TriggerEventsFromPuzzleHandler;
				return;
			}

			if(interactable != null)
				interactable.eventToTrigger += TriggerEvents;

			if (button != null)
				button.onClick.AddListener(TriggerEvents);
		}

		private void OnDisable()
		{
			if(interactable != null)
				interactable.eventToTrigger -= TriggerEvents;
			
			if (puzzleHandler != null)
				puzzleHandler.outputAction -= TriggerEventsFromPuzzleHandler;
			
			if (button != null)
				button.onClick.RemoveListener(TriggerEvents);
		}

		public void TriggerEventsFromPuzzleHandler(PuzzleHandlerTriggerMode mode)
		{
			if (triggerMode == PuzzleHandlerTriggerMode.Both)
			{
				StartCoroutine(TriggerEventsCoroutine());
				return;
			}

			if (mode == triggerMode)
			{
				StartCoroutine(TriggerEventsCoroutine());
				return;
			}

			// Just adding this because I couldn't find a better way
			// to reset a puzzle... --Justus
			if (mode == PuzzleHandlerTriggerMode.OnReset)
			{
				_hasRun = false;
			}
		}

		public void TriggerEvents(int id)
		{
			StartCoroutine(TriggerEventsCoroutine());
		}

		public void TriggerEvents()
		{
			TriggerEvents(0);
		}
		
		private IEnumerator TriggerEventsCoroutine()
		{
			if (_hasRun && disableAfterRunning)
				yield break;
			
			_hasRun = true;
			
			for (int i = 0; i < events.Length; i++)
			{
				yield return StartCoroutine(TriggerEvent(events[i]));
			}

			if (disableAfterRunning && interactable != null)
			{
				interactable.enabled = false;
			}
		}

		private IEnumerator TriggerEvent(InteractionEvent currentEvent)
		{
			if (currentEvent.delayed)
				yield return new WaitForSeconds(currentEvent.delayInSecs);
			
			switch (currentEvent.eventType)
			{
				case InteractionEventType.EnableDisableObjects:
				{
					if (currentEvent.mode == EnableDisableMode.Enable)
						foreach (GameObject obj in currentEvent.relevantObjects)
						{
							obj.SetActive(true);
						}
					else if (currentEvent.mode == EnableDisableMode.Disable)
						foreach (GameObject obj in currentEvent.relevantObjects)
						{
							obj.SetActive(false);
						}
					else
						foreach (GameObject obj in currentEvent.relevantObjects)
						{
							obj.SetActive(!obj.activeSelf);
						}

					break;
				}
				case InteractionEventType.EnableDisableScripts:
				{
					if (currentEvent.mode == EnableDisableMode.Enable)
						foreach (MonoBehaviour mono in currentEvent.relevantObjects)
						{
							if (mono is Interactable)
							{
								((Interactable) mono).enabled = true;
							}

							mono.enabled = true;
						}
					else if (currentEvent.mode == EnableDisableMode.Disable)
						foreach (MonoBehaviour mono in currentEvent.relevantObjects)
						{
							if (mono is Interactable)
							{
								((Interactable) mono).enabled = false;
							}

							mono.enabled = false;
						}
					else
						foreach (MonoBehaviour mono in currentEvent.relevantObjects)
						{
							if (mono is Interactable)
							{
								var interact = ((Interactable) mono);
								interact.enabled = !interact.enabled;
							}

							mono.enabled = !mono.enabled;
						}

					break;
				}
				case InteractionEventType.SetAnimationProperty:
				{
					Animator animator = currentEvent.targetAnimation;
					string propertyName = currentEvent.contextualName;
					switch (currentEvent.animPropertyType)
					{
						case AnimationPropertyType.SetTrigger:
							animator.SetTrigger(propertyName);
							break;
						case AnimationPropertyType.SetBool:
							animator.SetBool(propertyName, currentEvent.animationBoolToSet);
							break;
						case AnimationPropertyType.SetFloat:
							animator.SetFloat(propertyName, currentEvent.animationFloatToSet);
							break;
						case AnimationPropertyType.SetInteger:
							animator.SetInteger(propertyName, currentEvent.animationIntToSet);
							break;
						case AnimationPropertyType.Play:
							animator.Play(currentEvent.contextualName);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					break;
				}
				case InteractionEventType.DeleteObjects:
				{
					foreach (GameObject obj in currentEvent.relevantObjects)
					{
						Destroy(obj);
					}

					break;
				}
				case InteractionEventType.DeleteScripts:
				{
					foreach (MonoBehaviour mono in currentEvent.relevantObjects)
					{
						Destroy(mono);
					}

					break;
				}
				case InteractionEventType.SpawnObjects:
				{
					foreach (GameObject obj in currentEvent.relevantObjects)
					{
						var currentObj = Instantiate(obj);

						currentObj.transform.position += currentEvent.position;
						currentObj.transform.rotation = Quaternion.Euler(currentEvent.rotation);
					}

					break;
				}
				case InteractionEventType.MoveObjects:
				{
					yield return StartCoroutine(MoveObjects(currentEvent.relevantObjects, currentEvent.position, currentEvent.lerpSpeed));

					break;
				}
				case InteractionEventType.Log:
				{
					Debug.Log(currentEvent.message);
					break;
				}
				case InteractionEventType.EnableDisablePortal:
				{
					if (currentEvent.mode == EnableDisableMode.Enable)
					{
						foreach (Object obj in currentEvent.relevantObjects)
						{
							((PortalParent) obj).TurnPortalOn();
						}
					}
					else if (currentEvent.mode == EnableDisableMode.Disable)
					{
						foreach (Object obj in currentEvent.relevantObjects)
						{
							((PortalParent) obj).TurnPortalOff();
						}
					}
					else
					{
						foreach (Object obj in currentEvent.relevantObjects)
						{
							var portal = (PortalParent) obj;
							
							if(portal.transform.GetChild(0).gameObject.activeSelf)
								portal.TurnPortalOff();
							else
								portal.TurnPortalOn();
						}
					}
					break;
				}
				case InteractionEventType.TriggerCameraCutscene:
				{
					yield return new WaitUntil(() => !CutsceneManager.IsInCutscene);
					CameraController.CutscenedCameraEvent(currentEvent.camSettings, currentEvent.duration);
					break;
				}
				case InteractionEventType.ChangeMaterial:
				{
					if (currentEvent.lerpMaterial)
					{
						yield return StartCoroutine(LerpMaterial(currentEvent.relevantObjects, currentEvent.materialsToChangeTo, currentEvent.lerpSpeed));
					}
					else
					{
						foreach (GameObject obj in currentEvent.relevantObjects)
						{
							MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
							Material[] materials = meshRenderer.materials;
							for (int i = 0; i < currentEvent.materialsToChangeTo.Length; i++)
							{
								if(currentEvent.materialsToChangeTo[i] != null)
									materials[i] = currentEvent.materialsToChangeTo[i];
							}
							meshRenderer.materials = materials;
						}
					}
					break;
				}
				case InteractionEventType.EnableDisableMouseLock:
				{
					if (currentEvent.mode == EnableDisableMode.Enable)
						Cursor.lockState = CursorLockMode.Locked;
					
					else if (currentEvent.mode == EnableDisableMode.Disable)
						Cursor.lockState = CursorLockMode.None;
					
					else
						Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
					
					break;
				}
				case InteractionEventType.EnableDisablePlayerMovement:
				{
					var character = GameManager.Player.GetComponent<PlayerCharacter>();

					if (currentEvent.mode == EnableDisableMode.Disable)
					{
						if(currentEvent.changePlayerState)
							character.LockPlayerMovement();
						CameraController.SwitchToStaticCameraView(currentEvent.parentCameraToPlayer, currentEvent.position, Quaternion.Euler(currentEvent.rotation));
					}
					else if (currentEvent.mode == EnableDisableMode.Enable)
					{
						if(currentEvent.changePlayerState)
							character.UnlockPlayerMovement();
						CameraController.SwitchToStandardCameraView();
					}
					else
					{
						if(currentEvent.changePlayerState)
							character.TogglePlayerMovement();
						
						if(CameraController.CurrentState is CameraController.CS_FixedView)
							CameraController.SwitchToStandardCameraView();
						else
							CameraController.SwitchToStaticCameraView(currentEvent.parentCameraToPlayer, currentEvent.position, Quaternion.Euler(currentEvent.rotation));
					}
					break;
				}
				case InteractionEventType.OpenDiary:
				{
					foreach (Diary diary in currentEvent.relevantObjects)
					{
						bool diaryOpen = true;
						diary.gameObject.SetActive(true);
						diary.closeButton.onClick.AddListener(ClosedDiary);
						yield return new WaitUntil(() => !diaryOpen);
						
						void ClosedDiary()
						{
							diaryOpen = false;
							diary.closeButton.onClick.RemoveListener(ClosedDiary);
						}
					}
					break;
				}
				case InteractionEventType.ShowSubtitle:
				{
					SubtitleManager.DrawSubtitles(currentEvent.message, currentEvent.duration, currentEvent.subtitleColor);
					yield return new WaitForSeconds(currentEvent.duration);
					break;
				}
				case InteractionEventType.EnableFlickerLights:
				{
					foreach (BuildingLightSwitch obj in currentEvent.relevantObjects)
					{
						obj.PowerOn = true;
					}
				}
					break;
				case InteractionEventType.ChangeRespawnPoint:
				{
					GameManager.Instance.respawnPoint = currentEvent.respawnPoint;
					break;
				}
				case InteractionEventType.SwitchScene:
				{
					StudioEventEmitter[] emitters = FindObjectsOfType<StudioEventEmitter>();
					foreach (StudioEventEmitter emitter in emitters)
					{
						emitter.EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
					}
					
					SceneManager.LoadScene(currentEvent.contextualName);
					break;
				}
				case InteractionEventType.EnableDisableCrosshair:
				{
					InteractableRaycaster raycaster = GameManager.Player.GetComponent<InteractableRaycaster>();
					if (currentEvent.mode == EnableDisableMode.Disable) raycaster.SetCrosshairState(false);
					else if (currentEvent.mode == EnableDisableMode.Enable) raycaster.SetCrosshairState(true);
					else
					{
						if (raycaster.crosshair.gameObject.activeSelf)
							raycaster.SetCrosshairState(false);
						else
							raycaster.SetCrosshairState(true);
					}
					break;
				}
				case InteractionEventType.StopAllMusic:
				{
					StudioEventEmitter[] emitters = FindObjectsOfType<StudioEventEmitter>();
					foreach (StudioEventEmitter emitter in emitters)
					{
						emitter.EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
					}
					break;
				}
				case InteractionEventType.ClearInventory:
				{
					GameManager.Inventory.ClearInventory();
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}

			yield return null;
		}

		private IEnumerator MoveObjects(Object[] objects, Vector3 relativeDestination, float speed)
		{
			float t = 0f;
			Vector3[] origins = new Vector3[objects.Length];
			Vector3[] targetPositions = new Vector3[objects.Length];

			for (int i = 0; i < objects.Length; i++)
			{
				origins[i] = ((GameObject) objects[i]).transform.position;
				targetPositions[i] = origins[i] + relativeDestination;
			}

			while (t < 1f)
			{
				t += speed * Time.deltaTime;
				for (int i = 0; i < objects.Length; i++)
				{
					((GameObject) objects[i]).transform.position = Vector3.Lerp(origins[i], targetPositions[i], t);
				}
				yield return null;
			}
		}
		
		private IEnumerator LerpMaterial(Object[] objects, Material[] targetMaterials, float speed)
		{
			float t = 0f;
			MeshRenderer[] renderers = new MeshRenderer[objects.Length];
			Material[] originMaterials = new Material[objects.Length];

			for (int i = 0; i < objects.Length; i++)
			{
				renderers[i] = ((GameObject) objects[i]).GetComponent<MeshRenderer>();
				originMaterials[i] = renderers[i].material;
			}

			while (t < 1f)
			{
				t += speed * Time.deltaTime;
				for (int i = 0; i < objects.Length; i++)
				{
					Material[] materials = renderers[i].materials;
					foreach (Material mat in targetMaterials)
					{
						materials[i].Lerp(originMaterials[i], mat, t);
					}
					renderers[i].materials = materials;
				}
				yield return null;
			}
		}
	}
}