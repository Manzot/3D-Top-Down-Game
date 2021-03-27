using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Movement))]
public class MovementcriptEditor : Editor
{
    SerializedObject targetObject;
    SerializedProperty movementType;
    SerializedProperty patrolPointsArray;
    Movement script;

    public void OnEnable()
    {
        targetObject = new SerializedObject(target);
        script = (Movement)target;
            
        movementType = targetObject.FindProperty("movementType");
        patrolPointsArray = targetObject.FindProperty("tPatrolPoints");
    }
    public override void OnInspectorGUI()
    {
        //  base.OnInspectorGUI();
        serializedObject.Update();

        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((Movement)target), typeof(Movement), false);
        GUI.enabled = true;

        EditorGUILayout.PropertyField(movementType);

        if (script.movementType != MovementType.IDLE)
        {
            script.fSpeed = EditorGUILayout.FloatField("Speed", script.fSpeed);
            switch (script.movementType)
            {
                case MovementType.MOVE_RANDOM:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 65f;
                    script.fWalkTime = EditorGUILayout.FloatField("Walk Time", script.fWalkTime);
                    EditorGUIUtility.labelWidth = 60f;
                    script.fWaitTime = EditorGUILayout.FloatField("Wait Time", script.fWaitTime);
                    EditorGUILayout.EndHorizontal();
                    EditorGUIUtility.labelWidth = 140f;
                    script.fMaxWalkingDistance = EditorGUILayout.FloatField("Max Walking Distance", script.fMaxWalkingDistance);
                    break;
                case MovementType.PATROLLING:
                    script.fWaitTime = EditorGUILayout.FloatField("Wait Time", script.fWaitTime);
                    EditorGUILayout.PropertyField(patrolPointsArray, true);
                    EditorGUIUtility.labelWidth = 150f;
                    script.bReverseDirection = EditorGUILayout.Toggle("Reverse Direction At End", script.bReverseDirection);
                    script.bRandomizePoints = EditorGUILayout.Toggle("Randomize Points", script.bRandomizePoints);
                    break;
                case MovementType.CIRCULAR_MOTION:
                    script.bReverseDirection = EditorGUILayout.Toggle("Reverse Direction", script.bReverseDirection);
                    script.bRandomizePoints = EditorGUILayout.Toggle("Randomize Directions", script.bRandomizePoints);
                    script.fRandomizeDirAfter = EditorGUILayout.FloatField("Randomize After (Range from 1 to )", script.fRandomizeDirAfter);
                    script.fRotateSpeed = EditorGUILayout.FloatField("Rotate Speed", script.fRotateSpeed);
                 //   EditorGUIUtility.labelWidth = 60f;
                    script.fWalkTime = EditorGUILayout.FloatField("Walk Time", script.fWalkTime);
                  //  EditorGUIUtility.labelWidth = 60f;
                    script.fWaitTime = EditorGUILayout.FloatField("Wait Time", script.fWaitTime);
                    script.fMaxWalkingDistance = EditorGUILayout.FloatField("Max Walking Distance", script.fMaxWalkingDistance);
                    break;
            }
        }

        targetObject.ApplyModifiedProperties();
    }
}
