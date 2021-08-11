using System;
using UnityEditor;
using UnityEngine;

namespace GP2_Team7.EditorScripts
{
    using Objects;
    using Managers;
    
    [CustomEditor(typeof(InteractableMoveToPlatform))]
    public class InteractableMoveToPlatformEditor : EditorLib<InteractableMoveToPlatform>
    {
        private bool _reset = true;

        private void OnEnable()
        {
            Init();
        }

       private void OnDisable()
       {
           if (scriptInstance.toolHasBeenSet)
           {
               Tools.current = scriptInstance.oldTool;
               scriptInstance.toolHasBeenSet = false;
           }
       }

       private void OnSceneGUI()
        {
            if (scriptInstance.showMovementArc)
            {
                if (_reset)
                {
                    scriptInstance.oldTool = Tools.current;
                    scriptInstance.toolHasBeenSet = true;
                    Tools.current = Tool.None;
                    _reset = false;
                }
                
                Vector3 newPlayerTangent, newPlatformTangent;
                
                Vector3 playerPosition = GameObject.FindWithTag("Player").transform.position;
                Vector3 targetPosition = scriptInstance.TargetPosition;
                
                Vector3 worldSpacePlayerTangent = scriptInstance.playerBezierTangent + playerPosition;
                Vector3 worldSpacePlatformTangent = scriptInstance.platformBezierTangent + targetPosition;
                
                newPlayerTangent = Handles.PositionHandle(worldSpacePlayerTangent, Quaternion.identity);
                newPlatformTangent = Handles.PositionHandle(worldSpacePlatformTangent, Quaternion.identity);
                
                scriptInstance.playerBezierTangent = newPlayerTangent - playerPosition;
                scriptInstance.platformBezierTangent = newPlatformTangent - targetPosition;
                
                Handles.DrawBezier(playerPosition, targetPosition, 
                    playerPosition + scriptInstance.playerBezierTangent, targetPosition + scriptInstance.platformBezierTangent, scriptInstance.movementArcColor, Texture2D.whiteTexture, 2f);
                
                if(GUI.changed)
                    EditorUtility.SetDirty(scriptInstance);
            }
            else if (!_reset)
            {
                Tools.current = scriptInstance.oldTool;
                _reset = true;
            }
        }
    }
}