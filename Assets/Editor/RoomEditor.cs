using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomCreator))]
public class RoomEditor : Editor
{
    RoomCreator roomCreator;
    bool roomsChangedSinceLastRepaint;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        string helpMessage = "TEST TEST TEST";
        EditorGUILayout.HelpBox(helpMessage, MessageType.Info);

        
        for (int i = 0; i < roomCreator.getRoomPositions().Count; i++) 
        {
            roomCreator.getRoomPositions()[i] = EditorGUILayout.Vector3Field("room " + (i + 1) + ": ", roomCreator.getRoomPositions()[i]);
            //Deactivate y point movement TODO: better?
            roomCreator.getRoomPositions()[i] = new Vector3(roomCreator.getRoomPositions()[i].x, 0, roomCreator.getRoomPositions()[i].z);
            roomCreator.getRoomSizes()[i] = EditorGUILayout.FloatField("room " + (i + 1) + " size: ", roomCreator.getRoomSizes()[i]);
        }
        if (GUILayout.Button("Add Room"))
        {
            roomCreator.getRoomPositions().Add(new Vector3(roomCreator.getRoomPositions()[roomCreator.getRoomPositions().Count - 1].x, roomCreator.getRoomPositions()[roomCreator.getRoomPositions().Count-1].y, roomCreator.getRoomPositions()[roomCreator.getRoomPositions().Count - 1].z));
            roomCreator.getRoomSizes().Add(1.0f);
            roomCreator.getRoomTypes().Add(RoomType.Undefined);
        }


        if (GUI.changed)
        {
            roomsChangedSinceLastRepaint = true;
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI()
    {
        Event guiEvent = Event.current;

        if (guiEvent.type == EventType.Repaint)
        {
            Draw();
        }
        else if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else
        {
            /*
            HandleInput(guiEvent);
            if (shapeChangedSinceLastRepaint)
            {
                HandleUtility.Repaint();
            }
            */
        }
    }

    void Draw() {
        //Debug.Log("hallo" + roomCreator.getRoomPositions().Count);
        //Handles.DrawSolidDisc(new Vector3(-20, 0, 2), Vector3.up, 1.0f);
        Handles.color = roomCreator.roomColor;
        GUIStyle style = new GUIStyle();
        for (int i = 0; i < roomCreator.getRoomPositions().Count; i++) 
        {
            if (roomCreator.drawRoomPositions)
            {
                Handles.DrawSolidDisc(roomCreator.getRoomPositions()[i], Vector3.up, roomCreator.roomDrawSize * roomCreator.getRoomSizes()[i]);
                if (roomCreator.drawRoomLabels)
                {
                    style.normal.textColor = Color.red;
                    Handles.Label(roomCreator.getRoomPositions()[i], "room " + (i + 1), style);
                }
            }
        }

        if (roomsChangedSinceLastRepaint)
        {
            roomCreator.UpdateRoomDisplay();
        }
        roomsChangedSinceLastRepaint = false;
    }

    void OnEnable()
    {
        //shapeChangedSinceLastRepaint = true;
        roomCreator = target as RoomCreator;
        //selectionInfo = new SelectionInfo();
        Undo.undoRedoPerformed += OnUndoOrRedo;
        Tools.hidden = true;
    }

    void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoOrRedo;
        Tools.hidden = false;
    }

    void OnUndoOrRedo()
    {
        /*
        if (selectionInfo.selectedShapeIndex >= shapeCreator.shapes.Count || selectionInfo.selectedShapeIndex == -1)
        {
            selectionInfo.selectedShapeIndex = shapeCreator.shapes.Count - 1;
        }
        shapeChangedSinceLastRepaint = true;
        */
    }
}
