using System;
using GP2_Team7.Objects.Cameras;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace GP2_Team7.EditorScripts
{
	using Objects;
	using Managers;
	
	[CustomEditor(typeof(InteractableEventSystem))]
	public class InteractableEventSystemEditor : EditorLib<InteractableEventSystem>
	{
		private SerializedProperty _interactable;
		private SerializedProperty _puzzleHandler;
		private SerializedProperty _button;
		private SerializedProperty _runEventsOnEnable;
		private SerializedProperty _disableAfterRunning;
		private SerializedProperty _events;

		private SerializedProperty _mainFoldoutBool;
		private SerializedProperty _eventFoldouts;
		private SerializedProperty _eventObjectFoldouts;
		private SerializedProperty _eventMaterialFoldouts;

		private void OnEnable()
		{
			_interactable = serializedObject.FindProperty(nameof(InteractableEventSystem.interactable));
			_puzzleHandler = serializedObject.FindProperty(nameof(InteractableEventSystem.puzzleHandler));
			_button = serializedObject.FindProperty(nameof(InteractableEventSystem.button));
			_runEventsOnEnable = serializedObject.FindProperty(nameof(InteractableEventSystem.runEventsOnEnable));
			_disableAfterRunning = serializedObject.FindProperty(nameof(InteractableEventSystem.disableAfterRunning));
			_events = serializedObject.FindProperty(nameof(InteractableEventSystem.events));
			
			_mainFoldoutBool = serializedObject.FindProperty(nameof(InteractableEventSystem.mainFoldoutBool));
			_eventFoldouts = serializedObject.FindProperty(nameof(InteractableEventSystem.eventFoldouts));
			_eventObjectFoldouts = serializedObject.FindProperty(nameof(InteractableEventSystem.eventObjectFoldouts));
			_eventMaterialFoldouts = serializedObject.FindProperty(nameof(InteractableEventSystem.eventMaterialFoldouts));
		}

		public override void OnInspectorGUI()
		{
			Init();
			
			ObjectField<Interactable>(
				new GUIContent("Interactable",
					"The interactable that should trigger this event system"), _interactable, 0,
				250, true);

			ObjectField<PuzzleHandler>(new GUIContent("Puzzle Handler", "Puzzle Handler that should trigger this event system"), _puzzleHandler, 0, 250, true);

			ObjectField<Button>(new GUIContent("Button", "A UI button that will trigger the event system on click"),
				_button, 0, 250, true);

			BoolField(
				new GUIContent("Run events on enable",
					"Instead of running events when a interactable/puzzle handler object sends an event, this option will instead run the events as soon as this event system is enabled"),
				_runEventsOnEnable);

			if (scriptInstance.interactable == null)
			{
				scriptInstance.interactable = scriptInstance.GetComponent<Interactable>();
			}

			if (scriptInstance.interactable != null && scriptInstance.puzzleHandler != null)
			{
				if (scriptInstance.GetComponent<Interactable>() != null)
				{
					Debug.LogWarning("There cannot be both an Interactable and a PuzzleHandler on the same object as this event system, destroying Interactable script...");
					DestroyImmediate(scriptInstance.interactable);
				}
				else
				{
					Debug.LogWarning("There is a Interactable script referenced to this event system when a PuzzleHandler is already referenced, dereferencing interactable script...");
					scriptInstance.interactable = null;
				}
			}

			BoolField(new GUIContent("Disable After Running", 
					"If the interactable object and this event system should be disabled after running"), 
				_disableAfterRunning);
			
			Space();
			
			Foldout(new GUIContent("Events", 
				"The events that should be run when the selected interactable's event is triggered"), _mainFoldoutBool);

			if (_mainFoldoutBool.boolValue)
			{
				for (int i = 0; i < _events.arraySize; i++)
				{
					Space();

					SerializedProperty currentEvent = _events.GetArrayElementAtIndex(i);
					SerializedProperty currentEventName =
						currentEvent.FindPropertyRelative(nameof(InteractionEvent.eventName));
					SerializedProperty objects =
						currentEvent.FindPropertyRelative(nameof(InteractionEvent.relevantObjects));

					if (_eventFoldouts.arraySize != _events.arraySize)
						_eventFoldouts.arraySize = _events.arraySize;

					if (_eventObjectFoldouts.arraySize != _events.arraySize)
						_eventObjectFoldouts.arraySize = _events.arraySize;

					if (_eventMaterialFoldouts.arraySize != _events.arraySize)
						_eventMaterialFoldouts.arraySize = _events.arraySize;

					SerializedProperty currentFoldoutBool = _eventFoldouts.GetArrayElementAtIndex(i);

					using (Horizontal)
					{
						string typeIdentifier = GetTypeIdentifier(currentEvent);

						Indent();
						Foldout(new GUIContent($"Event {i} ({typeIdentifier})"), currentFoldoutBool);

						StringField(
							new GUIContent("Name",
								"The name of the event (does nothing logically, only makes it easier to organize in the inspector)"),
							currentEventName, FieldMode.Instant, 0, 40);

						if (_events.arraySize > 1 && i != _events.arraySize - 1)
							if (Button(new GUIContent("Down", "Moves this event down"), 45))
							{
								var current = scriptInstance.events[i];
								scriptInstance.events[i] = scriptInstance.events[i + 1];
								scriptInstance.events[i + 1] = current;
							}

						if (i != 0)
							if (Button(new GUIContent("Up", "Moves this event up"), 30))
							{
								var current = scriptInstance.events[i];
								scriptInstance.events[i] = scriptInstance.events[i - 1];
								scriptInstance.events[i - 1] = current;
							}

						if (Button("Remove", 60))
						{
							_events.DeleteArrayElementAtIndex(i);
							_eventFoldouts.DeleteArrayElementAtIndex(i);
							_eventObjectFoldouts.DeleteArrayElementAtIndex(i);
							_eventMaterialFoldouts.DeleteArrayElementAtIndex(i);
							ApplyProperties();
							return;
						}
					}

					if (currentFoldoutBool.boolValue)
					{
						Title("General Event Settings", null, false, 2);
						ApplyProperties();

						SerializedProperty currentEventType =
							currentEvent.FindPropertyRelative(nameof(InteractionEvent.eventType));
						SerializedProperty currentDelayedBool =
							currentEvent.FindPropertyRelative(nameof(InteractionEvent.delayed));
						SerializedProperty currentDelayInSecs =
							currentEvent.FindPropertyRelative(nameof(InteractionEvent.delayInSecs));

						EnumField<InteractionEventType>(new GUIContent("Event Type", "The current event's type"),
							currentEventType, 2);

						if (serializedObject.hasModifiedProperties)
						{
							objects.ClearArray();
							serializedObject.ApplyModifiedProperties();
						}

						BoolField(
							new GUIContent("Delayed", "If this event should run after a specified amount of seconds"),
							currentDelayedBool, 2);

						if (currentDelayedBool.boolValue)
							FloatField(new GUIContent("Delay in secs"), currentDelayInSecs, FieldMode.Instant, 2);

						Title("Type specific settings", null, true, 2);

						switch ((InteractionEventType) currentEventType.enumValueIndex)
						{
							case InteractionEventType.EnableDisableObjects:
							{
								SerializedProperty currentEnableDisableMode =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.mode));

								EnumField<EnableDisableMode>(new GUIContent("Mode",
									"Whether the object should be enabled or disabled"), currentEnableDisableMode, 2);

								ShowObjectArray<GameObject>(i);
								break;
							}
							case InteractionEventType.EnableDisableScripts:
							{
								SerializedProperty currentEnableDisableMode =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.mode));

								EnumField<EnableDisableMode>(new GUIContent("Mode",
									"Whether the object should be enabled or disabled"), currentEnableDisableMode, 2);

								ShowObjectArray<MonoBehaviour>(i);
								break;
							}
							case InteractionEventType.SetAnimationProperty:
							{
								SerializedProperty currentAnimator =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.targetAnimation));

								SerializedProperty currentAnimationPropType =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.animPropertyType));

								SerializedProperty currentPropertyName =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.contextualName));

								ObjectField<Animator>(new GUIContent("Target Animation",
										"The targeted animator component to apply properties to"), currentAnimator, 2,
									250,
									true);

								EnumField<AnimationPropertyType>(new GUIContent("Animation Property Type",
									"What type of property you want to set"), currentAnimationPropType, 2);

								switch ((AnimationPropertyType) currentAnimationPropType.enumValueIndex)
								{
									case AnimationPropertyType.SetTrigger:
									{
										StringField(new GUIContent("Trigger Name", "The name of the animation trigger"),
											currentPropertyName, FieldMode.Instant, 2);
										break;
									}
									case AnimationPropertyType.SetBool:
									{
										StringField(
											new GUIContent("Bool Property Name", "The name of the animation property"),
											currentPropertyName, FieldMode.Instant, 2);

										SerializedProperty currentBoolProperty =
											currentEvent.FindPropertyRelative(
												nameof(InteractionEvent.animationBoolToSet));

										BoolField(new GUIContent("Bool Property Value"), currentBoolProperty, 2);
										break;
									}
									case AnimationPropertyType.SetFloat:
									{
										StringField(
											new GUIContent("Float Property Name", "The name of the animation property"),
											currentPropertyName, FieldMode.Instant, 2);

										SerializedProperty currentFloatProperty = currentEvent.FindPropertyRelative(
											nameof(InteractionEvent.animationFloatToSet));

										FloatField(new GUIContent("Float Property Value"), currentFloatProperty,
											FieldMode.Instant, 2);
										break;
									}
									case AnimationPropertyType.SetInteger:
									{
										StringField(
											new GUIContent("Integer Property Name",
												"The name of the animation property"),
											currentPropertyName, FieldMode.Instant, 2);

										SerializedProperty currentIntProperty =
											currentEvent.FindPropertyRelative(
												nameof(InteractionEvent.animationIntToSet));

										IntField(new GUIContent("Int Property Value"), currentIntProperty,
											FieldMode.Instant, 2);
										break;
									}
									case AnimationPropertyType.Play:
									{
										StringField(
											new GUIContent("Animation State Name",
												"The name of the animation state to play"),
											currentPropertyName, FieldMode.Instant, 2);
										break;
									}
									default:
										throw new ArgumentOutOfRangeException();
								}

								break;
							}
							case InteractionEventType.DeleteObjects:
							{
								ShowObjectArray<GameObject>(i);
								break;
							}
							case InteractionEventType.DeleteScripts:
							{
								ShowObjectArray<MonoBehaviour>(i);
								break;
							}
							case InteractionEventType.SpawnObjects:
							{
								SerializedProperty currentPosition =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.position));

								SerializedProperty currentRotation =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.rotation));

								Vector3Field(new GUIContent("Relative Pos",
									"The position of the spawned GameObject's relative to the position " +
									"specified in the prefab"), currentPosition, 2);

								Vector3Field(new GUIContent("Rotation",
									"The absolute rotation of the spawned GameObject's (specified in euler " +
									"angles)"), currentRotation, 2);

								ShowObjectArray<GameObject>(i, false);
								break;
							}
							case InteractionEventType.MoveObjects:
							{
								SerializedProperty currentPosition =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.position));

								SerializedProperty currentSpeed =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.lerpSpeed));

								Vector3Field(new GUIContent("Relative Destination",
									"The position to move these objects to relative to their current " +
									"position"), currentPosition, 2);

								FloatField(new GUIContent("Movement speed",
										"This value will be added to the movement interpolator every frame * Time.deltaTime (higher value = faster movement)"),
									currentSpeed, FieldMode.Instant, 2);

								ShowObjectArray<GameObject>(i);
								break;
							}
							case InteractionEventType.Log:
							{
								SerializedProperty currentLogMessage =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.message));

								StringField(new GUIContent("Log message"), currentLogMessage, FieldMode.Instant, 2);
								break;
							}
							case InteractionEventType.EnableDisablePortal:
							{
								SerializedProperty currentEnableDisableMode =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.mode));

								EnumField<EnableDisableMode>(new GUIContent("Mode",
									"Whether the object should be enabled or disabled"), currentEnableDisableMode, 2);

								ShowObjectArray<PortalParent>(i);
								break;
							}
							case InteractionEventType.TriggerCameraCutscene:
							{
								SerializedProperty camSettings =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.camSettings));
								SerializedProperty camCutsceneDuration =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.duration));

								SerializedProperty destPosition =
									camSettings.FindPropertyRelative(nameof(CamFixedViewSettings.destPosition));
								SerializedProperty smoothTranslation =
									camSettings.FindPropertyRelative(nameof(CamFixedViewSettings.smoothTranslation));
								SerializedProperty translationSpeed =
									camSettings.FindPropertyRelative(nameof(CamFixedViewSettings.translationSpeed));
								SerializedProperty destRotation =
									camSettings.FindPropertyRelative(nameof(CamFixedViewSettings.destRotation));
								SerializedProperty rotationLerp =
									camSettings.FindPropertyRelative(nameof(CamFixedViewSettings.rotationLerp));
								SerializedProperty camEnterType =
									camSettings.FindPropertyRelative(nameof(CamFixedViewSettings.camEnterType));
								SerializedProperty camExitType =
									camSettings.FindPropertyRelative(nameof(CamFixedViewSettings.camExitType));
								SerializedProperty cameraOffset =
									camSettings.FindPropertyRelative(nameof(CamFixedViewSettings.cameraOffset));
								SerializedProperty rotationOffset =
									camSettings.FindPropertyRelative(nameof(CamFixedViewSettings.rotationOffset));
								SerializedProperty transformDestination =
									camSettings.FindPropertyRelative(nameof(CamFixedViewSettings.transformDestination));
								SerializedProperty focusObject =
									camSettings.FindPropertyRelative(nameof(CamFixedViewSettings.focusObject));
								SerializedProperty showBlackBars =
									camSettings.FindPropertyRelative(nameof(CamFixedViewSettings.showBlackBars));

								FloatField(
									new GUIContent("Cutscene Duration",
										"How long the cutscene will last in seconds (when hitting 0 it will lerp back to player)"),
									camCutsceneDuration, FieldMode.Instant, 2);
								Vector3Field(
									new GUIContent("Destination Position",
										"The position the camera should move towards"), destPosition, 2);
								FloatField(
									new GUIContent("Smooth Translation",
										"The smoothness (acceleration/deceleration) of the translation. The lower, the stiffer"),
									smoothTranslation, FieldMode.Instant, 2);
								FloatField(
									new GUIContent("Translation Speed",
										"The speed at which the camera moves towards the destination. Recommended to be up in the 80-100's"),
									translationSpeed, FieldMode.Instant, 2);
								Vector3Field(
									new GUIContent("Destination Rotation",
										"The rotation the camera should transition into"), destRotation, 2);
								FloatField(
									new GUIContent("Rotation Lerp",
										"The smoothness of the transition. Recommended to be between 5-15"),
									rotationLerp, FieldMode.Instant, 2);
								EnumField<CamTransitionType>(
									new GUIContent("Cam Enter Type",
										"How the camera should move towards its destination"), camEnterType, 2);
								EnumField<CamTransitionType>(
									new GUIContent("Cam Exit Type",
										"How the camera should later move back to the player"), camExitType, 2);
								Vector3Field(
									new GUIContent("Camera Offset", "Local offset of the camera from the pivot"),
									cameraOffset, 2);
								Vector3Field(
									new GUIContent("Rotation Offset",
										"Local rotation offset of the pivot point from the destination"),
									rotationOffset, 2);
								ObjectField<Transform>(
									new GUIContent("Transform Destination",
										"Allows destination to be determined by a transform, though this isn't mandatory. Overrides destPosition/destRotation"),
									transformDestination, 2, 250, true);
								ObjectField<Transform>(
									new GUIContent("Focus Object",
										"If the camera should focus on an object from the view. Leave null to remain fixed"),
									focusObject, 2, 250, true);
								BoolField(
									new GUIContent("Show black bars", "Whether or not to show black bars in cutscene"),
									showBlackBars, 2);
								break;
							}
							case InteractionEventType.ChangeMaterial:
							{
								SerializedProperty currentMaterialArray =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.materialsToChangeTo));

								SerializedProperty currentSpeed =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.lerpSpeed));
								SerializedProperty lerpBool =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.lerpMaterial));

								ShowObjectArray<GameObject>(i, true);

								SerializedProperty currentMaterialFoldout =
									_eventMaterialFoldouts.GetArrayElementAtIndex(i);

								Foldout(new GUIContent("Materials"), currentMaterialFoldout, 2);

								if (currentMaterialFoldout.boolValue)
								{
									for (int j = 0; j < currentMaterialArray.arraySize; j++)
									{
										SerializedProperty currentMaterial =
											currentMaterialArray.GetArrayElementAtIndex(j);
										using (Horizontal)
										{
											Indent(3);

											ObjectField<Material>(new GUIContent($"Material {j}"),
												currentMaterial);

											if (Button("Remove", 60))
											{
												currentMaterialArray.DeleteArrayElementAtIndex(j);
											}
										}
									}

									using (Horizontal)
									{
										if (Button(EditorGUIUtility.IconContent("CreateAddNew@2x"), 30, 3))
										{
											currentMaterialArray.InsertArrayElementAtIndex(
												Math.Max(currentMaterialArray.arraySize - 1, 0));
											currentMaterialArray
												.GetArrayElementAtIndex(currentMaterialArray.arraySize - 1)
												.objectReferenceValue = null;
										}
									}

									ApplyProperties();
								}

								Space();

								BoolField(
									new GUIContent("Lerp",
										"If the material change should lerp instead of being instant"), lerpBool, 2);

								if (lerpBool.boolValue)
								{
									FloatField(new GUIContent("Material Lerp Speed",
											"This value will be added to the material interpolator every frame * Time.deltaTime (higher value = faster movement)"),
										currentSpeed, FieldMode.Instant, 2);
								}

								break;
							}
							case InteractionEventType.EnableDisableMouseLock:
							{
								SerializedProperty currentEnableDisableMode =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.mode));

								EnumField<EnableDisableMode>(new GUIContent("Mode",
									"Whether the object should be enabled or disabled"), currentEnableDisableMode, 2);

								break;
							}
							case InteractionEventType.EnableDisablePlayerMovement:
							{
								SerializedProperty currentEnableDisableMode =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.mode));
								SerializedProperty currentParentCameraToPlayerBool =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.parentCameraToPlayer));
								SerializedProperty staticPlayerStateBool =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.changePlayerState));

								EnumField<EnableDisableMode>(new GUIContent("Mode",
									"Whether the object should be enabled or disabled"), currentEnableDisableMode, 2);

								BoolField(
									new GUIContent("Change PlayerState",
										"Added this option since this can be used for black room puzzle etc."),
									staticPlayerStateBool, 2);

								BoolField(new GUIContent("Parent Camera To Player"), currentParentCameraToPlayerBool,
									2);

								SerializedProperty currentPosition =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.position));

								SerializedProperty currentRotation =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.rotation));

								if (currentParentCameraToPlayerBool.boolValue)
								{
									Vector3Field(new GUIContent("Local Position",
											"The local position of the camera in the players local space"),
										currentPosition,
										2);

									Vector3Field(new GUIContent("Rotation",
										"The absolute rotation of the player"), currentRotation, 2);
								}

								break;
							}
							case InteractionEventType.OpenDiary:
							{
								ShowObjectArray<Diary>(i, true);
								break;
							}
							case InteractionEventType.ShowSubtitle:
							{
								SerializedProperty duration =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.duration));
								SerializedProperty subtitle =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.message));
								SerializedProperty subtitleColor =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.subtitleColor));

								using (Horizontal)
								{
									Indent(2);
									Label(new GUIContent("Subtitle", "The subtitle to show"), 0, null,
										GUILayout.Width(250));
									subtitle.stringValue =
										EditorGUILayout.TextArea(subtitle.stringValue, GUILayout.Height(42f));
								}

								ColorField(new GUIContent("Color of subtitle"), subtitleColor, 2);

								FloatField(
									new GUIContent("Duration",
										"The time in seconds to show the subtitle before hiding it"), duration,
									FieldMode.Instant, 2);
								break;
							}
							case InteractionEventType.EnableFlickerLights:
							{
								ShowObjectArray<BuildingLightSwitch>(i);
								break;
							}
							case InteractionEventType.ChangeRespawnPoint:
							{
								SerializedProperty respawnPoint =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.respawnPoint));

								ObjectField<Transform>(
									new GUIContent("Respawn point",
										"The transform the player should teleport to when respawning"), respawnPoint,
									2);
								break;
							}
							case InteractionEventType.SwitchScene:
							{
								SerializedProperty sceneName = currentEvent.FindPropertyRelative(nameof(InteractionEvent.contextualName));
								
								StringField(new GUIContent("Scene name", "Name of the scene to load"), sceneName, FieldMode.Instant, 2);
								break;
							}
							case InteractionEventType.EnableDisableCrosshair:
							{
								SerializedProperty currentEnableDisableMode =
									currentEvent.FindPropertyRelative(nameof(InteractionEvent.mode));

								EnumField<EnableDisableMode>(new GUIContent("Mode",
									"Whether the object should be enabled or disabled"), currentEnableDisableMode, 2);
								break;
							}
							case InteractionEventType.StopAllMusic:
							{
								break;
							}
							case InteractionEventType.ClearInventory:
							{
								break;
							}
							default:
								throw new ArgumentOutOfRangeException();
						}
					}

					void ShowObjectArray<T>(int indexOfCurrentEvent, bool allowSceneObjects = true)
						where T : UnityEngine.Object
					{
						SerializedProperty currentObjectFoldout =
							_eventObjectFoldouts.GetArrayElementAtIndex(indexOfCurrentEvent);

						Foldout(new GUIContent("Objects"), currentObjectFoldout, 2);

						if (currentObjectFoldout.boolValue)
						{
							for (int j = 0; j < objects.arraySize; j++)
							{
								SerializedProperty currentObject = objects.GetArrayElementAtIndex(j);
								using (Horizontal)
								{
									Indent(3);

									ObjectField<T>(new GUIContent($"Objects {j}"),
										currentObject, 0, 250, allowSceneObjects);

									if (Button("Remove", 60))
									{
										objects.DeleteArrayElementAtIndex(j);
									}
								}
							}

							using (Horizontal)
							{
								if (Button(EditorGUIUtility.IconContent("CreateAddNew@2x"), 30, 3))
								{
									objects.InsertArrayElementAtIndex(Math.Max(objects.arraySize - 1, 0));
									objects.GetArrayElementAtIndex(objects.arraySize - 1).objectReferenceValue = null;
								}
							}

							ApplyProperties();
						}
					}
				}

				using (Horizontal)
				{
					if (Button(EditorGUIUtility.IconContent("CreateAddNew@2x"), 30, 1))
					{
						_events.InsertArrayElementAtIndex(Math.Max(_events.arraySize - 1, 0));

						_events.GetArrayElementAtIndex(_events.arraySize - 1)
							.FindPropertyRelative(nameof(InteractionEvent.relevantObjects)).ClearArray();

						_eventFoldouts.InsertArrayElementAtIndex(Math.Max(_eventFoldouts.arraySize - 1, 0));
						_eventObjectFoldouts.InsertArrayElementAtIndex(Math.Max(_eventObjectFoldouts.arraySize - 1, 0));
						_eventMaterialFoldouts.InsertArrayElementAtIndex(Math.Max(_eventMaterialFoldouts.arraySize - 1,
							0));
					}
				}
			}

			ApplyProperties();
		}

		public string GetTypeIdentifier(SerializedProperty currentEvent)
		{
			var currentEventType = (InteractionEventType) currentEvent.FindPropertyRelative(nameof(InteractionEvent.eventType)).enumValueIndex;
			var enableDisableMode = (EnableDisableMode) currentEvent.FindPropertyRelative(nameof(InteractionEvent.mode)).enumValueIndex;
			
			switch(currentEventType)
			{
				case InteractionEventType.EnableDisableObjects:
				{
					return enableDisableMode + " Objects";
				}
				case InteractionEventType.EnableDisableScripts:
				{
					return enableDisableMode + " Scripts";
				}
				case InteractionEventType.EnableDisablePortal:
				{
					return enableDisableMode + " Portal";
				}
				case InteractionEventType.SetAnimationProperty:
				{
					var animationPropertyType = (AnimationPropertyType) currentEvent.FindPropertyRelative(nameof(InteractionEvent.animPropertyType)).enumValueIndex;
					return "Set Animation " + animationPropertyType.ToString().Substring(3);
				}
				case InteractionEventType.DeleteObjects:
				{
					return "Delete Objects";
				}
				case InteractionEventType.DeleteScripts:
				{
					return "Delete Scripts";
				}
				case InteractionEventType.SpawnObjects:
				{
					return "Spawn Objects";
				}
				case InteractionEventType.MoveObjects:
				{
					return "Move Objects";
				}
				case InteractionEventType.Log:
				{
					return "Log";
				}
				case InteractionEventType.TriggerCameraCutscene:
				{
					return "Trigger Camera Cutscene";
				}
				case InteractionEventType.ChangeMaterial:
				{
					return "Change Material";
				}
				case InteractionEventType.EnableDisableMouseLock:
				{
					if (enableDisableMode == EnableDisableMode.Disable)
						return "Unlock Mouse";
					if (enableDisableMode == EnableDisableMode.Enable)
						return "Lock Mouse";
                    
					return "Toggle Mouse Lock";
				}
				case InteractionEventType.EnableDisablePlayerMovement:
				{
					return enableDisableMode + " Character Movement";
				}
				case InteractionEventType.OpenDiary:
				{
					return "Open Diary";
				}
				case InteractionEventType.ShowSubtitle:
				{
					return "Show Subtitle";
				}
				case InteractionEventType.EnableFlickerLights:
				{
					return "Enable Flicker Lights";
				}
				case InteractionEventType.ChangeRespawnPoint:
				{
					return "Change Respawn Point";
				}
				case InteractionEventType.SwitchScene:
				{
					return "Switch Scene";
				}
				case InteractionEventType.EnableDisableCrosshair:
				{
					return enableDisableMode + " Crosshair";
				}
				case InteractionEventType.StopAllMusic:
				{
					return "Stop All Music";
				}
				case InteractionEventType.ClearInventory:
				{
					return "Clear Inventory";
				}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}