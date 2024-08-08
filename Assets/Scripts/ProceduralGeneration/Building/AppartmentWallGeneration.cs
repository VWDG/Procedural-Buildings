using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Geometry;

//[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class AppartmentWallGeneration : MonoBehaviour
{
    Mesh mesh_apartment;

    Vector3[] vertices_apartment;
    int[] triangles_apartment;
    Vector3[] normals_apartment;

    Vector2[] uv_apartment;

    public float appartment_height;
    public BuildingParameter building_paramter;

    public Vector2[] floorplan_corner_INPUT;
    public SegmentDeclaration[] floorplan_edges_INPUT;

    public bool useRoomCreator = false;
    public RoomCreator rCreator;
    public int buildingNum = 0;

    List<bool> walled_corner_INPUT;

    SegmentDeclaration[] floorplan_edges_rooms;
    Vector2[] floorplan_corner_rooms;

    Vector3[] floorplan_inner;
    Vector3[] floorplan_outer;


    List<Vector3> corner_points = new();
    List<Vector3> corner_points_normals = new();
    List<Vector2> corner_points_uv = new();
    List<int> triangles_list = new();

    Vector3[] corner_points_full;



    //Creating Differen GameObjects for each room
    List<List<SegmentDeclaration>> segments_single_elements = new();
    List<List<Vector3>> corner_points_single_elements = new();
    List<List<Vector2>> corner_points_uv_single_elements = new();
    List<List<int>> triangles_list_single_elements = new();

    //Creating Differen GameObjects for each room
    List<List<Vector3>> corner_points_single_elements_floor = new();
    List<List<Vector2>> corner_points_uv_single_elements_floor = new();
    List<List<int>> triangles_list_single_elements_floor = new();

    List<List<Vector3>> corner_points_single_elements_ceiling = new();
    List<List<Vector2>> corner_points_uv_single_elements_ceiling = new();
    List<List<int>> triangles_list_single_elements_ceiling = new();


    //Gizmos
    public bool drawGizmosLines = false;
    public bool drawGizmosPoints = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.LogError("WALL TEST --> start");
        if (useRoomCreator)
        {
            CreateParameterFromRoomCreator();
            Debug.LogError("WALL TEST --> generating");
        }
        /*
        mesh_apartment = new Mesh();
        mesh_apartment.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        GetComponent<MeshFilter>().mesh = mesh_apartment;
        */
        UpgradeFloorplan(0.2f);
        if (useRoomCreator)
        {
            Debug.LogError("WALL TEST --> UpgradeFloorplan");
        }
        SplitIntoIndividualRooms(0.2f);
        if (useRoomCreator)
        {
            Debug.LogError("WALL TEST --> SplitIntoIndividualRooms");
        }
        SplitIntoIndividualObjects();
        if (useRoomCreator)
        {
            Debug.LogError("WALL TEST --> SplitIntoIndividualObjects");
        }
        GenerateWalls_OnSingleObjects();
        if (useRoomCreator)
        {
            Debug.LogError("WALL TEST --> GenerateWalls_OnSingleObjects");
        }
        GenerateFloorAndCeiling();
        if (useRoomCreator)
        {
            Debug.LogError("WALL TEST --> GenerateFloorAndCeiling");
        }
        CreateSingleObjects();
        if (useRoomCreator)
        {
            Debug.LogError("WALL TEST --> CreateSingleObjects");
        }

        //GenerateWalls();
        //Draw_appartment(floorplan_corner_INPUT, 2.40f, 0.4f);
        //UpdateMesh();

    }

    // Update is called once per frame
    void Update()
    {
        /*
        corner_points = new();
        triangles_list = new();
        corner_points_normals = new();
        corner_points_uv = new();
        SplitIntoIndividualRooms(0.2f);
        GenerateWalls();

        //Update Mesh on Values Changed
        //TODO REMOVE LATER
        //Draw_appartment(floorplan_corner_INPUT, 2.40f, 0.6f);
        UpdateMesh();
        */
    }

    private void CreateParameterFromRoomCreator()
    {
        rCreator.GetShapeCreator().UpdateMeshDisplay();
        rCreator.UpdateRoomDisplay();
        Vector2[] corners = rCreator.getWallCornerForSpecificBuilding(buildingNum);
        Segment[] segments = rCreator.getSegmentsForSpecificBuilding(buildingNum);

        floorplan_corner_INPUT = corners;

        floorplan_edges_INPUT = new SegmentDeclaration[segments.Length];
        for (int i = 0; i < segments.Length; i++) 
        {
            floorplan_edges_INPUT[i] = new SegmentDeclaration(segments[i].GetIndexFrom(), segments[i].GetIndexTo());
        }

    }

    private void CreateSingleObjects()
    {
        //Debug.Log(corner_points_single_elements.Count);
        for (int i = 0; i < corner_points_single_elements.Count; i++)
        {
            Vector3[] vertices = corner_points_single_elements[i].ToArray();
            int[] triangles = triangles_list_single_elements[i].ToArray();
            Vector2[] uv = corner_points_uv_single_elements[i].ToArray();

            string room;
            if (segments_single_elements[i][0].room_type_left == RoomType.Undefined)
            {
                room = segments_single_elements[i][0].room_type_right.ToString();
            }
            else 
            {
                room = segments_single_elements[i][0].room_type_left.ToString();
            }

            GameObject game_object = new GameObject(("bulding" + i + "_" + room));
            game_object.transform.position= this.transform.position;
            game_object.transform.parent = this.transform;
            //game_object.name = ("bulding_part"+i);
            MeshFilter mesh_filter = game_object.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh_filter.mesh = mesh;

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            MeshCollider mesh_collider = game_object.AddComponent<MeshCollider>();
            mesh_collider.sharedMesh = mesh;



            MeshRenderer mesh_render = game_object.AddComponent<MeshRenderer>();
            mesh_render.material = building_paramter.room_mat[0];

            RoomType room_type = segments_single_elements[i][0].room_type_left;

            if (room_type == RoomType.Undefined) 
            {
                room_type = segments_single_elements[i][0].room_type_right;
            }
            if (room_type != RoomType.Outside)
            {
                mesh_render.material = building_paramter.room_mat[1];
            }

        }
        int skip_rooms = 0;
        for (int i = 0; i < corner_points_single_elements_floor.Count; i++)
        {
            //Debug.LogError(i + ": " + corner_points_single_elements_floor[i].Count);
            Vector3[] vertices = corner_points_single_elements_floor[i].ToArray();
            int[] triangles = triangles_list_single_elements_floor[i].ToArray();
            //Vector2[] uv = corner_points_uv_single_elements_floor[i].ToArray();

            if (segments_single_elements[i][0].room_type_left == RoomType.Outside || segments_single_elements[i][0].room_type_right == RoomType.Outside)
            {
                skip_rooms++;
            }
            string room;
            if (segments_single_elements[i + skip_rooms][0].room_type_left == RoomType.Undefined)
            {
                room = segments_single_elements[i + skip_rooms][0].room_type_right.ToString();
            }
            else
            {
                room = segments_single_elements[i + skip_rooms][0].room_type_left.ToString();
            }

            GameObject game_object = new GameObject(("floor" + i + "_" + room));
            game_object.transform.position = this.transform.position;
            game_object.transform.parent = this.transform;
            MeshFilter mesh_filter = game_object.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh_filter.mesh = mesh;

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            //mesh.uv = uv;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            MeshCollider mesh_collider = game_object.AddComponent<MeshCollider>();
            mesh_collider.sharedMesh = mesh;



            MeshRenderer mesh_render = game_object.AddComponent<MeshRenderer>();
            mesh_render.material = building_paramter.room_mat[0];

            RoomType room_type = segments_single_elements[i + skip_rooms][0].room_type_left;

            if (room_type == RoomType.Undefined)
            {
                room_type = segments_single_elements[i+ skip_rooms][0].room_type_right;
            }
            if (room_type != RoomType.Outside)
            {
                mesh_render.material = building_paramter.room_mat[1];
            }

        }

        skip_rooms = 0;
        for (int i = 0; i < corner_points_single_elements_ceiling.Count; i++)
        {
            //Debug.LogError(i + ": " + corner_points_single_elements_ceiling[i].Count);
            Vector3[] vertices = corner_points_single_elements_ceiling[i].ToArray();
            int[] triangles = triangles_list_single_elements_ceiling[i].ToArray();
            //Vector2[] uv = corner_points_uv_single_elements_ceiling[i].ToArray();

            if (segments_single_elements[i][0].room_type_left == RoomType.Outside || segments_single_elements[i][0].room_type_right == RoomType.Outside)
            {
                skip_rooms++;
            }
            string room;
            if (segments_single_elements[i + skip_rooms][0].room_type_left == RoomType.Undefined)
            {
                room = segments_single_elements[i + skip_rooms][0].room_type_right.ToString();
            }
            else
            {
                room = segments_single_elements[i + skip_rooms][0].room_type_left.ToString();
            }

            GameObject game_object = new GameObject(("ceiling" + i + "_" + room));
            game_object.transform.position = this.transform.position;
            game_object.transform.parent = this.transform;
            MeshFilter mesh_filter = game_object.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh_filter.mesh = mesh;

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            //mesh.uv = uv;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            MeshCollider mesh_collider = game_object.AddComponent<MeshCollider>();
            mesh_collider.sharedMesh = mesh;



            MeshRenderer mesh_render = game_object.AddComponent<MeshRenderer>();
            mesh_render.material = building_paramter.room_mat[0];

            RoomType room_type = segments_single_elements[i + skip_rooms][0].room_type_left;

            if (room_type == RoomType.Undefined)
            {
                room_type = segments_single_elements[i + skip_rooms][0].room_type_right;
            }
            if (room_type != RoomType.Outside)
            {
                mesh_render.material = building_paramter.room_mat[1];
            }

        }
    }

    private void UpgradeFloorplan(float wall_thickness) 
    {
        //Debug.LogWarning(floorplan_corner_INPUT.Length);
        List<Vector2> floorplan_input_corners = new(floorplan_corner_INPUT);
        List<SegmentDeclaration> floorplan_input_edges = new();

        SortedList<float, SegmentDeclaration>[] neighboured_seg = new SortedList<float, SegmentDeclaration>[floorplan_edges_INPUT.Length];
        Debug.LogError("WALL TEST " + neighboured_seg.Length + " edges: " + floorplan_edges_INPUT.Length + ", corners: " + floorplan_corner_INPUT.Length);
        foreach (var seg in floorplan_edges_INPUT)
        {
            if (neighboured_seg[seg.id_from] == null)
                neighboured_seg[seg.id_from] = new();

            if (neighboured_seg[seg.id_to] == null)
                neighboured_seg[seg.id_to] = new();

            Vector2 direction_from = floorplan_corner_INPUT[seg.id_to] - floorplan_corner_INPUT[seg.id_from];

            Vector2 direction_to = floorplan_corner_INPUT[seg.id_from] - floorplan_corner_INPUT[seg.id_to];


            float angle_from = Vector2.SignedAngle(direction_from, Vector2.up);
            if (angle_from < 0)
                angle_from = 360 + angle_from;

            float angle_to = Vector2.SignedAngle(direction_to, Vector2.up);
            if (angle_to < 0)
                angle_to = 360 + angle_to;

            if (neighboured_seg[seg.id_from].ContainsKey(angle_from)) 
            {
                Debug.LogError("KEY EXISTING");
            }
            neighboured_seg[seg.id_from].Add(angle_from, seg);
            neighboured_seg[seg.id_to].Add(angle_to, seg);
        }

        int index= floorplan_corner_INPUT.Length;
        foreach (var seg in floorplan_edges_INPUT)
        {
            
            int neighbour_count_from = 0;
            int neighbour_count_to = 0;
            SortedList<float, SegmentDeclaration> neighbours_from = new();
            SortedList<float, SegmentDeclaration> neighbours_to = new();

            foreach (var con_seg in neighboured_seg[seg.id_from])
            {
                //if (con_seg.Value != seg && con_seg.Value.segment_type != SegmentType.Empty)
                if (con_seg.Value.segment_type != SegmentType.Empty)
                {
                    neighbour_count_from++;
                    neighbours_from.Add(con_seg.Key, con_seg.Value);
                }
            }
            foreach (var con_seg in neighboured_seg[seg.id_to])
            {
                //if (con_seg.Value != seg && con_seg.Value.segment_type != SegmentType.Empty)
                if (con_seg.Value.segment_type != SegmentType.Empty)
                {
                    neighbour_count_to++;
                    neighbours_to.Add(con_seg.Key, con_seg.Value);
                }
            }

            if (seg.segment_type != SegmentType.Wall)
            {
                //TODO look and neihgbour segments, if the angle between this segment and a neighbour segment is < 90 degree or above 90 degre, the model gets some weird corners
                //also dont add these segments, wenn there is only 1 neihgbour segment and the angle between both segment is 180 degree


                
                Vector2 direction = (floorplan_corner_INPUT[seg.id_to] - floorplan_corner_INPUT[seg.id_from]).normalized;
                int index_from = seg.id_from;
                int index_to = seg.id_to;

                //Add extra Segment on From Side
                if (neighbour_count_from >= 1) 
                {
                    Vector2 direction_from = floorplan_corner_INPUT[seg.id_to] - floorplan_corner_INPUT[seg.id_from];
                    float angle_from = Vector2.SignedAngle(direction_from, Vector2.up);
                    if (angle_from < 0)
                        angle_from = 360 + angle_from;

                    int index_seg_from = neighbours_from.IndexOfKey(angle_from);
                    int index_n1 = index_seg_from + 1;
                    if (index_n1 == neighbours_from.Count)
                        index_n1 = 0;
                    int index_n2 = index_seg_from - 1;
                    if (index_n2 < 0)
                        index_n2 = neighbours_from.Count - 1;

                    SegmentDeclaration neighbour_from1 = neighbours_from.Values[index_n1];
                    SegmentDeclaration neighbour_from2 = neighbours_from.Values[index_n2];

                    //Watch for weired corners

                    //Debug.Log("AUFGEPASST: " + neighbour_from1.id_from + ", " + neighbour_from1.id_to + " und " + neighbour_from2.id_from + ", " + neighbour_from2.id_to + " ABER " + floorplan_corner_INPUT.Length);
                    if (Vector2.Angle((floorplan_corner_INPUT[seg.id_to]- floorplan_corner_INPUT[seg.id_from]).normalized, (floorplan_corner_INPUT[neighbour_from1.id_to]- floorplan_corner_INPUT[neighbour_from1.id_from]).normalized)%180 != 0 ||
                        Vector2.Angle((floorplan_corner_INPUT[seg.id_to] - floorplan_corner_INPUT[seg.id_from]).normalized, (floorplan_corner_INPUT[neighbour_from2.id_to] - floorplan_corner_INPUT[neighbour_from2.id_from]).normalized)%180 != 0) 
                    {
                        //Debug.Log(Vector2.Angle((floorplan_corner_INPUT[seg.id_to] - floorplan_corner_INPUT[seg.id_from]).normalized, (floorplan_corner_INPUT[neighbour_from1.id_to] - floorplan_corner_INPUT[neighbour_from1.id_from]).normalized) + ", " + Vector2.Angle((floorplan_corner_INPUT[seg.id_to] - floorplan_corner_INPUT[seg.id_from]).normalized, (floorplan_corner_INPUT[neighbour_from2.id_to] - floorplan_corner_INPUT[neighbour_from2.id_from]).normalized));
                        float angle_diff1 = ((neighbours_from.Keys[index_n1] - angle_from) + 360) % 360;
                        float angle_diff2 = ((angle_from - neighbours_from.Keys[index_n2]) + 360) % 360;
                        float angle_diff = angle_diff1/2;
                        if (angle_diff2 < angle_diff1)
                            angle_diff = angle_diff2/2;
                        float thickness = (wall_thickness / Mathf.Sin(angle_diff * Mathf.Deg2Rad)) * Mathf.Sin((90 - angle_diff) * Mathf.Deg2Rad);
                        //Debug.Log("F" + seg.id_from + "[" + angle_from + "]" + ": " + angle_diff1 + "[" + neighbours_from.Keys[index_n1] + "]" + ", " + angle_diff2 + "[" + neighbours_from.Keys[index_n2] + "]" + " --> " + angle_diff + " resultierend: " + thickness);
                        //if (thickness < wall_thickness)
                        //  thickness = wall_thickness;


                        //NewPoint
                        floorplan_input_corners.Add(floorplan_corner_INPUT[seg.id_from] + direction * thickness);
                        //Create new Segments
                        SegmentDeclaration newSegment1 = new();
                        newSegment1.id_from = seg.id_from;
                        newSegment1.id_to = index;
                        newSegment1.segment_type = SegmentType.Wall;
                        newSegment1.room_type_left = seg.room_type_left;
                        newSegment1.room_type_right = seg.room_type_right;
                        floorplan_input_edges.Add(newSegment1);

                        index_from = index;
                        index++;
                    }

                     float dist_from = 0;
                    //index++;
                    
                }


                //Add extra Segment on To Side
                if (neighbour_count_to >= 1)
                {
                    Vector2 direction_to = floorplan_corner_INPUT[seg.id_from] - floorplan_corner_INPUT[seg.id_to];
                    float angle_to = Vector2.SignedAngle(direction_to, Vector2.up);
                    if (angle_to < 0)
                        angle_to = 360 + angle_to;

                    int index_seg_to = neighbours_to.IndexOfKey(angle_to);
                    int index_n1 = index_seg_to + 1;
                    if (index_n1 == neighbours_to.Count)
                        index_n1 = 0;
                    int index_n2 = index_seg_to - 1;
                    if (index_n2 < 0)
                        index_n2 = neighbours_to.Count - 1;

                    SegmentDeclaration neighbour_to1 = neighbours_to.Values[index_n1];
                    SegmentDeclaration neighbour_to2 = neighbours_to.Values[index_n2];

                    if (Vector2.Angle((floorplan_corner_INPUT[seg.id_to] - floorplan_corner_INPUT[seg.id_from]).normalized, (floorplan_corner_INPUT[neighbour_to1.id_to] - floorplan_corner_INPUT[neighbour_to1.id_from]).normalized)%180 != 0 ||
                        Vector2.Angle((floorplan_corner_INPUT[seg.id_to] - floorplan_corner_INPUT[seg.id_from]).normalized, (floorplan_corner_INPUT[neighbour_to2.id_to] - floorplan_corner_INPUT[neighbour_to2.id_from]).normalized)%180 != 0)
                    {
                        //Debug.Log(Vector2.Angle((floorplan_corner_INPUT[seg.id_to] - floorplan_corner_INPUT[seg.id_from]).normalized, (floorplan_corner_INPUT[neighbour_to1.id_to] - floorplan_corner_INPUT[neighbour_to1.id_from]).normalized) + ", " + Vector2.Angle((floorplan_corner_INPUT[seg.id_to] - floorplan_corner_INPUT[seg.id_from]).normalized, (floorplan_corner_INPUT[neighbour_to2.id_to] - floorplan_corner_INPUT[neighbour_to2.id_from]).normalized));

                        float angle_diff1 = ((neighbours_to.Keys[index_n1] - angle_to) + 360) % 360;
                        float angle_diff2 = ((angle_to - neighbours_to.Keys[index_n2]) + 360) % 360;
                        float angle_diff = angle_diff1 / 2;
                        if (angle_diff2 < angle_diff1)
                            angle_diff = angle_diff2 / 2;
                        float thickness = (wall_thickness / Mathf.Sin(angle_diff * Mathf.Deg2Rad)) * Mathf.Sin((90 - angle_diff) * Mathf.Deg2Rad);
                        //Debug.Log(seg.id_to + "[" + angle_to + "]" + ": " + angle_diff1 + "[" + neighbours_to.Keys[index_n1] + "]" + ", " + angle_diff2 + "[" + neighbours_to.Keys[index_n2] + "]" + " --> " + angle_diff + " resultierend: " + thickness);
                        //if (thickness < wall_thickness)
                        //    thickness = wall_thickness;

                        //NewPoint
                        floorplan_input_corners.Add(floorplan_corner_INPUT[seg.id_to] - direction * thickness);
                        //Create new Segments
                        SegmentDeclaration newSegment1 = new();
                        newSegment1.id_from = index;
                        newSegment1.id_to = seg.id_to;
                        newSegment1.segment_type = SegmentType.Wall;
                        newSegment1.room_type_left = seg.room_type_left;
                        newSegment1.room_type_right = seg.room_type_right;
                        floorplan_input_edges.Add(newSegment1);

                        index_to = index;
                        index++;
                    }

                    float dist_to = 0;
                    //index++;
                }

                SegmentDeclaration newSegment = new();
                newSegment.id_from = index_from;
                newSegment.id_to = index_to;
                newSegment.segment_type = seg.segment_type;
                newSegment.room_type_left = seg.room_type_left;
                newSegment.room_type_right = seg.room_type_right;
                floorplan_input_edges.Add(newSegment);
            }
            else 
            {
                floorplan_input_edges.Add(seg);
            }
            
        }

        //Debug.Log(floorplan_corner_INPUT.Length + ", " + floorplan_edges_INPUT.Length);
        floorplan_corner_INPUT = floorplan_input_corners.ToArray();
        floorplan_edges_INPUT = floorplan_input_edges.ToArray();
        //Debug.Log(floorplan_corner_INPUT.Length + ", " + floorplan_edges_INPUT.Length);
        //Debug.LogWarning(floorplan_corner_INPUT.Length);
    }

    void SplitIntoIndividualRooms(float wall_thickness) 
    {
        floorplan_edges_rooms = new SegmentDeclaration[floorplan_edges_INPUT.Length * 2];
        for (int i = 0; i < floorplan_edges_INPUT.Length; i++) 
        {
            floorplan_edges_rooms[2 * i] = new SegmentDeclaration();
            floorplan_edges_rooms[2 * i].segment_type = floorplan_edges_INPUT[i].segment_type;
            floorplan_edges_rooms[2 * i].room_type_left = RoomType.Undefined;
            floorplan_edges_rooms[2 * i].room_type_right = floorplan_edges_INPUT[i].room_type_right;
            floorplan_edges_rooms[2 * i].segment_id = 2 * i;

            floorplan_edges_rooms[(2 * i) + 1] = new SegmentDeclaration();
            floorplan_edges_rooms[(2 * i) + 1].segment_type = floorplan_edges_INPUT[i].segment_type;
            floorplan_edges_rooms[(2 * i) + 1].room_type_left = floorplan_edges_INPUT[i].room_type_left;
            floorplan_edges_rooms[(2 * i) + 1].room_type_right = RoomType.Undefined;
            floorplan_edges_rooms[(2 * i) + 1].segment_id = (2 * i) + 1;

            floorplan_edges_rooms[2 * i].SetOppositeSegment(floorplan_edges_rooms[(2 * i) + 1]);
            floorplan_edges_rooms[(2 * i) + 1].SetOppositeSegment(floorplan_edges_rooms[2 * i]);
        }
        //List<SegmentDeclaration> floorplan_edges_rooms_helper = new(floorplan_edges_rooms);

        //Create Segment Array Mapping
        int[,] segment_array_mapping = new int[floorplan_corner_INPUT.Length, floorplan_corner_INPUT.Length];
        for (int i = 0; i < floorplan_edges_INPUT.Length; i++)
        {
            var segment = floorplan_edges_INPUT[i];
            segment_array_mapping[segment.id_from, segment.id_to] = i + 1;
            segment_array_mapping[segment.id_to, segment.id_from] = i + 1;

        }

        List<SegmentDeclaration> additional_segment = new();
        List<Vector2> corners_rooms = new List<Vector2>();
        for (int i = 0; i < floorplan_corner_INPUT.Length; i++)
        {
            //Create List of connected Segments and a liste of the corresponding ids
            List<SegmentDeclaration> connected_segments = new List<SegmentDeclaration>();
            List<int> connected_segment_ids = new List<int>();
            for (int k = 0; k < segment_array_mapping.GetLength(0); k++) 
            {
                if (segment_array_mapping[i, k] != 0) 
                {
                    connected_segments.Add(floorplan_edges_INPUT[segment_array_mapping[i, k] - 1]);
                    connected_segment_ids.Add(segment_array_mapping[i, k] - 1);
                }
            }

            //Calulate Angle to 0 for each segment
            SortedList<float, SegmentDeclaration> connected_segments_sorted = new();
            SortedList<float, int> connected_segment_ids_sorted = new();

            //Add them in sorted order, to calculate the new corner point only between neighbour segments
            //Vereinfachen?

            for (int k = 0; k < connected_segments.Count; k++)
            {
                
                var segment = connected_segments[k];
                Vector2 direction;

                if (segment.id_from == i)
                {
                    direction = floorplan_corner_INPUT[segment.id_to] - floorplan_corner_INPUT[segment.id_from];
                }
                else 
                {
                    direction = floorplan_corner_INPUT[segment.id_from] - floorplan_corner_INPUT[segment.id_to];
                }
                
                float angle = Vector2.SignedAngle(direction, Vector2.up);
                if (angle < 0)
                    angle = 360 + angle;
                //Debug.Log("COUNT: " + connected_segments.Count +  " // " + angle + ", " + direction + "; Punkt: " + segment.id_from + "," + segment.id_to + " mit " + floorplan_corner_INPUT[segment.id_from] + " und " + floorplan_corner_INPUT[segment.id_to] + " ( Details: " + segment);
                connected_segments_sorted.Add(angle, segment);
                connected_segment_ids_sorted.Add(angle, connected_segment_ids[k]);
            }


            //Special Case only one connected Segment
            if (connected_segments_sorted.Count <= 1)
            {
                //Debug.Log(connected_segments.Count + ", " + i);
                SegmentDeclaration segment1 = connected_segments_sorted.Values[0];
                int segment1_id = connected_segment_ids_sorted.Values[0];

                int pointA_id = segment1.id_from;
                if (pointA_id == i)
                {
                    pointA_id = segment1.id_to;
                }
                Vector2 pointA = floorplan_corner_INPUT[pointA_id]- floorplan_corner_INPUT[i];

                Vector2 new_corner_point1 = CalculateNewPointPosition(floorplan_corner_INPUT[i] + pointA, floorplan_corner_INPUT[i], floorplan_corner_INPUT[i] + pointA * (-1), 180, wall_thickness);
                Vector2 new_corner_point2 = CalculateNewPointPosition(floorplan_corner_INPUT[i] + pointA * (-1), floorplan_corner_INPUT[i], floorplan_corner_INPUT[i] + pointA, 180, wall_thickness);

                
                //TODO 
                //Can here be the same point? that is already existing
                if (corners_rooms.Contains(new_corner_point1) || corners_rooms.Contains(new_corner_point2))
                    Debug.LogError("SAME POINT ON ISOLATED EDGES --> DONT ADD EXISITNG POINTS");

                corners_rooms.Add(new_corner_point1);
                int corner1_id = corners_rooms.Count - 1;

                corners_rooms.Add(new_corner_point2);
                int corner2_id = corners_rooms.Count - 1;

                //Add additional segment
                SegmentDeclaration newSeg = new();
                newSeg.id_from = corner1_id;
                newSeg.id_to = corner2_id;
                newSeg.room_type_left = segment1.room_type_left;
                newSeg.room_type_right = RoomType.Undefined;
                newSeg.segment_type = segment1.segment_type;
                //newSeg.SetFromNeighbourSegment();

                //Debug.Log("newSeg: " + new_corner_point1 + ", " + new_corner_point2 + " mit rechts: " + newSeg.room_type_right + " und links: " + newSeg.room_type_left);

                additional_segment.Add(newSeg);



                if (segment1.id_from == i)
                {
                    floorplan_edges_rooms[2 * segment1_id].id_from = corner1_id;
                    floorplan_edges_rooms[(2 * segment1_id) + 1].id_from = corner2_id;
                }
                else
                {
                    floorplan_edges_rooms[(2 * segment1_id)].id_to = corner2_id;
                    floorplan_edges_rooms[(2 * segment1_id)+1].id_to = corner1_id;
                }
            }
            else
            {
                //Calulate new Corner Point between both vectors
                for (int k = 1; k <= connected_segments_sorted.Count; k++)
                {
                    int index = k;

                    //get to neighboured segments
                    SegmentDeclaration segment1 = connected_segments_sorted.Values[index - 1];
                    float angle1 = connected_segments_sorted.Keys[index - 1];
                    int segment1_id = connected_segment_ids_sorted.Values[index - 1];

                    
                    //if seg1 is the last segment, the neighbour is the first segment
                    if (index == connected_segments_sorted.Count)
                    {
                        index = 0;
                    }

                    SegmentDeclaration segment2 = connected_segments_sorted.Values[index];
                    float angle2 = connected_segments_sorted.Keys[index];
                    int segment2_id = connected_segment_ids_sorted.Values[index];


                    //Calculate the other 2 endpoints for the connected segments
                    int pointA_id = segment1.id_from;
                    if (pointA_id == i)
                    {
                        pointA_id = segment1.id_to;
                    }
                    Vector2 pointA = floorplan_corner_INPUT[pointA_id];

                    int pointB_id = segment2.id_from;
                    if (pointB_id == i)
                    {
                        pointB_id = segment2.id_to;
                    }
                    Vector2 pointB = floorplan_corner_INPUT[pointB_id];


                    //Calulate new Corner Point between both vectors
                    float angle = (Vector2.SignedAngle((pointA - floorplan_corner_INPUT[i]).normalized, (pointB - floorplan_corner_INPUT[i]).normalized)+360)%360;
                    //Vector2 test1 = (pointA - floorplan_corner_INPUT[i]).normalized;
                    //Vector2 test2 = (pointB - floorplan_corner_INPUT[i]).normalized;
                    //Debug.Log(pointA +", " + floorplan_corner_INPUT[i] +", "+ pointB + ", " + angle + "vergleich: "+ (pointA - floorplan_corner_INPUT[i])+", "+ (pointB - floorplan_corner_INPUT[i])+", " + Vector2.SignedAngle(new Vector2(0.2f, 0), new Vector2(-2.6f, 0)) + "und das: "+ Vector2.SignedAngle(pointA - floorplan_corner_INPUT[i], pointB - floorplan_corner_INPUT[i]) + ", hä: " +test1 + ", " + test2 + ", "+ Vector2.SignedAngle(test1, test2));
                    Vector2 new_corner_point = CalculateNewPointPosition(pointA, floorplan_corner_INPUT[i], pointB, angle, wall_thickness);

                    //Add new Point to the List of new Points and add the point to his new neighbour segments
                    int corner_id;
                    bool contains = false;
                    int id = -1;
                    for (int j = 0; j < corners_rooms.Count; j++)
                    {
                        Vector2 corner = corners_rooms[j];
                        if (corner == new_corner_point) 
                        {
                            contains = true;
                            id = j;
                        }
                    }
                    //if (corners_rooms.Contains(new_corner_point))
                    if (contains)
                    {
                        corner_id = id;
                        //corner_id = corners_rooms.IndexOf(new_corner_point);
                    }
                    else
                    {

                        corners_rooms.Add(new_corner_point);
                        corner_id = corners_rooms.Count - 1;
                    }
                    //Debug.Log("aktuelle Corner ID: " + corner_id);

                    if (segment1.id_from == i)
                    {
                        floorplan_edges_rooms[2 * segment1_id].id_from = corner_id;
                        //segment1.SetFromNeighbourSegment(segment2);
                    }
                    else
                    {
                        floorplan_edges_rooms[(2 * segment1_id) + 1].id_to = corner_id;
                        //segment1.SetToNeighbourSegment(segment2);
                    }

                    if (segment2.id_from == i)
                    {
                        floorplan_edges_rooms[(2 * segment2_id) + 1].id_from = corner_id;
                        //segment2.SetFromNeighbourSegment(segment1);
                    }
                    else
                    {
                        floorplan_edges_rooms[2 * segment2_id].id_to = corner_id;
                        //segment2.SetToNeighbourSegment(segment1);
                    }
                }
            }
        }

        
        //Remove Segments with a length of 0
        //Create Segment Array Mapping
        List<SegmentDeclaration>[] neighboured_seg = new List<SegmentDeclaration>[corners_rooms.Count];
        foreach (var seg in floorplan_edges_rooms)
        {
            if (neighboured_seg[seg.id_from] == null)
                neighboured_seg[seg.id_from] = new();

            if (neighboured_seg[seg.id_to] == null)
                neighboured_seg[seg.id_to] = new();

            neighboured_seg[seg.id_from].Add(seg);
            neighboured_seg[seg.id_to].Add(seg);
        }


        List<SegmentDeclaration> floorplan_edges_rooms_helper = new(floorplan_edges_rooms);
        //List<Vector2> remove_corners 

        //Debug.Log(floorplan_edges_rooms.Length);
        foreach (var seg in floorplan_edges_rooms)
        {
            if (corners_rooms[seg.id_from] == corners_rooms[seg.id_to]) 
            {
                /*
                foreach (var con_seg in neighboured_seg[seg.id_from])
                {
                    if (con_seg.id_from == seg.id_from)
                    {
                        con_seg.id_from = seg.id_to;
                    }
                    else 
                    {
                        con_seg.id_to = seg.id_to;
                    }
                }
                */
                foreach (var change_seg in floorplan_edges_rooms)
                {
                    if (change_seg.segment_id > seg.segment_id) 
                    {
                        change_seg.segment_id--;
                    }
                }
                SegmentDeclaration op_seg = seg.GetOppositeSegment();
                if(op_seg != null)
                    op_seg.SetOppositeSegment(null);
                //TODO set opp is not impaorten for seg, cuz seg get removed anyways
                seg.SetOppositeSegment(null);
                floorplan_edges_rooms_helper.Remove(seg);
                //Debug.Log("it happend");
            }
        }

        foreach (var seg in additional_segment)
        {
            floorplan_edges_rooms_helper.Add(seg);
        }

        floorplan_edges_rooms = floorplan_edges_rooms_helper.ToArray();
        //Debug.Log(floorplan_edges_rooms.Length);




        //CreateCornerArray
        floorplan_corner_rooms = new Vector2[corners_rooms.Count];
        for (int i = 0; i < corners_rooms.Count; i++) 
        {
            floorplan_corner_rooms[i] = corners_rooms[i];
        }

    }

    void SplitIntoIndividualObjects() 
    {
        /*
        List<Vector2> test = new();
        int j = 0;
        foreach (var cor in floorplan_corner_rooms)
        {
            //if (test.Contains(cor))
                Debug.LogWarning(j + ": " + cor);
            //else
            //    test.Add(cor);
            j++;
        }
        Debug.LogWarning(floorplan_corner_rooms.Length + " und " + test.Count);
        if(floorplan_corner_rooms[0] == floorplan_corner_rooms[30]) 
        {
            Debug.LogWarning("HÄ");
        }
        */
        
        //Create Segment Array Mapping
        List<SegmentDeclaration>[] neighboured_seg = new List<SegmentDeclaration>[floorplan_corner_rooms.Length];
        foreach (var seg in floorplan_edges_rooms)
        {
            if (neighboured_seg[seg.id_from] == null)
                neighboured_seg[seg.id_from] = new();

            if (neighboured_seg[seg.id_to] == null)
                neighboured_seg[seg.id_to] = new();

            neighboured_seg[seg.id_from].Add(seg);
            neighboured_seg[seg.id_to].Add(seg);
        }

        //Add neighboured segments in each segment
        foreach (var seg in floorplan_edges_rooms) 
        {
            SegmentDeclaration other_seg;
            if (neighboured_seg[seg.id_from].Count == 2)
            {
                other_seg = neighboured_seg[seg.id_from][0];
                if (other_seg == seg)
                    other_seg = neighboured_seg[seg.id_from][1];
            }
            else 
            {
                other_seg = null;
                Debug.LogError("Falsche Anzahl an connected segments");
            }
            seg.SetFromNeighbourSegment(other_seg);

            if (neighboured_seg[seg.id_to].Count == 2)
            {
                other_seg = neighboured_seg[seg.id_to][0];
                if (other_seg == seg)
                    other_seg = neighboured_seg[seg.id_to][1];
            }
            else
            {
                other_seg = null;
                Debug.LogError(neighboured_seg[seg.id_to].Count + "da");
            }
            seg.SetToNeighbourSegment(other_seg);
        }

        List<int> all_corner = new();
        for (int i = 0; i < floorplan_corner_rooms.Length-1; i++) 
        {
            all_corner.Add(i);
        }
        List<int> remaining_corners = new();
        List<int> connected_corners = new();

        List<SegmentDeclaration> connected_segments = new();
        int current_corner = -1;

        current_corner = all_corner[0];
        all_corner.RemoveAt(0);
        remaining_corners.Add(current_corner);
        while (all_corner.Count != 0 || remaining_corners.Count != 0) 
        {
            if (remaining_corners.Count != 0)
            {
                current_corner = remaining_corners[0];
                remaining_corners.RemoveAt(0);
                connected_corners.Add(current_corner);
                foreach (var seg in neighboured_seg[current_corner])
                {
                    if (!connected_segments.Contains(seg)) 
                    {
                        connected_segments.Add(seg);
                        if (all_corner.Contains(seg.id_from))
                        {
                            remaining_corners.Add(seg.id_from);
                            all_corner.Remove(seg.id_from);
                        }
                        if (all_corner.Contains(seg.id_to)) 
                        {
                            remaining_corners.Add(seg.id_to);
                            all_corner.Remove(seg.id_to);
                        }
                    }
                }
            }
            else 
            {
                segments_single_elements.Add(connected_segments);
                //Debug.Log("Länge: " + connected_segments.Count);
                connected_segments = new();
                current_corner = all_corner[0];
                all_corner.RemoveAt(0);
                remaining_corners.Add(current_corner);
            }            
        }
        segments_single_elements.Add(connected_segments);
    }

    Vector3 CalculateNewPointPosition(Vector2 point_right, Vector2 point, Vector2 point_left, float angle, float dist) 
    {
        Vector2 vec1 = (point_left - point).normalized;
        Vector2 vec2 = (point_right - point).normalized;

        //Add both vecotrs to calculate the resulting vector and normalize it
        var pos = (vec1 + vec2).normalized;
        //Debug.Log("Winkel an Punkt: "+ point.x + ", "+point.y + " ist: "+ angle);

        //Handle the Case that both vectors are on the same line (e.g. a window in a wall)
        if (pos == Vector2.zero)
        {
            //Debug.Log(vec1.x + ", " + vec1.y);
            pos.x = vec1.y * (-1);
            pos.y = vec1.x ;
            if (angle < 0) 
            {
                pos *= (-1);
                angle = Mathf.Abs(angle);
            }
        }

        if (angle < 180)
        {
            pos *= (dist / Mathf.Sin(((angle / 2) * Mathf.PI) / 180))*(-1);
        }
        else 
        {
            pos *= (dist / Mathf.Sin(((angle / 2) * Mathf.PI) / 180));
        }
        

        //Debug.Log("aktueller Punkt: " + point.x + ", " + point.y + " mit dem Winkel: " + angle);

        return new Vector2(point.x + pos.x, point.y + pos.y);
    }

    private bool PointInTriangle(Vector2 point, Vector2 triangleA, Vector2 triangleB, Vector2 triangleC) 
    {
        // Compute vectors        
        Vector2 v0 = triangleC - triangleA;
        Vector2 v1 = triangleB - triangleA;
        Vector2 v2 = point - triangleA;

        // Compute dot products
        var dot00 = Vector2.Dot(v0, v0);
        var dot01 = Vector2.Dot(v0, v1);
        var dot02 = Vector2.Dot(v0, v2);
        var dot11 = Vector2.Dot(v1, v1);
        var dot12 = Vector2.Dot(v1, v2);

        // Compute barycentric coordinates
        var invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        // Check if point is in triangle
        return (u >= 0) && (v >= 0) && (u + v < 1);
    }

    private bool IntersectingPoints(List<int> points, Vector2 triangleA, Vector2 triangleB, Vector2 triangleC) 
    {
        bool intersect = false;
        foreach (var point in points)
        {
            if (floorplan_corner_rooms[point] != triangleA && floorplan_corner_rooms[point] != triangleB && floorplan_corner_rooms[point] != triangleC && PointInTriangle(floorplan_corner_rooms[point], triangleA, triangleB, triangleC))
            {
                intersect = true;
                //Debug.LogW("test " + "[ Punkt: " + floorplan_corner_rooms[point] + "]: " + triangleA + ", " + triangleB + ", " + triangleC + " --> " + intersect);
                //Debug.LogWarning("doch");
            }
        }
        Debug.Log("test " +"[" + points.Count + "]: " + triangleA + ", " + triangleB + ", " + triangleC + " --> " + intersect);
        return intersect;
    }

    void GenerateFloorAndCeiling() 
    {
        List<SegmentDeclaration>[] neighboured_seg = new List<SegmentDeclaration>[floorplan_corner_rooms.Length];
        foreach (var seg in floorplan_edges_rooms)
        {
            if (neighboured_seg[seg.id_from] == null)
                neighboured_seg[seg.id_from] = new();

            if (neighboured_seg[seg.id_to] == null)
                neighboured_seg[seg.id_to] = new();

            neighboured_seg[seg.id_from].Add(seg);
            neighboured_seg[seg.id_to].Add(seg);
        }

        int room_id = 0;
        foreach (var seg_list in segments_single_elements)
        {
            Debug.Log("---------------------------------" + seg_list[0].room_type_left + " " + seg_list[0].room_type_right + "-----------------------------------------------------");
            if (!(seg_list[0].room_type_left == RoomType.Outside || seg_list[0].room_type_right == RoomType.Outside))
            {
                corner_points_single_elements_floor.Add(new());
                corner_points_uv_single_elements_floor.Add(new());
                triangles_list_single_elements_floor.Add(new());

                corner_points_single_elements_ceiling.Add(new());
                corner_points_uv_single_elements_ceiling.Add(new());
                triangles_list_single_elements_ceiling.Add(new());

                List<int> corner_point_ids = new();

                List<int> remaining_corner_ids = new();
                foreach (var seg in seg_list)
                {
                    if (!remaining_corner_ids.Contains(seg.id_from))
                        remaining_corner_ids.Add(seg.id_from);
                    if (!remaining_corner_ids.Contains(seg.id_to))
                        remaining_corner_ids.Add(seg.id_to);
                }

                if (remaining_corner_ids.Count < 3)
                {
                    Debug.LogError("one room with not enough segments ( Cornercount: " + remaining_corner_ids.Count + ")");
                }

                int index_adapt = 0;
                int index = 0;
                int counter = 0;
                while (remaining_corner_ids.Count >= 3 && counter < 1000)
                {
                    counter++;
                    index = remaining_corner_ids[index_adapt];

                    if (neighboured_seg[index].Count != 2)
                    {
                        Debug.Log(floorplan_corner_rooms[neighboured_seg[index][0].id_from] + ", " + floorplan_corner_rooms[neighboured_seg[index][0].id_to] + " und " + floorplan_corner_rooms[neighboured_seg[index][1].id_from] + ", " + floorplan_corner_rooms[neighboured_seg[index][1].id_to] + " und " + floorplan_corner_rooms[neighboured_seg[index][2].id_from] + ", " + floorplan_corner_rooms[neighboured_seg[index][2].id_to]);
                        Debug.LogError("wrong number of connected segments at " + index + " ( segmemtcount: " + neighboured_seg[index].Count + ")");
                    }
                    SegmentDeclaration seg1 = neighboured_seg[index][0];
                    SegmentDeclaration seg2 = neighboured_seg[index][1];
                    int other_index1 = seg1.id_to;
                    int other_index2 = seg2.id_to;

                    //Debug.Log(remaining_corner_ids.Count + ", ids: " + index + " [" + floorplan_corner_rooms[index] + "] , " + other_index1 + " [" + floorplan_corner_rooms[other_index1] + "] , " +other_index2 + " [" + floorplan_corner_rooms[other_index2] + "]");

                    Vector2 vec1 = floorplan_corner_rooms[seg1.id_to] - floorplan_corner_rooms[seg1.id_from];
                    if (index == seg1.id_to)
                    {
                        vec1 = floorplan_corner_rooms[seg1.id_from] - floorplan_corner_rooms[seg1.id_to];
                        other_index1 = seg1.id_from;
                    }
                    Vector2 vec2 = floorplan_corner_rooms[seg2.id_to] - floorplan_corner_rooms[seg2.id_from];
                    if (index == seg2.id_to)
                    {
                        vec2 = floorplan_corner_rooms[seg2.id_from] - floorplan_corner_rooms[seg2.id_to];
                        other_index2 = seg2.id_from;
                    }

                    float angle;


                    angle = Vector2.SignedAngle(vec1, vec2);


                    if ((seg1.id_from == index && seg1.room_type_left == RoomType.Undefined) || (seg1.id_to == index && seg1.room_type_right == RoomType.Undefined))
                        angle *= -1;

                    bool intersect_with_other_corners = IntersectingPoints(remaining_corner_ids, floorplan_corner_rooms[index], floorplan_corner_rooms[other_index1], floorplan_corner_rooms[other_index2]);
                    if (angle > 0 && angle < 180 && !intersect_with_other_corners)
                    {
                        //Add missing points
                        int id_pointA;
                        if (corner_point_ids.Contains(index))
                        {
                            id_pointA = corner_point_ids.IndexOf(index);
                        }
                        else
                        {
                            corner_point_ids.Add(index);
                            id_pointA = corner_point_ids.Count - 1;
                        }

                        int id_pointB;
                        if (corner_point_ids.Contains(other_index1))
                        {
                            id_pointB = corner_point_ids.IndexOf(other_index1);
                        }
                        else
                        {
                            corner_point_ids.Add(other_index1);
                            id_pointB = corner_point_ids.Count - 1;
                        }

                        int id_pointC;
                        if (corner_point_ids.Contains(other_index2))
                        {
                            id_pointC = corner_point_ids.IndexOf(other_index2);
                        }
                        else
                        {
                            corner_point_ids.Add(other_index2);
                            id_pointC = corner_point_ids.Count - 1;
                        }

                        //Add Triangles
                        Debug.Log("room" + room_id + ": add triangle with angle: " + angle + " ids: " + id_pointA  +" [" + floorplan_corner_rooms[corner_point_ids[id_pointA]] + "] , " + id_pointB + " [" + floorplan_corner_rooms[corner_point_ids[id_pointB]] + "], " + id_pointC + " [" + floorplan_corner_rooms[corner_point_ids[id_pointC]] + "]");
                        if (Vector2.SignedAngle(vec1, vec2) < 0)
                        {
                            triangles_list_single_elements_floor[room_id].Add(id_pointA);
                            triangles_list_single_elements_floor[room_id].Add(id_pointB);
                            triangles_list_single_elements_floor[room_id].Add(id_pointC);

                            triangles_list_single_elements_ceiling[room_id].Add(id_pointA);
                            triangles_list_single_elements_ceiling[room_id].Add(id_pointC);
                            triangles_list_single_elements_ceiling[room_id].Add(id_pointB);
                        }
                        else 
                        {
                            triangles_list_single_elements_floor[room_id].Add(id_pointA);
                            triangles_list_single_elements_floor[room_id].Add(id_pointC);
                            triangles_list_single_elements_floor[room_id].Add(id_pointB);

                            triangles_list_single_elements_ceiling[room_id].Add(id_pointA);
                            triangles_list_single_elements_ceiling[room_id].Add(id_pointB);
                            triangles_list_single_elements_ceiling[room_id].Add(id_pointC);
                        }

                    }

                    if (angle == 180 || angle == -180 || angle == 0 || (angle > 0 && !intersect_with_other_corners))
                    {
                        //Remove Corner
                        remaining_corner_ids.Remove(index);
                        Debug.Log("remove: " + floorplan_corner_rooms[index]);

                        SegmentDeclaration helperSeg = new();
                        helperSeg.id_from = other_index1;
                        helperSeg.id_to = other_index2;

                        if (seg1.id_to == index)
                        {
                            helperSeg.room_type_left = seg1.room_type_left;
                            helperSeg.room_type_right = seg1.room_type_right;
                        }
                        else
                        {
                            helperSeg.room_type_left = seg1.room_type_right;
                            helperSeg.room_type_right = seg1.room_type_left;
                        }

                        neighboured_seg[other_index1].Remove(seg1);
                        neighboured_seg[other_index1].Add(helperSeg);

                        neighboured_seg[other_index2].Remove(seg2);
                        neighboured_seg[other_index2].Add(helperSeg);
                    }


                    index_adapt = (index_adapt + 1) % remaining_corner_ids.Count;
                }
                foreach (var id in corner_point_ids)
                {
                    corner_points_single_elements_floor[room_id].Add(new Vector3(floorplan_corner_rooms[id].x, 0, floorplan_corner_rooms[id].y));
                    corner_points_single_elements_ceiling[room_id].Add(new Vector3(floorplan_corner_rooms[id].x, appartment_height, floorplan_corner_rooms[id].y));
                }

                room_id++;
            } 
        }
    }

    void GenerateWalls_OnSingleObjects()
    {
        // List of all Corner points
        //corner_points
        //List of all triangles
        //triangles_list
        float[] point_heights = new float[8];
        point_heights[0] = 0;                           //Floorplan corner
        point_heights[1] = appartment_height;           //Appartmenthöhe
        point_heights[2] = appartment_height / 2;       //half wall
        point_heights[3] = (appartment_height / 5) * 4; //Tür Oberkante
        point_heights[4] = (appartment_height / 3);     //Fenster Unterkante
        point_heights[5] = (appartment_height / 3) * 2; //Fenster Oberkante
        point_heights[6] = (appartment_height / 6);     //Höhe Unterer Sockel
        point_heights[7] = (appartment_height / 6) * 5; //Höhe oberer Sockel

        foreach (var seg_list in segments_single_elements)
        {

            corner_points = new();
            triangles_list = new();
            corner_points_uv = new();
            corner_points_normals = new();
            //Debug.Log("---------------------------------------" + seg_list.Count +" " + seg_list[0].room_type_left + "|" + seg_list[0].room_type_right + "-----------------------------------------");
            foreach (var segment in seg_list)
            {
                //Debug.Log(segment.segment_id + ": " + floorplan_corner_rooms[segment.id_from] +", "+ floorplan_corner_rooms[segment.id_to] + " mit " + segment.GetOppositeSegment().segment_id + "(" + floorplan_corner_rooms[segment.GetOppositeSegment().id_from] + ", " + floorplan_corner_rooms[segment.GetOppositeSegment().id_to] + ")");
                //Debug.Log(segment.segment_id + ": " + "(" + floorplan_corner_rooms[segment.id_from].x + ". " + floorplan_corner_rooms[segment.id_from].y  + ") "+ ", " + "(" + floorplan_corner_rooms[segment.id_to].x + ". " + floorplan_corner_rooms[segment.id_to].y + ") ");
                //Debug.Log(segment.GetFromNeighbourSegment().segment_id + ", " + segment.GetToNeighbourSegment().segment_id);
                int current_index_corners = corner_points.Count;
                Vector3 direction;
                float adapt_height;
                int other_id;
                SegmentDeclaration segment2;
                Vector3 middlepoint1;
                Vector3 middlepoint2;
                int correction;

                //Add points and triangles
                switch (segment.segment_type)
                {
                    case SegmentType.Wall:
                        //Add points
                        //segment1 [0-3]
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[0], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[0], floorplan_corner_rooms[segment.id_to].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[1], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[1], floorplan_corner_rooms[segment.id_to].y));
                        //UV segment1
                        corner_points_uv.Add(new Vector2(0, 0));
                        corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 1], corner_points[current_index_corners + 0]), 0));
                        corner_points_uv.Add(new Vector2(0, Vector3.Distance(corner_points[current_index_corners + 2], corner_points[current_index_corners + 0])));
                        corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 1], corner_points[current_index_corners + 0]), Vector3.Distance(corner_points[current_index_corners + 2], corner_points[current_index_corners + 0])));

                        /*
                        other_id = segment.segment_id;
                        if (other_id % 2 == 1)
                        {
                            other_id--;
                        }
                        else
                        {
                            other_id++;
                        }
                        Debug.Log(segment.segment_id + ", " + other_id + ", " + floorplan_edges_rooms.Length + " --> " + segment.id_from + ", " + segment.id_to + " " + floorplan_corner_rooms[segment.id_from] + ", " + floorplan_corner_rooms[segment.id_to]);
                        segment2 = floorplan_edges_rooms[other_id];
                        */
                        segment2 = segment.GetOppositeSegment();

                        correction = 0;
                        if (segment2 != null &&(segment.GetFromNeighbourSegment() == null || segment.GetFromNeighbourSegment().segment_type == SegmentType.Empty))
                        {
                            middlepoint1 = (new Vector3(floorplan_corner_rooms[segment2.id_from].x, point_heights[2], floorplan_corner_rooms[segment2.id_from].y) - new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[2], floorplan_corner_rooms[segment.id_from].y)) / 2;
                            //segment_wall_fill_from
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[0], floorplan_corner_rooms[segment.id_from].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[0], floorplan_corner_rooms[segment.id_from].y) + middlepoint1);
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[1], floorplan_corner_rooms[segment.id_from].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[1], floorplan_corner_rooms[segment.id_from].y) + middlepoint1);
                            //UV segment_right
                            corner_points_uv.Add(new Vector2(0, 0));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 5 + correction], corner_points[current_index_corners + 4 + correction]), 0));
                            corner_points_uv.Add(new Vector2(0, Vector3.Distance(corner_points[current_index_corners + 6 + correction], corner_points[current_index_corners + 4 + correction])));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 5 + correction], corner_points[current_index_corners + 4 + correction]), Vector3.Distance(corner_points[current_index_corners + 6 + correction], corner_points[current_index_corners + 4 + correction])));
                            correction += 4;
                        }
                        if (segment2 != null && (segment.GetToNeighbourSegment() == null || segment.GetToNeighbourSegment().segment_type == SegmentType.Empty))
                        {
                            middlepoint2 = (new Vector3(floorplan_corner_rooms[segment2.id_to].x, point_heights[2], floorplan_corner_rooms[segment2.id_to].y) - new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[2], floorplan_corner_rooms[segment.id_to].y)) / 2;
                            //segment_wall_fill_to
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[0], floorplan_corner_rooms[segment.id_to].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[0], floorplan_corner_rooms[segment.id_to].y) + middlepoint2);
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[1], floorplan_corner_rooms[segment.id_to].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[1], floorplan_corner_rooms[segment.id_to].y) + middlepoint2);
                            //UV segment_right
                            corner_points_uv.Add(new Vector2(0, 0));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 5 + correction], corner_points[current_index_corners + 4 + correction]), 0));
                            corner_points_uv.Add(new Vector2(0, Vector3.Distance(corner_points[current_index_corners + 6 + correction], corner_points[current_index_corners + 4 + correction])));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 5 + correction], corner_points[current_index_corners + 4 + correction]), Vector3.Distance(corner_points[current_index_corners + 6 + correction], corner_points[current_index_corners + 4 + correction])));
                        }

                        //Add triangles
                        //Frontside: Clockwise
                        if (segment.room_type_left == RoomType.Undefined)
                        {
                            //#1
                            triangles_list.Add(current_index_corners + 1);
                            triangles_list.Add(current_index_corners);
                            triangles_list.Add(current_index_corners + 3);
                            //#2
                            triangles_list.Add(current_index_corners);
                            triangles_list.Add(current_index_corners + 2);
                            triangles_list.Add(current_index_corners + 3);

                            correction = 0;
                            if (segment2 != null && (segment.GetFromNeighbourSegment() == null || segment.GetFromNeighbourSegment().segment_type == SegmentType.Empty))
                            {
                                //#3
                                triangles_list.Add(current_index_corners + 5 + correction);
                                triangles_list.Add(current_index_corners + 7 + correction);
                                triangles_list.Add(current_index_corners + 4 + correction);
                                //#4
                                triangles_list.Add(current_index_corners + 7 + correction);
                                triangles_list.Add(current_index_corners + 6 + correction);
                                triangles_list.Add(current_index_corners + 4 + correction);

                                correction += 4;
                            }
                            if (segment2 != null && (segment.GetToNeighbourSegment() == null || segment.GetToNeighbourSegment().segment_type == SegmentType.Empty))
                            {
                                //#5
                                triangles_list.Add(current_index_corners + 4 + correction);
                                triangles_list.Add(current_index_corners + 7 + correction);
                                triangles_list.Add(current_index_corners + 5 + correction);
                                //#6
                                triangles_list.Add(current_index_corners + 4 + correction);
                                triangles_list.Add(current_index_corners + 6 + correction);
                                triangles_list.Add(current_index_corners + 7 + correction);
                            }
                        }
                        else 
                        {
                            //#1
                            triangles_list.Add(current_index_corners + 3);
                            triangles_list.Add(current_index_corners);
                            triangles_list.Add(current_index_corners + 1);
                            //#2
                            triangles_list.Add(current_index_corners + 3);
                            triangles_list.Add(current_index_corners + 2);
                            triangles_list.Add(current_index_corners);

                            correction = 0;
                            if (segment2 != null && (segment.GetFromNeighbourSegment() == null || segment.GetFromNeighbourSegment().segment_type == SegmentType.Empty))
                            {
                                //#3
                                triangles_list.Add(current_index_corners + 4 + correction);
                                triangles_list.Add(current_index_corners + 7 + correction);
                                triangles_list.Add(current_index_corners + 5 + correction);
                                //#4
                                triangles_list.Add(current_index_corners + 4 + correction);
                                triangles_list.Add(current_index_corners + 6 + correction);
                                triangles_list.Add(current_index_corners + 7 + correction);

                                correction += 4;
                            }
                            if (segment2 != null && (segment.GetToNeighbourSegment() == null || segment.GetToNeighbourSegment().segment_type == SegmentType.Empty))
                            {
                                //#5
                                triangles_list.Add(current_index_corners + 5 + correction);
                                triangles_list.Add(current_index_corners + 7 + correction);
                                triangles_list.Add(current_index_corners + 4 + correction);
                                //#6
                                triangles_list.Add(current_index_corners + 7 + correction);
                                triangles_list.Add(current_index_corners + 6 + correction);
                                triangles_list.Add(current_index_corners + 4 + correction);
                            }
                        }


                        //Add normals
                        //to the right side
                        direction = new Vector3((floorplan_corner_rooms[segment.id_to] - floorplan_corner_rooms[segment.id_from]).y, 0, (floorplan_corner_rooms[segment.id_to] - floorplan_corner_rooms[segment.id_from]).x * (-1)).normalized;
                        //corner_points_normals
                        corner_points_normals.Add(direction);
                        corner_points_normals.Add(direction);
                        corner_points_normals.Add(direction);
                        corner_points_normals.Add(direction);

                        break;
                    case SegmentType.Wall_half:
                        //Add points
                        //segment1 [0-3]
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[0], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[0], floorplan_corner_rooms[segment.id_to].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[2], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[2], floorplan_corner_rooms[segment.id_to].y));
                        // Add segment1 UV
                        corner_points_uv.Add(new Vector2(0, 0));
                        corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 1], corner_points[current_index_corners + 0]), 0));
                        corner_points_uv.Add(new Vector2(0, Vector3.Distance(corner_points[current_index_corners + 2], corner_points[current_index_corners + 0])));
                        corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 1], corner_points[current_index_corners + 0]), Vector3.Distance(corner_points[current_index_corners + 2], corner_points[current_index_corners + 0])));


                        //segmentTop_half
                        /*
                        other_id = segment.segment_id;
                        if (other_id % 2 == 1)
                        {
                            other_id--;
                        }
                        else 
                        {
                            other_id++;
                        }
                        segment2 = floorplan_edges_rooms[other_id];
                        */
                        segment2 = segment.GetOppositeSegment();

                        middlepoint1 = (new Vector3(floorplan_corner_rooms[segment2.id_from].x, point_heights[2], floorplan_corner_rooms[segment2.id_from].y) - new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[2], floorplan_corner_rooms[segment.id_from].y)) / 2;
                        middlepoint2 = (new Vector3(floorplan_corner_rooms[segment2.id_to].x, point_heights[2], floorplan_corner_rooms[segment2.id_to].y) - new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[2], floorplan_corner_rooms[segment.id_to].y)) / 2;

                        //Debug.Log(segment.segment_id + ", ->" + Vector2.Distance(floorplan_corner_rooms[segment2.id_from], floorplan_corner_rooms[segment2.id_to]) + "<- " + segment2.segment_id + " Mittelpunkt: " + middlepoint1 + ", " + middlepoint2);

                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[2], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[2], floorplan_corner_rooms[segment.id_to].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[2], floorplan_corner_rooms[segment.id_from].y) + middlepoint1);
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[2], floorplan_corner_rooms[segment.id_to].y) + middlepoint2);
                        //UV segmentTop_half
                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 4].x, corner_points[current_index_corners + 4].z));
                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 5].x, corner_points[current_index_corners + 5].z));
                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 6].x, corner_points[current_index_corners + 6].z));
                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 7].x, corner_points[current_index_corners + 7].z));

                        correction = 0;
                        if (segment.GetFromNeighbourSegment() != null && !(segment.GetFromNeighbourSegment().segment_type == SegmentType.Wall_half || segment.GetFromNeighbourSegment().segment_type == SegmentType.Empty))
                        {
                            //segment_left [8-11]
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[2], floorplan_corner_rooms[segment.id_from].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[2], floorplan_corner_rooms[segment.id_from].y) + middlepoint1);
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[1], floorplan_corner_rooms[segment.id_from].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[1], floorplan_corner_rooms[segment.id_from].y) + middlepoint1);
                            //UV segment_left
                            corner_points_uv.Add(new Vector2(0, 0));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 9], corner_points[current_index_corners + 8]), 0));
                            corner_points_uv.Add(new Vector2(0, Vector3.Distance(corner_points[current_index_corners + 10], corner_points[current_index_corners + 8])));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 9], corner_points[current_index_corners + 8]), Vector3.Distance(corner_points[current_index_corners + 10], corner_points[current_index_corners + 8])));
                            correction += 4;
                        }
                        if (segment.GetToNeighbourSegment() != null && !(segment.GetToNeighbourSegment().segment_type == SegmentType.Wall_half || segment.GetToNeighbourSegment().segment_type == SegmentType.Empty))
                        {
                            //segment_right [12-15]
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[2], floorplan_corner_rooms[segment.id_to].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[2], floorplan_corner_rooms[segment.id_to].y) + middlepoint2);
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[1], floorplan_corner_rooms[segment.id_to].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[1], floorplan_corner_rooms[segment.id_to].y) + middlepoint2);
                            //UV segment_right
                            corner_points_uv.Add(new Vector2(0, 0));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 9 + correction], corner_points[current_index_corners + 8 + correction]), 0));
                            corner_points_uv.Add(new Vector2(0, Vector3.Distance(corner_points[current_index_corners + 10 + correction], corner_points[current_index_corners + 8 + correction])));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 9 + correction], corner_points[current_index_corners + 8 + correction]), Vector3.Distance(corner_points[current_index_corners + 10 + correction], corner_points[current_index_corners + 8 + correction])));
                            correction += 4;
                        }
                        if (segment.GetFromNeighbourSegment() == null || segment.GetFromNeighbourSegment().segment_type == SegmentType.Empty)
                        {
                            //segment_wall_fill_from
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[0], floorplan_corner_rooms[segment.id_from].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[0], floorplan_corner_rooms[segment.id_from].y) + middlepoint1);
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[2], floorplan_corner_rooms[segment.id_from].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[2], floorplan_corner_rooms[segment.id_from].y) + middlepoint1);
                            //UV segment_right
                            corner_points_uv.Add(new Vector2(0, 0));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 9 + correction], corner_points[current_index_corners + 8 + correction]), 0));
                            corner_points_uv.Add(new Vector2(0, Vector3.Distance(corner_points[current_index_corners + 10 + correction], corner_points[current_index_corners + 8 + correction])));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 9 + correction], corner_points[current_index_corners + 8 + correction]), Vector3.Distance(corner_points[current_index_corners + 10 + correction], corner_points[current_index_corners + 8 + correction])));
                            correction += 4;
                            
                        }
                        if (segment.GetToNeighbourSegment() == null || segment.GetToNeighbourSegment().segment_type == SegmentType.Empty)
                        {
                            //segment_wall_fill_to
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[0], floorplan_corner_rooms[segment.id_to].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[0], floorplan_corner_rooms[segment.id_to].y) + middlepoint2);
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[2], floorplan_corner_rooms[segment.id_to].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[2], floorplan_corner_rooms[segment.id_to].y) + middlepoint2);
                            //UV segment_right
                            corner_points_uv.Add(new Vector2(0, 0));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 9 + correction], corner_points[current_index_corners + 8 + correction]), 0));
                            corner_points_uv.Add(new Vector2(0, Vector3.Distance(corner_points[current_index_corners + 10 + correction], corner_points[current_index_corners + 8 + correction])));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 9 + correction], corner_points[current_index_corners + 8 + correction]), Vector3.Distance(corner_points[current_index_corners + 10 + correction], corner_points[current_index_corners + 8 + correction])));
                        }

                        //Add triangles
                        //Frontside: Clockwise
                        if (segment.room_type_left == RoomType.Undefined)
                        {
                            //#1
                            triangles_list.Add(current_index_corners + 1);
                            triangles_list.Add(current_index_corners);
                            triangles_list.Add(current_index_corners + 3);
                            //#2
                            triangles_list.Add(current_index_corners);
                            triangles_list.Add(current_index_corners + 2);
                            triangles_list.Add(current_index_corners + 3);
                           
                            //#3d
                            triangles_list.Add(current_index_corners + 4);
                            triangles_list.Add(current_index_corners + 7);
                            triangles_list.Add(current_index_corners + 5);
                            //#4
                            triangles_list.Add(current_index_corners + 4);
                            triangles_list.Add(current_index_corners + 6);
                            triangles_list.Add(current_index_corners + 7);

                            correction = 0;
                            if (segment.GetFromNeighbourSegment() != null && !(segment.GetFromNeighbourSegment().segment_type == SegmentType.Wall_half || segment.GetFromNeighbourSegment().segment_type == SegmentType.Empty))
                            {
                                
                                //#5
                                triangles_list.Add(current_index_corners + 8);
                                triangles_list.Add(current_index_corners + 11);
                                triangles_list.Add(current_index_corners + 9);
                                //#6
                                triangles_list.Add(current_index_corners + 8);
                                triangles_list.Add(current_index_corners + 10);
                                triangles_list.Add(current_index_corners + 11);

                                correction += 4;
                                
                            }
                            if (segment.GetToNeighbourSegment() != null && !(segment.GetToNeighbourSegment().segment_type == SegmentType.Wall_half || segment.GetToNeighbourSegment().segment_type == SegmentType.Empty))
                            {
                                
                                //#7
                                triangles_list.Add(current_index_corners + 9 + correction);
                                triangles_list.Add(current_index_corners + 11 + correction);
                                triangles_list.Add(current_index_corners + 8 + correction);
                                //#8
                                triangles_list.Add(current_index_corners + 11 + correction);
                                triangles_list.Add(current_index_corners + 10 + correction);
                                triangles_list.Add(current_index_corners + 8 + correction);

                                correction += 4;
                                
                            }
                            if (segment.GetFromNeighbourSegment() == null || segment.GetFromNeighbourSegment().segment_type == SegmentType.Empty)
                            { 
                                
                                //#9
                                triangles_list.Add(current_index_corners + 9 + correction);
                                triangles_list.Add(current_index_corners + 11 + correction);
                                triangles_list.Add(current_index_corners + 8 + correction);
                                //#10
                                triangles_list.Add(current_index_corners + 11 + correction);
                                triangles_list.Add(current_index_corners + 10 + correction);
                                triangles_list.Add(current_index_corners + 8 + correction);

                                correction += 4;
                                
                            }
                            if (segment.GetToNeighbourSegment() == null || segment.GetToNeighbourSegment().segment_type == SegmentType.Empty)
                            {
                                
                                //#11
                                triangles_list.Add(current_index_corners + 8 + correction);
                                triangles_list.Add(current_index_corners + 11 + correction);
                                triangles_list.Add(current_index_corners + 9 + correction);
                                //#12
                                triangles_list.Add(current_index_corners + 8 + correction);
                                triangles_list.Add(current_index_corners + 10 + correction);
                                triangles_list.Add(current_index_corners + 11 + correction);
                               
                            }
                        }
                        else 
                        {
                            //#1
                            triangles_list.Add(current_index_corners + 3);
                            triangles_list.Add(current_index_corners);
                            triangles_list.Add(current_index_corners + 1);
                            //#2
                            triangles_list.Add(current_index_corners + 3);
                            triangles_list.Add(current_index_corners + 2);
                            triangles_list.Add(current_index_corners);

                            //#3
                            triangles_list.Add(current_index_corners + 5);
                            triangles_list.Add(current_index_corners + 7);
                            triangles_list.Add(current_index_corners + 4);
                            //#4
                            triangles_list.Add(current_index_corners + 7);
                            triangles_list.Add(current_index_corners + 6);
                            triangles_list.Add(current_index_corners + 4);

                            correction = 0;
                            if (segment.GetFromNeighbourSegment() != null && !(segment.GetFromNeighbourSegment().segment_type == SegmentType.Wall_half || segment.GetFromNeighbourSegment().segment_type == SegmentType.Empty))
                            {
                                
                                //#5
                                triangles_list.Add(current_index_corners + 9);
                                triangles_list.Add(current_index_corners + 11);
                                triangles_list.Add(current_index_corners + 8);
                                //#6
                                triangles_list.Add(current_index_corners + 11);
                                triangles_list.Add(current_index_corners + 10);
                                triangles_list.Add(current_index_corners + 8);

                                correction += 4;
                                
                            }
                            if (segment.GetToNeighbourSegment() != null && !(segment.GetToNeighbourSegment().segment_type == SegmentType.Wall_half || segment.GetToNeighbourSegment().segment_type == SegmentType.Empty))
                            {
                                
                                //#7
                                triangles_list.Add(current_index_corners + 8 + correction);
                                triangles_list.Add(current_index_corners + 11 + correction);
                                triangles_list.Add(current_index_corners + 9 + correction);
                                //#8
                                triangles_list.Add(current_index_corners + 8 + correction);
                                triangles_list.Add(current_index_corners + 10 + correction);
                                triangles_list.Add(current_index_corners + 11 + correction);

                                correction += 4;
                                
                            }
                            if (segment.GetFromNeighbourSegment() == null || segment.GetFromNeighbourSegment().segment_type == SegmentType.Empty)
                            {
                                
                                //#9
                                triangles_list.Add(current_index_corners + 8 + correction);
                                triangles_list.Add(current_index_corners + 11 + correction);
                                triangles_list.Add(current_index_corners + 9 + correction);
                                //#10
                                triangles_list.Add(current_index_corners + 8 + correction);
                                triangles_list.Add(current_index_corners + 10 + correction);
                                triangles_list.Add(current_index_corners + 11 + correction);

                                correction += 4;
                                
                            }
                            if (segment.GetToNeighbourSegment() == null || segment.GetToNeighbourSegment().segment_type == SegmentType.Empty)
                            {
                                
                                //#11
                                triangles_list.Add(current_index_corners + 9 + correction);
                                triangles_list.Add(current_index_corners + 11 + correction);
                                triangles_list.Add(current_index_corners + 8 + correction);
                                //#12
                                triangles_list.Add(current_index_corners + 11 + correction);
                                triangles_list.Add(current_index_corners + 10 + correction);
                                triangles_list.Add(current_index_corners + 8 + correction);
                                
                            }
                        }

                        //TODO im richtigen Winkel berechnen
                        //corner_points_uv.Add(new Vector2(0, 0));
                        //corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 5], corner_points[current_index_corners + 4]), 0));


                        //Add normals
                        //to the right side
                        direction = new Vector3((floorplan_corner_rooms[segment.id_to] - floorplan_corner_rooms[segment.id_from]).y, 0, (floorplan_corner_rooms[segment.id_to] - floorplan_corner_rooms[segment.id_from]).x * (-1)).normalized;
                        //corner_points_normals
                        corner_points_normals.Add(direction);
                        corner_points_normals.Add(direction);
                        corner_points_normals.Add(direction);
                        corner_points_normals.Add(direction);
                        break;

                    case SegmentType.Window:
                        //Add points
                        //segment_bottom [0-3]
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[0], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[0], floorplan_corner_rooms[segment.id_to].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[4], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[4], floorplan_corner_rooms[segment.id_to].y));
                        //UV segment_bottom
                        corner_points_uv.Add(new Vector2(0, 0));
                        corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 1], corner_points[current_index_corners + 0]), 0));
                        corner_points_uv.Add(new Vector2(0, Vector3.Distance(corner_points[current_index_corners + 2], corner_points[current_index_corners + 0])));
                        corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 1], corner_points[current_index_corners + 0]), Vector3.Distance(corner_points[current_index_corners + 2], corner_points[current_index_corners + 0])));

                        //segmentTop_half
                        /*
                        other_id = segment.segment_id;
                        if (other_id % 2 == 1)
                        {
                            other_id--;
                        }
                        else
                        {
                            other_id++;
                        }
                        segment2 = floorplan_edges_rooms[other_id];
                        */
                        segment2 = segment.GetOppositeSegment();
                        middlepoint1 = (new Vector3(floorplan_corner_rooms[segment2.id_from].x, point_heights[4], floorplan_corner_rooms[segment2.id_from].y) - new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[4], floorplan_corner_rooms[segment.id_from].y)) / 2;
                        middlepoint2 = (new Vector3(floorplan_corner_rooms[segment2.id_to].x, point_heights[4], floorplan_corner_rooms[segment2.id_to].y) - new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[4], floorplan_corner_rooms[segment.id_to].y)) / 2;

                        //segment_bottom_upperpart [4-7]
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[4], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[4], floorplan_corner_rooms[segment.id_to].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[4], floorplan_corner_rooms[segment.id_from].y) + middlepoint1);
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[4], floorplan_corner_rooms[segment.id_to].y) + middlepoint2);
                        //UV segment_bottom_upperpart
                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 4].x, corner_points[current_index_corners + 4].z));
                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 5].x, corner_points[current_index_corners + 5].z));
                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 6].x, corner_points[current_index_corners + 6].z));
                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 7].x, corner_points[current_index_corners + 7].z));


                        //segment_top [8-11]
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[5], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[5], floorplan_corner_rooms[segment.id_to].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[1], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[1], floorplan_corner_rooms[segment.id_to].y));
                        //UV segment_top
                        corner_points_uv.Add(new Vector2(0, 0));
                        corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 9], corner_points[current_index_corners + 8]), 0));
                        corner_points_uv.Add(new Vector2(0, Vector3.Distance(corner_points[current_index_corners + 10], corner_points[current_index_corners + 8])));
                        corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 9], corner_points[current_index_corners + 8]), Vector3.Distance(corner_points[current_index_corners + 10], corner_points[current_index_corners + 8])));


                        //segment_top_lowerpart [12-15]
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[5], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[5], floorplan_corner_rooms[segment.id_to].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[5], floorplan_corner_rooms[segment.id_from].y) + middlepoint1);
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[5], floorplan_corner_rooms[segment.id_to].y) + middlepoint2);
                        //UV segment_top_lowerpart
                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 12].x, corner_points[current_index_corners + 12].z));
                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 13].x, corner_points[current_index_corners + 13].z));
                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 14].x, corner_points[current_index_corners + 14].z));
                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 15].x, corner_points[current_index_corners + 15].z));

                        correction = 0;
                        if (segment.GetFromNeighbourSegment() != null && (segment.GetFromNeighbourSegment().segment_type != SegmentType.Window || segment2.GetFromNeighbourSegment().segment_type != SegmentType.Window))
                        {
                            //segment_left [16-19]
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[4], floorplan_corner_rooms[segment.id_from].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[4], floorplan_corner_rooms[segment.id_from].y) + middlepoint1);
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[5], floorplan_corner_rooms[segment.id_from].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[5], floorplan_corner_rooms[segment.id_from].y) + middlepoint1);
                            //UV segment_left
                            corner_points_uv.Add(new Vector2(0, 0));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 17], corner_points[current_index_corners + 16]), 0));
                            corner_points_uv.Add(new Vector2(0, Vector3.Distance(corner_points[current_index_corners + 18], corner_points[current_index_corners + 16])));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 17], corner_points[current_index_corners + 16]), Vector3.Distance(corner_points[current_index_corners + 18], corner_points[current_index_corners + 16])));

                            correction += 4;
                        }
                        if (segment.GetToNeighbourSegment() != null && (segment.GetToNeighbourSegment().segment_type != SegmentType.Window || segment2.GetToNeighbourSegment().segment_type != SegmentType.Window))
                        {
                            //segment_right [20-23]
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[4], floorplan_corner_rooms[segment.id_to].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[4], floorplan_corner_rooms[segment.id_to].y) + middlepoint2);
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[5], floorplan_corner_rooms[segment.id_to].y));
                            corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[5], floorplan_corner_rooms[segment.id_to].y) + middlepoint2);
                            //UV segment_right
                            corner_points_uv.Add(new Vector2(0, 0));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 17 + correction], corner_points[current_index_corners + 16 + correction]), 0));
                            corner_points_uv.Add(new Vector2(0, Vector3.Distance(corner_points[current_index_corners + 18 + correction], corner_points[current_index_corners + 16 + correction])));
                            corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 17 + correction], corner_points[current_index_corners + 16 + correction]), Vector3.Distance(corner_points[current_index_corners + 18 + correction], corner_points[current_index_corners + 16 + correction])));
                        }

                        //Add UV






                        //Add triangles
                        //Frontside: Clockwise
                        if (segment.room_type_left == RoomType.Undefined)
                        {
                            //#1
                            triangles_list.Add(current_index_corners + 1);
                            triangles_list.Add(current_index_corners);
                            triangles_list.Add(current_index_corners + 3);
                            //#2
                            triangles_list.Add(current_index_corners);
                            triangles_list.Add(current_index_corners + 2);
                            triangles_list.Add(current_index_corners + 3);
                            //#3
                            triangles_list.Add(current_index_corners + 4);
                            triangles_list.Add(current_index_corners + 7);
                            triangles_list.Add(current_index_corners + 5);
                            //#4
                            triangles_list.Add(current_index_corners + 4);
                            triangles_list.Add(current_index_corners + 6);
                            triangles_list.Add(current_index_corners + 7);
                            //#5
                            triangles_list.Add(current_index_corners + 9);
                            triangles_list.Add(current_index_corners + 8);
                            triangles_list.Add(current_index_corners + 11);
                            //#6
                            triangles_list.Add(current_index_corners + 8);
                            triangles_list.Add(current_index_corners + 10);
                            triangles_list.Add(current_index_corners + 11);
                            //#7
                            triangles_list.Add(current_index_corners + 13);
                            triangles_list.Add(current_index_corners + 15);
                            triangles_list.Add(current_index_corners + 12);
                            //#8
                            triangles_list.Add(current_index_corners + 15);
                            triangles_list.Add(current_index_corners + 14);
                            triangles_list.Add(current_index_corners + 12);
                            correction = 0;
                            if (segment.GetFromNeighbourSegment() != null && (segment.GetFromNeighbourSegment().segment_type != SegmentType.Window || segment2.GetFromNeighbourSegment().segment_type != SegmentType.Window))
                            {
                                //#9
                                triangles_list.Add(current_index_corners + 16);
                                triangles_list.Add(current_index_corners + 19);
                                triangles_list.Add(current_index_corners + 17);
                                //#10
                                triangles_list.Add(current_index_corners + 16);
                                triangles_list.Add(current_index_corners + 18);
                                triangles_list.Add(current_index_corners + 19);
                                correction += 4;
                            }
                            if (segment.GetToNeighbourSegment() != null && (segment.GetToNeighbourSegment().segment_type != SegmentType.Window || segment2.GetToNeighbourSegment().segment_type != SegmentType.Window))
                            {
                                //#11
                                triangles_list.Add(current_index_corners + 17 + correction);
                                triangles_list.Add(current_index_corners + 19 + correction);
                                triangles_list.Add(current_index_corners + 16 + correction);
                                //#102
                                triangles_list.Add(current_index_corners + 19 + correction);
                                triangles_list.Add(current_index_corners + 18 + correction);
                                triangles_list.Add(current_index_corners + 16 + correction);
                            }
                        }
                        else
                        {
                            //#1
                            triangles_list.Add(current_index_corners + 3);
                            triangles_list.Add(current_index_corners);
                            triangles_list.Add(current_index_corners + 1);
                            //#2
                            triangles_list.Add(current_index_corners + 3);
                            triangles_list.Add(current_index_corners + 2);
                            triangles_list.Add(current_index_corners);
                            //#3
                            triangles_list.Add(current_index_corners + 5);
                            triangles_list.Add(current_index_corners + 7);
                            triangles_list.Add(current_index_corners + 4);
                            //#4
                            triangles_list.Add(current_index_corners + 7);
                            triangles_list.Add(current_index_corners + 6);
                            triangles_list.Add(current_index_corners + 4);
                            //#5
                            triangles_list.Add(current_index_corners + 11);
                            triangles_list.Add(current_index_corners + 8);
                            triangles_list.Add(current_index_corners + 9);
                            //#6
                            triangles_list.Add(current_index_corners + 11);
                            triangles_list.Add(current_index_corners + 10);
                            triangles_list.Add(current_index_corners + 8);
                            //#7
                            triangles_list.Add(current_index_corners + 12);
                            triangles_list.Add(current_index_corners + 15);
                            triangles_list.Add(current_index_corners + 13);
                            //#8
                            triangles_list.Add(current_index_corners + 12);
                            triangles_list.Add(current_index_corners + 14);
                            triangles_list.Add(current_index_corners + 15);
                            correction = 0;
                            if (segment.GetFromNeighbourSegment() != null && (segment.GetFromNeighbourSegment().segment_type != SegmentType.Window || segment2.GetFromNeighbourSegment().segment_type != SegmentType.Window))
                            {
                                //#9
                                triangles_list.Add(current_index_corners + 17);
                                triangles_list.Add(current_index_corners + 19);
                                triangles_list.Add(current_index_corners + 16);
                                //#10
                                triangles_list.Add(current_index_corners + 19);
                                triangles_list.Add(current_index_corners + 18);
                                triangles_list.Add(current_index_corners + 16);

                                correction += 4;
                            }
                            if (segment.GetToNeighbourSegment() != null && (segment.GetToNeighbourSegment().segment_type != SegmentType.Window || segment2.GetToNeighbourSegment().segment_type != SegmentType.Window))
                            {
                                //#11
                                triangles_list.Add(current_index_corners + 16 + correction);
                                triangles_list.Add(current_index_corners + 19 + correction);
                                triangles_list.Add(current_index_corners + 17 + correction);
                                //#102
                                triangles_list.Add(current_index_corners + 16 + correction);
                                triangles_list.Add(current_index_corners + 18 + correction);
                                triangles_list.Add(current_index_corners + 19 + correction);
                            }
                        }


                        //TODO im richtigen Winkel berechnen
                        //corner_points_uv.Add(new Vector2(0, 0));
                        //corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 5], corner_points[current_index_corners + 4]), 0));


                        //Add normals
                        //to the right side
                        direction = new Vector3((floorplan_corner_rooms[segment.id_to] - floorplan_corner_rooms[segment.id_from]).y, 0, (floorplan_corner_rooms[segment.id_to] - floorplan_corner_rooms[segment.id_from]).x * (-1)).normalized;
                        //corner_points_normals
                        corner_points_normals.Add(direction);
                        corner_points_normals.Add(direction);
                        corner_points_normals.Add(direction);
                        corner_points_normals.Add(direction);

                        break;
                    case SegmentType.Window_low:
                        break;
                    case SegmentType.Window_high:
                        break;
                    case SegmentType.Window_full:
                        break;
                    case SegmentType.Door:
                        //Add points
                        //segment_top [0-3]
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[3], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[3], floorplan_corner_rooms[segment.id_to].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[1], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[1], floorplan_corner_rooms[segment.id_to].y));

                        //segmentTop_half
                        /*
                        other_id = segment.segment_id;
                        if (other_id % 2 == 1)
                        {
                            other_id--;
                        }
                        else
                        {
                            other_id++;
                        }
                        segment2 = floorplan_edges_rooms[other_id];
                        */
                        segment2 = segment.GetOppositeSegment();
                        middlepoint1 = (new Vector3(floorplan_corner_rooms[segment2.id_from].x, point_heights[3], floorplan_corner_rooms[segment2.id_from].y) - new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[3], floorplan_corner_rooms[segment.id_from].y)) / 2;
                        middlepoint2 = (new Vector3(floorplan_corner_rooms[segment2.id_to].x, point_heights[3], floorplan_corner_rooms[segment2.id_to].y) - new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[3], floorplan_corner_rooms[segment.id_to].y)) / 2;

                        //segment_top_lowerpart [4-7]
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[3], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[3], floorplan_corner_rooms[segment.id_to].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[3], floorplan_corner_rooms[segment.id_from].y) + middlepoint1);
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[3], floorplan_corner_rooms[segment.id_to].y) + middlepoint2);

                        //segment_left [8-11]
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[0], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[0], floorplan_corner_rooms[segment.id_from].y) + middlepoint1);
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[3], floorplan_corner_rooms[segment.id_from].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_from].x, point_heights[3], floorplan_corner_rooms[segment.id_from].y) + middlepoint1);

                        //segment_right [12-15]
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[0], floorplan_corner_rooms[segment.id_to].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[0], floorplan_corner_rooms[segment.id_to].y) + middlepoint2);
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[3], floorplan_corner_rooms[segment.id_to].y));
                        corner_points.Add(new Vector3(floorplan_corner_rooms[segment.id_to].x, point_heights[3], floorplan_corner_rooms[segment.id_to].y) + middlepoint2);

                        //Add triangles
                        //Frontside: Clockwise
                        if (segment.room_type_left == RoomType.Undefined)
                        {
                            //#1
                            triangles_list.Add(current_index_corners + 1);
                            triangles_list.Add(current_index_corners);
                            triangles_list.Add(current_index_corners + 3);
                            //#2
                            triangles_list.Add(current_index_corners);
                            triangles_list.Add(current_index_corners + 2);
                            triangles_list.Add(current_index_corners + 3);
                            //#3
                            triangles_list.Add(current_index_corners + 5);
                            triangles_list.Add(current_index_corners + 7);
                            triangles_list.Add(current_index_corners + 4);
                            //#4
                            triangles_list.Add(current_index_corners + 7);
                            triangles_list.Add(current_index_corners + 6);
                            triangles_list.Add(current_index_corners + 4);
                            //#5
                            triangles_list.Add(current_index_corners + 8);
                            triangles_list.Add(current_index_corners + 11);
                            triangles_list.Add(current_index_corners + 9);
                            //#6
                            triangles_list.Add(current_index_corners + 8);
                            triangles_list.Add(current_index_corners + 10);
                            triangles_list.Add(current_index_corners + 11);
                            //#7
                            triangles_list.Add(current_index_corners + 13);
                            triangles_list.Add(current_index_corners + 15);
                            triangles_list.Add(current_index_corners + 12);
                            //#8
                            triangles_list.Add(current_index_corners + 15);
                            triangles_list.Add(current_index_corners + 14);
                            triangles_list.Add(current_index_corners + 12);
                        }
                        else
                        {
                            //#1
                            triangles_list.Add(current_index_corners + 3);
                            triangles_list.Add(current_index_corners);
                            triangles_list.Add(current_index_corners + 1);
                            //#2
                            triangles_list.Add(current_index_corners + 3);
                            triangles_list.Add(current_index_corners + 2);
                            triangles_list.Add(current_index_corners);
                            //#3
                            triangles_list.Add(current_index_corners + 4);
                            triangles_list.Add(current_index_corners + 7);
                            triangles_list.Add(current_index_corners + 5);
                            //#4
                            triangles_list.Add(current_index_corners + 4);
                            triangles_list.Add(current_index_corners + 6);
                            triangles_list.Add(current_index_corners + 7);
                            //#5
                            triangles_list.Add(current_index_corners + 9);
                            triangles_list.Add(current_index_corners + 11);
                            triangles_list.Add(current_index_corners + 8);
                            //#6
                            triangles_list.Add(current_index_corners + 11);
                            triangles_list.Add(current_index_corners + 10);
                            triangles_list.Add(current_index_corners + 8);
                            //#7
                            triangles_list.Add(current_index_corners + 12);
                            triangles_list.Add(current_index_corners + 15);
                            triangles_list.Add(current_index_corners + 13);
                            //#8
                            triangles_list.Add(current_index_corners + 12);
                            triangles_list.Add(current_index_corners + 14);
                            triangles_list.Add(current_index_corners + 15);
                        }



                        //Add UV
                        corner_points_uv.Add(new Vector2(0, 0));
                        corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 1], corner_points[current_index_corners + 0]), 0));
                        corner_points_uv.Add(new Vector2(0, Vector3.Distance(corner_points[current_index_corners + 2], corner_points[current_index_corners + 0])));
                        corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 1], corner_points[current_index_corners + 0]), Vector3.Distance(corner_points[current_index_corners + 2], corner_points[current_index_corners + 0])));

                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 4].x, corner_points[current_index_corners + 4].z));
                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 5].x, corner_points[current_index_corners + 5].z));
                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 6].x, corner_points[current_index_corners + 6].z));
                        corner_points_uv.Add(new Vector2(corner_points[current_index_corners + 7].x, corner_points[current_index_corners + 7].z));

                        corner_points_uv.Add(new Vector2(0, 0));
                        corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 9], corner_points[current_index_corners + 8]), 0));
                        corner_points_uv.Add(new Vector2(0, Vector3.Distance(corner_points[current_index_corners + 10], corner_points[current_index_corners + 8])));
                        corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 9], corner_points[current_index_corners + 8]), Vector3.Distance(corner_points[current_index_corners + 10], corner_points[current_index_corners + 8])));

                        corner_points_uv.Add(new Vector2(0, 0));
                        corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 13], corner_points[current_index_corners + 12]), 0));
                        corner_points_uv.Add(new Vector2(0, Vector3.Distance(corner_points[current_index_corners + 14], corner_points[current_index_corners + 12])));
                        corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 13], corner_points[current_index_corners + 12]), Vector3.Distance(corner_points[current_index_corners + 14], corner_points[current_index_corners + 12])));

                        //TODO im richtigen Winkel berechnen
                        //corner_points_uv.Add(new Vector2(0, 0));
                        //corner_points_uv.Add(new Vector2(Vector3.Distance(corner_points[current_index_corners + 5], corner_points[current_index_corners + 4]), 0));


                        //Add normals
                        //to the right side
                        direction = new Vector3((floorplan_corner_rooms[segment.id_to] - floorplan_corner_rooms[segment.id_from]).y, 0, (floorplan_corner_rooms[segment.id_to] - floorplan_corner_rooms[segment.id_from]).x * (-1)).normalized;
                        //corner_points_normals
                        corner_points_normals.Add(direction);
                        corner_points_normals.Add(direction);
                        corner_points_normals.Add(direction);
                        corner_points_normals.Add(direction);
                        break;
                    case SegmentType.Door_full:
                        break;
                    case SegmentType.Door_WindowAbove:
                        break;
                    default:
                        break;
                }
            }
            corner_points_single_elements.Add(corner_points);
            corner_points_uv_single_elements.Add(corner_points_uv);
            triangles_list_single_elements.Add(triangles_list);
        }

        vertices_apartment = corner_points.ToArray();
        triangles_apartment = triangles_list.ToArray();
        //normals_apartment = corner_points_normals.ToArray();
        uv_apartment = corner_points_uv.ToArray();
    }

    private void OnDrawGizmos()
    {
        if (vertices_apartment == null)
            return;

        if (drawGizmosPoints)
        {
            Gizmos.color = Color.black;
            for (int i = 0; i < floorplan_corner_INPUT.Length; i++)
            {
                Gizmos.DrawSphere(new Vector3(floorplan_corner_INPUT[i].x, 0, floorplan_corner_INPUT[i].y), 0.05f);
            }
            

            //Gizmos.color = Color.red;
            //Gizmos.DrawSphere(floorplan_inner[1], 0.05f);
            /*
            Gizmos.color = Color.red;
            for (int i = 0; i < floorplan_inner.Length; i++)
            {
                Gizmos.DrawSphere(floorplan_inner[i], 0.05f);
            }

            Gizmos.color = Color.blue;
            for (int i = 0; i < floorplan_outer.Length; i++)
            {
                Gizmos.DrawSphere(floorplan_outer[i], 0.05f);
            }
            */


            Gizmos.color = Color.cyan;
            for (int i = 0; i < floorplan_corner_rooms.Length; i++)
            {
                Gizmos.DrawSphere(new Vector3(floorplan_corner_rooms[i].x, 0, floorplan_corner_rooms[i].y), 0.05f);
            }

            /*
            Gizmos.color = Color.white;
            for (int i = 0; i < corner_points.Count; i++)
            {
                Gizmos.DrawSphere(corner_points[i], 0.05f);
            }
            */

        }

        
        if (drawGizmosLines)
        {
            foreach (var seg_list in segments_single_elements)
            {

                foreach (var segment in seg_list)
                {
                    Gizmos.DrawLine(new Vector3(floorplan_corner_rooms[segment.id_from].x, 0, floorplan_corner_rooms[segment.id_from].y), new Vector3(floorplan_corner_rooms[segment.id_to].x, 0, floorplan_corner_rooms[segment.id_to].y));
                }
            }

                    Gizmos.color = Color.white;
            foreach (var segment in floorplan_edges_INPUT)
            {
                Gizmos.DrawLine(new Vector3 (floorplan_corner_INPUT[segment.id_from].x, 0, floorplan_corner_INPUT[segment.id_from].y), new Vector3(floorplan_corner_INPUT[segment.id_to].x, 0, floorplan_corner_INPUT[segment.id_to].y));
            }

            
            Gizmos.color = Color.green;
            foreach (var segment in floorplan_edges_rooms)
            {
                RoomType t = segment.room_type_left;
                if (t == RoomType.Undefined)
                {
                    
                    t = segment.room_type_right;
                }
                if (segment.room_type_left == RoomType.Undefined && segment.room_type_right == RoomType.Undefined)
                    Debug.Log("ÄHM");
                switch (t)
                {
                    case RoomType.Outside:
                        Gizmos.color = Color.green;
                        break;
                    case RoomType.Floor:
                        Gizmos.color = Color.blue;
                        break;
                    case RoomType.LivingRoom:
                        Gizmos.color = Color.red;
                        break;
                    case RoomType.Kitchen:
                        Gizmos.color = Color.black;
                        break;
                    case RoomType.Bathroom:
                        break;
                    case RoomType.WC:
                        break;
                    case RoomType.Studyroom:
                        break;
                    case RoomType.Bedroom:
                        Gizmos.color = Color.yellow;
                        break;
                    case RoomType.Kidsroom:
                        break;
                    default:
                        break;
                }
                Gizmos.DrawLine(new Vector3(floorplan_corner_rooms[segment.id_from].x, 0, floorplan_corner_rooms[segment.id_from].y), new Vector3(floorplan_corner_rooms[segment.id_to].x, 0, floorplan_corner_rooms[segment.id_to].y));
            }
            
        }
    }
}
