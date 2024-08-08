using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;
using System;
using System.Linq;

public class RoomCreator : MonoBehaviour
{
    public ShapeCreator shapeCreator;

    public Color roomColor = Color.green;
    public float roomDrawSize = 0.5f;

    public bool useRandomRooms = false;
    public RandomDistributionMethod randDistMethod = RandomDistributionMethod.fullRandom;
    public int roomNumber = 9;
    public int randomSeed = 42;
    public float useMatrixCellSize = 0.25f;
    public int roomInfluenceRange = 10;
    public bool useDifferentRoomSizes = false;
    public bool useDifferentRoomSizesTESTVALUE = false;
    public String roomGeneratingSizes = "";
    public bool shuffleSizeList = false;
    public bool drawRoomPositions = true;
    public bool drawRoomLabels = true;

    public bool drawMatrix = false;
    public float matrixDrawSize = 0.25f;
    public bool drawInnerSplitting = true;

    public bool drawSpecifcSegment = false;
    public int drawSpecificSegmentNum = 1;
    public bool showDebugLog = false;
    public int show_polygonID = 0;
    public int show_iteration = 0;

    public bool drawCornerPoints = false;
    public bool drawOutsideSegments = false;
    public float smallCornersTreshold = 1.5f;
    public bool checkForSmallCorners = false;
    public bool straightenEdges = false;
    public int numOfIterationsForStraightenEdges = 100;

    public bool simplifyEdges = false;
    public bool allowShortCornerSwap = false;
    public bool allowMoreSegmentShifts = false;
    public bool drawSmallEdges = false;
    public float smallSegmentTreshold = 1.5f;
    public int numOfSimplifyIterations = 5;
    public int numOfSimplifyOperationsPerIteration = 200;

    public bool showVorornoiSplitsForSpecificRoomTEST = false;
    public int showPolygonNumberTEST = 0;
    public int showRoomNumberTEST = 0;

    public bool drawSpecificPoint = false;
    public int specificPointIndex = 0;
    public float specificPointSize = 0.2f;

    public bool drawSpecificOutsideSegment = false;
    public int specificOutsideSegmentIndex = 0;

    public int drawCounter = 100;

    //[HideInInspector]
    public List<Vector3> roomPositions = new List<Vector3>();

    //[HideInInspector]
    public List<float> roomSizes = new List<float>();

    public List<RoomType> roomTypes = new List<RoomType>();

    List<Vector3> rand_roomPositions = new List<Vector3>();
    List<float> rand_roomSizes = new List<float>();
    List<RoomType> rand_roomTypes = new List<RoomType>();

    List<Vector2>[] points;
    List<Vector2>[] directions;
    List<Vector2>[] intersections;
    List<List<Vector2[]>>[] test = new List<List<Vector2[]>>[0];
    List<Vector2[]> splittingPoints;
    List<Vector2[]> splittingPointsTEST;
    List<bool[]> isCut;
    List<bool[]> isCutTEST;
    List<Vector2[]>[] allP = new List<Vector2[]>[0];
    List<List<int>> connectivity;
    List<Vector2> allPoints;

    List<Segment> segments;
    List<Vector2> pointsVoronoi;
    List<Segment> outsideSegments;
    List<Segment> problemSegments;

    Segment[][] segmentsPerPolygon;
    Vector2[][] pointsVoronoiPerPolygon;
    Segment[][] outsideSegmentsPerPolygon;
    Segment[][] problemSegmentsPerPolygon;


    List<Segment> segments_straightened;
    List<Vector2> pointsVoronoi_straightened;
    List<Segment> outsideSegments_straightened;
    Segment[][] segmentsPerPolygon_straightened;
    Vector2[][] pointsVoronoiPerPolygon_straightened;
    Segment[][] outsideSegmentsPerPolygon_straightened;
    Segment[][] segmentsPerPolygon_straightened_cleanCOPY;
    Vector2[][] pointsVoronoiPerPolygon_straightened_cleanCOPY;
    Segment[][] outsideSegmentsPerPolygon_straightened_cleanCOPY;

    List<Segment> segments_simplyfied;
    List<Vector2> pointsVoronoi_simplyfied;
    List<Segment> outsideSegments_simplyfied;
    List<Segment> smallSegments_simplyfied;
    Segment[][] segmentsPerPolygon_simplyfied;
    Vector2[][] pointsVoronoiPerPolygon_simplyfied;
    Segment[][] outsideSegmentsPerPolygon_simplyfied;
    Segment[][] smallSegmentsPerPolygon_simplyfied;

    List<Vector2[]>[] smallCorners;

    Polygon[] polygons;

    List<MatrixData[,]> matrixPerPolygon = new List<MatrixData[,]>();
    List<int>[] timesPerPolygon;

    string usedMethod = "";

    int time = 0;
    public void UpdateRoomDisplay()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        shapeCreator.UpdateMeshDisplay();
        
        polygons = shapeCreator.GetPolygons();
        List<int[]> polygonTriangles = new List<int[]>();

        List<List<int>> roomIdsPerPolygon = new List<List<int>>();

        //Get Triangles to all Polygons
        for (int i = 0; i < polygons.Length; i++)
        {
            Triangulator triangulator = new Triangulator(polygons[i]);
            polygonTriangles.Add(triangulator.Triangulate());
        }

        if (useRandomRooms)
        {
            stopwatch.Start();
            //Debug.LogError("USING RANDOM ROOMS");
            System.Random r = new System.Random(randomSeed);
            matrixPerPolygon = new List<MatrixData[,]>();

            rand_roomPositions = new List<Vector3>();
            rand_roomSizes = new List<float>();
            rand_roomTypes = new List<RoomType>();

            for (int i = 0; i < polygons.Length; i++)
            {
                Polygon p = polygons[i];
                float x_min = float.PositiveInfinity;
                float x_max = float.NegativeInfinity;
                float y_min = float.PositiveInfinity;
                float y_max = float.NegativeInfinity;
                foreach (Vector2 point in p.points)
                {
                    if (point.x > x_max)
                    {
                        x_max = point.x;
                    }
                    if (point.x < x_min)
                    {
                        x_min = point.x;
                    }
                    if (point.y > y_max)
                    {
                        y_max = point.y;
                    }
                    if (point.y < y_min)
                    {
                        y_min = point.y;
                    }
                }

                float x_diff = x_max - x_min;
                float y_diff = y_max - y_min;

                int[] polygonTrianglesOfP = polygonTriangles[i];

                GetRooms(out Vector2[] generated_pos, out float[] generated_sizes, out RoomType[] generated_types, roomNumber, x_diff, y_diff, x_min, y_min, p, polygonTrianglesOfP, r);

                //Debug.LogError(i + " asdfghj " + generated_pos.Length);

                for (int k = 0; k < generated_pos.Length; k++)
                {
                    rand_roomPositions.Add(new Vector3(generated_pos[k].x, 0, generated_pos[k].y));
                    rand_roomSizes.Add(generated_sizes[k]);
                    rand_roomTypes.Add(generated_types[k]);
                }
            }

            //Identify the correct Polygon for each room
            for (int i = 0; i < rand_roomPositions.Count; i++)
            {
                for (int k = 0; k < polygons.Length; k++)
                {
                    roomIdsPerPolygon.Add(new List<int>());
                    if (PointInsidePolygon(polygons[k], polygonTriangles[k], rand_roomPositions[i]))
                    {
                        roomIdsPerPolygon[k].Add(i);
                    }
                }
            }
            stopwatch.Stop();
            Debug.LogWarning("ROOMTIME: " + stopwatch.Elapsed.TotalMilliseconds);

        }
        else 
        {
            rand_roomPositions = new List<Vector3>();
            rand_roomSizes = new List<float>();
            rand_roomTypes = new List<RoomType>();

            rand_roomPositions = roomPositions;
            rand_roomSizes = roomSizes;
            rand_roomTypes = roomTypes;

            //Identify the correct Polygon for each room
            for (int i = 0; i < rand_roomPositions.Count; i++)
            {
                for (int k = 0; k < polygons.Length; k++)
                {
                    roomIdsPerPolygon.Add(new List<int>());
                    if (PointInsidePolygon(polygons[k], polygonTriangles[k], rand_roomPositions[i]))
                    {
                        roomIdsPerPolygon[k].Add(i);
                    }
                }
            }
        }
        

        /*
        points = new List<Vector2>[polygons.Length];
        directions = new List<Vector2>[polygons.Length];
        intersections = new List<Vector2>[polygons.Length];
        //Debug.Log("--------------------------------");
        for (int i = 0; i < polygons.Length; i++) 
        {
            //At max 1 room inside the building --> the whole Building belong to that room, no additional walls needed
            if (roomIdsPerPolygon[i].Count <= 1) 
            {
                break;
            }

            //More then 1 room
            for (int k = 0; k < roomIdsPerPolygon[i].Count; k++) 
            {
                Vector3 roomPos = rand_roomPositions[roomIdsPerPolygon[i][k]];
                float roomSize = rand_roomSizes[roomIdsPerPolygon[i][k]];
                for (int m = 0; m < roomIdsPerPolygon[i].Count; m++) 
                {
                    if (m != k) 
                    {
                        Vector3 otherRoomPos = rand_roomPositions[roomIdsPerPolygon[i][m]];
                        float otherRoomSize = rand_roomSizes[roomIdsPerPolygon[i][m]];

                        //Calulate Splitting Line
                        Maths2D.CalculateCenterPerpendicular(new Vector2(roomPos.x, roomPos.z), new Vector2(otherRoomPos.x, otherRoomPos.z), roomSize, otherRoomSize, out Vector2 middlepoint, out Vector2 direction);

                        Vector2[] intersectionPoints = polygons[i].GetIntersectionPoints(middlepoint, direction);
                        ////Debug.Log((k+1) + " zu " + (m+1) + ": " + intersectionPoints.Length);
                        foreach (Vector2 intersec in intersectionPoints) 
                        {
                            intersections[i].Add(intersec);
                        }
                        
                        ////Debug.Log("Raum: " + (k+1) + ", anderer Raum: " + (m+1) + " --> Mittelpunkt: " + middlepoint + ", direction: (" + direction.x + "; " + direction.y + ")");
                        points[i].Add(middlepoint);
                        directions[i].Add(direction);
                    }
                }
            }
        }
        */

        //CALCULATE A ROUGH SPLITTING OF THE ROOMS

        int roomID = 0;
        //int polygonID = 0;
        Vector3[][] rooms = new Vector3[polygons.Length][];
        float[][] sizes = new float[polygons.Length][];
        RoomType[][] types = new RoomType[polygons.Length][];

        segmentsPerPolygon = new Segment[polygons.Length][];
        pointsVoronoiPerPolygon = new Vector2[polygons.Length][];
        outsideSegmentsPerPolygon = new Segment[polygons.Length][];
        problemSegmentsPerPolygon = new Segment[polygons.Length][];

        timesPerPolygon = new List<int>[polygons.Length];
        

        for (int polygonID = 0; polygonID < polygons.Length; polygonID++)
        {
            stopwatch.Start();

            rooms[polygonID] = new Vector3[roomIdsPerPolygon[polygonID].Count];
            sizes[polygonID] = new float[roomIdsPerPolygon[polygonID].Count];
            types[polygonID] = new RoomType[roomIdsPerPolygon[polygonID].Count];

            test = new List<List<Vector2[]>>[polygons.Length];
            allP = new List<Vector2[]>[polygons.Length];



            for (int i = 0; i < roomIdsPerPolygon[polygonID].Count; i++)
            {
                ////Debug.Log(roomIdsPerPolygon[0][i]);
                rooms[polygonID][i] = rand_roomPositions[roomIdsPerPolygon[polygonID][i]];
                sizes[polygonID][i] = rand_roomSizes[roomIdsPerPolygon[polygonID][i]];
                types[polygonID][i] = rand_roomTypes[roomIdsPerPolygon[polygonID][i]];
            }

            //hier statt rand_roomPositions nur die Room Positions von den Räumen auf dem selben Polygon reingeben
            test[polygonID] = new List<List<Vector2[]>>();
            allP[polygonID] = new List<Vector2[]>();
            for (int i = 0; i < rooms[polygonID].Length; i++)
            {
                roomID = i;
                //TODO result of test dont need to be safed
                //Debug.LogError(polygonID);
                test[polygonID].Add(polygons[polygonID].GetVoronoiRegion(rooms[polygonID], sizes[polygonID], roomID, out splittingPoints, out isCut));
                allP[polygonID].Add(polygons[polygonID].GetSingleVoronoiCell(splittingPoints, isCut, new Vector2(rooms[polygonID][roomID].x, rooms[polygonID][roomID].z), out connectivity, out allPoints));
                if (polygonID == showPolygonNumberTEST && i == showRoomNumberTEST) 
                {
                    splittingPointsTEST = new List<Vector2[]>(splittingPoints.ToArray());
                    isCutTEST = new List<bool[]>(isCut.ToArray());
                }
            }
            CalculateFullVoronoi(polygonID, types[polygonID]);

            /*
            segmentsPerRoom[polygonID] = new Segment[segments.Count];
            for (int i = 0; i < segments.Count; i++) 
            {
                segmentsPerRoom[polygonID][i] = segments[i];
            }
            */
            segmentsPerPolygon[polygonID] = segments.ToArray();
            pointsVoronoiPerPolygon[polygonID] = pointsVoronoi.ToArray();
            outsideSegmentsPerPolygon[polygonID] = outsideSegments.ToArray();
            problemSegmentsPerPolygon[polygonID] = problemSegments.ToArray();

            stopwatch.Stop();
            timesPerPolygon[polygonID] = new List<int>();
            timesPerPolygon[polygonID].Add(Mathf.RoundToInt((float)stopwatch.Elapsed.TotalMilliseconds));
            stopwatch.Reset();
        }


        //TODO --> Remove Duplicates from all ...perPolygon Lists/Arrays ... current State: every Segments is in both directions included



        //CORRECT THE SPLITTING BY STRAIGHTEN THE SPLITTING EDGES
        //<<<<<<>>>>>>>>><<<<<<<<<<>>>>>>><<<<<<<<>>>>>>>>>><<<<<<<<<>>>>>>>>>>><<<<<<<<<<<<<>>>>>>>>>>

        segmentsPerPolygon_straightened = new Segment[polygons.Length][];
        pointsVoronoiPerPolygon_straightened = new Vector2[polygons.Length][];
        outsideSegmentsPerPolygon_straightened = new Segment[polygons.Length][];

        segmentsPerPolygon_straightened_cleanCOPY = new Segment[polygons.Length][];
        pointsVoronoiPerPolygon_straightened_cleanCOPY = new Vector2[polygons.Length][];
        outsideSegmentsPerPolygon_straightened_cleanCOPY = new Segment[polygons.Length][];

        for (int polygonID = 0; polygonID < polygons.Length; polygonID++)
        {
            stopwatch.Start();
            //TODO siehe oben, duplicate entfernen:
            //segmentsPerPolygon[polygonID]
            //outsideSegmentsPerPolygon[polygonID]
            //>>>>>>>>>>>>>>>
            List<Segment> segmentsInThisPolygon = new List<Segment>();
            List<Segment> outsideSegmentsInThisPolygon = new List<Segment>();

            foreach (Segment element in segmentsPerPolygon[polygonID])
            {
                bool included = false;
                foreach (Segment element2 in segmentsInThisPolygon)
                {
                    if (element2.GetIndexFrom() == element.GetIndexTo() && element2.GetIndexTo() == element.GetIndexFrom())
                    {
                        included = true;
                    }
                    if (element2.GetIndexFrom() == element.GetIndexFrom() && element2.GetIndexTo() == element.GetIndexTo())
                    {
                        included = true;
                    }
                }
                if (!included)
                {
                    segmentsInThisPolygon.Add(element);
                }
            }

            foreach (Segment element in outsideSegmentsPerPolygon[polygonID])
            {
                bool included = false;
                foreach (Segment element2 in outsideSegmentsInThisPolygon)
                {
                    if (element2.GetIndexFrom() == element.GetIndexTo() && element2.GetIndexTo() == element.GetIndexFrom())
                    {
                        included = true;
                    }
                    if (element2.GetIndexFrom() == element.GetIndexFrom() && element2.GetIndexTo() == element.GetIndexTo())
                    {
                        included = true;
                    }
                }
                if (!included)
                {
                    outsideSegmentsInThisPolygon.Add(element);
                }
            }
            //<<<<<<<<<<<<

            //Liste mit benachbarten Segmenten
            List<List<Segment>> conect = new List<List<Segment>>();
            for (int i = 0; i < pointsVoronoiPerPolygon[polygonID].Length; i++)
            {
                conect.Add(new List<Segment>());
            }
            for (int i = 0; i < segmentsInThisPolygon.Count; i++)
            {
                conect[segmentsInThisPolygon[i].GetIndexFrom()].Add(segmentsInThisPolygon[i]);
                conect[segmentsInThisPolygon[i].GetIndexTo()].Add(segmentsInThisPolygon[i]);
            }


            segments_straightened = new List<Segment>();
            pointsVoronoi_straightened = new List<Vector2>(pointsVoronoiPerPolygon[polygonID]);
            outsideSegments_straightened = new List<Segment>();

            //Liste with not fully inspected segments
            List<Segment> notFullyInspected = new List<Segment>();
            List<Segment> outside = new List<Segment>(outsideSegmentsInThisPolygon);

            //Remove default Outside-Segments
            for (int i = 0; i < segmentsInThisPolygon.Count; i++)
            {
                Segment currentSeg = segmentsInThisPolygon[i];
                //if (outside.Contains(currentSeg))
                if (ListContainsSegment(outside, currentSeg))
                {
                    //1.
                    segments_straightened.Add(currentSeg);
                    outsideSegments_straightened.Add(currentSeg);
                }
                else
                {
                    notFullyInspected.Add(currentSeg);
                }
            }

            List<Segment> AddedInNextCycle = new List<Segment>();
            List<Segment> ReturnToNotFullyInspected = new List<Segment>();
            int saveCount = 0;
            ////Debug.Log("polygonID: " + polygonID + " --> notFullyInspected Anzahl: " + notFullyInspected.Count + " (" + outsideSegmentsInThisPolygon.Count + ", " + segmentsInThisPolygon.Count + " and points: " + pointsVoronoiPerPolygon[polygonID].Length +")");
            while (notFullyInspected.Count > 0 && saveCount < numOfIterationsForStraightenEdges)
            {
                saveCount++;
                if (saveCount == numOfIterationsForStraightenEdges)
                {
                    ////Debug.LogError("not fully inspected segments never run out of elements! --> ENDLOSSCHLEIFE");
                }

                foreach (Segment currentSeg in notFullyInspected)
                {
                    bool straightened = false;
                    List<Segment> neighboursFrom = new List<Segment>();
                    foreach (Segment s in conect[currentSeg.GetIndexFrom()])
                    {
                        if (s != currentSeg)
                        {
                            neighboursFrom.Add(s);
                        }
                    }
                    List<Segment> neighboursTo = new List<Segment>();
                    foreach (Segment s in conect[currentSeg.GetIndexTo()])
                    {
                        if (s != currentSeg)
                        {
                            neighboursTo.Add(s);
                        }
                    }


                    //>>>>>>>>>>>>>>>>>>> NEW NEW NEW - START - NEW NEW NEW >>>>>>>>>>>>>>>>>>>

                    //IDEA:
                    // only look for the directly neighboured segments 
                    // from the Start Point --> A1 and A2 (it could be only A1) (biggest and smallest angle)
                    // from the End Point --> B1 and B2 (it could be only B1) (biggest and smallest angle)

                    Segment a1 = null; //smallest angle
                    Segment a2 = null; //biggest angle
                    Segment b1 = null; //smallest angle
                    Segment b2 = null; //biggest angle

                    bool seg_a1_is_already_straight = false;
                    bool seg_a2_is_already_straight = false;
                    bool seg_b1_is_already_straight = false;
                    bool seg_b2_is_already_straight = false;

                    float angle_a1 = -181.0f;
                    float angle_a2 = 181.0f;
                    float angle_b1 = -181.0f;
                    float angle_b2 = 181.0f;

                    if (neighboursFrom.Count < 1)
                    {
                        //Debug.LogError("no connected Segment Error");
                    }
                    if (neighboursTo.Count < 1)
                    {
                        //Debug.LogError("no connected Segment Error");
                    }

                    int x1 = currentSeg.GetIndexFrom();
                    int x2 = currentSeg.GetIndexTo();
                    int x3 = -1;
                    foreach (Segment s in neighboursFrom)
                    {
                        x3 = s.GetIndexFrom();
                        if (x3 == x1)
                        {
                            x3 = s.GetIndexTo();
                        }

                        Vector2 seg1 = pointsVoronoi_straightened[x2] - pointsVoronoi_straightened[x1];
                        Vector2 seg2 = pointsVoronoi_straightened[x3] - pointsVoronoi_straightened[x1];
                        float angle = Vector2.SignedAngle(seg1, seg2);

                        if (angle > angle_a1)
                        {
                            angle_a1 = angle;
                            a1 = s;
                        }
                        if (angle < angle_a2)
                        {
                            angle_a2 = angle;
                            a2 = s;
                        }
                    }

                    x1 = currentSeg.GetIndexTo();
                    x2 = currentSeg.GetIndexFrom();
                    x3 = -1;
                    foreach (Segment s in neighboursTo)
                    {
                        x3 = s.GetIndexFrom();
                        if (x3 == x1)
                        {
                            x3 = s.GetIndexTo();
                        }

                        Vector2 seg1 = pointsVoronoi_straightened[x2] - pointsVoronoi_straightened[x1];
                        Vector2 seg2 = pointsVoronoi_straightened[x3] - pointsVoronoi_straightened[x1];
                        float angle = Vector2.SignedAngle(seg1, seg2);

                        if (angle > angle_b1)
                        {
                            angle_b1 = angle;
                            b1 = s;
                        }
                        if (angle < angle_b2)
                        {
                            angle_b2 = angle;
                            b2 = s;
                        }
                    }

                    seg_a1_is_already_straight = ListContainsSegment(segments_straightened, a1);
                    seg_a2_is_already_straight = ListContainsSegment(segments_straightened, a2);
                    seg_b1_is_already_straight = ListContainsSegment(segments_straightened, b1);
                    seg_b2_is_already_straight = ListContainsSegment(segments_straightened, b2);

                    //CASES:
                    //0.
                    //segment is already orthogonal or parallel

                    //1.
                    // if at least (A1 or A2) and (B1 or B2) are already straight
                    // --> split the Segment in half

                    //2.
                    // if only the Side A (A1 and/or A2) or the Side B (B1 and/or B2) are already straight
                    // --> try to straight the Segment by bringing it orthogonal to one of the straight segments
                    // --> if that is not possible: do a edge connect from one of the already straight segments

                    //0.
                    if ((seg_a1_is_already_straight && angle_a1 % 90 == 0) || (seg_a2_is_already_straight && angle_a2 % 90 == 0) ||
                       (seg_b1_is_already_straight && angle_b1 % 90 == 0) || (seg_b2_is_already_straight && angle_b2 % 90 == 0))
                    {
                        AddedInNextCycle.Add(currentSeg);
                        straightened = true;
                    }
                    //1.
                    else
                    {
                        if ((seg_a1_is_already_straight || seg_a2_is_already_straight) && (seg_b1_is_already_straight || seg_b2_is_already_straight))
                        {
                            Vector2 middlePoint = Maths2D.CalculateMiddlePoint(pointsVoronoi_straightened[currentSeg.GetIndexFrom()], pointsVoronoi_straightened[currentSeg.GetIndexTo()], 1, 1);

                            pointsVoronoi_straightened.Add(middlePoint);
                            Segment newS1 = new Segment(currentSeg.GetIndexFrom(), pointsVoronoi_straightened.Count - 1);
                            Segment newS2 = new Segment(pointsVoronoi_straightened.Count - 1, currentSeg.GetIndexTo());

                            //Add in next Cycle
                            ReturnToNotFullyInspected.Add(newS1);
                            ReturnToNotFullyInspected.Add(newS2);

                            //Update connectivity
                            conect[currentSeg.GetIndexFrom()].Remove(currentSeg);
                            conect[currentSeg.GetIndexFrom()].Add(newS1);
                            conect[currentSeg.GetIndexTo()].Remove(currentSeg);
                            conect[currentSeg.GetIndexTo()].Add(newS2);
                            conect.Add(new List<Segment>());
                            conect[pointsVoronoi_straightened.Count - 1].Add(newS1);
                            conect[pointsVoronoi_straightened.Count - 1].Add(newS2);

                            if (conect.Count != pointsVoronoi_straightened.Count) 
                            {
                                //Debug.LogError("WRONG NUMBER OF ELEMENTS");
                            }

                            straightened = true;
                        }

                        //2.
                        if (((seg_a1_is_already_straight || seg_a2_is_already_straight) && (!seg_b1_is_already_straight && !seg_b2_is_already_straight)) ||
                            ((!seg_a1_is_already_straight && !seg_a2_is_already_straight) && (seg_b1_is_already_straight || seg_b2_is_already_straight)))
                        {
                            //at max 2 already straight connected segments
                            List<Segment> connected_segments_already_straight = new List<Segment>();
                            int index_straight_segments = -1;
                            int index_not_stragiht_segments = -1;

                            if (seg_a1_is_already_straight)
                            {
                                connected_segments_already_straight.Add(a1);
                                index_straight_segments = currentSeg.GetIndexFrom();
                                index_not_stragiht_segments = currentSeg.GetIndexTo();
                            }
                            if (seg_a2_is_already_straight)
                            {
                                connected_segments_already_straight.Add(a2);
                                index_straight_segments = currentSeg.GetIndexFrom();
                                index_not_stragiht_segments = currentSeg.GetIndexTo();
                            }
                            if (seg_b1_is_already_straight)
                            {
                                connected_segments_already_straight.Add(b1);
                                index_straight_segments = currentSeg.GetIndexTo();
                                index_not_stragiht_segments = currentSeg.GetIndexFrom();
                            }
                            if (seg_b2_is_already_straight)
                            {
                                connected_segments_already_straight.Add(b2);
                                index_straight_segments = currentSeg.GetIndexTo();
                                index_not_stragiht_segments = currentSeg.GetIndexFrom();
                            }

                            List<Vector2> calculatedConnectionPoints = new List<Vector2>();
                            List<int> indexOfPossibleOrthogonalConnection = new List<int>();

                            foreach (Segment s in connected_segments_already_straight)
                            {
                                Vector2 pointOnSegment;
                                if (Maths2D.CalculatePerpendicularToSegmentThroughPoint(pointsVoronoi_straightened[index_not_stragiht_segments], pointsVoronoi_straightened[s.GetIndexFrom()], pointsVoronoi_straightened[s.GetIndexTo()], out pointOnSegment))
                                {
                                    indexOfPossibleOrthogonalConnection.Add(calculatedConnectionPoints.Count);
                                }
                                calculatedConnectionPoints.Add(pointOnSegment);

                                //Add alternative edge connections:
                                Vector2 edgeconnectionPoint1 = pointsVoronoi_straightened[index_not_stragiht_segments] + (pointOnSegment - pointsVoronoi_straightened[s.GetIndexFrom()]);
                                Vector2 edgeconnectionPoint2 = pointsVoronoi_straightened[index_not_stragiht_segments] + (pointOnSegment - pointsVoronoi_straightened[s.GetIndexTo()]);
                                calculatedConnectionPoints.Add(edgeconnectionPoint1);
                                calculatedConnectionPoints.Add(edgeconnectionPoint2);
                            }

                            //Orthogonal Connection possible --> no edge connect needed
                            if (indexOfPossibleOrthogonalConnection.Count > 0)
                            {
                                //choose index --> 0
                                int orthogonal_index = 0;
                                //point on segment
                                Vector2 pointOnSegment = calculatedConnectionPoints[indexOfPossibleOrthogonalConnection[orthogonal_index]];

                                //If the new Segment alignment would bring a room position outside of the room, or another room position inside the room, the ideal correction is not possible
                                //instead the correction is only done to the room position, so that the roompositions alsways stay inside their rooms
                                if (CheckForRoomIntersectionsWhenStraightingEdges(roomIdsPerPolygon, polygonID, currentSeg, pointOnSegment, out List<int> roomIDsInsideTriangle))
                                {
                                    // TODO Make it single Method to reuse it
                                    float minDist = -1.0f;
                                    int closestRoomID = -1;

                                    foreach (int possibleRoomID in roomIDsInsideTriangle) 
                                    {
                                        float dist = Vector2.Distance(pointsVoronoi_straightened[currentSeg.GetIndexFrom()], new Vector2(rand_roomPositions[possibleRoomID].x, rand_roomPositions[possibleRoomID].z))+
                                                     Vector2.Distance(pointsVoronoi_straightened[currentSeg.GetIndexTo()], new Vector2(rand_roomPositions[possibleRoomID].x, rand_roomPositions[possibleRoomID].z));
                                        if(minDist == -1.0f || dist < minDist) 
                                        {
                                            minDist = dist;
                                            closestRoomID = possibleRoomID;
                                        }
                                    }

                                    Vector2 splittingPoint = new Vector2(rand_roomPositions[closestRoomID].x, rand_roomPositions[closestRoomID].z);

                                    pointsVoronoi_straightened.Add(splittingPoint);
                                    Segment newS1 = new Segment(currentSeg.GetIndexFrom(), pointsVoronoi_straightened.Count - 1);
                                    Segment newS2 = new Segment(pointsVoronoi_straightened.Count - 1, currentSeg.GetIndexTo());

                                    //Add in next Cycle
                                    ReturnToNotFullyInspected.Add(newS1);
                                    ReturnToNotFullyInspected.Add(newS2);

                                    //Update connectivity
                                    conect[currentSeg.GetIndexFrom()].Remove(currentSeg);
                                    conect[currentSeg.GetIndexFrom()].Add(newS1);
                                    conect[currentSeg.GetIndexTo()].Remove(currentSeg);
                                    conect[currentSeg.GetIndexTo()].Add(newS2);
                                    conect.Add(new List<Segment>());
                                    conect[pointsVoronoi_straightened.Count - 1].Add(newS1);
                                    conect[pointsVoronoi_straightened.Count - 1].Add(newS2);

                                    if (conect.Count != pointsVoronoi_straightened.Count)
                                    {
                                        //Debug.LogError("WRONG NUMBER OF ELEMENTS");
                                    }

                                    straightened = true;

                                }
                                else 
                                { 
                                    //segment
                                    Segment segment = connected_segments_already_straight[indexOfPossibleOrthogonalConnection[orthogonal_index] / 3];
                                    RemoveSegment(segments_straightened, segment, conect);

                                    pointsVoronoi_straightened.Add(pointOnSegment);
                                    Segment newCurrentSeg = new Segment(index_not_stragiht_segments, pointsVoronoi_straightened.Count - 1);

                                    Segment newA = new Segment(segment.GetIndexFrom(), pointsVoronoi_straightened.Count - 1);
                                    Segment newB = new Segment(pointsVoronoi_straightened.Count - 1, segment.GetIndexTo());


                                    AddedInNextCycle.Add(newCurrentSeg);
                                    AddedInNextCycle.Add(newA);
                                    AddedInNextCycle.Add(newB);

                                    //Update connectivity
                                    conect[index_straight_segments].Remove(currentSeg);
                                    conect[index_not_stragiht_segments].Remove(currentSeg);
                                    conect[segment.GetIndexFrom()].Remove(segment);
                                    conect[segment.GetIndexFrom()].Add(newA);
                                    conect[segment.GetIndexTo()].Remove(segment);
                                    conect[segment.GetIndexTo()].Add(newB);
                                    conect[index_not_stragiht_segments].Add(newCurrentSeg);

                                    conect.Add(new List<Segment>());
                                    conect[pointsVoronoi_straightened.Count - 1].Add(newA);
                                    conect[pointsVoronoi_straightened.Count - 1].Add(newB);
                                    conect[pointsVoronoi_straightened.Count - 1].Add(newCurrentSeg);

                                    if (conect.Count != pointsVoronoi_straightened.Count)
                                    {
                                        //Debug.LogError("WRONG NUMBER OF ELEMENTS");
                                    }

                                    straightened = true;
                                }
                            }
                            //Orthogonal Connection NOT possible -->edge connect needed!
                            else
                            {
                                //choose index --> 0
                                int edge_connect_index = 0;
                                //point on segment
                                Vector2 pointOnSegment = calculatedConnectionPoints[edge_connect_index];

                                //If the new Segment alignment would bring a room position outside of the room, or another room position inside the room, the ideal correction is not possible
                                //instead the correction is only done to the room position, so that the roompositions alsways stay inside their rooms
                                if (CheckForRoomIntersectionsWhenStraightingEdges(roomIdsPerPolygon, polygonID, currentSeg, pointOnSegment, out List<int> roomIDsInsideTriangle))
                                {
                                    // TODO Make it single Method
                                    float minDist = -1.0f;
                                    int closestRoomID = -1;

                                    foreach (int possibleRoomID in roomIDsInsideTriangle)
                                    {
                                        float dist = Vector2.Distance(pointsVoronoi_straightened[currentSeg.GetIndexFrom()], new Vector2(rand_roomPositions[possibleRoomID].x, rand_roomPositions[possibleRoomID].z)) +
                                                     Vector2.Distance(pointsVoronoi_straightened[currentSeg.GetIndexTo()], new Vector2(rand_roomPositions[possibleRoomID].x, rand_roomPositions[possibleRoomID].z));
                                        if (minDist == -1.0f || dist < minDist)
                                        {
                                            minDist = dist;
                                            closestRoomID = possibleRoomID;
                                        }
                                    }

                                    Vector2 splittingPoint = new Vector2(rand_roomPositions[closestRoomID].x, rand_roomPositions[closestRoomID].z);

                                    pointsVoronoi_straightened.Add(splittingPoint);
                                    Segment newS1 = new Segment(currentSeg.GetIndexFrom(), pointsVoronoi_straightened.Count - 1);
                                    Segment newS2 = new Segment(pointsVoronoi_straightened.Count - 1, currentSeg.GetIndexTo());

                                    //Add in next Cycle
                                    ReturnToNotFullyInspected.Add(newS1);
                                    ReturnToNotFullyInspected.Add(newS2);

                                    //Update connectivity
                                    conect[currentSeg.GetIndexFrom()].Remove(currentSeg);
                                    conect[currentSeg.GetIndexFrom()].Add(newS1);
                                    conect[currentSeg.GetIndexTo()].Remove(currentSeg);
                                    conect[currentSeg.GetIndexTo()].Add(newS2);
                                    conect.Add(new List<Segment>());
                                    conect[pointsVoronoi_straightened.Count - 1].Add(newS1);
                                    conect[pointsVoronoi_straightened.Count - 1].Add(newS2);

                                    if (conect.Count != pointsVoronoi_straightened.Count)
                                    {
                                        //Debug.LogError("WRONG NUMBER OF ELEMENTS");
                                    }

                                    straightened = true;
                                }
                                else 
                                {
                                    //segment
                                    Segment segment = connected_segments_already_straight[Mathf.FloorToInt(edge_connect_index / 3)];

                                    pointsVoronoi_straightened.Add(pointOnSegment);

                                    int indexOfClosestSegmentEdgePoint = segment.GetIndexFrom();
                                    int otherIndex = segment.GetIndexTo();
                                    if (Vector2.Distance(pointsVoronoi_straightened[segment.GetIndexTo()], pointOnSegment) < Vector2.Distance(pointsVoronoi_straightened[segment.GetIndexFrom()], pointOnSegment))
                                    {
                                        indexOfClosestSegmentEdgePoint = segment.GetIndexTo();
                                        otherIndex = segment.GetIndexFrom();
                                    }

                                    Segment newCurrentSeg1 = new Segment(index_not_stragiht_segments, pointsVoronoi_straightened.Count - 1);
                                    Segment newCurrentSeg2 = new Segment(pointsVoronoi_straightened.Count - 1, indexOfClosestSegmentEdgePoint);

                                    AddedInNextCycle.Add(newCurrentSeg1);
                                    AddedInNextCycle.Add(newCurrentSeg2);

                                    //Update connectivity
                                    conect[index_straight_segments].Remove(currentSeg);
                                    conect[index_not_stragiht_segments].Remove(currentSeg);
                                    conect[index_not_stragiht_segments].Add(newCurrentSeg1);
                                    conect[indexOfClosestSegmentEdgePoint].Add(newCurrentSeg2);

                                    conect.Add(new List<Segment>());
                                    conect[pointsVoronoi_straightened.Count - 1].Add(newCurrentSeg1);
                                    conect[pointsVoronoi_straightened.Count - 1].Add(newCurrentSeg2);

                                    if (conect.Count != pointsVoronoi_straightened.Count)
                                    {
                                        //Debug.LogError("WRONG NUMBER OF ELEMENTS");
                                    }

                                    straightened = true;
                                }
                            }
                        }
                    }
                    //REMAINING PROBLEMS
                    // - the new Edges can intersect with already existing Segments
                    // - the Room Center Point could be outside of the segments --> FIXED
                    //      - check the resuting triangle:
                    //      - if any room is inside --> triangle not possible
                    //      - for every room inside --> take that room position that is the closest to the end result of the triangle (how to calculate?)
                    // - a room could complete dissapear because the segemtns are overlaying each other after correction

                    // - when a new segment is added, check if it overlaps with other already straight segment parts

                    //<<<<<<<<<<<<<<<<<<< NEW NEW NEW -  END  - NEW NEW NEW <<<<<<<<<<<<<<<<<<<

                    if (!straightened)
                    {
                        ReturnToNotFullyInspected.Add(currentSeg);
                    }
                    
                }

                List<Segment> zeroLengthSegments = new List<Segment>();
                foreach (Segment element in AddedInNextCycle) 
                {
                    if (Vector2.Distance(pointsVoronoi_straightened[element.GetIndexFrom()], pointsVoronoi_straightened[element.GetIndexTo()]) < 0.00001f)
                    {
                        conect[element.GetIndexFrom()].Remove(element);
                        conect[element.GetIndexTo()].Remove(element);
                        List<Segment> newConnections = new List<Segment>();
                        foreach (Segment seg in conect[element.GetIndexFrom()])
                        {
                            newConnections.Add(element);
                        }
                        foreach (Segment seg in conect[element.GetIndexTo()])
                        {
                            if (!newConnections.Contains(element))
                            {
                                newConnections.Add(element);
                            }
                        }
                        conect[element.GetIndexFrom()] = newConnections;
                        conect[element.GetIndexTo()] = newConnections;

                        zeroLengthSegments.Add(element);
                    }
                }

                foreach (Segment element in zeroLengthSegments) 
                {
                    AddedInNextCycle.Remove(element);
                }
                

                foreach (Segment element in AddedInNextCycle) 
                {
                    foreach (Segment element2 in AddedInNextCycle)
                    {
                        if (element != element2) 
                        {
                            int i_a1 = element.GetIndexFrom();
                            int i_a2 = element.GetIndexTo();
                            int i_b1 = element2.GetIndexFrom();
                            int i_b2 = element2.GetIndexTo();
                            Vector2 p_a1 = pointsVoronoi_straightened[i_a1];
                            Vector2 p_a2 = pointsVoronoi_straightened[i_a2];
                            Vector2 p_b1 = pointsVoronoi_straightened[i_b1];
                            Vector2 p_b2 = pointsVoronoi_straightened[i_b2];
                            if (i_a1 == i_b1) 
                            {
                                if (Maths2D.PointOnLineSegment(p_a1, p_a2, p_b2)) 
                                {
                                    element.SetIndexFrom(i_b2);
                                    conect[i_b1].Remove(element);
                                    conect[i_b2].Add(element);
                                }
                                else if (Maths2D.PointOnLineSegment(p_b1, p_b2, p_a2))
                                {
                                    element2.SetIndexFrom(i_a2);
                                    conect[i_a1].Remove(element2);
                                    conect[i_a2].Add(element2);
                                }
                            }
                            else if (i_a1 == i_b2)
                            {
                                if (Maths2D.PointOnLineSegment(p_a1, p_a2, p_b1))
                                {
                                    element.SetIndexFrom(i_b1);
                                    conect[i_b2].Remove(element);
                                    conect[i_b1].Add(element);
                                }
                                else if (Maths2D.PointOnLineSegment(p_b1, p_b2, p_a2))
                                {
                                    element2.SetIndexTo(i_a2);
                                    conect[i_a1].Remove(element2);
                                    conect[i_a2].Add(element2);
                                }
                            }
                            else if (i_a2 == i_b1)
                            {
                                if (Maths2D.PointOnLineSegment(p_a1, p_a2, p_b2))
                                {
                                    element.SetIndexTo(i_b2);
                                    conect[i_b1].Remove(element);
                                    conect[i_b2].Add(element);
                                }
                                else if (Maths2D.PointOnLineSegment(p_b1, p_b2, p_a1))
                                {
                                    element2.SetIndexFrom(i_a1);
                                    conect[i_a2].Remove(element2);
                                    conect[i_a1].Add(element2);
                                }
                            }
                            else if (i_a2 == i_b2)
                            {
                                if (Maths2D.PointOnLineSegment(p_a1, p_a2, p_b1))
                                {
                                    element.SetIndexTo(i_b1);
                                    conect[i_b2].Remove(element);
                                    conect[i_b1].Add(element);
                                }
                                else if (Maths2D.PointOnLineSegment(p_b1, p_b2, p_a1))
                                {
                                    element2.SetIndexTo(i_a1);
                                    conect[i_a2].Remove(element2);
                                    conect[i_a1].Add(element2);
                                }
                            }
                        }
                    }
                }
                

                //segments_straightened.AddRange(AddedInNextCycle);
                foreach (Segment element in AddedInNextCycle)
                {
                    //AT THIS POINT ADD CHECK FOR:
                    // - zero length segments
                    // - segment parallel to connecting and already straight segment 
                    // - segments that lay exactly over each other:
                    //      - partially
                    //      - fully

                    //ZERO LENGTH
                    if (Vector2.Distance(pointsVoronoi_straightened[element.GetIndexFrom()], pointsVoronoi_straightened[element.GetIndexTo()]) < 0.00001f)
                    {
                        conect[element.GetIndexFrom()].Remove(element);
                        conect[element.GetIndexTo()].Remove(element);
                        List<Segment> newConnections = new List<Segment>();
                        foreach (Segment seg in conect[element.GetIndexFrom()])
                        {
                            newConnections.Add(element);
                        }
                        foreach (Segment seg in conect[element.GetIndexTo()])
                        {
                            if (!newConnections.Contains(element))
                            {
                                newConnections.Add(element);
                            }
                        }
                        conect[element.GetIndexFrom()] = newConnections;
                        conect[element.GetIndexTo()] = newConnections;
                    }
                    else
                    {
                        List<Segment> neighboursFrom = new List<Segment>();
                        foreach (Segment s in conect[element.GetIndexFrom()])
                        {
                            //  && !ListContainsSegment(AddedInNextCycle, s)
                            if (s != element)
                            {
                                neighboursFrom.Add(s);
                            }
                        }
                        List<Segment> neighboursTo = new List<Segment>();
                        foreach (Segment s in conect[element.GetIndexTo()])
                        {
                            if (s != element)
                            {
                                neighboursTo.Add(s);
                            }
                        }

                        // PARALLEL
                        if (neighboursFrom.Count == 1 && ListContainsSegment(segments_straightened, neighboursFrom[0]))
                        {
                            
                            int index_a = neighboursFrom[0].GetIndexFrom();
                            Vector2 point_a = pointsVoronoi_straightened[index_a];
                            if (point_a == pointsVoronoi_straightened[element.GetIndexFrom()])
                            {
                                index_a = neighboursFrom[0].GetIndexTo();
                                point_a = pointsVoronoi_straightened[index_a];
                            }
                            // && !Maths2D.PointOnLineSegment(pointsVoronoi_straightened[neighboursFrom[0].GetIndexFrom()], pointsVoronoi_straightened[neighboursFrom[0].GetIndexTo()], pointsVoronoi_straightened[element.GetIndexFrom()])
                            if (Maths2D.IsAngleBetweenTwoVectors180Degree(point_a, pointsVoronoi_straightened[element.GetIndexFrom()], pointsVoronoi_straightened[element.GetIndexTo()], 0.01f))
                            {
                                conect[neighboursFrom[0].GetIndexFrom()].Remove(neighboursFrom[0]);
                                conect[neighboursFrom[0].GetIndexTo()].Remove(neighboursFrom[0]);
                                //conect[element.GetIndexFrom()].Clear();
                                conect[element.GetIndexFrom()].Remove(element);

                                element.SetIndexFrom(index_a);
                                conect[index_a].Add(element);

                                segments_straightened.Remove(neighboursFrom[0]);
                            }
                        }
                        
                        if (neighboursTo.Count == 1 && ListContainsSegment(segments_straightened, neighboursTo[0]))
                        {
                            int index_a = neighboursTo[0].GetIndexFrom();
                            Vector2 point_a = pointsVoronoi_straightened[index_a];
                            if (point_a == pointsVoronoi_straightened[element.GetIndexTo()])
                            {
                                index_a = neighboursTo[0].GetIndexTo();
                                point_a = pointsVoronoi_straightened[index_a];
                            }
                            // && !Maths2D.PointOnLineSegment(pointsVoronoi_straightened[neighboursTo[0].GetIndexFrom()], pointsVoronoi_straightened[neighboursTo[0].GetIndexTo()], pointsVoronoi_straightened[element.GetIndexTo()])
                            if (Maths2D.IsAngleBetweenTwoVectors180Degree(point_a, pointsVoronoi_straightened[element.GetIndexTo()], pointsVoronoi_straightened[element.GetIndexFrom()], 0.01f))
                            {
                                conect[neighboursTo[0].GetIndexFrom()].Remove(neighboursTo[0]);
                                conect[neighboursTo[0].GetIndexTo()].Remove(neighboursTo[0]);
                                conect[element.GetIndexTo()].Remove(element);
                                //conect[element.GetIndexTo()].Clear();

                                element.SetIndexTo(index_a);
                                conect[index_a].Add(element);

                                segments_straightened.Remove(neighboursTo[0]);
                                //segments_straightened.Add(element);
                            }
                        }
                        
                        // OVERLAYING

                        segments_straightened.Add(element);
                    }
                }
                
                AddedInNextCycle = new List<Segment>();

                notFullyInspected = new List<Segment>();
                notFullyInspected.AddRange(ReturnToNotFullyInspected);
                //foreach (Segment element in ReturnToNotFullyInspected)
                //{
                //    notFullyInspected.Add(element);
                //}
                
                ReturnToNotFullyInspected = new List<Segment>();

                //Remove Zero Length Segments, Remove Segments with only 1 connected side, merge segments that have a connection angle of 180 degree (and no other connected segment on that point)
                RemoveZeroLengthSegmentsFromSegmentsStraightened(conect);
            }


            //Add missing segments --> should not happen
            foreach (Segment element in notFullyInspected) 
            {
                //Debug.LogError("not able to straighten all wall-segments ... try increasing the num of iterations");
                outsideSegments_straightened.Add(element);
            }

            MapStraightenSegmentsToNewPoints(pointsVoronoi_straightened, segments_straightened, outsideSegments_straightened, out List<Vector2> pointsVoronoi_straightenedReduced, out List<Segment> segments_straightenedReduced, out List<Segment> outsideSegments_straightenedReduced);

            //CREATE CLEAN COPYS OF ALL VALUES TO SWITCH BETWEEN EVOLOUTION STEPS
            List<Segment> segments_straightenedReduced_cleanCOPY = new List<Segment>();
            foreach (Segment s in segments_straightenedReduced) 
            {
                segments_straightenedReduced_cleanCOPY.Add(s.copy());
            }
            List<Segment> outsideSegments_straightenedReduced_cleanCOPY = new List<Segment>();
            foreach (Segment s in outsideSegments_straightenedReduced)
            {
                outsideSegments_straightenedReduced_cleanCOPY.Add(s.copy());
            }
            List<Vector2> pointsVoronoi_straightenedReduced_cleanCOPY = new List<Vector2>();
            foreach (Vector2 v in pointsVoronoi_straightenedReduced)
            {
                pointsVoronoi_straightenedReduced_cleanCOPY.Add(new Vector2(v.x, v.y));
            }

            segmentsPerPolygon_straightened[polygonID] = segments_straightenedReduced.ToArray();
            pointsVoronoiPerPolygon_straightened[polygonID] = pointsVoronoi_straightenedReduced.ToArray();
            outsideSegmentsPerPolygon_straightened[polygonID] = outsideSegments_straightenedReduced.ToArray();

            segmentsPerPolygon_straightened_cleanCOPY[polygonID] = segments_straightenedReduced_cleanCOPY.ToArray();
            pointsVoronoiPerPolygon_straightened_cleanCOPY[polygonID] = pointsVoronoi_straightenedReduced_cleanCOPY.ToArray();
            outsideSegmentsPerPolygon_straightened_cleanCOPY[polygonID] = outsideSegments_straightenedReduced_cleanCOPY.ToArray();

            //Reduce num of small segments

            stopwatch.Stop();
            timesPerPolygon[polygonID].Add(Mathf.RoundToInt((float)stopwatch.Elapsed.TotalMilliseconds));
            stopwatch.Reset();
        }


        //TODO 
        //IMPORTANT
        //FIX ERROR (right now error correction here) THAT SOME CORNER POINTS ARE NOT CORRECTLY CONNECTED WITH THE SEGMENTS THEY ARE LAYING ON
        // Problem probably --> Points with the same coordinates

        //FIX ERROR THAT SOME CORNERS LAY ON SEGMENTS, BUT DONT SPLIT THE SEGMENTS IN HALF (right now error correction here)



        //TODO
        //Remove Segments with only 1 connted side
        //right now the remove is in simplify (vvvvv down here vvvv)


        //SIMPLIFIY THE STRAIGHT SPLITTING
        //<---><---><---><---><---><---><---><---><---><---><---><---><---><---><---><---><---><---><---><--->


        segmentsPerPolygon_simplyfied = new Segment[polygons.Length][];
        pointsVoronoiPerPolygon_simplyfied = new Vector2[polygons.Length][];
        outsideSegmentsPerPolygon_simplyfied = new Segment[polygons.Length][];
        smallSegmentsPerPolygon_simplyfied = new Segment[polygons.Length][];

        for (int polygonID = 0; polygonID < polygons.Length; polygonID++)
        {
            stopwatch.Start();

            segments_simplyfied = new List<Segment>();
            pointsVoronoi_simplyfied = new List<Vector2>();
            outsideSegments_simplyfied = new List<Segment>();

            //First: create copy of every List
            for (int i = 0; i < segmentsPerPolygon_straightened[polygonID].Length; i++)
            {
                segments_simplyfied.Add(segmentsPerPolygon_straightened[polygonID][i]);
            }
            for (int i = 0; i < pointsVoronoiPerPolygon_straightened[polygonID].Length; i++)
            {
                pointsVoronoi_simplyfied.Add(pointsVoronoiPerPolygon_straightened[polygonID][i]);
            }
            for (int i = 0; i < outsideSegmentsPerPolygon_straightened[polygonID].Length; i++)
            {
                outsideSegments_simplyfied.Add(outsideSegmentsPerPolygon_straightened[polygonID][i]);
            }



            // >>>>>>>> FIX THIS IN STRAIGHTEN EDGES (FIX FOR POINTS ON SEGEMNTS WITHOUT SPLITTING THEM)
            List<Segment> addNewSegments = new List<Segment>();
            for (int i = 0; i < pointsVoronoi_simplyfied.Count; i++)
            {
                Vector2 point = pointsVoronoi_simplyfied[i];
                for (int k = 0; k < segments_simplyfied.Count; k++)
                {
                    Segment s = segments_simplyfied[k];
                    if (Maths2D.PointOnLineSegment(pointsVoronoi_simplyfied[s.GetIndexFrom()], pointsVoronoi_simplyfied[s.GetIndexTo()], point) &&
                        !Maths2D.PointEqualsPoint(pointsVoronoi_simplyfied[s.GetIndexFrom()], point) && !Maths2D.PointEqualsPoint(pointsVoronoi_simplyfied[s.GetIndexTo()], point))
                    {
                        //Split the existing segment in half
                        segments_simplyfied.Remove(s);
                        outsideSegments_simplyfied.Remove(s);
                        Segment newSeg1 = new Segment(s.GetIndexFrom(), i);
                        Segment newSeg2 = new Segment(i, s.GetIndexTo());
                        outsideSegments_simplyfied.Add(newSeg1);
                        outsideSegments_simplyfied.Add(newSeg2);
                        addNewSegments.Add(newSeg1);
                        addNewSegments.Add(newSeg2);
                    }
                }
            }

            foreach (Segment s in addNewSegments)
            {
                segments_simplyfied.Add(s);
            }

            //<<<<<<<<<< END FIX FOR STRAIGHTEN EDGES (FIX FOR POINTS ON SEGEMNTS WITHOUT SPLITTING THEM)

            // >>>>>>>> FIX THIS IN STRAIGHTEN EDGES (ZERO LENGTH SEGMENTS)
            List<Segment> zeroLengthSeg = new List<Segment>();
            foreach (Segment s in segments_simplyfied)
            {
                if (Vector2.Distance(pointsVoronoi_simplyfied[s.GetIndexFrom()], pointsVoronoi_simplyfied[s.GetIndexTo()]) + 0.0001f > 0 &&
                    Vector2.Distance(pointsVoronoi_simplyfied[s.GetIndexFrom()], pointsVoronoi_simplyfied[s.GetIndexTo()]) - 0.0001f < 0)
                {
                    zeroLengthSeg.Add(s);
                    ////Debug.LogError("ZERO LENGTH SEGMENT DETECTED");
                }
            }
            foreach (Segment s in zeroLengthSeg)
            {
                segments_simplyfied.Remove(s);
                outsideSegments_simplyfied.Remove(s);
            }

            //<<<<<<<<<< END FIX FOR STRAIGHTEN EDGES (ZERO LENGTH SEGMENTS)


            // >>>>>>>> FIX THIS IN STRAIGHTEN EDGES (POINTS WITH SAME COORDINATES BUT NO CONNECTION)
            //find points with the same coordinates
            Dictionary<int, int> mapPoint = new Dictionary<int, int>();
            for (int i = 0; i < pointsVoronoi_simplyfied.Count; i++)
            {
                for (int k = i + 1; k < pointsVoronoi_simplyfied.Count; k++)
                {
                    float culinaryArea = 0.0001f;
                    if ((pointsVoronoi_simplyfied[i].x + culinaryArea > pointsVoronoi_simplyfied[k].x) &&
                       (pointsVoronoi_simplyfied[i].x - culinaryArea < pointsVoronoi_simplyfied[k].x) &&
                       (pointsVoronoi_simplyfied[i].y + culinaryArea > pointsVoronoi_simplyfied[k].y) &&
                       (pointsVoronoi_simplyfied[i].y - culinaryArea < pointsVoronoi_simplyfied[k].y))
                    {
                        if (!mapPoint.ContainsKey(k))
                        {
                            mapPoint.Add(k, i);
                        }
                    }
                }
            }

            foreach (Segment s in segments_simplyfied)
            {
                int from = s.GetIndexFrom();
                int to = s.GetIndexTo();

                if (mapPoint.ContainsKey(from))
                {
                    s.SetIndexFrom(mapPoint[from]);
                }
                if (mapPoint.ContainsKey(to))
                {
                    s.SetIndexTo(mapPoint[to]);
                }
            }
            //<<<<<<<<<< END FIX FOR STRAIGHTEN EDGES (POINTS WITH SAME COORDINATES BUT NO CONNECTION)

            // >>>>>>>> FIX THIS IN STRAIGHTEN EDGES (DUPLICATE SEGMENTS)
            List<Segment> dupSeg = new List<Segment>();
            for (int i = 0; i < segments_simplyfied.Count; i++)
            {
                Segment s = segments_simplyfied[i];
                for (int k = i + 1; k < segments_simplyfied.Count; k++)
                {
                    Segment s2 = segments_simplyfied[k];
                    if ((s.GetIndexFrom() == s2.GetIndexFrom() && s.GetIndexTo() == s2.GetIndexTo()) ||
                        (s.GetIndexFrom() == s2.GetIndexTo() && s.GetIndexTo() == s2.GetIndexFrom()))
                    {
                        dupSeg.Add(s2);
                    }
                }
            }
            foreach (Segment s in dupSeg)
            {
                segments_simplyfied.Remove(s);
                outsideSegments_simplyfied.Remove(s);
            }

            //<<<<<<<<<< END FIX FOR STRAIGHTEN EDGES (DUPLICATE SEGMENTS)

            //CREATE
            //CREATE
            //CREATE: Liste mit benachbarten Segmenten
            List<List<Segment>> conect = new List<List<Segment>>();
            for (int i = 0; i < pointsVoronoi_simplyfied.Count; i++)
            {
                conect.Add(new List<Segment>());
            }

            for (int i = 0; i < segments_simplyfied.Count; i++)
            {
                conect[segments_simplyfied[i].GetIndexFrom()].Add(segments_simplyfied[i]);
                conect[segments_simplyfied[i].GetIndexTo()].Add(segments_simplyfied[i]);
            }


            // >>>>>>>> FIX STRAIGHT EDGES WHO ARE NOT A SINGLE SEGMENT
            for (int i = 0; i < conect.Count; i++)
            {
                if (conect[i].Count == 2)
                {
                    Segment seg1 = conect[i][0];
                    Segment seg2 = conect[i][1];

                    int fromIndex = seg1.GetIndexFrom();
                    if (fromIndex == i)
                    {
                        fromIndex = seg1.GetIndexTo();
                    }

                    int toIndex = seg2.GetIndexFrom();
                    if (toIndex == i)
                    {
                        toIndex = seg2.GetIndexTo();
                    }

                    Vector2 vec1 = (pointsVoronoi_simplyfied[fromIndex] - pointsVoronoi_simplyfied[i]).normalized;
                    Vector2 vec2 = (pointsVoronoi_simplyfied[toIndex] - pointsVoronoi_simplyfied[i]).normalized;

                    if (Maths2D.IsAngleBetweenTwoVectors180or360Degree(vec1, vec2, 0.01f))
                    {
                        conect[i].Remove(seg1);
                        conect[i].Remove(seg2);
                        if (seg1.GetIndexFrom() == i)
                        {
                            seg1.SetIndexFrom(toIndex);
                        }
                        else
                        {
                            seg1.SetIndexTo(toIndex);
                        }

                        conect[toIndex].Remove(seg2);
                        conect[toIndex].Add(seg1);

                        segments_simplyfied.Remove(seg2);
                        outsideSegments_simplyfied.Remove(seg2);
                    }
                }
            }

            //<<<<<<<<<< END FIX STRAIGHT EDGES WHO ARE NOT A SINGLE SEGMENT


            int numOutsideSegments = 0;
            List<Segment> newOutsideSegments = new List<Segment>();
            foreach (Segment s in segments_simplyfied) 
            {
                if (outsideSegments_simplyfied.Contains(s)) 
                {
                    numOutsideSegments++;
                    newOutsideSegments.Add(s);
                }
            }
            //outsideSegments_simplyfied = newOutsideSegments;
            //Debug.LogError(polygonID + " BBB---------------------------------> " + numOutsideSegments + " (count: " + outsideSegments_simplyfied.Count + ")");

            //Remove segments with only 1 connected side
            //Result: points without connection
            //TODO remove this points later --> problematic, because segments point to indexes of these points
            for (int i = 0; i < conect.Count; i++) 
            {
                List<Segment> connection = conect[i];
                if (connection.Count == 1)
                {
                    ////Debug.Log("HIT " + i + ", polygon: " + polygonID + " --> punkt: " + pointsVoronoi_simplyfied[i].x + ", " + pointsVoronoi_simplyfied[i].y);
                    Segment seg = connection[0];
                    //unnecessary wall
                    int otherSegmentCornerID = seg.GetIndexFrom();
                    if (otherSegmentCornerID == i) 
                    {
                        otherSegmentCornerID = seg.GetIndexTo();
                    }
                    conect[otherSegmentCornerID].Remove(seg);
                    conect[i].Remove(seg);

                    //pointsVoronoi_simplyfied.RemoveAt(i);
                    //conect.RemoveAt(i);
                    segments_simplyfied.Remove(seg);
                    outsideSegments_simplyfied.Remove(seg); 
                }
                if (connection.Count == 0) 
                {
                    
                }
            }

            // >>>>>>>> FIX OUTSIDE SEGMENTS
            ////Debug.LogError(polygonID + " setup: count edges: " + segments_simplyfied.Count);
            //if(segments_simplyfied.Count > 65)
            ////Debug.LogError(polygonID + " setup: länge 65: " + Vector2.Distance(pointsVoronoi_simplyfied[segments_simplyfied[65].GetIndexFrom()], pointsVoronoi_simplyfied[segments_simplyfied[65].GetIndexTo()]));
            /*
            List<Segment> fixedOutsideSegments_simplyfied = new List<Segment>();

            Vector2 startpoint = pointsVoronoi_simplyfied[0];
            int index_startpoint = 0;
            for (int i = 1; i < pointsVoronoi_simplyfied.Count; i++)
            {
                Vector2 p = pointsVoronoi_simplyfied[i];
                if (p.x < startpoint.x)
                {
                    startpoint = p;
                    index_startpoint = i;
                }
            }

            //First step
            int lastIndex = index_startpoint;
            int currentIndex = index_startpoint;
            Segment lastSegment = conect[currentIndex][0];

            List<Segment> possibleNextSegments = new List<Segment>();
            List<float> angles = new List<float>();
            int indexBiggestAngle = 0;
            float biggestAngle = -180;
            foreach (Segment s in conect[currentIndex])
            {
                possibleNextSegments.Add(s);
                int otherIndexNewSeg = s.GetIndexFrom();
                if (otherIndexNewSeg == currentIndex)
                {
                    otherIndexNewSeg = s.GetIndexTo();
                }
                Vector2 imaginaryPoint = new Vector2(pointsVoronoi_simplyfied[currentIndex].x - 1.0f, pointsVoronoi_simplyfied[currentIndex].y);
                Vector2 vec1 = (pointsVoronoi_simplyfied[currentIndex] - imaginaryPoint).normalized;
                Vector2 vec2 = (pointsVoronoi_simplyfied[otherIndexNewSeg] - pointsVoronoi_simplyfied[currentIndex]).normalized;
                float angle = Vector2.SignedAngle(vec1, vec2);
                ////Debug.LogError(polygonID + " setup >>> winkel " + angle + " raw: " + vec1.ToString("F5") + ", " + vec2.ToString("F5"));
                angles.Add(angle);
                if (angle > biggestAngle)
                {
                    biggestAngle = angle;
                    indexBiggestAngle = possibleNextSegments.Count - 1;
                }
            }
            lastSegment = possibleNextSegments[indexBiggestAngle];
            fixedOutsideSegments_simplyfied.Add(lastSegment);
            lastIndex = currentIndex;
            if (currentIndex == lastSegment.GetIndexFrom())
            {
                currentIndex = lastSegment.GetIndexTo();
            }
            else
            {
                currentIndex = lastSegment.GetIndexFrom();
            }

            //other steps
            while (currentIndex != index_startpoint && conect[currentIndex].Count >= 2)
            {
                possibleNextSegments = new List<Segment>();
                angles = new List<float>();
                indexBiggestAngle = 0;
                biggestAngle = -180;
                ////Debug.LogError(polygonID + " setup >>> " + conect[currentIndex].Count + " at postion: " + pointsVoronoi_simplyfied[currentIndex]);
                foreach (Segment s in conect[currentIndex])
                {
                    if (s != lastSegment)
                    {
                        possibleNextSegments.Add(s);
                        int otherIndexNewSeg = s.GetIndexFrom();
                        if (otherIndexNewSeg == currentIndex)
                        {
                            otherIndexNewSeg = s.GetIndexTo();
                        }
                        Vector2 vec1 = (pointsVoronoi_simplyfied[currentIndex] - pointsVoronoi_simplyfied[lastIndex]).normalized;
                        Vector2 vec2 = (pointsVoronoi_simplyfied[otherIndexNewSeg] - pointsVoronoi_simplyfied[currentIndex]).normalized;
                        float angle = Vector2.SignedAngle(vec1, vec2);
                        ////Debug.LogError(polygonID + " setup >>> winkel2 " + angle + " raw: " + vec1.ToString("F5") + ", " + vec2.ToString("F5"));
                        angles.Add(angle);
                        if (angle > biggestAngle)
                        {
                            biggestAngle = angle;
                            indexBiggestAngle = possibleNextSegments.Count - 1;
                        }
                    }
                }

                ////Debug.LogError(polygonID + " setup " + indexBiggestAngle + ", " + possibleNextSegments.Count);
                lastSegment = possibleNextSegments[indexBiggestAngle];
                fixedOutsideSegments_simplyfied.Add(lastSegment);
                lastIndex = currentIndex;
                if (currentIndex == lastSegment.GetIndexFrom())
                {
                    currentIndex = lastSegment.GetIndexTo();
                }
                else
                {
                    currentIndex = lastSegment.GetIndexFrom();
                }
            }
            */
            //>>> (Problem Holes) now WITH HOLES <<<


            outsideSegments_simplyfied = new List<Segment>();
            for (int i = 0; i < outsideSegmentsPerPolygon_straightened[polygonID].Length; i++)
            {
                outsideSegments_simplyfied.Add(outsideSegmentsPerPolygon_straightened[polygonID][i]);
            }

            List<List<Segment>> conectOutside = new List<List<Segment>>();
            for (int i = 0; i < pointsVoronoi_simplyfied.Count; i++)
            {
                conectOutside.Add(new List<Segment>());
            }

            for (int i = 0; i < outsideSegments_simplyfied.Count; i++)
            {
                conectOutside[outsideSegments_simplyfied[i].GetIndexFrom()].Add(outsideSegments_simplyfied[i]);
                conectOutside[outsideSegments_simplyfied[i].GetIndexTo()].Add(outsideSegments_simplyfied[i]);
            }

            bool foundConnetbaleSeg = true;
            int iteration = 0;
            ////Debug.LogError(polygonID + " ASDFASDFASDF " + iteration + ", outside segments count: " + outsideSegments_simplyfied.Count);
            //TODO: --> ich glaube While schleife ist unnötig, könnte entfernt werden
            while (foundConnetbaleSeg == true)
            {
                iteration++;
                foundConnetbaleSeg = false;
                for (int i = 0; i < conectOutside.Count; i++)
                {
                    if (conectOutside[i].Count == 2)
                    {
                        Segment seg1 = conectOutside[i][0];
                        Segment seg2 = conectOutside[i][1];

                        int fromIndex = seg1.GetIndexFrom();
                        if (fromIndex == i)
                        {
                            fromIndex = seg1.GetIndexTo();
                        }

                        int toIndex = seg2.GetIndexFrom();
                        if (toIndex == i)
                        {
                            toIndex = seg2.GetIndexTo();
                        }

                        Vector2 vec1 = (pointsVoronoi_simplyfied[fromIndex] - pointsVoronoi_simplyfied[i]).normalized;
                        Vector2 vec2 = (pointsVoronoi_simplyfied[toIndex] - pointsVoronoi_simplyfied[i]).normalized;

                        if (Maths2D.IsAngleBetweenTwoVectors180or360Degree(vec1, vec2, 0.01f))
                        {
                            foundConnetbaleSeg = true;
                            conectOutside[i].Remove(seg1);
                            conectOutside[i].Remove(seg2);
                            if (seg1.GetIndexFrom() == i)
                            {
                                seg1.SetIndexFrom(toIndex);
                            }
                            else
                            {
                                seg1.SetIndexTo(toIndex);
                            }

                            conectOutside[toIndex].Remove(seg2);
                            conectOutside[toIndex].Add(seg1);

                            outsideSegments_simplyfied.Remove(seg2);
                        }
                    }
                }
            }
            
            ////Debug.LogError(polygonID + " ASDFASDFASDF " + iteration + ", outside segments count: " + outsideSegments_simplyfied.Count);

            List<Segment> fixedOutsideSegments_simplyfied = new List<Segment>();
            
            foreach (Segment s in segments_simplyfied) 
            {
                Vector2 point1 = pointsVoronoi_simplyfied[s.GetIndexFrom()];
                Vector2 point2 = pointsVoronoi_simplyfied[s.GetIndexTo()];

                foreach (Segment oS in outsideSegments_simplyfied)
                {
                    Vector2 lineSegStartP = pointsVoronoi_simplyfied[oS.GetIndexFrom()];
                    Vector2 lineSegEndP = pointsVoronoi_simplyfied[oS.GetIndexTo()];
                    if (Maths2D.PointOnLineSegment(lineSegStartP, lineSegEndP, point1) && Maths2D.PointOnLineSegment(lineSegStartP, lineSegEndP, point2)) 
                    {
                        ////Debug.LogError(polygonID + ", TEST added new outside seg: " + s.GetIndexFrom() + " to " + s.GetIndexTo());
                        fixedOutsideSegments_simplyfied.Add(s);
                    }
                }
            }
            
            outsideSegments_simplyfied = fixedOutsideSegments_simplyfied;

            //<<<<<<<<<< END FIX OUTSIDE SEGMENTS


            List<Segment> addInNextCycle = new List<Segment>();
            List<Segment> removeInNextCycle = new List<Segment>();
            List<Segment> toShortSegments = new List<Segment>();

            //test
            smallSegments_simplyfied = new List<Segment>();
            foreach (Segment segment in segments_simplyfied) 
            {
                Vector2 p1 = pointsVoronoi_simplyfied[segment.GetIndexFrom()];
                Vector2 p2 = pointsVoronoi_simplyfied[segment.GetIndexTo()];
                
                if (Vector2.Distance(p1, p2) < smallSegmentTreshold) 
                {
                    toShortSegments.Add(segment);
                    //test
                    smallSegments_simplyfied.Add(segment);
                }
            }

            int num = show_iteration;
            int num_polygonid = show_polygonID;
            int countIterations = 0;
            bool noMoreStepsPossible = false;
            while (toShortSegments.Count >= 1 && countIterations < numOfSimplifyIterations && !noMoreStepsPossible)
            {
                //TODO remove count = 0, count is not used anymore
                int count = 0;
                bool fixed_in_this_round = false;
         
                if (toShortSegments.Count >= 1 && count < numOfSimplifyOperationsPerIteration)
                {
                    
                    Segment shortSegment = toShortSegments[0];
                    if (outsideSegments_simplyfied.Contains(shortSegment))
                    {
                        ////Debug.LogError("SHORTSEGMENT == OutsideSegment");
                    }
                    toShortSegments.RemoveAt(0);
                    List<Segment> neighboursFrom = new List<Segment>();
                    foreach (Segment s in conect[shortSegment.GetIndexFrom()]) 
                    {
                        neighboursFrom.Add(s);
                    }
                    neighboursFrom.Remove(shortSegment);
                    List<Segment> neighboursTo = new List<Segment>();
                    foreach (Segment s in conect[shortSegment.GetIndexTo()])
                    {
                        neighboursTo.Add(s);
                    }
                    neighboursTo.Remove(shortSegment);

                    if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                    {
                        //Debug.LogError(polygonID + " - 000B - 000C ---> segmentId: " + segments_simplyfied.IndexOf(shortSegment));
                    }
                    if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                    {
                        //Debug.LogError(polygonID + " - 000B - 000C ---> " + neighboursFrom.Count + ", " + neighboursTo.Count + ". segmentId: " + segments_simplyfied.IndexOf(shortSegment) + " and " + pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()].ToString("F5") + " to " + pointsVoronoi_simplyfied[shortSegment.GetIndexTo()].ToString("F5"));
                        //Debug.LogError(polygonID + " - 000B " + segments_simplyfied.Contains(shortSegment));
                    }

                    if (!fixed_in_this_round && neighboursFrom.Count == 0)
                    {
                        usedMethod = "short Segment nur einseitig (From) connected --> wird entfernt, Überprüfung ob übrige Segmente verbunden werden können";
                        if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                        {
                            //Debug.LogError(polygonID + " - 000B - A");
                        }
                        segments_simplyfied.Remove(shortSegment);
                        conect[shortSegment.GetIndexFrom()].Remove(shortSegment);
                        conect[shortSegment.GetIndexTo()].Remove(shortSegment);
                        //try to connect the remaining 2 segments if they are straight
                        if (conect[shortSegment.GetIndexTo()].Count == 2) 
                        {
                            List<Segment> listS = conect[shortSegment.GetIndexTo()];
                            Segment seg1 = listS[0];
                            Segment seg2 = listS[1];
                            Vector2 vec1 = pointsVoronoi_simplyfied[seg1.GetIndexTo()] - pointsVoronoi_simplyfied[seg1.GetIndexFrom()];
                            Vector2 vec2 = pointsVoronoi_simplyfied[seg2.GetIndexTo()] - pointsVoronoi_simplyfied[seg2.GetIndexFrom()];
                            if (Maths2D.IsAngleBetweenTwoVectors180or360Degree(vec1, vec2, 0.001f)) 
                            {
                                int newEndIndex = seg2.GetIndexFrom();
                                if (newEndIndex == shortSegment.GetIndexTo())
                                {
                                    newEndIndex = seg2.GetIndexTo();
                                }
                                if (seg1.GetIndexFrom() == shortSegment.GetIndexTo())
                                {
                                    seg1.SetIndexFrom(newEndIndex);
                                }
                                else
                                {
                                    seg1.SetIndexTo(newEndIndex);
                                }

                                segments_simplyfied.Remove(seg2);
                                toShortSegments.Remove(seg1);
                                toShortSegments.Remove(seg2);
                                outsideSegments_simplyfied.Remove(seg2);
                                conect[shortSegment.GetIndexTo()].Remove(seg1);
                                conect[shortSegment.GetIndexTo()].Remove(seg2);
                                conect[newEndIndex].Remove(seg2);
                                conect[newEndIndex].Add(seg1);
                            }
                        }
                        fixed_in_this_round = true;
                    }
                    if (!fixed_in_this_round && neighboursTo.Count == 0)
                    {
                        usedMethod = "short Segment nur einseitig (To) connected --> wird entfernt, Überprüfung ob übrige Segmente verbunden werden können";
                        if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                        {
                            //Debug.LogError(polygonID + " - 000B - B");
                        }
                        segments_simplyfied.Remove(shortSegment);
                        conect[shortSegment.GetIndexFrom()].Remove(shortSegment);
                        conect[shortSegment.GetIndexTo()].Remove(shortSegment);
                        if (conect[shortSegment.GetIndexFrom()].Count == 2)
                        {
                            List<Segment> listS = conect[shortSegment.GetIndexFrom()];
                            Segment seg1 = listS[0];
                            Segment seg2 = listS[1];
                            Vector2 vec1 = pointsVoronoi_simplyfied[seg1.GetIndexTo()] - pointsVoronoi_simplyfied[seg1.GetIndexFrom()];
                            Vector2 vec2 = pointsVoronoi_simplyfied[seg2.GetIndexTo()] - pointsVoronoi_simplyfied[seg2.GetIndexFrom()];
                            if (Maths2D.IsAngleBetweenTwoVectors180or360Degree(vec1, vec2, 0.001f))
                            {
                                int newEndIndex = seg2.GetIndexFrom();
                                if (newEndIndex == shortSegment.GetIndexFrom())
                                {
                                    newEndIndex = seg2.GetIndexTo();
                                }
                                if (seg1.GetIndexFrom() == shortSegment.GetIndexFrom())
                                {
                                    seg1.SetIndexFrom(newEndIndex);
                                }
                                else
                                {
                                    seg1.SetIndexTo(newEndIndex);
                                }

                                segments_simplyfied.Remove(seg2);
                                toShortSegments.Remove(seg1);
                                toShortSegments.Remove(seg2);
                                outsideSegments_simplyfied.Remove(seg2);
                                conect[shortSegment.GetIndexFrom()].Remove(seg1);
                                conect[shortSegment.GetIndexFrom()].Remove(seg2);
                                conect[newEndIndex].Remove(seg2);
                                conect[newEndIndex].Add(seg1);
                            }
                        }
                        fixed_in_this_round = true;
                    }
                    
                    
                    if (!fixed_in_this_round && neighboursFrom.Count == 1)
                    {
                        usedMethod = "short Segment ist im 180Grad Winkel mit nur genau einem Segmetn verbunden (From), beide Segmente werden vereint";
                        if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                        {
                            //Debug.LogError(polygonID + " - 000B - C");
                            //Debug.LogError(polygonID + " - 000B - C" + " with the segment: " + pointsVoronoi_simplyfied[neighboursFrom[0].GetIndexFrom()].ToString("F5") + " to " + pointsVoronoi_simplyfied[neighboursFrom[0].GetIndexTo()].ToString("F5"));
                            //Debug.LogError(polygonID + " - 000B - C testwinkel: " + Vector2.Angle(new Vector2(110, 90) - new Vector2(110, 85.19608f), new Vector2(110, 85.19608f) - new Vector2(110, 82.03333f)));
                            //Debug.LogError(polygonID + " - 000B - C testwinkel 2: " + Vector2.Angle(pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()] - pointsVoronoi_simplyfied[shortSegment.GetIndexTo()], pointsVoronoi_simplyfied[neighboursFrom[0].GetIndexFrom()] - pointsVoronoi_simplyfied[neighboursFrom[0].GetIndexTo()]));
                            //Debug.LogError(polygonID + " - 000B - C testwinkel 2 werte: seg1: " + (pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()] - pointsVoronoi_simplyfied[shortSegment.GetIndexTo()]).ToString("F5") + ", seg2: " + (pointsVoronoi_simplyfied[neighboursFrom[0].GetIndexFrom()] - pointsVoronoi_simplyfied[neighboursFrom[0].GetIndexTo()]).ToString("F5"));
                            //Debug.LogError(polygonID + " - 000B - C testwinkel 3: " + Vector2.Angle(new Vector2(0, 3.16274f), new Vector2(0, 4.80393f)));
                        }
                        Segment fromSegment = neighboursFrom[0];
                        Vector2 seg1 = (pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()] - pointsVoronoi_simplyfied[shortSegment.GetIndexTo()]).normalized;
                        Vector2 seg2 = (pointsVoronoi_simplyfied[fromSegment.GetIndexFrom()] - pointsVoronoi_simplyfied[fromSegment.GetIndexTo()]).normalized;
                        if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                        {
                            //Debug.LogError(polygonID + " - 000B - C - VERWIRRUNG: " + seg1.ToString("F5") + " und " + seg2.ToString("F5"));
                            //Debug.LogError(polygonID + " - 000B - C - VERWIRRUNG winklel: " + Vector2.Angle(seg1, seg2));
                            //Debug.LogError(polygonID + " - 000B - C - VERWIRRUNG winklel2: " + Vector2.SignedAngle(seg1, seg2));
                            //Debug.LogError(polygonID + " - 000B - C - VERWIRRUNG winklel3: " + Vector2.Angle(seg1.normalized, seg2.normalized));
                        }

                        if (Maths2D.IsAngleBetweenTwoVectors180or360Degree(seg1, seg2, 0.1f))
                        {
                            ////Debug.Log("180 degree angle -->" + polygonID + " --> from: " + +pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()].x + ", " + pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()].y + "; to: " + pointsVoronoi_simplyfied[shortSegment.GetIndexTo()].x + ", " + pointsVoronoi_simplyfied[shortSegment.GetIndexTo()].y);
                            //remove that segment, its already perfectly straight
                            ////Debug.LogError(polygonID + ": From (Count 1): Iteration: " + countIterations + ", round: " + count + " --> " + pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()] + ", " + pointsVoronoi_simplyfied[shortSegment.GetIndexTo()]); conect[shortSegment.GetIndexFrom()].Remove(shortSegment);
                            conect[shortSegment.GetIndexFrom()].Remove(fromSegment);
                            conect[shortSegment.GetIndexFrom()].Remove(shortSegment);
                            conect[shortSegment.GetIndexTo()].Remove(shortSegment);
                            conect[shortSegment.GetIndexTo()].Add(fromSegment);
                            if (fromSegment.GetIndexFrom() == shortSegment.GetIndexFrom())
                            {
                                fromSegment.SetIndexFrom(shortSegment.GetIndexTo());
                            }
                            else
                            {
                                fromSegment.SetIndexTo(shortSegment.GetIndexTo());
                            }

                            segments_simplyfied.Remove(shortSegment);
                            outsideSegments_simplyfied.Remove(shortSegment);
                            toShortSegments.Remove(fromSegment);
                            fixed_in_this_round = true;
                        }
                        if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                        {
                            //Debug.LogError(polygonID + " - 000B - C BUT IS IT FIXED " + fixed_in_this_round + " why?: winkel: " + Vector2.Angle(seg1, seg2));
                        }
                    }
                    
                    
                    if (!fixed_in_this_round && neighboursTo.Count == 1)
                    {
                        usedMethod = "short Segment ist im 180Grad Winkel mit nur genau einem Segmetn verbunden (To), beide Segmente werden vereint";
                        if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                        {
                            //Debug.LogError(polygonID + " - 000B - D");
                        }
                        Segment toSegment = neighboursTo[0];
                        Vector2 seg1 = (pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()] - pointsVoronoi_simplyfied[shortSegment.GetIndexTo()]).normalized;
                        Vector2 seg2 = (pointsVoronoi_simplyfied[toSegment.GetIndexFrom()] - pointsVoronoi_simplyfied[toSegment.GetIndexTo()]).normalized;
                        if (Maths2D.IsAngleBetweenTwoVectors180or360Degree(seg1, seg2, 0.1f))
                        {
                            ////Debug.Log("180 degree angle -->" + polygonID + " --> from: " + +pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()].x + ", " + pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()].y + "; to: " + pointsVoronoi_simplyfied[shortSegment.GetIndexTo()].x + ", " + pointsVoronoi_simplyfied[shortSegment.GetIndexTo()].y);
                            //remove that segment, its already perfectly straight
                            ////Debug.LogError(polygonID + ": To (Count 1): Iteration: " + countIterations + ", round: " + count + " --> " + pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()] + ", " + pointsVoronoi_simplyfied[shortSegment.GetIndexTo()]);
                            conect[shortSegment.GetIndexTo()].Remove(shortSegment);
                            conect[shortSegment.GetIndexTo()].Remove(toSegment);
                            conect[shortSegment.GetIndexFrom()].Remove(shortSegment);
                            conect[shortSegment.GetIndexFrom()].Add(toSegment);
                            if (toSegment.GetIndexFrom() == shortSegment.GetIndexTo())
                            {
                                toSegment.SetIndexFrom(shortSegment.GetIndexFrom());
                            }
                            else
                            {
                                toSegment.SetIndexTo(shortSegment.GetIndexFrom());
                            }
                            segments_simplyfied.Remove(shortSegment);
                            outsideSegments_simplyfied.Remove(shortSegment);
                            toShortSegments.Remove(toSegment);
                            fixed_in_this_round = true;
                            if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                            {
                                //Debug.LogError(polygonID + " - 000B - D outside?: " + outsideSegments_simplyfied.Contains(shortSegment));
                                //Debug.LogError(polygonID + " - 000B - D BUT IS IT FIXED " + fixed_in_this_round + " why?: winkel: " + Vector2.Angle(seg1, seg2));
                            }
                        }
                    }
                    
                    
                    if (!fixed_in_this_round)
                    {
                        if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                        {
                            //Debug.LogError(polygonID + " - 000B - E");
                        }
                        //requirements:
                        //1. only look for the extreme angles (segment with the smallest angle and segment with the biggest angle)
                        //2. not possible with segments with an angle of 180 degree

                        //See if the parallel displacement of the segment through the other point of the short segment intersects with
                        //any former neighbors of the displaced segment and with no other segments
                        //Make sure no room switches the sides with this technique

                        float biggest_from_angle = -180;
                        Segment from_neighbour_seg_big_angle = null;
                        float smallest_from_angle = 180;
                        Segment from_neighbour_seg_small_angle = null; ;
                        float biggest_to_angle = -180;
                        Segment to_neighbour_seg_big_angle = null; ;
                        float smallest_to_angle = 180;
                        Segment to_neighbour_seg_small_angle = null; ;

                        foreach (Segment s in neighboursFrom)
                        {
                            Vector2 vec1 = (pointsVoronoi_simplyfied[shortSegment.GetIndexTo()] - pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()]).normalized;
                            Vector2 vec2 = (pointsVoronoi_simplyfied[s.GetIndexTo()] - pointsVoronoi_simplyfied[s.GetIndexFrom()]).normalized;
                            float angle = Vector2.SignedAngle(vec1, vec2);

                            if (angle > biggest_from_angle)
                            {
                                biggest_from_angle = angle;
                                from_neighbour_seg_big_angle = s;
                            }
                            if (angle < smallest_from_angle)
                            {
                                smallest_from_angle = angle;
                                from_neighbour_seg_small_angle = s;
                            }
                        }
                        foreach (Segment s in neighboursTo)
                        {
                            Vector2 vec1 = (pointsVoronoi_simplyfied[shortSegment.GetIndexTo()] - pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()]).normalized;
                            Vector2 vec2 = (pointsVoronoi_simplyfied[s.GetIndexTo()] - pointsVoronoi_simplyfied[s.GetIndexFrom()]).normalized;
                            float angle = Vector2.SignedAngle(vec1, vec2);

                            if (angle > biggest_to_angle)
                            {
                                biggest_to_angle = angle;
                                to_neighbour_seg_big_angle = s;
                            }
                            if (angle < smallest_to_angle)
                            {
                                smallest_to_angle = angle;
                                to_neighbour_seg_small_angle = s;
                            }
                        }

                        if (!fixed_in_this_round && neighboursFrom.Count >= 1)
                        {
                            if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                            {
                                //Debug.LogError(polygonID + " - 000B - E-1");
                            }
                            if (!fixed_in_this_round && biggest_from_angle != -180 && !Maths2D.IsAngleBetweenTwoVectors180or360Degree(biggest_from_angle, 0.1f))
                            {
                                usedMethod = "short Segment Test ob Verschiebung des benachbarten Segments (From) möglich ist (größter Winkel)";
                                if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                                {
                                    //Debug.LogError(polygonID + " - 000B - E-1-A");
                                    //Debug.LogError(polygonID + " - 000B - E-1-A" + " with the segment: " + pointsVoronoi_simplyfied[from_neighbour_seg_big_angle.GetIndexFrom()].ToString("F5") + " to " + pointsVoronoi_simplyfied[from_neighbour_seg_big_angle.GetIndexTo()].ToString("F5"));
                                    //Debug.LogError(polygonID + " - 000B - E-1-A " + biggest_from_angle);
                                    //Debug.LogError(polygonID + " - 000B - E-1-A test andere Winkel: " +smallest_to_angle + ", " + biggest_to_angle + ", " + smallest_from_angle);
                                    //Debug.LogError(polygonID + " - 000B - E-1-A testWinkel: " + Vector2.SignedAngle(new Vector2(20, 108.06670f) - new Vector2(61.32f, 108.06670f), new Vector2(64.20876f, 108.06670f) - new Vector2(61.32f, 108.06670f)));

                                }
                                Segment from_seg = from_neighbour_seg_big_angle;
                                int connectedIndex = shortSegment.GetIndexFrom();
                                int notConnectedIndex = from_seg.GetIndexFrom();
                                if (notConnectedIndex == connectedIndex)
                                {
                                    notConnectedIndex = from_seg.GetIndexTo();
                                }
                                int newSegmentConIndex = shortSegment.GetIndexTo();

                                fixed_in_this_round = ShiftOneSideOfAToShortSegment(from_seg, shortSegment, conect,notConnectedIndex, connectedIndex, newSegmentConIndex, toShortSegments, polygonID, countIterations);
                                if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                                {
                                    //Debug.LogError(polygonID + " - 000B - E-1-A" + " is it fixed: " + fixed_in_this_round);
                                }
                            }
                            if (!fixed_in_this_round && smallest_from_angle != biggest_from_angle && smallest_from_angle != 180 && !Maths2D.IsAngleBetweenTwoVectors180or360Degree(smallest_from_angle, 0.1f))
                            {
                                usedMethod = "short Segment Test ob Verschiebung des benachbarten Segments (From) möglich ist (kleinster Winkel: " + smallest_from_angle + ")";
                                
                                if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                                {
                                    //Debug.LogError(polygonID + " - 000B - E-1-B");
                                }
                                if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                                {
                                    //Debug.LogError(polygonID + " - 000B - E-1-B");
                                    //Debug.LogError(polygonID + " - 000B - E-1-B" + " with the segment: " + pointsVoronoi_simplyfied[from_neighbour_seg_small_angle.GetIndexFrom()].ToString("F5") + " to " + pointsVoronoi_simplyfied[from_neighbour_seg_small_angle.GetIndexTo()].ToString("F5"));
                                    //Debug.LogError(polygonID + " - 000B - E-1-B " + smallest_from_angle);
                                    //Debug.LogError(polygonID + " - 000B - E-1-B test andere Winkel: " + smallest_to_angle + ", " + biggest_to_angle + ", " + biggest_from_angle);
                                }
                                Segment from_seg = from_neighbour_seg_small_angle;
                                int connectedIndex = shortSegment.GetIndexFrom();
                                int notConnectedIndex = from_seg.GetIndexFrom();
                                if (notConnectedIndex == connectedIndex)
                                {
                                    notConnectedIndex = from_seg.GetIndexTo();
                                }
                                int newSegmentConIndex = shortSegment.GetIndexTo();

                                fixed_in_this_round = ShiftOneSideOfAToShortSegment(from_seg, shortSegment, conect, notConnectedIndex, connectedIndex, newSegmentConIndex, toShortSegments, polygonID, countIterations);
                                if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                                {
                                    //Debug.LogError(polygonID + " - 000B - E-1-B" + " is it fixed: " + fixed_in_this_round);
                                }
                            }
                        }

                        if (!fixed_in_this_round && neighboursTo.Count >= 1)
                        {
                            if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                            {
                                //Debug.LogError(polygonID + " - 000B - E-2");
                            }
                            if (!fixed_in_this_round && biggest_to_angle != -180 && !Maths2D.IsAngleBetweenTwoVectors180or360Degree(biggest_to_angle, 0.1f))
                            {
                                usedMethod = "short Segment Test ob Verschiebung des benachbarten Segments (To) möglich ist (größter Winkel)";
                                if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                                {
                                    //Debug.LogError(polygonID + " - 000B - E-2-A");
                                    //Debug.LogError(polygonID + " - 000B - E-2-A" + " with the segment: " + pointsVoronoi_simplyfied[to_neighbour_seg_big_angle.GetIndexFrom()].ToString("F5") + " to " + pointsVoronoi_simplyfied[to_neighbour_seg_big_angle.GetIndexTo()].ToString("F5"));
                                    //Debug.LogError(polygonID + " - 000B - E-2-A " + biggest_to_angle);
                                    //Debug.LogError(polygonID + " - 000B - E-2-A test andere Winkel: " + smallest_to_angle + ", " + smallest_from_angle + ", " + biggest_from_angle);
                                }
                                Segment to_seg = to_neighbour_seg_big_angle;
                                int connectedIndex = shortSegment.GetIndexTo();
                                int notConnectedIndex = to_seg.GetIndexFrom();
                                if (notConnectedIndex == connectedIndex)
                                {
                                    notConnectedIndex = to_seg.GetIndexTo();
                                }
                                int newSegmentConIndex = shortSegment.GetIndexFrom(); 

                                fixed_in_this_round = ShiftOneSideOfAToShortSegment(to_seg, shortSegment, conect, notConnectedIndex, connectedIndex, newSegmentConIndex, toShortSegments, polygonID, countIterations);
                                if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                                {
                                    //Debug.LogError(polygonID + " - 000B - E-2-A" + " is it fixed: " + fixed_in_this_round);
                                }
                            }
                            if (!fixed_in_this_round && smallest_to_angle != biggest_to_angle && smallest_to_angle != 180 && !Maths2D.IsAngleBetweenTwoVectors180or360Degree(smallest_to_angle, 0.1f))
                            {
                                usedMethod = "short Segment Test ob Verschiebung des benachbarten Segments (To) möglich ist (kleinster Winkel)";
                                if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                                {
                                    //Debug.LogError(polygonID + " - 000B - E-2-B" + " with the segment: " + pointsVoronoi_simplyfied[to_neighbour_seg_small_angle.GetIndexFrom()].ToString("F5") + " to " + pointsVoronoi_simplyfied[to_neighbour_seg_small_angle.GetIndexTo()].ToString("F5"));
                                    //Debug.LogError(polygonID + " - 000B - E-2-B " + smallest_to_angle);
                                    //Debug.LogError(polygonID + " - 000B - E-2-B test andere Winkel: " + biggest_to_angle + ", " + smallest_from_angle + ", " + biggest_from_angle);
                                    //Debug.LogError(polygonID + " - 000B - E-2-B testWinkel: " + Vector2.SignedAngle(new Vector2(20, 108.06670f) - new Vector2(61.32f, 108.06670f), new Vector2(64.20876f, 108.06670f) - new Vector2(61.32f, 108.06670f)));
                                }
                                Segment to_seg = to_neighbour_seg_small_angle;
                                int connectedIndex = shortSegment.GetIndexTo();
                                int notConnectedIndex = to_seg.GetIndexFrom();
                                if (notConnectedIndex == connectedIndex)
                                {
                                    notConnectedIndex = to_seg.GetIndexTo();
                                }
                                int newSegmentConIndex = shortSegment.GetIndexFrom();

                                fixed_in_this_round = ShiftOneSideOfAToShortSegment(to_seg, shortSegment, conect, notConnectedIndex, connectedIndex, newSegmentConIndex, toShortSegments, polygonID, countIterations);
                                if (showDebugLog == true && polygonID == num_polygonid && countIterations == num)
                                {
                                    //Debug.LogError(polygonID + " - 000B - E-2-B" + " is it fixed: " + fixed_in_this_round);
                                }
                            }

                            //TODO:
                            //Schauen ob eine Ecke in die entsprechend andere Ecke umgewandelt werden kann
                            if (allowShortCornerSwap)
                            {
                                if (!fixed_in_this_round) 
                                {
                                    
                                }
                            }

                            //Test auf spitze Winkel --> evtl. auch schon vorher spitze Winkel vermeiden
                            //Generell ersteinmal schauen, welche Möglichkeiten bestehen die zu kurzen Segmente zu beseitigen und dann zu schauen, welche Variante die beste ist
                            // - produziert keine spitzen Winkel, reduziert insgesammt die Anzahl an Winkeln

                            //Test ob irgendwo Winkel enthalten sind, die keinen Rechten Winkel bilden und ob sich dies lösen lässt (aka Segmente die auf beiden Seiten keine rechten Winkel haben)

                            //Versuchen statt nur eines Segmentes ganze Segmentreihen zu verschieben (falls diese im 180 Grad-Winkel verbunden sind und es mit möglichen Seitenwinkeln kein Problem gibt, und keines der Segmente Outside-Segment ist)
                            if (allowMoreSegmentShifts) 
                            {
                                
                            }
                        }
                    }
                    count++;
                    /*
                    foreach (Segment s in segments_simplyfied)
                    {
                        if (Vector2.Distance(pointsVoronoi_simplyfied[s.GetIndexFrom()], pointsVoronoi_simplyfied[s.GetIndexTo()]) == 0)
                        {
                            //Debug.Log("ERROR ERROR ERROR zero length segment added " + polygonID + " - 000B -------->>>>>>>>>>> " + countIterations + ", fixed?: " + fixed_in_this_round);
                        }
                    }
                    */
                }

                /*
                if (drawSpecifcSegment && polygonID == num_polygonid) 
                {
                    int num2 = drawSpecificSegmentNum;
                    Vector2 p1 = pointsVoronoi_simplyfied[segments_simplyfied[num2].GetIndexFrom()];
                    Vector2 p2 = pointsVoronoi_simplyfied[segments_simplyfied[num2].GetIndexTo()];

                    ////Debug.LogError("real dist:" + Vector2.Distance(p1, p2) + " --> " + conect[segments_simplyfied[63].GetIndexFrom()].Count + ", " + conect[segments_simplyfied[63].GetIndexTo()].Count);
                    if (Vector2.Distance(p1, p2) < smallSegmentTreshold)
                    {
                        //Debug.LogError("is smaller");
                    }
                }
                */

                /*
                // >>>>>>>> FIX THIS IN STRAIGHTEN EDGES (POINTS WITH SAME COORDINATES BUT NO CONNECTION)
                //find points with the same coordinates
                Dictionary<int, int> mapPoint2 = new Dictionary<int, int>();
                for (int i = 0; i < pointsVoronoi_simplyfied.Count; i++)
                {
                    for (int k = i + 1; k < pointsVoronoi_simplyfied.Count; k++)
                    {

                        if ((pointsVoronoi_simplyfied[i].x + 0.0001f > pointsVoronoi_simplyfied[k].x) &&
                           (pointsVoronoi_simplyfied[i].x - 0.0001f < pointsVoronoi_simplyfied[k].x) &&
                           (pointsVoronoi_simplyfied[i].y + 0.0001f > pointsVoronoi_simplyfied[k].y) &&
                           (pointsVoronoi_simplyfied[i].y - 0.0001f < pointsVoronoi_simplyfied[k].y))
                        {
                            mapPoint2.Add(k, i);
                        }
                    }
                }

                foreach (Segment s in segments_simplyfied)
                {
                    int from = s.GetIndexFrom();
                    int to = s.GetIndexTo();

                    if (mapPoint2.ContainsKey(from))
                    {
                        //Debug.LogError("------------------> duplicate points");
                        s.SetIndexFrom(mapPoint2[from]);
                    }
                    if (mapPoint2.ContainsKey(to))
                    {
                        //Debug.LogError("------------------> duplicate points");
                        s.SetIndexTo(mapPoint2[to]);
                    }
                }
                //<<<<<<<<<< END FIX FOR STRAIGHTEN EDGES (POINTS WITH SAME COORDINATES BUT NO CONNECTION)
                */

                if (toShortSegments.Count == 0)
                {
                    toShortSegments = new List<Segment>();
                    //test
                    smallSegments_simplyfied = new List<Segment>();
                    foreach (Segment segment in segments_simplyfied)
                    {
                        Vector2 p1 = pointsVoronoi_simplyfied[segment.GetIndexFrom()];
                        Vector2 p2 = pointsVoronoi_simplyfied[segment.GetIndexTo()];

                        if (Vector2.Distance(p1, p2) < smallSegmentTreshold)
                        {
                            toShortSegments.Add(segment);
                            //test
                            smallSegments_simplyfied.Add(segment);
                        }
                    }
                    ////Debug.LogError(polygonID + " --> added " + toShortSegments.Count + " new small segments");
                }
                else 
                {
                    smallSegments_simplyfied = toShortSegments;
                }
                /*
                if (toShortSegments.Count == 0) 
                {
                    //Debug.LogError(polygonID + " --> EndReached");
                    noMoreStepsPossible = true;
                }
                */
                if (showDebugLog == true && polygonID == num_polygonid)
                {
                    ////Debug.LogError(polygonID + ": in iteration " + countIterations + " >>> outsideSegments: " + outsideSegments_simplyfied.Count);
                    //Debug.LogError(polygonID + ": in iteration " + countIterations + " >>> (Fixed: " + fixed_in_this_round + ") used Method: " + usedMethod);
                }
                countIterations++;
            }


            //FOR THE WALL CREATION --> dunno rly why^^
            //UpdateOverlayingSegments(conect, segments_simplyfied, pointsVoronoi_simplyfied);
            //UpdateOverlayingSegments(conect, outsideSegments_simplyfied, pointsVoronoi_simplyfied);
            //UpdateOverlayingSegments(conect, smallSegments_simplyfied, pointsVoronoi_simplyfied);

            RemoveZeroLengthSegmentsFromSegmentsList(conect, segments_simplyfied, pointsVoronoi_simplyfied, out List<Segment> segments_simplyfied_withoutZ);
            RemoveZeroLengthSegmentsFromSegmentsList(conect, outsideSegments_simplyfied, pointsVoronoi_simplyfied, out List<Segment> outsideSegments_simplyfied_withoutZ);
            RemoveZeroLengthSegmentsFromSegmentsList(conect, smallSegments_simplyfied, pointsVoronoi_simplyfied, out List<Segment> smallSegments_simplyfied_withoutZ);

            UpdateListCornerPoints(segments_simplyfied_withoutZ, pointsVoronoi_simplyfied, outsideSegments_simplyfied_withoutZ, smallSegments_simplyfied_withoutZ, out List<Segment> segments_simplyfied_U, out List<Vector2> pointsVoronoi_simplyfied_U, out List<Segment> outsideSegments_simplyfied_U, out List<Segment> smallSegments_simplyfied_U);



            segmentsPerPolygon_simplyfied[polygonID] = segments_simplyfied_U.ToArray();
            pointsVoronoiPerPolygon_simplyfied[polygonID] = pointsVoronoi_simplyfied_U.ToArray();
            outsideSegmentsPerPolygon_simplyfied[polygonID] = outsideSegments_simplyfied_U.ToArray();
            //outsideSegmentsPerPolygon_simplyfied[polygonID] = fixedOutsideSegments_simplyfied.ToArray();
            //outsideSegmentsPerPolygon_simplyfied[polygonID] = segments_simplyfied.ToArray();
            //outsideSegmentsPerPolygon_simplyfied[polygonID] = outsideSegmentsPerPolygon[polygonID];
            //outsideSegmentsPerPolygon_simplyfied[polygonID] = outsideSegmentsPerPolygon_straightened[polygonID];
            smallSegmentsPerPolygon_simplyfied[polygonID] = smallSegments_simplyfied_U.ToArray();

            stopwatch.Stop();
            timesPerPolygon[polygonID].Add(Mathf.RoundToInt((float)stopwatch.Elapsed.TotalMilliseconds));
            stopwatch.Reset();
        }


        //FIND ALL HALLWAYS IN ROOMS THAT ARE TO SMALL
        /*      
        smallCorners = new List<Vector2[]>[polygons.Length];

        for (int polygonID = 0; polygonID < polygons.Length; polygonID++)
        {
            smallCorners[polygonID] = new List<Vector2[]>();
            foreach (Vector2 p in pointsVoronoiPerPolygon[polygonID])
            {
                foreach (Segment s in segmentsPerPolygon[polygonID])
                {
                    Vector2 closestPoint = Maths2D.ClosestPointOnLineSegment(pointsVoronoiPerPolygon[polygonID][s.GetIndexFrom()], pointsVoronoiPerPolygon[polygonID][s.GetIndexTo()], p);
                    float dist = Vector2.Distance(p, closestPoint);
                    ////Debug.Log("TEST: " + dist + ", REFERENZE: " + smallCornersTreshold + " and " + 1.5f);
                    if (dist <= smallCornersTreshold && dist > 0)
                    {
                        Vector2[] smallC = new Vector2[2];
                        smallC[0] = p;
                        smallC[1] = closestPoint;

                        smallCorners[polygonID].Add(smallC);
                    }
                }
            }
        }
        */

        //TODO
        //Merge Points that are close to each other if possible (probably merge them earlier in development process (right after creation?))
        //  --> points that are connected and close to each other AND no points is part of and outside-segment can be merged in the middle
        //  --> points that are connected and close to each other where ONE point is part of an outside-segment can be merged in the points of the outside-Segment
        //  --> points that are connected and close to each other where BOTH points are outside-segments can be merged in the middle if no point is an outside-corner,
        //      or merged in the outside-corner, if only 1 point is an outside corner
        //  --> points that are connected and close to each other but are both outside-corners cant be merged
        //Remove points (and merge segments) with only 2 connected segment , where the angle between both segements equals 180 degree

        //Exclude the look from outside-points to outside-segments
        //Exclude small corners along an existing segment

        //--> now Fix this Problems!!!
        //  --> add the small-corner-connection OR if possible add the direct connection between the small point and the start and endpoint of the segment

        //time = timeNow;
        for (int i = 0; i < timesPerPolygon.Length; i++) 
        {
            //String timesSingleLine = ("polygon (" + i + ") ");
            String timesSingleLine = ("--> (" + i + ") ");
            int timeSum = 0;
            for (int k = 0; k < timesPerPolygon[i].Count; k++) 
            {
                int timeI = timesPerPolygon[i][k];
                timeSum += timeI;
                //timesSingleLine += ("with " + k + " was " + timeI + ", ");
                timesSingleLine += (timeI + ", ");
            }
            timesSingleLine += ("SUM: " + timeSum);
            Debug.LogWarning("TIME " + timesSingleLine);
        }
    }

    private void UpdateOverlayingSegments(List<List<Segment>> conect, List<Segment> segments, List<Vector2> pointsVoronoi)
    {
        foreach (Segment s in segments) 
        {
            int id_from = s.GetIndexFrom();
            int id_to = s.GetIndexTo();

            List<Segment> neighboutsFrom = new List<Segment>();
            foreach (Segment neig in conect[id_from]) 
            {
                neighboutsFrom.Add(neig);
            }
            neighboutsFrom.Remove(s);
            foreach (Segment neighbour in neighboutsFrom)
            {
                int loosePointId = neighbour.GetIndexFrom();
                if (loosePointId == id_from) 
                {
                    loosePointId = neighbour.GetIndexTo();
                }

                Vector2 point = pointsVoronoi[loosePointId];

                if (Maths2D.PointOnLineSegment(pointsVoronoi[id_from], pointsVoronoi[id_to], point))
                {
                    ////Debug.LogError("Overlaying Segment Found");
                    s.SetIndexFrom(loosePointId);
                    conect[id_from].Remove(s);
                    conect[loosePointId].Add(s);
                }
            }

            List<Segment> neighboutsTo = new List<Segment>();
            foreach (Segment neig in conect[id_to])
            {
                neighboutsTo.Add(neig);
            }
            neighboutsTo.Remove(s);
            foreach (Segment neighbour in neighboutsTo)
            {
                int loosePointId = neighbour.GetIndexFrom();
                if (loosePointId == id_to)
                {
                    loosePointId = neighbour.GetIndexTo();
                }

                Vector2 point = pointsVoronoi[loosePointId];

                if (Maths2D.PointOnLineSegment(pointsVoronoi[id_from], pointsVoronoi[id_to], point))
                {
                    ////Debug.LogError("Overlaying Segment Found");
                    s.SetIndexTo(loosePointId);
                    conect[id_to].Remove(s);
                    conect[loosePointId].Add(s);
                }
            }

        }
    }

    private void UpdateListCornerPoints(List<Segment> segments, List<Vector2> pointsVoronoi, List<Segment> outsideSegments, List<Segment> smallSegments, out List<Segment> segments_UPDATED, out List<Vector2> pointsVoronoi_UPDATED, out List<Segment> outsideSegments_UPDATED, out List<Segment> smallSegments_UPDATED) 
    {
        Dictionary<int, int> mapCornerPoints = new Dictionary<int, int>();

        segments_UPDATED = new List<Segment>();
        pointsVoronoi_UPDATED = new List<Vector2>();
        outsideSegments_UPDATED = new List<Segment>();
        smallSegments_UPDATED = new List<Segment>();

        for (int i = 0; i < segments.Count; i++) 
        {
            Segment s = segments[i];
            int id_from = s.GetIndexFrom();
            int id_to = s.GetIndexTo();

            int new_id_from;
            int new_id_to;
            if (mapCornerPoints.ContainsKey(id_from))
            {
                new_id_from = mapCornerPoints[id_from];
            }
            else 
            {
                Vector2 corner = pointsVoronoi[id_from];
                pointsVoronoi_UPDATED.Add(corner);
                new_id_from = pointsVoronoi_UPDATED.Count-1;
                mapCornerPoints.Add(id_from, new_id_from);
            }
            if (mapCornerPoints.ContainsKey(id_to))
            {
                new_id_to = mapCornerPoints[id_to];
            }
            else
            {
                Vector2 corner = pointsVoronoi[id_to];
                pointsVoronoi_UPDATED.Add(corner);
                new_id_to = pointsVoronoi_UPDATED.Count-1;
                mapCornerPoints.Add(id_to, new_id_to);
            }

            Segment newS = new Segment(new_id_from, new_id_to);
            segments_UPDATED.Add(newS);
        }

        for (int i = 0; i < outsideSegments.Count; i++)
        {
            Segment s = outsideSegments[i];
            int id_from = s.GetIndexFrom();
            int id_to = s.GetIndexTo();

            int new_id_from;
            int new_id_to;
            if (mapCornerPoints.ContainsKey(id_from))
            {
                new_id_from = mapCornerPoints[id_from];
            }
            else
            {
                Vector2 corner = pointsVoronoi[id_from];
                pointsVoronoi_UPDATED.Add(corner);
                new_id_from = pointsVoronoi_UPDATED.Count-1;
                mapCornerPoints.Add(id_from, new_id_from);
            }
            if (mapCornerPoints.ContainsKey(id_to))
            {
                new_id_to = mapCornerPoints[id_to];
            }
            else
            {
                Vector2 corner = pointsVoronoi[id_to];
                pointsVoronoi_UPDATED.Add(corner);
                new_id_to = pointsVoronoi_UPDATED.Count-1;
                mapCornerPoints.Add(id_to, new_id_to);
            }

            Segment newS = new Segment(new_id_from, new_id_to);
            outsideSegments_UPDATED.Add(newS);
        }

        for (int i = 0; i < smallSegments.Count; i++)
        {
            Segment s = smallSegments[i];
            int id_from = s.GetIndexFrom();
            int id_to = s.GetIndexTo();

            int new_id_from;
            int new_id_to;
            if (mapCornerPoints.ContainsKey(id_from))
            {
                new_id_from = mapCornerPoints[id_from];
            }
            else
            {
                Vector2 corner = pointsVoronoi[id_from];
                pointsVoronoi_UPDATED.Add(corner);
                new_id_from = pointsVoronoi_UPDATED.Count-1;
                mapCornerPoints.Add(id_from, new_id_from);
            }
            if (mapCornerPoints.ContainsKey(id_to))
            {
                new_id_to = mapCornerPoints[id_to];
            }
            else
            {
                Vector2 corner = pointsVoronoi[id_to];
                pointsVoronoi_UPDATED.Add(corner);
                new_id_to = pointsVoronoi_UPDATED.Count-1;
                mapCornerPoints.Add(id_to, new_id_to);
            }

            Segment newS = new Segment(new_id_from, new_id_to);
            smallSegments_UPDATED.Add(newS);
        }
    }

    private void RemoveZeroLengthSegmentsFromSegmentsList(List<List<Segment>> conect, List<Segment> segments, List<Vector2> pointsVoronoi, out List<Segment> segementsReduced)
    {

        List<Segment> removeSeg = new List<Segment>();
        foreach (Segment s in segments)
        {
            int id_from = s.GetIndexFrom();
            int id_to = s.GetIndexTo();
            if (Vector2.Distance(pointsVoronoi[id_from], pointsVoronoi[id_to]) < 0.00001f)
            {
                //Debug.LogError("asdfgh Zero Length Segment");
                //id_to >>> id_from

                removeSeg.Add(s);
                conect[s.GetIndexFrom()].Remove(s);
                conect[s.GetIndexTo()].Remove(s);
                List<Segment> newConnections = new List<Segment>();
                foreach (Segment seg in conect[s.GetIndexFrom()])
                {
                    if (seg.GetIndexFrom() == id_to) 
                    {
                        seg.SetIndexFrom(id_from);
                    }
                    if (seg.GetIndexTo() == id_to)
                    {
                        seg.SetIndexTo(id_from);
                    }
                    newConnections.Add(s);
                }
                foreach (Segment seg in conect[s.GetIndexTo()])
                {
                    if (seg.GetIndexFrom() == id_to)
                    {
                        seg.SetIndexFrom(id_from);
                    }
                    if (seg.GetIndexTo() == id_to)
                    {
                        seg.SetIndexTo(id_from);
                    }
                    if (!newConnections.Contains(s))
                    {
                        newConnections.Add(s);
                    }
                }
                conect[id_from] = newConnections;
                conect[id_to] = new List<Segment>();
            }
        }
        foreach (Segment s in removeSeg) 
        {
            segments.Remove(s);
        }
        segementsReduced = segments;
    }

    public List<Vector3> getRoomPositions() 
    {
        return rand_roomPositions;
    }

    public List<float> getRoomSizes()
    {
        return rand_roomSizes;
    }

    public List<RoomType> getRoomTypes()
    {
        return rand_roomTypes;
    }

    public Vector2[] getWallCornerForSpecificBuilding(int buildingNum) 
    {
        return pointsVoronoiPerPolygon_simplyfied[buildingNum];
    }

    public Segment[] getSegmentsForSpecificBuilding(int buildingNum) 
    {
        return segmentsPerPolygon_simplyfied[buildingNum];
    }

    public ShapeCreator GetShapeCreator() 
    {
        return shapeCreator;
    }

    bool ShiftOneSideOfAToShortSegment(Segment from_seg, Segment shortSegment, List<List<Segment>> conect, int notConnectedIndex, int connectedIndex, int newSegmentConIndex, List<Segment>toShortSegments, int polygonID, int countIterations) 
    {
        if (allowMoreSegmentShifts) 
        {
            return ShiftOneSideWihtMultipleSegmentsOfAToShortSegment(from_seg, shortSegment, conect, notConnectedIndex, connectedIndex, newSegmentConIndex, toShortSegments, polygonID, countIterations);
        }

        //if (show_polygonID == 6 && show_iteration == 65)
        //{
        //    //Debug.LogError("6" + " - 000B - E-1-B --> Connectivität ersetztes Element: " + "connectedIndex: " + conect[connectedIndex].Count + ", notCon: " + conect[notConnectedIndex].Count);
        //}
        //from_seg, shortSegment, conect, notConnectedIndex, connectedIndex, newSegmentConIndex, toShortSegments, polygonID, countIterations
        List<Segment> neighboursAngleSeg = new List<Segment>();
        foreach (Segment s in conect[notConnectedIndex])
        {
            neighboursAngleSeg.Add(s);
        }
        neighboursAngleSeg.Remove(from_seg);


        List<Vector2> intersections = new List<Vector2>();
        float shortestDist = -1;
        int indexSegShortDist = -1;
        foreach (Segment s in neighboursAngleSeg)
        {
            ////Debug.Log(pointsVoronoi_simplyfied.Count + " --> " + newSegmentConIndex + ", " + notConnectedIndex + "; " + connectedIndex + ", " + s.GetIndexFrom() + ", " + s.GetIndexTo());
            if (Maths2D.LineLineSegmentIntersection(out Vector2 possibleIntersection, pointsVoronoi_simplyfied[newSegmentConIndex], (pointsVoronoi_simplyfied[notConnectedIndex] - pointsVoronoi_simplyfied[connectedIndex]), pointsVoronoi_simplyfied[s.GetIndexFrom()], pointsVoronoi_simplyfied[s.GetIndexTo()]) && possibleIntersection != pointsVoronoi_simplyfied[newSegmentConIndex])
            {
                //Check --> no other segments get cut
                //Check --> no room switches
                //Debug.LogError(polygonID + " connection possible with one neighbhour segment");
                bool possible = true;
                foreach (Segment checkS in segments_simplyfied)
                {
                    if (!neighboursAngleSeg.Contains(checkS) && checkS != shortSegment)
                    {
                        if (Maths2D.LineSegmentsIntersect(pointsVoronoi_simplyfied[newSegmentConIndex], possibleIntersection, pointsVoronoi_simplyfied[checkS.GetIndexFrom()], pointsVoronoi_simplyfied[checkS.GetIndexTo()]))
                        {
                            //Debug.LogError(polygonID + " possible connection would cut another segment");
                            possible = false;
                        }
                    }
                }

                foreach (Vector3 roomPos in rand_roomPositions)
                {
                    Vector2 roomPos2 = new Vector2(roomPos.x, roomPos.z);
                    if (Maths2D.PointInTriangle(pointsVoronoi_simplyfied[newSegmentConIndex], pointsVoronoi_simplyfied[connectedIndex], possibleIntersection, roomPos2))
                    {
                        //Debug.LogError(polygonID + " roompoint would switch the room");
                        possible = false;
                    }
                    if (Maths2D.PointInTriangle(pointsVoronoi_simplyfied[connectedIndex], pointsVoronoi_simplyfied[notConnectedIndex], possibleIntersection, roomPos2))
                    {
                        //Debug.LogError(polygonID + " roompoint would switch the room");
                        possible = false;
                    }
                }

                if (ListContainsSegment(outsideSegments_simplyfied, from_seg))
                {
                    //Debug.LogError(polygonID + " okay es ist schon manchmal hieran gescheitert");
                    possible = false;
                }

                if (possible)
                {
                    float dist = Vector2.Distance(pointsVoronoi_simplyfied[newSegmentConIndex], possibleIntersection);
                    intersections.Add(possibleIntersection);
                    if (shortestDist == -1 || dist < shortestDist)
                    {
                        shortestDist = dist;
                        indexSegShortDist = intersections.Count - 1;
                    }
                }
                else
                {
                    intersections.Add(Vector2.zero);
                }
            }
            else
            {
                intersections.Add(Vector2.zero);
            }
        }

        //POSSIBLE SHIFT
        if (indexSegShortDist != -1)
        {
            Vector2 intersection = intersections[indexSegShortDist];

            int index_intersec;
            //Vector2 pointIntersection;
            Segment cutSeg = neighboursAngleSeg[indexSegShortDist];
            if (intersection == pointsVoronoi_simplyfied[cutSeg.GetIndexFrom()])
            {
                index_intersec = cutSeg.GetIndexFrom();
            }
            else if (intersection == pointsVoronoi_simplyfied[cutSeg.GetIndexTo()])
            {
                index_intersec = cutSeg.GetIndexTo();
            }
            else
            {
                if (Maths2D.ListContainsVector(pointsVoronoi_simplyfied, intersection, out int index))
                {
                    index_intersec = index;
                }
                else 
                {
                    pointsVoronoi_simplyfied.Add(intersection);
                    conect.Add(new List<Segment>());
                    index_intersec = pointsVoronoi_simplyfied.Count - 1;
                }
            }

            //Check if the shifted segment is overlaying an already eixisting segment
            //3 possibitlities: 1. the shifted segment is longer, 2. both segments have the same lenght, 3. the overlaying segment is longer
            //1. the overlaying segment splits the shifted segment and only the second part is added
            //2. nothing is added (i think this is also not possible --> would result in a dissapearing of a room)
            //3. the shifted segment splits the overlaying segment --> not possible
            int connectFromIndex = newSegmentConIndex;
            foreach (Segment s in conect[newSegmentConIndex])
            {
                int index = s.GetIndexFrom();
                if (index == newSegmentConIndex)
                {
                    index = s.GetIndexTo();
                }
                if (Maths2D.PointOnLineSegment(pointsVoronoi_simplyfied[newSegmentConIndex], pointsVoronoi_simplyfied[index_intersec], pointsVoronoi_simplyfied[index]))
                {
                    connectFromIndex = index;
                }
            }

            from_seg.SetIndexFrom(connectFromIndex);
            from_seg.SetIndexTo(index_intersec);
            if (!conect[index_intersec].Contains(from_seg))
            {
                conect[index_intersec].Add(from_seg);
            }
            conect[notConnectedIndex].Remove(from_seg);
            conect[connectedIndex].Remove(from_seg);
            if (!conect[connectFromIndex].Contains(from_seg))
            {
                conect[connectFromIndex].Add(from_seg);
            }

            if (index_intersec != cutSeg.GetIndexFrom() || index_intersec != cutSeg.GetIndexTo())
            {
                segments_simplyfied.Remove(cutSeg);
                
                toShortSegments.Remove(cutSeg);
                Segment newSeg1 = new Segment(cutSeg.GetIndexFrom(), index_intersec);
                Segment newSeg2 = new Segment(index_intersec, cutSeg.GetIndexTo());

                bool wasOutsideSeg = false;
                //Debug.LogError(polygonID + " cutSegment --> " + cutSeg.GetIndexFrom() + " to " + cutSeg.GetIndexTo() + " with index: " + segments_simplyfied.IndexOf(cutSeg) + " (short seg: " + shortSegment.GetIndexFrom() + " to " + shortSegment.GetIndexTo() + " / index: " + segments_simplyfied.IndexOf(shortSegment) + ")" + " (from seg: " + from_seg.GetIndexFrom() + " to " + from_seg.GetIndexTo() + " / index: " + segments_simplyfied.IndexOf(from_seg) + ")");
                if (outsideSegments_simplyfied.Remove(cutSeg)) 
                {
                    //Debug.LogError(polygonID + " war outside Segment ... wurde geteilt");
                    wasOutsideSeg = true;
                }

                conect[cutSeg.GetIndexFrom()].Remove(cutSeg);
                conect[cutSeg.GetIndexTo()].Remove(cutSeg);

                //if (show_polygonID == 6 && show_iteration == 65) 
                //{
                //  //Debug.LogError("6" + " - 000B - E-1-B --> " + "from: " + conect[cutSeg.GetIndexFrom()].Count + ", to: " + conect[cutSeg.GetIndexTo()].Count);
                //  //Debug.LogError("6" + " - 000B - E-1-B --> Connectivität ersetztes Element: " + "connectedIndex: " + conect[connectedIndex].Count + ", notCon: " + conect[notConnectedIndex].Count);
                //}

                if (conect[cutSeg.GetIndexFrom()].Count > 0)
                {
                    if (!ListContainsSegment(conect[cutSeg.GetIndexFrom()], newSeg1))
                    {
                        conect[cutSeg.GetIndexFrom()].Add(newSeg1);
                    }
                    if (!ListContainsSegment(conect[index_intersec], newSeg1))
                    {
                        conect[index_intersec].Add(newSeg1);
                    }
                    if (!ListContainsSegment(segments_simplyfied, newSeg1))
                    {
                        segments_simplyfied.Add(newSeg1);
                        if (wasOutsideSeg) 
                        {
                            outsideSegments_simplyfied.Add(newSeg1);
                        }
                    }
                }

                if (conect[cutSeg.GetIndexTo()].Count > 0)
                {
                    if (!ListContainsSegment(conect[cutSeg.GetIndexTo()], newSeg2))
                    {
                        conect[cutSeg.GetIndexTo()].Add(newSeg2);
                    }
                    if (!ListContainsSegment(conect[index_intersec], newSeg2))
                    {
                        conect[index_intersec].Add(newSeg2);
                    }
                    if (!ListContainsSegment(segments_simplyfied, newSeg2))
                    {
                        segments_simplyfied.Add(newSeg2);
                        if (wasOutsideSeg)
                        {
                            outsideSegments_simplyfied.Add(newSeg2);
                        }
                    }
                }
            }

            if (conect[connectedIndex].Count == 1)
            {
                conect[connectedIndex].Remove(shortSegment);
                conect[newSegmentConIndex].Remove(shortSegment);
                segments_simplyfied.Remove(shortSegment);
                outsideSegments_simplyfied.Remove(shortSegment);
            }

            //now a single segment on the short segment
            if (conect[connectedIndex].Count == 2)
            {
                //if (show_polygonID == 6 && show_iteration == 65)
                //{
                //    //Debug.LogError("6" + " - 000B - E-1-B --> CONNECTED.COUNT == 2");
                //}
                List<Segment> listS = conect[connectedIndex];
                Segment other_segment = listS[0];
                if (other_segment == shortSegment) 
                {
                    other_segment = listS[1];
                }
                Vector2 vec1 = (pointsVoronoi_simplyfied[shortSegment.GetIndexTo()] - pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()]).normalized;
                Vector2 vec2 = (pointsVoronoi_simplyfied[other_segment.GetIndexTo()] - pointsVoronoi_simplyfied[other_segment.GetIndexFrom()]).normalized;
                //if (show_polygonID == 6 && show_iteration == 65)
                //{
                //    //Debug.LogError("6" + " - 000B - E-1-B --> CONNECTED --> angle: " + Vector2.Angle(vec1.normalized, vec2.normalized));
                //}
                if (Maths2D.IsAngleBetweenTwoVectors180or360Degree(vec1, vec2, 0.1f))
                {
                    int newEndIndex = other_segment.GetIndexFrom();
                    if (newEndIndex == connectedIndex) 
                    {
                        newEndIndex = other_segment.GetIndexTo();
                    }
                    if (shortSegment.GetIndexFrom() == connectedIndex)
                    {
                        shortSegment.SetIndexFrom(newEndIndex);
                    }
                    else 
                    {
                        shortSegment.SetIndexTo(newEndIndex);
                    }

                    segments_simplyfied.Remove(other_segment);
                    toShortSegments.Remove(other_segment);
                    outsideSegments_simplyfied.Remove(other_segment);
                    conect[connectedIndex].Remove(shortSegment);
                    conect[connectedIndex].Remove(other_segment);
                    conect[newEndIndex].Remove(other_segment);
                    if (!conect[newEndIndex].Contains(shortSegment))
                    {
                        conect[newEndIndex].Add(shortSegment);
                    }
                }
            }


            //possible merge on the other side of the shift
            if (conect[notConnectedIndex].Count == 2)
            {
                //if (show_polygonID == 6 && show_iteration == 65)
                //{
                //    //Debug.LogError("6" + " - 000B - E-1-B --> NOTCONNECTED.COUNT == 2");
                //}
                List<Segment> listS = conect[notConnectedIndex];
                Segment seg1 = listS[0];
                Segment seg2 = listS[1];

                Vector2 vec1 = (pointsVoronoi_simplyfied[seg1.GetIndexTo()] - pointsVoronoi_simplyfied[seg1.GetIndexFrom()]).normalized;
                Vector2 vec2 = (pointsVoronoi_simplyfied[seg2.GetIndexTo()] - pointsVoronoi_simplyfied[seg2.GetIndexFrom()]).normalized;
                //if (show_polygonID == 6 && show_iteration == 65)
                //{
                //    //Debug.LogError("6" + " - 000B - E-1-B --> NOTCONNECTED --> angle: " + Vector2.Angle(vec1.normalized, vec2.normalized));
                //}
                if (Maths2D.IsAngleBetweenTwoVectors180or360Degree(vec1, vec2, 0.1f))
                {
                    int newEndIndex = seg2.GetIndexFrom();
                    if (newEndIndex == notConnectedIndex)
                    {
                        newEndIndex = seg2.GetIndexTo();
                    }
                    if (seg1.GetIndexFrom() == notConnectedIndex)
                    {
                        seg1.SetIndexFrom(newEndIndex);
                    }
                    else 
                    {
                        seg1.SetIndexTo(newEndIndex);
                    }

                    segments_simplyfied.Remove(seg2);
                    toShortSegments.Remove(seg1);
                    toShortSegments.Remove(seg2);
                    outsideSegments_simplyfied.Remove(seg2);
                    conect[notConnectedIndex].Remove(seg1);
                    conect[notConnectedIndex].Remove(seg2);
                    conect[newEndIndex].Remove(seg2);
                    if (!conect[newEndIndex].Contains(seg1))
                    {
                        conect[newEndIndex].Add(seg1);
                    }
                }
            
            }

            //if (show_polygonID == 6 && show_iteration == 65)
            //{
            //    //Debug.LogError("6" + " - 000B - E-1-B --> Connectivität ersetztes Element: " + "connectedIndex: " + conect[connectedIndex].Count + ", notCon: " + conect[notConnectedIndex].Count);
            //}

            return true;
        }

        return false;
    }


    bool ShiftOneSideWihtMultipleSegmentsOfAToShortSegment(Segment shift_seg, Segment shortSegment, List<List<Segment>> conect, int notConnectedIndex, int connectedIndex, int newSegmentConIndex, List<Segment> toShortSegments, int polygonID, int countIterations)
    {
        //Debug.LogError("[MULTIPLE] 0 --->>><<<--- shortseg: " +pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()] + " to " + pointsVoronoi_simplyfied[shortSegment.GetIndexTo()] + ", index: " + segments_simplyfied.IndexOf(shortSegment));
        //Debug.LogError("[MULTIPLE] 02 --->>><<<--- shiftseg: " + pointsVoronoi_simplyfied[shift_seg.GetIndexFrom()] + " to " + pointsVoronoi_simplyfied[shift_seg.GetIndexTo()] + ", index: " + segments_simplyfied.IndexOf(shift_seg));
        Segment currentSeg = shift_seg;
        int currentIndex = notConnectedIndex;

        bool includeNextSegmentPossible = true;
        List<int> indicesOfPossibleSegmentConnections = new List<int>();
        List<Segment> segmentsOfPossibleSegmentConnections = new List<Segment>();
        List<List<Segment>> neighboursOfPossibleSegmentConnections = new List<List<Segment>>();

        while (includeNextSegmentPossible) 
        {
            includeNextSegmentPossible = false;
            //if the current Segment (Segment that can maybe moved) is not a outside Segment --> otherwise it wouldnt be moveable
            if (!ListContainsSegment(outsideSegments_simplyfied, currentSeg))
            {
                //Get a List of all connected Segemnts (in the direction, away from the short Segment) --> to check if a shift would also be possible including this segments
                List<Segment> neighboursAngleSeg = new List<Segment>();
                foreach (Segment s in conect[currentIndex])
                {
                    neighboursAngleSeg.Add(s);
                }
                neighboursAngleSeg.Remove(currentSeg);

                //Saveing all related Segment data (the connection Index --> direction: away from short segment, the segment and neighboured Segments (excluding the segment))
                indicesOfPossibleSegmentConnections.Add(currentIndex);
                segmentsOfPossibleSegmentConnections.Add(currentSeg);
                neighboursOfPossibleSegmentConnections.Add(neighboursAngleSeg);

                //Check if the angle to one of the next Segments would be 180 degree and so a possible candidate for a multiple segment shift
                Segment nextSeg = currentSeg;
                for (int i = 0; i < neighboursAngleSeg.Count; i++)
                {
                    //next Segment + index of the connection Points (last seg = connection point with the current seg, nextSeg = next connection point)
                    Segment s = neighboursAngleSeg[i];
                    int indexPointLastSeg = currentSeg.GetIndexFrom();
                    if (indexPointLastSeg == currentIndex)
                    {
                        indexPointLastSeg = currentSeg.GetIndexTo();
                    }
                    int indexPointNextSeg = s.GetIndexFrom();
                    if (indexPointNextSeg == currentIndex)
                    {
                        indexPointNextSeg = s.GetIndexTo();
                    }
                    ////Debug.LogError("[MULTIPLE] Test_conditions --->>><<<--- currentSeg.GetIndexFrom(): " + currentSeg.GetIndexFrom() + ", currentSeg.GetIndexTo(): " + currentSeg.GetIndexTo() + ", s.GetIndexFrom(): " + s.GetIndexFrom() + ", s.GetIndexTo(): " + s.GetIndexTo());

                    if (Maths2D.IsAngleBetweenTwoVectors180or360Degree(pointsVoronoi_simplyfied[indexPointLastSeg], pointsVoronoi_simplyfied[currentIndex], pointsVoronoi_simplyfied[indexPointNextSeg], 0.1f))
                    {
                        ////Debug.LogError("[MULTIPLE] S_conditions --->>><<<--- indexPointLastSeg: " + indexPointLastSeg + ", currentIndex: " + currentIndex + ", indexPointNextSeg: " + indexPointNextSeg);
                        ////Debug.LogError("[MULTIPLE] S --->>><<<--- s with 180 angle: " + pointsVoronoi_simplyfied[s.GetIndexFrom()] + " to " + pointsVoronoi_simplyfied[s.GetIndexTo()] + ", index: " + segments_simplyfied.IndexOf(s));
                        i = neighboursAngleSeg.Count;
                        neighboursAngleSeg.Remove(s);         
                        //when the connection point have more then 3 connections (current Seg, next Seg and at most 1 other segment), it would be possible that the shift would later required 
                        //different new points for all connected segments --> and this would not simplyfiy the result
                        if (neighboursAngleSeg.Count <= 1)
                        {
                            includeNextSegmentPossible = true;
                            nextSeg = s;
                        }
                        //only possible when both other segments share the same angle (1 side connecting from above and 1 connection from below)
                        //with more then 3 oter segments, it wouldnt be possible for the segments to all share a same next connecting point --> so shift would not simplify anything
                        if (neighboursAngleSeg.Count == 2)
                        {
                            Segment seg1 = neighboursAngleSeg[0];
                            int index_seg1 = seg1.GetIndexFrom();
                            if (index_seg1 == currentIndex)
                            {
                                index_seg1 = seg1.GetIndexTo();
                            }
                            Segment seg2 = neighboursAngleSeg[1];
                            int index_seg2 = seg2.GetIndexFrom();
                            if (index_seg2 == currentIndex)
                            {
                                index_seg2 = seg2.GetIndexTo();
                            }
                            float angle1 = Vector2.Angle(pointsVoronoi_simplyfied[indexPointLastSeg] - pointsVoronoi_simplyfied[currentIndex], pointsVoronoi_simplyfied[index_seg1] - pointsVoronoi_simplyfied[currentIndex]);
                            float angle2 = Vector2.Angle(pointsVoronoi_simplyfied[indexPointLastSeg] - pointsVoronoi_simplyfied[currentIndex], pointsVoronoi_simplyfied[index_seg2] - pointsVoronoi_simplyfied[currentIndex]);

                            //Debug.LogError(polygonID + " in iteration " + countIterations + " HELP wtf is happening: " + angle1 + " and " + angle2);
                            //Debug.LogError(polygonID + " in iteration " + countIterations + " HELP wtf is happening: {");
                            //Debug.LogError(polygonID + " in iteration " + countIterations + " HELP wtf is happening: indexPointLastSeg: " + indexPointLastSeg + "; currentIndex: " + currentIndex + "; index_seg1: " + index_seg1 + "; index_seg2: " + index_seg2);

                            //Debug.LogError(polygonID + " in iteration " + countIterations + " HELP wtf is happening: seg1 index: " + seg1.GetIndexFrom() + " to " + seg1.GetIndexTo() + "; seg2 index: " + seg2.GetIndexFrom() + " to " + seg2.GetIndexTo() + "; s index: " + s.GetIndexFrom() + " to " + s.GetIndexTo());
                            //Debug.LogError(polygonID + " in iteration " + countIterations + " HELP wtf is happening: seg1 length: " + Vector2.Distance(pointsVoronoi_simplyfied[seg1.GetIndexFrom()], pointsVoronoi_simplyfied[seg1.GetIndexTo()]) + "; seg2 length: " + Vector2.Distance(pointsVoronoi_simplyfied[seg2.GetIndexFrom()], pointsVoronoi_simplyfied[seg2.GetIndexTo()]) + "; s length: " + Vector2.Distance(pointsVoronoi_simplyfied[s.GetIndexFrom()], pointsVoronoi_simplyfied[s.GetIndexTo()]));
                            //Debug.LogError(polygonID + " in iteration " + countIterations + " HELP wtf is happening: }");

                            if (Maths2D.AngleEqualsAngle(angle1, angle2, 0.01f))
                            {
                                includeNextSegmentPossible = true;
                                nextSeg = s;
                            }
                        }
                    }
                }

                //if the next segment would fit, set the current segment to next segment and continue
                if (includeNextSegmentPossible)
                {
                    currentSeg = nextSeg;
                    if (nextSeg.GetIndexFrom() == currentIndex)
                    {
                        currentIndex = nextSeg.GetIndexTo();
                    }
                    else
                    {
                        currentIndex = nextSeg.GetIndexFrom();
                    }
                }
            }
        }

        //now all possible segments are saved in a list (stopped as soon as the first next segment wouldnt be a possible shiftable candidate)
        //--> now check that every segment shift dont result in forbidden shifts (cutting segments, letting rooms switch side, etc.)

        List<Vector2> possibleIntersectionOfPossibleSegmentConnections = new List<Vector2>();
        List<Segment> possibleCutSegmentOfPossibleSegmentConnections = new List<Segment>();
        List<int> possibleEndIndicesOfPossibleSegmentConnections = new List<int>();
        List<int> possibleConnectionPointsIndiecsOfPossibleSegmentConnections = new List<int>();

        int maxIndex = -2;
        for (int i = 0; i < segmentsOfPossibleSegmentConnections.Count; i++) 
        {
            Vector2 intersection = Vector2.negativeInfinity;
            List<Segment> neighboursAngleSeg = neighboursOfPossibleSegmentConnections[i];
            int currentConnectionIndex = indicesOfPossibleSegmentConnections[i];
            Segment possibleCutSeg = new Segment(-1, -1);

            currentSeg = segmentsOfPossibleSegmentConnections[i];
            int index_current_from = currentSeg.GetIndexFrom();
            int index_current_to = currentSeg.GetIndexTo();
            if (index_current_from == currentConnectionIndex) 
            {
                index_current_from = currentSeg.GetIndexTo();
                index_current_to = currentSeg.GetIndexFrom();
            }

            //check if one of the neighboured segments would be a possiblitly to shift (it exists a intersecting point)
            float shortestDist = -1;
            bool newPointRequired = false;
            bool intersectionFound = false;
            foreach (Segment s in neighboursAngleSeg)
            {
                Vector2 possibleIntersection = Vector2.negativeInfinity;
                if (Maths2D.LineLineSegmentIntersection(out possibleIntersection, pointsVoronoi_simplyfied[newSegmentConIndex], (pointsVoronoi_simplyfied[index_current_to] - pointsVoronoi_simplyfied[index_current_from]), pointsVoronoi_simplyfied[s.GetIndexFrom()], pointsVoronoi_simplyfied[s.GetIndexTo()])) // && possibleIntersection != pointsVoronoi_simplyfied[newSegmentConIndex] --> possibleIntersection != pointsVoronoi_simplyfied[index_current_to]
                {
                    intersectionFound = true;
                    float calc_dist = Vector2.Distance(pointsVoronoi_simplyfied[newSegmentConIndex], possibleIntersection);
                    if (shortestDist == -1 || calc_dist < shortestDist)
                    {
                        shortestDist = calc_dist;
                        intersection = possibleIntersection;
                        possibleCutSeg = s;
                    }
                }
            }

            if (countIterations == numOfSimplifyIterations - 1)
            {
                ////Debug.LogError("[MULTIPLE] A --->>><<<--- neihgbours: " + neighboursAngleSeg.Count);
                /*
                if(neighboursAngleSeg.Count >= 1)
                    //Debug.LogError("[MULTIPLE] A2--->>><<<--- neihgbours[0]: " + pointsVoronoi_simplyfied[neighboursAngleSeg[0].GetIndexFrom()] + " to " + pointsVoronoi_simplyfied[neighboursAngleSeg[0].GetIndexTo()] +", index: " + segments_simplyfied.IndexOf(neighboursAngleSeg[0]));
                if (neighboursAngleSeg.Count >= 2)
                    //Debug.LogError("[MULTIPLE] A2--->>><<<--- neihgbours[1]: " + pointsVoronoi_simplyfied[neighboursAngleSeg[1].GetIndexFrom()] + " to " + pointsVoronoi_simplyfied[neighboursAngleSeg[1].GetIndexTo()] + ", index: " + segments_simplyfied.IndexOf(neighboursAngleSeg[1]));
                if (neighboursAngleSeg.Count >= 3)
                    //Debug.LogError("[MULTIPLE] A2--->>><<<--- neihgbours[2]: " + pointsVoronoi_simplyfied[neighboursAngleSeg[2].GetIndexFrom()] + " to " + pointsVoronoi_simplyfied[neighboursAngleSeg[2].GetIndexTo()] + ", index: " + segments_simplyfied.IndexOf(neighboursAngleSeg[2]));
                */
            }

            //if no intersection with an neighboured segments was found --> add a intersection point where the intersection is between the other neighboured segments when shifting the current seg
            if (!intersectionFound && neighboursAngleSeg.Count > 0) 
            {
                Vector2 newPointIntersection = Vector2.negativeInfinity;
                Segment s = neighboursAngleSeg[0];
                if (Maths2D.LineLineIntersection(out newPointIntersection, pointsVoronoi_simplyfied[newSegmentConIndex], (pointsVoronoi_simplyfied[index_current_to] - pointsVoronoi_simplyfied[index_current_from]), pointsVoronoi_simplyfied[currentConnectionIndex], (pointsVoronoi_simplyfied[s.GetIndexTo()] - pointsVoronoi_simplyfied[s.GetIndexFrom()])))
                {
                    newPointRequired = true;
                }
                intersection = newPointIntersection;
            }

            //if also that is not possible --> becouse the point have no neighboured segments --> the new intersection point is the current point plus the short segment
            //--> should normaly not happen cuz this would mean both segments are connected in a straight line ... so they should be a single segment
            if (!intersectionFound && !newPointRequired) 
            {
                intersection = pointsVoronoi_simplyfied[currentConnectionIndex] + (pointsVoronoi_simplyfied[newSegmentConIndex] - pointsVoronoi_simplyfied[connectedIndex]);
            }

            //Check --> no other segments get cut
            //Check --> no room switches
            //Check --> no outside segment

            //use default values in the first iteration, use the intersection otherwise
            Vector2 lastIntersec = pointsVoronoi_simplyfied[newSegmentConIndex];
            List<Segment> lastNeighbours = new List<Segment>();
            int lastCurrentIndex = connectedIndex;
            if (i > 0)
            {
                lastIntersec = possibleIntersectionOfPossibleSegmentConnections[i - 1];
                lastNeighbours = neighboursOfPossibleSegmentConnections[i - 1];
                lastCurrentIndex = indicesOfPossibleSegmentConnections[i - 1];
            }

            bool possible = true;
            foreach (Segment checkS in segments_simplyfied)
            {
                if (!neighboursAngleSeg.Contains(checkS) && !lastNeighbours.Contains(checkS) && checkS != shortSegment)
                {
                    if (Maths2D.LineSegmentsIntersect(lastIntersec, intersection, pointsVoronoi_simplyfied[checkS.GetIndexFrom()], pointsVoronoi_simplyfied[checkS.GetIndexTo()]))
                    {
                        ////Debug.LogError(polygonID + " [MULTIPLE]" +  " possible connection would cut another segment");
                        possible = false;
                    }
                }
            }

            foreach (Vector3 roomPos in rand_roomPositions)
            {
                Vector2 roomPos2 = new Vector2(roomPos.x, roomPos.z);
                //if (Maths2D.PointInTriangle(pointsVoronoi_simplyfied[newSegmentConIndex], pointsVoronoi_simplyfied[connectedIndex], intersection, roomPos2))
                if (Maths2D.PointInTriangle(lastIntersec, pointsVoronoi_simplyfied[index_current_from], intersection, roomPos2))
                {
                    ////Debug.LogError(polygonID + " [MULTIPLE]" + " roompoint would switch the room");
                    possible = false;
                }
                //if (Maths2D.PointInTriangle(pointsVoronoi_simplyfied[connectedIndex], pointsVoronoi_simplyfied[notConnectedIndex], intersection, roomPos2))
                if (Maths2D.PointInTriangle(pointsVoronoi_simplyfied[index_current_from], pointsVoronoi_simplyfied[index_current_to], intersection, roomPos2))
                {
                    ////Debug.LogError(polygonID + " [MULTIPLE]" + " roompoint would switch the room");
                    possible = false;
                }
            }

            if (ListContainsSegment(outsideSegments_simplyfied, currentSeg))
            {
                ////Debug.LogError(polygonID + " [MULTIPLE]" + " okay es ist schon manchmal hieran gescheitert");
                possible = false;
            }

            ////Debug.LogError("[MULTIPLE] possible --->>><<<--- possible: " + possible);

            //if one of the conditioins above is not fullfilled and there is not already a smaler maxIndex --> set the maxIndex to the currentSegmentsIndex (i)
            //TODO set it to i-1?????
            if (!possible && maxIndex == -2) 
            {
                maxIndex = i-1;
            }

            if (intersectionFound)
            {
                possibleEndIndicesOfPossibleSegmentConnections.Add(i);
            }

            possibleIntersectionOfPossibleSegmentConnections.Add(intersection);
            possibleCutSegmentOfPossibleSegmentConnections.Add(possibleCutSeg);
            possibleConnectionPointsIndiecsOfPossibleSegmentConnections.Add(index_current_to);
        }

        if (maxIndex == -2) 
        {
            maxIndex = segmentsOfPossibleSegmentConnections.Count - 1;
        }

        //check for intersections in the new Segments
        float dist = 0;
        for (int i = 0; i < possibleIntersectionOfPossibleSegmentConnections.Count; i++) 
        {
            float new_dist = Vector2.Distance(pointsVoronoi_simplyfied[newSegmentConIndex], possibleIntersectionOfPossibleSegmentConnections[i]);
            if (new_dist > dist)
            {
                dist = new_dist;
            }
            else 
            {
                if (i < maxIndex) 
                {
                    maxIndex = i;
                }
            }
        }
        

        int endindex = -1;
        for (int i = 0; i < possibleEndIndicesOfPossibleSegmentConnections.Count; i++) 
        {
            //TODO USE THIS CODE USE THIS CODE USE THIS CODE
            if (possibleEndIndicesOfPossibleSegmentConnections[i] <= maxIndex && possibleEndIndicesOfPossibleSegmentConnections[i] > endindex) 
            {
                endindex = possibleEndIndicesOfPossibleSegmentConnections[i];
            }

            //if (possibleEndIndicesOfPossibleSegmentConnections[i] <= maxIndex && possibleEndIndicesOfPossibleSegmentConnections[i] == 0)
            //{
            //    endindex = 0;
            //}
        }

        if (countIterations == numOfSimplifyIterations-1)
        {
            //Debug.LogError("[MULTIPLE] B --->>><<<--- endindex: " + endindex + ", maxindex: " + maxIndex + ", (i = " + possibleEndIndicesOfPossibleSegmentConnections.Count + ")");
            if (possibleEndIndicesOfPossibleSegmentConnections.Count >= 1) {
                //Debug.LogError("[MULTIPLE] B2--->>><<<--- endindex[0]: " + possibleEndIndicesOfPossibleSegmentConnections[0]);
            }
            if (possibleEndIndicesOfPossibleSegmentConnections.Count >= 2)
            {
                //Debug.LogError("[MULTIPLE] B2--->>><<<--- endindex[1]: " + possibleEndIndicesOfPossibleSegmentConnections[1]);
            }
        }

        if (endindex < 0) 
        {
            return false;
        }
        if (endindex == 0) 
        {
            Segment cutSeg = possibleCutSegmentOfPossibleSegmentConnections[endindex];
            Vector2 intersection = possibleIntersectionOfPossibleSegmentConnections[endindex];
            int index_intersec;

            if (intersection == pointsVoronoi_simplyfied[cutSeg.GetIndexFrom()])
            {
                index_intersec = cutSeg.GetIndexFrom();
            }
            else if (intersection == pointsVoronoi_simplyfied[cutSeg.GetIndexTo()])
            {
                index_intersec = cutSeg.GetIndexTo();
            }
            else
            {
                if (Maths2D.ListContainsVector(pointsVoronoi_simplyfied, intersection, out int index))
                {
                    index_intersec = index;
                }
                else
                {
                    pointsVoronoi_simplyfied.Add(intersection);
                    conect.Add(new List<Segment>());
                    index_intersec = pointsVoronoi_simplyfied.Count - 1;
                }
            }

            int connectFromIndex = newSegmentConIndex;
            foreach (Segment s in conect[newSegmentConIndex])
            {
                int index = s.GetIndexFrom();
                if (index == newSegmentConIndex)
                {
                    index = s.GetIndexTo();
                }
                if (Maths2D.PointOnLineSegment(pointsVoronoi_simplyfied[newSegmentConIndex], pointsVoronoi_simplyfied[index_intersec], pointsVoronoi_simplyfied[index]))
                {
                    connectFromIndex = index;
                }
            }

            Segment shiftSeg = segmentsOfPossibleSegmentConnections[0];
            shiftSeg.SetIndexFrom(connectFromIndex);
            shiftSeg.SetIndexTo(index_intersec);
            if (!conect[index_intersec].Contains(shiftSeg))
            {
                conect[index_intersec].Add(shiftSeg);
            }
            conect[indicesOfPossibleSegmentConnections[0]].Remove(shiftSeg);
            conect[connectedIndex].Remove(shiftSeg);
            if (!conect[connectFromIndex].Contains(shiftSeg))
            {
                conect[connectFromIndex].Add(shiftSeg);
            }


            //Test that no zero length segments are added
            if (shiftSeg.GetIndexFrom() == shiftSeg.GetIndexTo())
            {
                //Debug.LogError("[Multiple but single] --> short Segment added (same index)");
                segments_simplyfied.Remove(shiftSeg);
                toShortSegments.Remove(shiftSeg);
                outsideSegments.Remove(shiftSeg);
            }
            if (Vector2.Distance(pointsVoronoi_simplyfied[shiftSeg.GetIndexFrom()], pointsVoronoi_simplyfied[shiftSeg.GetIndexTo()]) == 0) 
            {
                //Debug.LogError("[Multiple but single] --> short Segment added (different index)");
            }



            if (index_intersec != cutSeg.GetIndexFrom() && index_intersec != cutSeg.GetIndexTo())
            {
                segments_simplyfied.Remove(cutSeg);

                toShortSegments.Remove(cutSeg);
                Segment newSeg1 = new Segment(cutSeg.GetIndexFrom(), index_intersec);
                Segment newSeg2 = new Segment(index_intersec, cutSeg.GetIndexTo());

                bool wasOutsideSeg = false;
                //Debug.LogError(polygonID + " [MULTIPLE]" + " cutSegment --> " + cutSeg.GetIndexFrom() + " to " + cutSeg.GetIndexTo() + " with index: " + segments_simplyfied.IndexOf(cutSeg) + " (short seg: " + shortSegment.GetIndexFrom() + " to " + shortSegment.GetIndexTo() + " / index: " + segments_simplyfied.IndexOf(shortSegment) + ")" + " (from seg: " + shift_seg.GetIndexFrom() + " to " + shift_seg.GetIndexTo() + " / index: " + segments_simplyfied.IndexOf(shift_seg) + ")");
                if (outsideSegments_simplyfied.Remove(cutSeg))
                {
                    //Debug.LogError(polygonID + " [MULTIPLE]" + " war outside Segment ... wurde geteilt");
                    wasOutsideSeg = true;
                }

                conect[cutSeg.GetIndexFrom()].Remove(cutSeg);
                conect[cutSeg.GetIndexTo()].Remove(cutSeg);

                if (conect[cutSeg.GetIndexFrom()].Count > 0)
                {
                    if (!ListContainsSegment(conect[cutSeg.GetIndexFrom()], newSeg1))
                    {
                        conect[cutSeg.GetIndexFrom()].Add(newSeg1);
                    }
                    if (!ListContainsSegment(conect[index_intersec], newSeg1))
                    {
                        conect[index_intersec].Add(newSeg1);
                    }
                    if (!ListContainsSegment(segments_simplyfied, newSeg1))
                    {
                        segments_simplyfied.Add(newSeg1);
                        if (wasOutsideSeg)
                        {
                            outsideSegments_simplyfied.Add(newSeg1);
                        }
                    }
                }

                if (conect[cutSeg.GetIndexTo()].Count > 0)
                {
                    if (!ListContainsSegment(conect[cutSeg.GetIndexTo()], newSeg2))
                    {
                        conect[cutSeg.GetIndexTo()].Add(newSeg2);
                    }
                    if (!ListContainsSegment(conect[index_intersec], newSeg2))
                    {
                        conect[index_intersec].Add(newSeg2);
                    }
                    if (!ListContainsSegment(segments_simplyfied, newSeg2))
                    {
                        segments_simplyfied.Add(newSeg2);
                        if (wasOutsideSeg)
                        {
                            outsideSegments_simplyfied.Add(newSeg2);
                        }
                    }
                }
            }

            if (conect[connectedIndex].Count == 1)
            {
                conect[connectedIndex].Remove(shortSegment);
                conect[newSegmentConIndex].Remove(shortSegment);
                segments_simplyfied.Remove(shortSegment);
                outsideSegments_simplyfied.Remove(shortSegment);
            }

            //now a single segment on the short segment
            if (conect[connectedIndex].Count == 2)
            {
                List<Segment> listS = conect[connectedIndex];
                Segment other_segment = listS[0];
                if (other_segment == shortSegment)
                {
                    other_segment = listS[1];
                }
                Vector2 vec1 = (pointsVoronoi_simplyfied[shortSegment.GetIndexTo()] - pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()]).normalized;
                Vector2 vec2 = (pointsVoronoi_simplyfied[other_segment.GetIndexTo()] - pointsVoronoi_simplyfied[other_segment.GetIndexFrom()]).normalized;

                if (Maths2D.IsAngleBetweenTwoVectors180or360Degree(vec1, vec2, 0.1f))
                {
                    int newEndIndex = other_segment.GetIndexFrom();
                    if (newEndIndex == connectedIndex)
                    {
                        newEndIndex = other_segment.GetIndexTo();
                    }
                    if (shortSegment.GetIndexFrom() == connectedIndex)
                    {
                        shortSegment.SetIndexFrom(newEndIndex);
                    }
                    else
                    {
                        shortSegment.SetIndexTo(newEndIndex);
                    }

                    segments_simplyfied.Remove(other_segment);
                    toShortSegments.Remove(other_segment);
                    outsideSegments_simplyfied.Remove(other_segment);
                    conect[connectedIndex].Remove(shortSegment);
                    conect[connectedIndex].Remove(other_segment);
                    conect[newEndIndex].Remove(other_segment);
                    if (!conect[newEndIndex].Contains(shortSegment))
                    {
                        conect[newEndIndex].Add(shortSegment);
                    }
                }
            }

            //possible merge on the other side of the shift
            if (conect[notConnectedIndex].Count == 2)
            {
                List<Segment> listS = conect[notConnectedIndex];
                Segment seg1 = listS[0];
                Segment seg2 = listS[1];

                Vector2 vec1 = (pointsVoronoi_simplyfied[seg1.GetIndexTo()] - pointsVoronoi_simplyfied[seg1.GetIndexFrom()]).normalized;
                Vector2 vec2 = (pointsVoronoi_simplyfied[seg2.GetIndexTo()] - pointsVoronoi_simplyfied[seg2.GetIndexFrom()]).normalized;

                if (Maths2D.IsAngleBetweenTwoVectors180or360Degree(vec1, vec2, 0.1f))
                {
                    int newEndIndex = seg2.GetIndexFrom();
                    if (newEndIndex == notConnectedIndex)
                    {
                        newEndIndex = seg2.GetIndexTo();
                    }
                    if (seg1.GetIndexFrom() == notConnectedIndex)
                    {
                        seg1.SetIndexFrom(newEndIndex);
                    }
                    else
                    {
                        seg1.SetIndexTo(newEndIndex);
                    }

                    segments_simplyfied.Remove(seg2);
                    toShortSegments.Remove(seg1);
                    toShortSegments.Remove(seg2);
                    outsideSegments_simplyfied.Remove(seg2);
                    conect[notConnectedIndex].Remove(seg1);
                    conect[notConnectedIndex].Remove(seg2);
                    conect[newEndIndex].Remove(seg2);
                    if (!conect[newEndIndex].Contains(seg1))
                    {
                        conect[newEndIndex].Add(seg1);
                    }
                }

            }
        }
        
        

        if (endindex > 0) 
        {

            //1. connect the first segment (check if its overlapping with another segment)
            int index = indicesOfPossibleSegmentConnections[0];
            int connectFromIndex = newSegmentConIndex;
            foreach (Segment s in conect[newSegmentConIndex])
            {
                int con_index = s.GetIndexFrom();
                if (con_index == newSegmentConIndex)
                {
                    con_index = s.GetIndexTo();
                }
                if (Maths2D.PointOnLineSegment(pointsVoronoi_simplyfied[newSegmentConIndex], pointsVoronoi_simplyfied[index], pointsVoronoi_simplyfied[con_index]))
                {
                    connectFromIndex = con_index;
                }
            }

            int newToIndex = possibleConnectionPointsIndiecsOfPossibleSegmentConnections[0];

            Segment shiftSeg = segmentsOfPossibleSegmentConnections[0];
            shiftSeg.SetIndexFrom(connectFromIndex);
            shiftSeg.SetIndexTo(newToIndex);
            if (!conect[newToIndex].Contains(shiftSeg))
            {
                conect[newToIndex].Add(shiftSeg);
            }
            conect[connectedIndex].Remove(shiftSeg);
            if (!conect[connectFromIndex].Contains(shiftSeg))
            {
                conect[connectFromIndex].Add(shiftSeg);
            }

            if (conect[connectedIndex].Count == 1)
            {
                conect[connectedIndex].Remove(shortSegment);
                conect[newSegmentConIndex].Remove(shortSegment);
                segments_simplyfied.Remove(shortSegment);
                outsideSegments_simplyfied.Remove(shortSegment);
            }

            //now a single segment on the short segment
            if (conect[connectedIndex].Count == 2)
            {
                List<Segment> listS = conect[connectedIndex];
                Segment other_segment = listS[0];
                if (other_segment == shortSegment)
                {
                    other_segment = listS[1];
                }
                Vector2 vec1 = (pointsVoronoi_simplyfied[shortSegment.GetIndexTo()] - pointsVoronoi_simplyfied[shortSegment.GetIndexFrom()]).normalized;
                Vector2 vec2 = (pointsVoronoi_simplyfied[other_segment.GetIndexTo()] - pointsVoronoi_simplyfied[other_segment.GetIndexFrom()]).normalized;

                if (Maths2D.IsAngleBetweenTwoVectors180or360Degree(vec1, vec2, 0.1f))
                {
                    int newEndIndex = other_segment.GetIndexFrom();
                    if (newEndIndex == connectedIndex)
                    {
                        newEndIndex = other_segment.GetIndexTo();
                    }
                    if (shortSegment.GetIndexFrom() == connectedIndex)
                    {
                        shortSegment.SetIndexFrom(newEndIndex);
                    }
                    else
                    {
                        shortSegment.SetIndexTo(newEndIndex);
                    }

                    segments_simplyfied.Remove(other_segment);
                    toShortSegments.Remove(other_segment);
                    outsideSegments_simplyfied.Remove(other_segment);
                    conect[connectedIndex].Remove(shortSegment);
                    conect[connectedIndex].Remove(other_segment);
                    conect[newEndIndex].Remove(other_segment);
                    if (!conect[newEndIndex].Contains(shortSegment))
                    {
                        conect[newEndIndex].Add(shortSegment);
                    }
                }
            }

            //Test that no zero length segments are added
            if (shiftSeg.GetIndexFrom() == shiftSeg.GetIndexTo())
            {
                //Debug.LogError("[Multiple] --> short Segment added (same index)");
                segments_simplyfied.Remove(shiftSeg);
                toShortSegments.Remove(shiftSeg);
                outsideSegments.Remove(shiftSeg);
            }
            if (Vector2.Distance(pointsVoronoi_simplyfied[shiftSeg.GetIndexFrom()], pointsVoronoi_simplyfied[shiftSeg.GetIndexTo()]) == 0)
            {
                //Debug.LogError("[Multiple] --> short Segment added (different index)");
            }


            //2. connect the last segment (all neccasarry checks)
            int lastIndex = possibleConnectionPointsIndiecsOfPossibleSegmentConnections[endindex-1];
            Segment cutSeg = possibleCutSegmentOfPossibleSegmentConnections[endindex];
            Vector2 intersection = possibleIntersectionOfPossibleSegmentConnections[endindex];
            int index_intersec;
            int indexOldIntersection = possibleConnectionPointsIndiecsOfPossibleSegmentConnections[endindex];
            ////Debug.LogError(polygonID + " [MULTIPLE] >>>--->>>--->>>--->>> lastIndex: " + lastIndex + ": " + pointsVoronoi_simplyfied[lastIndex].x + "; " + pointsVoronoi_simplyfied[lastIndex].y);
            ////Debug.LogError(polygonID + " [MULTIPLE] >>>--->>>--->>>--->>> cutSeg: " + segments_simplyfied.IndexOf(cutSeg) + " from: " + pointsVoronoi_simplyfied[cutSeg.GetIndexFrom()].x + "; " + pointsVoronoi_simplyfied[cutSeg.GetIndexFrom()].y + " to: " + pointsVoronoi_simplyfied[cutSeg.GetIndexTo()].x + "; " + pointsVoronoi_simplyfied[cutSeg.GetIndexTo()].y);
            ////Debug.LogError(polygonID + " [MULTIPLE] >>>--->>>--->>>--->>> intersection: " + intersection.x + "; " + intersection.y);

            //check if the new intersection point already exists --> if not create a new point and add it to the points list
            if (Maths2D.PointEqualsPoint(intersection, pointsVoronoi_simplyfied[cutSeg.GetIndexFrom()]))
            {
                index_intersec = cutSeg.GetIndexFrom();
            }
            else if (Maths2D.PointEqualsPoint(intersection, pointsVoronoi_simplyfied[cutSeg.GetIndexTo()]))
            {
                index_intersec = cutSeg.GetIndexTo();
            }
            else
            {
                if (Maths2D.ListContainsVector(pointsVoronoi_simplyfied, intersection, out int intersec_index))
                {
                    index_intersec = intersec_index;
                }
                else
                {
                    pointsVoronoi_simplyfied.Add(intersection);
                    conect.Add(new List<Segment>());
                    index_intersec = pointsVoronoi_simplyfied.Count - 1;
                }
            }

            ////Debug.LogError(polygonID + " [MULTIPLE] >>>--->>>--->>>--->>> intersectionIndex: " + index_intersec + ": " + pointsVoronoi_simplyfied[index_intersec].x + "; " + pointsVoronoi_simplyfied[index_intersec].y);

            Segment shiftSeg2 = segmentsOfPossibleSegmentConnections[endindex];
            ////Debug.LogError(polygonID + " [MULTIPLE] >>>--->>>--->>>--->>> shiftSeg2: " + segments_simplyfied.IndexOf(shiftSeg2) + " from: " + pointsVoronoi_simplyfied[shiftSeg2.GetIndexFrom()].x + "; " + pointsVoronoi_simplyfied[shiftSeg2.GetIndexFrom()].y + " to: " + pointsVoronoi_simplyfied[shiftSeg2.GetIndexTo()].x + "; " + pointsVoronoi_simplyfied[shiftSeg2.GetIndexTo()].y);
            shiftSeg2.SetIndexFrom(lastIndex);
            shiftSeg2.SetIndexTo(index_intersec);
            if (!conect[index_intersec].Contains(shiftSeg2))
            {
                conect[index_intersec].Add(shiftSeg2);
            }
            conect[indexOldIntersection].Remove(shiftSeg2);

            if (index_intersec != cutSeg.GetIndexFrom() && index_intersec != cutSeg.GetIndexTo())
            {
                segments_simplyfied.Remove(cutSeg);

                toShortSegments.Remove(cutSeg);
                Segment newSeg1 = new Segment(cutSeg.GetIndexFrom(), index_intersec);
                Segment newSeg2 = new Segment(index_intersec, cutSeg.GetIndexTo());

                bool wasOutsideSeg = false;
                if (outsideSegments_simplyfied.Remove(cutSeg))
                {
                    wasOutsideSeg = true;
                }

                conect[cutSeg.GetIndexFrom()].Remove(cutSeg);
                conect[cutSeg.GetIndexTo()].Remove(cutSeg);

                if (conect[cutSeg.GetIndexFrom()].Count > 0)
                {
                    if (!ListContainsSegment(conect[cutSeg.GetIndexFrom()], newSeg1))
                    {
                        conect[cutSeg.GetIndexFrom()].Add(newSeg1);
                    }
                    if (!ListContainsSegment(conect[index_intersec], newSeg1))
                    {
                        conect[index_intersec].Add(newSeg1);
                    }
                    if (!ListContainsSegment(segments_simplyfied, newSeg1))
                    {
                        segments_simplyfied.Add(newSeg1);
                        if (wasOutsideSeg)
                        {
                            outsideSegments_simplyfied.Add(newSeg1);
                        }
                    }
                }

                if (conect[cutSeg.GetIndexTo()].Count > 0)
                {
                    if (!ListContainsSegment(conect[cutSeg.GetIndexTo()], newSeg2))
                    {
                        conect[cutSeg.GetIndexTo()].Add(newSeg2);
                    }
                    if (!ListContainsSegment(conect[index_intersec], newSeg2))
                    {
                        conect[index_intersec].Add(newSeg2);
                    }
                    if (!ListContainsSegment(segments_simplyfied, newSeg2))
                    {
                        segments_simplyfied.Add(newSeg2);
                        if (wasOutsideSeg)
                        {
                            outsideSegments_simplyfied.Add(newSeg2);
                        }
                    }
                }
            }

            //possible merge on the other side of the shift
            if (conect[indexOldIntersection].Count == 2)
            {
                List<Segment> listS = conect[lastIndex];
                Segment seg1 = listS[0];
                Segment seg2 = listS[1];

                Vector2 vec1 = (pointsVoronoi_simplyfied[seg1.GetIndexTo()] - pointsVoronoi_simplyfied[seg1.GetIndexFrom()]).normalized;
                Vector2 vec2 = (pointsVoronoi_simplyfied[seg2.GetIndexTo()] - pointsVoronoi_simplyfied[seg2.GetIndexFrom()]).normalized;

                if (Maths2D.IsAngleBetweenTwoVectors180or360Degree(vec1, vec2, 0.1f))
                {
                    int newEndIndex = seg2.GetIndexFrom();
                    if (newEndIndex == indexOldIntersection)
                    {
                        newEndIndex = seg2.GetIndexTo();
                    }
                    if (seg1.GetIndexFrom() == indexOldIntersection)
                    {
                        seg1.SetIndexFrom(newEndIndex);
                    }
                    else
                    {
                        seg1.SetIndexTo(newEndIndex);
                    }

                    segments_simplyfied.Remove(seg2);
                    toShortSegments.Remove(seg1);
                    toShortSegments.Remove(seg2);
                    outsideSegments_simplyfied.Remove(seg2);
                    conect[indexOldIntersection].Remove(seg1);
                    conect[indexOldIntersection].Remove(seg2);
                    conect[newEndIndex].Remove(seg2);
                    if (!conect[newEndIndex].Contains(seg1))
                    {
                        conect[newEndIndex].Add(seg1);
                    }
                }

            }

            //Test that no zero length segments are added
            if (shiftSeg2.GetIndexFrom() == shiftSeg2.GetIndexTo())
            {
                //Debug.LogError("[Multiple] --> short Segment added (same index) (shiftSeg2)");
                segments_simplyfied.Remove(shiftSeg2);
                toShortSegments.Remove(shiftSeg2);
                outsideSegments.Remove(shiftSeg2);
            }
            if (Vector2.Distance(pointsVoronoi_simplyfied[shiftSeg2.GetIndexFrom()], pointsVoronoi_simplyfied[shiftSeg2.GetIndexTo()]) == 0)
            {
                //Debug.LogError("[Multiple] --> short Segment added (different index) (shiftSeg2)");
            }


            //3. shift all points in between to new location
            for (int i = 0; i < endindex; i++) 
            {
                int pointToShiftIndex = possibleConnectionPointsIndiecsOfPossibleSegmentConnections[i];
                
                //Vector2 pointToShift = pointsVoronoi_simplyfied[pointToShiftIndex];
                ////Debug.LogError(polygonID + " [MULTIPLE] >>>--->>>--->>> x: " + pointToShift.x + "; y: " + pointToShift.y + "  >>>  x: " + possibleIntersectionOfPossibleSegmentConnections[i].x + "; y: " + possibleIntersectionOfPossibleSegmentConnections[i].y);
                
                //new point location:Help
                Vector2 newLoc = possibleIntersectionOfPossibleSegmentConnections[i];
                pointsVoronoi_simplyfied[pointToShiftIndex] = newLoc; 
            }
      
            //if (seg_to_shift.GetIndexFrom() == index)
            //{
            //    conect[seg_to_shift.GetIndexTo()].Remove(seg_to_shift);
            //    seg_to_shift.SetIndexTo(newSegmentConIndex);
            //    conect[seg_to_shift.GetIndexTo()].Add(seg_to_shift);
            //}
            //else 
            //{
            //    seg_to_shift.SetIndexFrom(newSegmentConIndex);
            //}
            


            /*
            for (int i = 1; i < endindex-1; i++) 
            {
                int connect_index = indicesOfPossibleSegmentConnections[i];
                Vector2 connect_point = pointsVoronoi_simplyfied[connect_index];
                connect_point.x = possibleIntersectionOfPossibleSegmentConnections[i].x;
                connect_point.y = possibleIntersectionOfPossibleSegmentConnections[i].y;
            }
            */


        }
    
        

        return true;
    }


    void MapStraightenSegmentsToNewPoints(List<Vector2> pointsVoronoi_straightened, List<Segment> segments_straightened, List<Segment> outsideSegments_straightened, out List<Vector2> pointsVoronoi_straightenedReduced, out List<Segment> segments_straightenedReduced, out List<Segment> outsideSegments_straightenedReduced) 
    {
        pointsVoronoi_straightenedReduced = new List<Vector2>();
        segments_straightenedReduced = new List<Segment>();
        outsideSegments_straightenedReduced = new List<Segment>();

        Dictionary<int, int> mapping = new Dictionary<int, int>();

        foreach (Segment element in outsideSegments_straightened) 
        {
            int key1 = element.GetIndexFrom();
            int key2 = element.GetIndexTo();
            if (!mapping.TryGetValue(key1, out int keyMapping1))
            {
                pointsVoronoi_straightenedReduced.Add(pointsVoronoi_straightened[key1]);
                keyMapping1 = pointsVoronoi_straightenedReduced.Count - 1;
                mapping.Add(key1, keyMapping1);
            }
            if (!mapping.TryGetValue(key2, out int keyMapping2))
            {
                pointsVoronoi_straightenedReduced.Add(pointsVoronoi_straightened[key2]);
                keyMapping2 = pointsVoronoi_straightenedReduced.Count - 1;
                mapping.Add(key2, keyMapping2);
            }

            outsideSegments_straightenedReduced.Add(new Segment(keyMapping1, keyMapping2));
        }

        foreach (Segment element in segments_straightened)
        {
            int key1 = element.GetIndexFrom();
            int key2 = element.GetIndexTo();
            if (!mapping.TryGetValue(key1, out int keyMapping1))
            {
                pointsVoronoi_straightenedReduced.Add(pointsVoronoi_straightened[key1]);
                keyMapping1 = pointsVoronoi_straightenedReduced.Count - 1;
                mapping.Add(key1, keyMapping1);
            }
            if (!mapping.TryGetValue(key2, out int keyMapping2))
            {
                pointsVoronoi_straightenedReduced.Add(pointsVoronoi_straightened[key2]);
                keyMapping2 = pointsVoronoi_straightenedReduced.Count - 1;
                mapping.Add(key2, keyMapping2);
            }

            segments_straightenedReduced.Add(new Segment(keyMapping1, keyMapping2));
        }

    }

    bool CheckForRoomIntersectionsWhenStraightingEdges(List<List<int>> roomIdsPerPolygon, int polygonID, Segment currentSegment, Vector2 c, out List<int> roomIDsInsideTriangle) 
    {
        roomIDsInsideTriangle = new List<int>();
        for (int i = 0; i < roomIdsPerPolygon[polygonID].Count; i++)
        {
            Vector2 a = pointsVoronoi_straightened[currentSegment.GetIndexFrom()];
            Vector2 b = pointsVoronoi_straightened[currentSegment.GetIndexTo()];
            Vector2 p = new Vector2(rand_roomPositions[roomIdsPerPolygon[polygonID][i]].x, rand_roomPositions[roomIdsPerPolygon[polygonID][i]].z);
            if (Maths2D.PointInTriangle(a, b, c, p) && !(a.x == p.x && a.y == p.y) && !(b.x == p.x && b.y == p.y) & !(c.x == p.x && c.y == p.y)) 
            {
                roomIDsInsideTriangle.Add(roomIdsPerPolygon[polygonID][i]);
            }

        }
        if (roomIDsInsideTriangle.Count > 0)
        {
            return true;
        }
        return false;
    }

    void RemoveZeroLengthSegmentsFromSegmentsStraightened(List<List<Segment>> conect) 
    {
        foreach (Segment s in segments_straightened.ToArray()) 
        {
            if (Vector2.Distance(pointsVoronoi_straightened[s.GetIndexFrom()], pointsVoronoi_straightened[s.GetIndexTo()]) < 0.00001f) 
            {
                //Debug.LogError("Zero Length Segment");
                segments_straightened.Remove(s);
                conect[s.GetIndexFrom()].Remove(s);
                conect[s.GetIndexTo()].Remove(s);
                List<Segment> newConnections = new List<Segment>();
                foreach (Segment seg in conect[s.GetIndexFrom()])
                {
                    newConnections.Add(s);
                }
                foreach (Segment seg in conect[s.GetIndexTo()])
                {
                    if (!newConnections.Contains(s))
                    {
                        newConnections.Add(s);
                    }
                }
                conect[s.GetIndexFrom()] = newConnections;
                conect[s.GetIndexTo()] = newConnections;
            }
        }
    }

    void GetRooms(out Vector2[] generated_pos, out float[] generated_sizes, out RoomType[] generated_types, int numberOfRooms, float diff_x, float diff_y, float x_min, float y_min, Polygon p, int[] trianglesP, System.Random r)
    {
        switch (randDistMethod) 
        {
            case RandomDistributionMethod.fullRandom:
                GetRoomsFullRandom(out generated_pos, out generated_sizes, out generated_types, roomNumber, diff_x, diff_y, x_min, y_min, p, trianglesP, r);
                break;
            case RandomDistributionMethod.fullRandomFitting:
                GetRoomsFullRandomFitting(out generated_pos, out generated_sizes, out generated_types, roomNumber, diff_x, diff_y, x_min, y_min, p, trianglesP, r);
                break;
            case RandomDistributionMethod.matrix:
                GetRoomsMatrixFitting(out generated_pos, out generated_sizes, out generated_types, roomNumber, diff_x, diff_y, x_min, y_min, p, trianglesP, r);
                break;
            default:
                GetRoomsFullRandom(out generated_pos, out generated_sizes, out generated_types, roomNumber, diff_x, diff_y, x_min, y_min, p, trianglesP, r);
                break;
        } 
    }

    void GetRoomsFullRandom(out Vector2[] generated_pos, out float[] generated_sizes, out RoomType[] generated_types, int numberOfRooms, float diff_x, float diff_y, float x_min, float y_min, Polygon p, int[] trianglesP, System.Random r) 
    {
        generated_pos = new Vector2[numberOfRooms];
        generated_sizes = new float[numberOfRooms];
        generated_types = new RoomType[numberOfRooms];


        for (int i = 0; i < numberOfRooms; i++)
        {
            float x = (((float)r.NextDouble()) * diff_x) + x_min;
            float y = (((float)r.NextDouble()) * diff_y) + y_min;

            Vector2 point = new Vector2(x, y);
            generated_pos[i] = point;

            generated_sizes[i] = 1;

            generated_types[i] = RoomType.Undefined;
        }
    }

    void GetRoomsFullRandomFitting(out Vector2[] generated_pos, out float[] generated_sizes, out RoomType[] generated_types, int numberOfRooms, float diff_x, float diff_y, float x_min, float y_min, Polygon p, int[] trianglesP, System.Random r)
    {
        generated_pos = new Vector2[numberOfRooms];
        generated_sizes = new float[numberOfRooms];
        generated_types = new RoomType[numberOfRooms];

        for (int i = 0; i < numberOfRooms; i++)
        {
            float x = (((float)r.NextDouble()) * diff_x) + x_min;
            float y = (((float)r.NextDouble()) * diff_y) + y_min;

            Vector2 point = new Vector2(x, y);

            while (!PointInsidePolygonExclusive(p, trianglesP, new Vector3(point.x, 0, point.y))) 
            {
                x = (((float)r.NextDouble()) * diff_x) + x_min;
                y = (((float)r.NextDouble()) * diff_y) + y_min;
                point = new Vector2(x, y);
            }
            generated_pos[i] = point;

            generated_sizes[i] = 1;

            generated_types[i] = RoomType.Undefined;
        }
    }

    void GetRoomsMatrixFitting(out Vector2[] generated_pos, out float[] generated_sizes, out RoomType[] generated_types, int numberOfRooms, float diff_x, float diff_y, float x_min, float y_min, Polygon p, int[] trianglesP, System.Random r)
    {
        //create matrix

        float x_min_value_rounded = Mathf.Ceil(x_min / useMatrixCellSize) * useMatrixCellSize;
        float y_min_value_rounded = Mathf.Ceil(y_min / useMatrixCellSize) * useMatrixCellSize;

        int array_size_x = Mathf.CeilToInt(diff_x / useMatrixCellSize) + 1;
        int array_size_y = Mathf.CeilToInt(diff_y / useMatrixCellSize) + 1;

        MatrixData[,] matrix = new MatrixData[array_size_x, array_size_y];

        //CREATE INITAL MATRIX
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                float x = x_min_value_rounded + i * useMatrixCellSize;
                float y = y_min_value_rounded + j * useMatrixCellSize;
                if (i != 0 && j != 0 && i != matrix.GetLength(0) - 1 && j != matrix.GetLength(1) - 1 && PointInsidePolygon(p, trianglesP, new Vector3(x, 0, y)))
                {
                    matrix[i, j] = new MatrixData(0, i, j);
                }
                else
                {
                    matrix[i, j] = new MatrixData(-1, i, j);
                }
            }
        }

        
        //Update Matrix --> remove entrys on or near walls
        for (int i = 1; i < matrix.GetLength(0) - 1; i++)
        {
            for (int j = 1; j < matrix.GetLength(1) - 1; j++)
            {
                if (matrix[i, j].getValue() == 0 && (matrix[i + 1, j].getValue() == -1 || matrix[i, j + 1].getValue() == -1 || matrix[i - 1, j].getValue() == -1 || matrix[i, j - 1].getValue() == -1 || matrix[i + 1, j + 1].getValue() == -1 || matrix[i + 1, j - 1].getValue() == -1 || matrix[i - 1, j + 1].getValue() == -1 || matrix[i - 1, j - 1].getValue() == -1)) 
                {
                    matrix[i, j].updateValue(-2);
                }
            }
        }
        

        SortedDictionary<int, List<MatrixData>> possibleRoomPoints = new SortedDictionary<int, List<MatrixData>>();
        //create dictonry of lists with possible room points with the same value

        int count = 0;
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                MatrixData entry = matrix[i, j];
                int entry_value = entry.getValue();
                if (entry_value >= 0)
                {
                    if (!possibleRoomPoints.ContainsKey(entry_value))
                    {
                        possibleRoomPoints.Add(entry_value, new List<MatrixData>());
                    }
                    List<MatrixData> list = possibleRoomPoints[entry_value];
                    list.Add(entry);
                    count++;
                }
            }
        }

        if (count < numberOfRooms) 
        {
            numberOfRooms = count;
        }

        generated_pos = new Vector2[numberOfRooms];
        generated_sizes = new float[numberOfRooms];
        generated_types = new RoomType[numberOfRooms];

        //For Room Sizes
        String[] individualSizes = roomGeneratingSizes.Split(';');
        List<float> roomGeneratingSizesList = new List<float>();
        foreach (String s in individualSizes)
        {
            roomGeneratingSizesList.Add(float.Parse(s));
        }

  
        //int currentMinValue = 0;
        for (int i = 0; i < numberOfRooms; i++)
        {
            ////Debug.LogError("länge: " + possibleRoomPoints.Count + " KEYS: " + possibleRoomPoints.Keys.Count);
            int smallestKey = possibleRoomPoints.Keys.First();
            List<MatrixData> list = possibleRoomPoints[smallestKey];
            ////Debug.LogError("list length: " + list.Count);
            while (list.Count == 0) 
            {
                possibleRoomPoints.Remove(smallestKey);
                smallestKey = possibleRoomPoints.Keys.First();
                list = possibleRoomPoints[smallestKey];
            }

            int randomInt = r.Next(0, list.Count);

            MatrixData roomPosition = list[randomInt];
            roomPosition.updateValue(-3);
            list.RemoveAt(randomInt);

            //Handling room sizes
            float roomSize = 1.0f;
                        
            if (useDifferentRoomSizes && roomGeneratingSizesList.Count > 0) 
            {
                if (shuffleSizeList)
                {
                    int index = r.Next(0, roomGeneratingSizesList.Count);
                    roomSize = roomGeneratingSizesList[index];
                    roomGeneratingSizesList.RemoveAt(index);
                }
                else 
                {
                    roomSize = roomGeneratingSizesList[0];
                    roomGeneratingSizesList.RemoveAt(0);
                }
            }
            UpdateMatrix(matrix, possibleRoomPoints, roomPosition, roomSize);

            float rand_OffsetX = (((float)r.NextDouble()) * useMatrixCellSize) - (useMatrixCellSize/2);
            float rand_OffsetY = (((float)r.NextDouble()) * useMatrixCellSize) - (useMatrixCellSize / 2);

            float x = x_min_value_rounded + roomPosition.getPosX() * useMatrixCellSize + rand_OffsetX;
            float y = y_min_value_rounded + roomPosition.getPosY() * useMatrixCellSize + rand_OffsetY;

            Vector2 point = new Vector2(x, y);

            generated_pos[i] = point;
            generated_sizes[i] = roomSize;
            if (useDifferentRoomSizesTESTVALUE)
            {
                generated_sizes[i] = 1.0f;
            }
            generated_types[i] = RoomType.Undefined;
        }



        matrixPerPolygon.Add(matrix);
    }

    private void UpdateMatrix(MatrixData[,] matrix, SortedDictionary<int, List<MatrixData>> possibleRoomPoints, MatrixData selectedRoomPos, float roomSize) 
    {
        List<MatrixData> updatedMatrixEntries = new List<MatrixData>();
        List<MatrixData> updatedInLastStep = new List<MatrixData>();
        List<MatrixData> updatedInThisStep = new List<MatrixData>();

        updatedInLastStep.Add(selectedRoomPos);

        int startingUpdateValue = roomInfluenceRange + Mathf.RoundToInt(roomInfluenceRange * (roomSize-1)/2);

        for (int i = 0; i < startingUpdateValue; i++) 
        {
            int currentUpdateValue = startingUpdateValue - i;
            foreach (MatrixData md in updatedInLastStep) 
            {
                int x = md.getPosX();
                int y = md.getPosY();
                //Check all 4 neighboured cells
                if (x > 0) 
                {
                    //check x-1, y
                    MatrixData updateMD = matrix[x - 1, y];
                    if (!updatedMatrixEntries.Contains(updateMD) && !updatedInThisStep.Contains(updateMD))
                    {
                        UpadteMatrixEntry(updateMD, possibleRoomPoints, currentUpdateValue);
                        updatedInThisStep.Add(updateMD);
                    }
                }
                if (x < matrix.GetLength(0) - 1) 
                {
                    //check x+1, y
                    MatrixData updateMD = matrix[x + 1, y];
                    if (!updatedMatrixEntries.Contains(updateMD) && !updatedInThisStep.Contains(updateMD))
                    {
                        UpadteMatrixEntry(updateMD, possibleRoomPoints, currentUpdateValue);
                        updatedInThisStep.Add(updateMD);
                    }
                }
                if (y > 0)
                {
                    //check x, y-1
                    MatrixData updateMD = matrix[x, y - 1];
                    if (!updatedMatrixEntries.Contains(updateMD) && !updatedInThisStep.Contains(updateMD))
                    {
                        UpadteMatrixEntry(updateMD, possibleRoomPoints, currentUpdateValue);
                        updatedInThisStep.Add(updateMD);
                    }
                }
                if (y < matrix.GetLength(1) - 1)
                {
                    //check x, y+1
                    MatrixData updateMD = matrix[x, y + 1];
                    if (!updatedMatrixEntries.Contains(updateMD) && !updatedInThisStep.Contains(updateMD))
                    {
                        UpadteMatrixEntry(updateMD, possibleRoomPoints, currentUpdateValue);
                        updatedInThisStep.Add(updateMD);
                    }
                }

                updatedMatrixEntries.Add(md);
            }
            updatedInLastStep = updatedInThisStep;
            updatedInThisStep = new List<MatrixData>();
        }

    }

    private void UpadteMatrixEntry(MatrixData updateMD, SortedDictionary<int, List<MatrixData>> possibleRoomPoints, int currentUpdateValue) 
    {
        int value = updateMD.getValue();
        if (value >= 0)
        {
            possibleRoomPoints[value].Remove(updateMD);
            int updatedValue = value + currentUpdateValue;
            updateMD.updateValue(updatedValue);

            if (!possibleRoomPoints.ContainsKey(updatedValue))
            {
                possibleRoomPoints.Add(updatedValue, new List<MatrixData>());
            }
            List<MatrixData> list = possibleRoomPoints[updatedValue];
            list.Add(updateMD);
        }
    }

    Segment RemoveSegmentMerge(List<Segment> final_segments, int index, int index2, List<List<Segment>> conect) 
    {
        Segment remove = FindSegmentInListThatConnectsPointsAandB(segments_straightened, index, index2);
        segments_straightened.Remove(remove);
        conect[index].Remove(remove);
        conect[index2].Remove(remove);
        List<Segment> newConnections = new List<Segment>();
        foreach (Segment s in conect[index])
        {
            newConnections.Add(s);
        }
        foreach (Segment s in conect[index2])
        {
            if (!newConnections.Contains(s))
            {
                newConnections.Add(s);
            }
        }

        return remove;
    }

    Segment RemoveSegment(List<Segment> final_segments, Segment s, List<List<Segment>> conect) 
    {
        segments_straightened.Remove(s);
        conect[s.GetIndexFrom()].Remove(s);
        conect[s.GetIndexTo()].Remove(s);

        return s;
    }
    Segment RemoveSegment(List<Segment> final_segments, int index, int index2, List<List<Segment>> conect)
    {
        Segment remove = FindSegmentInListThatConnectsPointsAandB(segments_straightened, index, index2);
        segments_straightened.Remove(remove);
        conect[index].Remove(remove);
        conect[index2].Remove(remove);

        return remove;
    }

    Segment FindSegmentInListThatConnectsPointsAandB(List<Segment> list, int indexA, int indexB) 
    {
        foreach (Segment element in list) 
        {
            if ((element.GetIndexFrom() == indexA && element.GetIndexTo() == indexB) || (element.GetIndexTo() == indexA && element.GetIndexFrom() == indexB)) 
            {
                return element;
            }
        }
        //Debug.LogError("Element not found");
        return new Segment(-1, -1);
    }

    bool ListContainsSegment(List<Segment> list, Segment s) 
    {
        foreach (Segment element in list) 
        {
            if ((element.GetIndexFrom() == s.GetIndexFrom() && element.GetIndexTo() == s.GetIndexTo()) || (element.GetIndexFrom() == s.GetIndexTo() && element.GetIndexTo() == s.GetIndexFrom())) 
            {
                return true;
            }
        }
        return false;
    }

    /*
    bool ListsHaveOverlaps<T>(List<T> a, List<T> b) 
    {
        foreach (T element in a) 
        {
            if (b.Contains(element)) 
            {
                return true;
            }
        }
        return false;
    }
    */
    List<Segment> ListsFindOverlaps(List<Segment> a, List<Segment> b)
    {
        List<Segment> overlaps = new List<Segment>();
        foreach (Segment element in a)
        {
            //if (b.Contains(element))
            if (ListContainsSegment(b, element))
            {
                overlaps.Add(element);
            }
        }
        return overlaps;
    }

    List<Segment> FindConnectedOutsideSegments(List<Segment> possibleCandidates, List<Segment> referenceOutside) 
    {
        List<Segment> connectedOutsideSegments = new List<Segment>();
        foreach (Segment element in possibleCandidates)
        {
            if (referenceOutside.Contains(element))
            {
                connectedOutsideSegments.Add(element);
            }
        }
        return connectedOutsideSegments;
    }

    void CalculateFullVoronoi(int polygonID, RoomType[] types) 
    {
        pointsVoronoi = new List<Vector2>();
        segments = new List<Segment>();
        int indexOfLastPoint = -1;
        int indexOfFirstPoint = -1;

        ////Debug.Log("<<<>>>");
        int test_sum = 0;
        int test_sum2 = 0;
        for (int k = 0; k < allP[polygonID].Count; k++)
        {


            ////Debug.Log(k + ": " + allP[k].Length);
            test_sum += allP[polygonID][k].Length;
            for (int i = 0; i < allP[polygonID][k].Length; i++)
            {
                
                if (!Maths2D.ListContainsVector(pointsVoronoi, allP[polygonID][k][i], out int indexOfPoint))
                {
                    pointsVoronoi.Add(allP[polygonID][k][i]);
                    indexOfPoint = pointsVoronoi.Count-1;
                }

                if (i >= 1)
                {
                    test_sum2 +=SegmentAlreadyExists(indexOfLastPoint, indexOfPoint);
                    segments.Add(new Segment(indexOfLastPoint, indexOfPoint, types[k], k));
                }
                else 
                {
                    indexOfFirstPoint = indexOfPoint;
                }
                indexOfLastPoint = indexOfPoint;
            }
            test_sum2 += SegmentAlreadyExists(indexOfLastPoint, indexOfFirstPoint);
            segments.Add(new Segment(indexOfLastPoint, indexOfFirstPoint, types[k], k));
        }

        ////Debug.Log(pointsVoronoi.Count + " (max: " + test_sum + ")");
        ////Debug.Log("paired_segments: " + test_sum2);


        //SEARCH FOR MORE INTERSECTION SEGMENTS INSIDE
        for (int i = 0; i < pointsVoronoi.Count; i++) 
        {
            for (int k = 0; k < segments.Count; k++) 
            {
                if (Maths2D.PointOnLineSegment(pointsVoronoi[segments[k].GetIndexFrom()], pointsVoronoi[segments[k].GetIndexTo()], pointsVoronoi[i]) && ((i != segments[k].GetIndexFrom()) && (i != segments[k].GetIndexTo()))) 
                {
                    RoomType r = segments[k].getRoomType();
                    int id = segments[k].getRoomID();
                    segments.Add(new Segment(segments[k].GetIndexFrom(), i, r, id));
                    segments.Add(new Segment(i, segments[k].GetIndexTo(), r, id));
                    segments.RemoveAt(k);
                }
            }
        }





        //CALCULATE HULL + HOLES
        outsideSegments = new List<Segment>();
        List<int> outsideIndices = new List<int>();
        int startindex;
        int endindex;
        problemSegments = new List<Segment>();

        for (int k = 0; k < polygons[polygonID].numHoles + 1; k++)
        {
            //Startindex
            if (k == 0)
            {
                startindex = 0;
            }
            else
            {
                startindex = polygons[polygonID].holeStartIndices[k - 1];
            }

            //Endindex
            if (polygons[polygonID].numHoles > k)
            {
                endindex = polygons[polygonID].holeStartIndices[k] - 1;
            }
            else
            {
                endindex = polygons[polygonID].points.Length - 1;
            }


            for (int i = startindex; i <= endindex; i++)
            {
                //Get corresponding Indices from List
                if (i + 1 > endindex)
                {
                    if (!Maths2D.ListContainsVector(pointsVoronoi, polygons[polygonID].points[startindex], out int indexOfPoint1))
                    {
                        pointsVoronoi.Add(polygons[polygonID].points[startindex]);
                        indexOfPoint1 = pointsVoronoi.Count - 1;
                    }

                    if (!Maths2D.ListContainsVector(pointsVoronoi, polygons[polygonID].points[endindex], out int indexOfPoint2))
                    {
                        pointsVoronoi.Add(polygons[polygonID].points[endindex]);
                        indexOfPoint2 = pointsVoronoi.Count - 1;
                    }
                    outsideSegments.Add(new Segment(indexOfPoint2, indexOfPoint1));
                }
                else
                {
                    if (!Maths2D.ListContainsVector(pointsVoronoi, polygons[polygonID].points[i], out int indexOfPoint1))
                    {
                        pointsVoronoi.Add(polygons[polygonID].points[i]);
                        indexOfPoint1 = pointsVoronoi.Count - 1;
                    }

                    if (!Maths2D.ListContainsVector(pointsVoronoi, polygons[polygonID].points[i+1], out int indexOfPoint2))
                    {
                        pointsVoronoi.Add(polygons[polygonID].points[i+1]);
                        indexOfPoint2 = pointsVoronoi.Count - 1;
                    }
                    outsideSegments.Add(new Segment(indexOfPoint1, indexOfPoint2));
                }
            }
        }

        //Initial Outside Segemtsn finished:
        //Now Adding Seperations:

        for (int i = 0; i < pointsVoronoi.Count; i++) 
        {
            for (int k = 0; k < outsideSegments.Count; k++) 
            {
                if (Maths2D.PointOnLineSegment(pointsVoronoi[outsideSegments[k].GetIndexFrom()], pointsVoronoi[outsideSegments[k].GetIndexTo()], pointsVoronoi[i]) && ((i != outsideSegments[k].GetIndexFrom()) &&( i != outsideSegments[k].GetIndexTo()))) 
                {
                    outsideSegments.Add(new Segment(outsideSegments[k].GetIndexFrom(), i));
                    outsideSegments.Add(new Segment(i, outsideSegments[k].GetIndexTo()));
                    outsideSegments.RemoveAt(k);
                    k = outsideSegments.Count;
                }
            }
        }

        ////Debug.LogWarning("segments: " + segments.Count + " (+" + outsideSegments.Count + ")");

        //Outside with Seperations done:
        for (int i = 0; i < outsideSegments.Count; i++) 
        {
            //test_sum2 += SegmentAlreadyExists(outsideSegments[i].GetIndexFrom(), outsideSegments[i].GetIndexTo());
            if (SegmentAlreadyExists(outsideSegments[i].GetIndexFrom(), outsideSegments[i].GetIndexTo()) < 0)
            {
                //problemSegments.Add(new Segment(outsideSegments[i].GetIndexFrom(), outsideSegments[i].GetIndexTo()));
                if (SegmentAlreadyExists(outsideSegments[i].GetIndexTo(), outsideSegments[i].GetIndexFrom()) < 0) 
                {
                    //Debug.LogError("Segment already full inside");
                }
                segments.Add(new Segment(outsideSegments[i].GetIndexTo(), outsideSegments[i].GetIndexFrom(), RoomType.Outside, -1));
               
            }
            else
            {
                outsideSegments[i].setRoomType(RoomType.Outside);
                outsideSegments[i].setRoomID(-1);
                segments.Add(outsideSegments[i]);
            }
        }

        ////Debug.Log("segments: " + segments.Count);
        ////Debug.Log("paired_segments: " + test_sum2);
        ////Debug.Log("problem_segments: " + problemSegments.Count);

        //Outside added all missing segments
        //NOW search for segments with only one side

        bool paired = false;
        for (int i = 0; i < segments.Count; i++) 
        {
            for (int k = 0; k < segments.Count; k++)
            {
                if (segments[i].GetIndexFrom() == segments[k].GetIndexTo() && segments[i].GetIndexTo() == segments[k].GetIndexFrom())
                {
                    paired = true;
                }
            }
            if (!paired) 
            {
                problemSegments.Add(segments[i]);
                ////Debug.LogWarning(segments[i].getRoomType().ToString());
            }
            paired = false;
        }


        //Group single Segments after their intern connection to each other --> building circles
        List<List<Segment>> problemSegmentsSorted = new List<List<Segment>>();
        List<int> connectedSegmentIds = new List<int>();

        int save = 0;
        int save2 = 0;
        
        while (problemSegments.Count != 0 && save < 1000) 
        {
            Segment start = problemSegments[0];
            Segment currentSeg = start;
            problemSegments.RemoveAt(0);
            int startIndex = start.GetIndexFrom();
            int currentIndex = start.GetIndexTo();

            problemSegmentsSorted.Add(new List<Segment>());
            problemSegmentsSorted[problemSegmentsSorted.Count - 1].Add(currentSeg);

            //TODO WHEN NOT WORKING --> ADDED currentIndex != -1 AND id != -1 loop
            while (currentIndex != startIndex && save2 < 1000 && currentIndex != -1) 
            {
                //Debug.LogError(currentIndex);
                int id = getNextPossibleSegment(problemSegments, currentSeg, currentIndex, out currentIndex);
                //Debug.LogError(polygonID + "here problem: " + id + " --> " + problemSegments.Count + " current: " + currentIndex + " start from: " + start.GetIndexFrom() + " to " + start.GetIndexTo());
                if (id != -1)
                {
                    currentSeg = problemSegments[id];
                    problemSegments.RemoveAt(id);

                    problemSegmentsSorted[problemSegmentsSorted.Count - 1].Add(currentSeg);

                }
                save2++;
            }
            save++;
            save2 = 0;
        }
        
        ////Debug.Log("# single segement areals: " + problemSegmentsSorted.Count);

        ////Debug.Log("problem_segments: " + problemSegments.Count);



        //Attaching the Areals to the neighboured rooms
        for (int i = 0; i < problemSegmentsSorted.Count; i++) 
        {
            Dictionary<int, float> neighbouredRooms = new Dictionary<int, float>();
            float maxLength = 0f;
            int maxRoomID = -1;
            for (int k = 0; k < problemSegmentsSorted[i].Count; k++) 
            {
                Segment s = problemSegmentsSorted[i][k];
                if (s.getRoomID() != -1) 
                {
                    int id = s.getRoomID();
                    float length = Vector2.Distance(pointsVoronoi[s.GetIndexFrom()], pointsVoronoi[s.GetIndexTo()]);

                    if (!neighbouredRooms.ContainsKey(id))
                    {
                        neighbouredRooms.Add(id, length);
                    }
                    else 
                    {
                        length += neighbouredRooms[id];
                        neighbouredRooms[id] = length;
                    }
                    if (length > maxLength)
                    {
                        maxLength = length;
                        maxRoomID = id;
                    }
                }
            }

            if (maxRoomID != -1) 
            {
                ////Debug.Log("maxRoomID: " + maxRoomID);
                for (int k = 0; k < problemSegmentsSorted[i].Count; k++)
                {
                    Segment s = problemSegmentsSorted[i][k];
                    int id = s.getRoomID();
                    if (id == maxRoomID)
                    {
                        segments.Remove(s);
                    }
                    else 
                    {
                        segments.Add(new Segment(s.GetIndexTo(), s.GetIndexFrom(), types[maxRoomID], maxRoomID));
                    }
                }
            }
        }

    }

    int getNextPossibleSegment(List<Segment> list, Segment currentSeg, int lastIndex, out int currentIndex) 
    {
        //MULTIPLE SEGEMENTS CONNECTED IN 1 POINT ARE CURRENTLY NOT SUPPORTED

        for (int i = 0; i < list.Count; i++) 
        {
            if (list[i] != currentSeg) 
            {
                if (list[i].GetIndexFrom() == lastIndex) 
                {
                    currentIndex = list[i].GetIndexTo();
                    return i;
                }
                if (list[i].GetIndexTo() == lastIndex)
                {
                    currentIndex = list[i].GetIndexFrom();
                    return i;
                }
            }
        }
        currentIndex = -1;
        return -1;
    }


    int SegmentAlreadyExists(int index_from, int index_to) 
    {
        for (int i = 0; i < segments.Count; i++) 
        {
            if (segments[i].GetIndexFrom() == index_from && segments[i].GetIndexTo() == index_to) 
            {
                ////Debug.LogError("SEGMENT ALREADY INSIDE");
                return -100000;
            }
            if (segments[i].GetIndexFrom() == index_to && segments[i].GetIndexTo() == index_from)
            {
                ////Debug.Log("paired segment exists");
                return 1;
            }
        }
        return 0;
    }

    bool PointInsidePolygon(Polygon polygon, int[] triangles, Vector3 p) 
    {
        bool pointInsideShape = false;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (Maths2D.PointInTriangle(polygon.points[triangles[i]], polygon.points[triangles[i + 1]], polygon.points[triangles[i + 2]], new Vector2(p.x, p.z)))
            {
                pointInsideShape = true;
                break;
            }
        }

        return pointInsideShape;
    }

    bool PointInsidePolygonExclusive(Polygon polygon, int[] triangles, Vector3 p)
    {
        bool pointInsideShape = false;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (Maths2D.PointInTriangleExclusive(polygon.points[triangles[i]], polygon.points[triangles[i + 1]], polygon.points[triangles[i + 2]], new Vector2(p.x, p.z)))
            {
                pointInsideShape = true;
                break;
            }
        }

        return pointInsideShape;
    }



    private void OnDrawGizmos() 
    {
        /*
        Gizmos.color = Color.magenta;
        float size = 0.35f;
        Gizmos.DrawSphere(new Vector3(75.992f, 0, 119.0082f), size);
        Gizmos.DrawSphere(new Vector3(76.84932f, 0, 108.62470f), size);
        Gizmos.DrawSphere(new Vector3(101.00300f, 0, 59.08675f), size);
        Gizmos.DrawSphere(new Vector3(60.99807f, 0, 127.95610f), size);
        Gizmos.DrawSphere(new Vector3(58.94525f, 0, 84.99584f), size);
        Gizmos.DrawSphere(new Vector3(58.05795f, 0, 54.83902f), size);
        Gizmos.DrawSphere(new Vector3(67.13230f, 0, 141.19020f), size);
        Gizmos.DrawSphere(new Vector3(40.95390f, 0, 89.13883f), size);
        Gizmos.DrawSphere(new Vector3(111.34330f, 0, 102.3012f), size);
        */
        if (randDistMethod == RandomDistributionMethod.matrix && drawMatrix)
        {
            for (int pol = 0; pol < polygons.Length; pol++)
            {
                Polygon p = polygons[pol];
                float x_min = float.PositiveInfinity;
                float x_max = float.NegativeInfinity;
                float y_min = float.PositiveInfinity;
                float y_max = float.NegativeInfinity;
                foreach (Vector2 point in p.points)
                {
                    if (point.x > x_max)
                    {
                        x_max = point.x;
                    }
                    if (point.x < x_min)
                    {
                        x_min = point.x;
                    }
                    if (point.y > y_max)
                    {
                        y_max = point.y;
                    }
                    if (point.y < y_min)
                    {
                        y_min = point.y;
                    }
                }

                float x_min_value_rounded = Mathf.Ceil(x_min / useMatrixCellSize) * useMatrixCellSize;
                float y_min_value_rounded = Mathf.Ceil(y_min / useMatrixCellSize) * useMatrixCellSize;

                ////Debug.LogError(matrixPerPolygon[pol].GetLength(0) + " ERROR " + matrixPerPolygon[pol].GetLength(1));

                for (int i = 0; i < matrixPerPolygon[pol].GetLength(0); i++)
                {
                    for (int j = 0; j < matrixPerPolygon[pol].GetLength(1); j++)
                    {
                        float x = x_min_value_rounded + i * useMatrixCellSize;
                        float y = y_min_value_rounded + j * useMatrixCellSize;

                        switch (matrixPerPolygon[pol][i, j].getValue()) 
                        {
                            case -1:
                                Gizmos.color = Color.blue;
                                
                                break;
                            case -2:
                                Gizmos.color = Color.cyan;
                                break;
                            case -3:
                                Gizmos.color = Color.yellow;
                                break;
                            case 0:
                                Gizmos.color = new Color32(0, 0, 0, 255);
                                break;
                            case 1:
                                Gizmos.color = new Color32(21, 6, 10, 255);
                                break;
                            case 2:
                                Gizmos.color = new Color32(33, 12, 18, 255);
                                break;
                            case 3:
                                Gizmos.color = new Color32(45, 14, 23, 255);
                                break;
                            case 4:
                                Gizmos.color = new Color32(57, 15, 27, 255);
                                break;
                            case 5:
                                Gizmos.color = new Color32(70, 15, 30, 255);
                                break;
                            case 6:
                                Gizmos.color = new Color32(83, 13, 33, 255);
                                break;
                            case 7:
                                Gizmos.color = new Color32(97, 11, 36, 255);
                                break;
                            case 8:
                                Gizmos.color = new Color32(110, 7, 38, 255);
                                break;
                            case 9:
                                Gizmos.color = new Color32(124, 2, 39, 255);
                                break;
                            case 10:
                                Gizmos.color = new Color32(138, 0, 40, 255);
                                break;
                            case 11:
                                Gizmos.color = new Color32(151, 0, 40, 255);
                                break;
                            case 12:
                                Gizmos.color = new Color32(165, 0, 39, 255);
                                break;
                            case 13:
                                Gizmos.color = new Color32(178, 0, 38, 255);
                                break;
                            case 14:
                                Gizmos.color = new Color32(192, 0, 36, 255);
                                break;
                            case 15:
                                Gizmos.color = new Color32(205, 0, 33, 255);
                                break;
                            case 16:
                                Gizmos.color = new Color32(218, 0, 29, 255);
                                break;
                            case 17:
                                Gizmos.color = new Color32(231, 0, 23, 255);
                                break;
                            case 18:
                                Gizmos.color = new Color32(243, 0, 15, 255);
                                break;
                            default:
                                Gizmos.color = new Color32(255, 0, 0, 255);
                                break;
                        }
                        Gizmos.DrawSphere(new Vector3(x, 0, y), matrixDrawSize);
                    }
                }
            }
        }
        

        if (!straightenEdges)
        {
            if (drawInnerSplitting)
            {
                Gizmos.color = Color.green;
                for (int p = 0; p < segmentsPerPolygon.Length; p++)
                {
                    for (int i = 0; i < segmentsPerPolygon[p].Length; i++)
                    {
                        ////Debug.LogError(i + " huhu " + pointsVoronoiPerPolygon[p].Length);
                        Gizmos.DrawLine(new Vector3(pointsVoronoiPerPolygon[p][segmentsPerPolygon[p][i].GetIndexFrom()].x, 0, pointsVoronoiPerPolygon[p][segmentsPerPolygon[p][i].GetIndexFrom()].y), new Vector3(pointsVoronoiPerPolygon[p][segmentsPerPolygon[p][i].GetIndexTo()].x, 0, pointsVoronoiPerPolygon[p][segmentsPerPolygon[p][i].GetIndexTo()].y));
                    }
                }
            }
            if (drawOutsideSegments)
            {
                Gizmos.color = Color.black;
                if (!drawInnerSplitting)
                {
                    Gizmos.color = Color.green;
                }
                for (int p = 0; p < outsideSegmentsPerPolygon.Length; p++)
                {
                    for (int i = 0; i < outsideSegmentsPerPolygon[p].Length; i++)
                    {
                        Gizmos.DrawLine(new Vector3(pointsVoronoiPerPolygon[p][outsideSegmentsPerPolygon[p][i].GetIndexFrom()].x, 0, pointsVoronoiPerPolygon[p][outsideSegmentsPerPolygon[p][i].GetIndexFrom()].y), new Vector3(pointsVoronoiPerPolygon[p][outsideSegmentsPerPolygon[p][i].GetIndexTo()].x, 0, pointsVoronoiPerPolygon[p][outsideSegmentsPerPolygon[p][i].GetIndexTo()].y));
                    }
                }
            }
            if (drawCornerPoints)
            {
                Gizmos.color = Color.white;
                for (int p = 0; p < pointsVoronoiPerPolygon.Length; p++)
                {
                    for (int i = 0; i < pointsVoronoiPerPolygon[p].Length; i++)
                    {
                        Gizmos.DrawSphere(new Vector3(pointsVoronoiPerPolygon[p][i].x, 0, pointsVoronoiPerPolygon[p][i].y), 0.05f);
                    }
                }
            }
        }
        else 
        {
            if (!simplifyEdges)
            {
                if (drawInnerSplitting)
                {
                    Gizmos.color = Color.green;
                    for (int p = 0; p < segmentsPerPolygon_straightened_cleanCOPY.Length; p++)
                    {
                        for (int i = 0; i < segmentsPerPolygon_straightened_cleanCOPY[p].Length; i++)
                        {
                            if (i < drawCounter)
                            {
                                Gizmos.DrawLine(new Vector3(pointsVoronoiPerPolygon_straightened_cleanCOPY[p][segmentsPerPolygon_straightened_cleanCOPY[p][i].GetIndexFrom()].x, 0, pointsVoronoiPerPolygon_straightened_cleanCOPY[p][segmentsPerPolygon_straightened_cleanCOPY[p][i].GetIndexFrom()].y), new Vector3(pointsVoronoiPerPolygon_straightened_cleanCOPY[p][segmentsPerPolygon_straightened_cleanCOPY[p][i].GetIndexTo()].x, 0, pointsVoronoiPerPolygon_straightened_cleanCOPY[p][segmentsPerPolygon_straightened_cleanCOPY[p][i].GetIndexTo()].y));
                            }
                        }
                    }
                }
                if (drawOutsideSegments)
                {
                    Gizmos.color = new Color(0.0f, 0.0f, 0.05f, 1.0f);
                    if (!drawInnerSplitting)
                    {
                        Gizmos.color = Color.green;
                    }
                    for (int p = 0; p < outsideSegmentsPerPolygon_straightened_cleanCOPY.Length; p++)
                    {
                        for (int i = 0; i < outsideSegmentsPerPolygon_straightened_cleanCOPY[p].Length; i++)
                        {
                            Gizmos.DrawLine(new Vector3(pointsVoronoiPerPolygon_straightened_cleanCOPY[p][outsideSegmentsPerPolygon_straightened_cleanCOPY[p][i].GetIndexFrom()].x, 0, pointsVoronoiPerPolygon_straightened_cleanCOPY[p][outsideSegmentsPerPolygon_straightened_cleanCOPY[p][i].GetIndexFrom()].y), new Vector3(pointsVoronoiPerPolygon_straightened_cleanCOPY[p][outsideSegmentsPerPolygon_straightened_cleanCOPY[p][i].GetIndexTo()].x, 0, pointsVoronoiPerPolygon_straightened_cleanCOPY[p][outsideSegmentsPerPolygon_straightened_cleanCOPY[p][i].GetIndexTo()].y));
                        }
                    }
                }
                if (drawCornerPoints)
                {
                    Gizmos.color = Color.white;
                    for (int p = 0; p < pointsVoronoiPerPolygon_straightened_cleanCOPY.Length; p++)
                    {
                        for (int i = 0; i < pointsVoronoiPerPolygon_straightened_cleanCOPY[p].Length; i++)
                        {
                            Gizmos.DrawSphere(new Vector3(pointsVoronoiPerPolygon_straightened_cleanCOPY[p][i].x, 0, pointsVoronoiPerPolygon_straightened_cleanCOPY[p][i].y), 0.05f);
                        }
                    }
                }
            }
            else 
            {
                if (segmentsPerPolygon_simplyfied != null)
                {
                    //Gizmos.color = new Color(0.3f, 0.6f, 0.3f, 1.0f);
                    if (drawInnerSplitting)
                    {
                        Gizmos.color = Color.green;
                        for (int p = 0; p < segmentsPerPolygon_simplyfied.Length; p++)
                        {
                            for (int i = 0; i < segmentsPerPolygon_simplyfied[p].Length; i++)
                            {
                                if (i < drawCounter)
                                {
                                    ////Debug.LogWarning(i + " From: " + pointsVoronoiPerPolygon_simplyfied[p][segmentsPerPolygon_simplyfied[p][i].GetIndexFrom()].x + "; " + pointsVoronoiPerPolygon_simplyfied[p][segmentsPerPolygon_simplyfied[p][i].GetIndexFrom()].y + ", To: " + pointsVoronoiPerPolygon_simplyfied[p][segmentsPerPolygon_simplyfied[p][i].GetIndexTo()].x + "; " + pointsVoronoiPerPolygon_simplyfied[p][segmentsPerPolygon_simplyfied[p][i].GetIndexTo()].y);
                                    ////Debug.Log(p + "; " + pointsVoronoiPerPolygon_simplyfied.Length + "; " + segmentsPerPolygon_simplyfied[p][i].GetIndexFrom() + "; " + segmentsPerPolygon_simplyfied[p][i].GetIndexTo() + "; " + pointsVoronoiPerPolygon_simplyfied[p].Length);
                                    Gizmos.DrawLine(new Vector3(pointsVoronoiPerPolygon_simplyfied[p][segmentsPerPolygon_simplyfied[p][i].GetIndexFrom()].x, 0, pointsVoronoiPerPolygon_simplyfied[p][segmentsPerPolygon_simplyfied[p][i].GetIndexFrom()].y), new Vector3(pointsVoronoiPerPolygon_simplyfied[p][segmentsPerPolygon_simplyfied[p][i].GetIndexTo()].x, 0, pointsVoronoiPerPolygon_simplyfied[p][segmentsPerPolygon_simplyfied[p][i].GetIndexTo()].y));
                                }
                            }
                        }
                    }
                    if (drawSmallEdges)
                    {
                        //Gizmos.color = new Color(0.02f, 0.8f, 0.01f, 1.0f);
                        Gizmos.color = new Color(0.4f, 0.1f, 0.01f, 1.0f);
                        for (int p = 0; p < smallSegmentsPerPolygon_simplyfied.Length; p++)
                        {
                            for (int i = 0; i < smallSegmentsPerPolygon_simplyfied[p].Length; i++)
                            {
                                Gizmos.DrawLine(new Vector3(pointsVoronoiPerPolygon_simplyfied[p][smallSegmentsPerPolygon_simplyfied[p][i].GetIndexFrom()].x, 0, pointsVoronoiPerPolygon_simplyfied[p][smallSegmentsPerPolygon_simplyfied[p][i].GetIndexFrom()].y), new Vector3(pointsVoronoiPerPolygon_simplyfied[p][smallSegmentsPerPolygon_simplyfied[p][i].GetIndexTo()].x, 0, pointsVoronoiPerPolygon_simplyfied[p][smallSegmentsPerPolygon_simplyfied[p][i].GetIndexTo()].y));
                            }
                        }
                    }
                    if (drawOutsideSegments)
                    {
                        //Gizmos.color = new Color(0.02f, 0.8f, 0.01f, 1.0f);
                        Gizmos.color = new Color(0.9f, 0.1f, 0.01f, 1.0f);
                        if (!drawInnerSplitting)
                        {
                            Gizmos.color = Color.green;
                        }
                        for (int p = 0; p < outsideSegmentsPerPolygon_simplyfied.Length; p++)
                        {
                            for (int i = 0; i < outsideSegmentsPerPolygon_simplyfied[p].Length; i++)
                            {
                                Gizmos.DrawLine(new Vector3(pointsVoronoiPerPolygon_simplyfied[p][outsideSegmentsPerPolygon_simplyfied[p][i].GetIndexFrom()].x, 0, pointsVoronoiPerPolygon_simplyfied[p][outsideSegmentsPerPolygon_simplyfied[p][i].GetIndexFrom()].y), new Vector3(pointsVoronoiPerPolygon_simplyfied[p][outsideSegmentsPerPolygon_simplyfied[p][i].GetIndexTo()].x, 0, pointsVoronoiPerPolygon_simplyfied[p][outsideSegmentsPerPolygon_simplyfied[p][i].GetIndexTo()].y));
                            }
                        }
                    }
                    if (drawCornerPoints)
                    {
                        Gizmos.color = Color.white;
                        for (int p = 0; p < pointsVoronoiPerPolygon_simplyfied.Length; p++)
                        {
                            for (int i = 0; i < pointsVoronoiPerPolygon_simplyfied[p].Length; i++)
                            {
                                Gizmos.DrawSphere(new Vector3(pointsVoronoiPerPolygon_simplyfied[p][i].x, 0, pointsVoronoiPerPolygon_simplyfied[p][i].y), 0.05f);
                            }
                        }
                    }

                    if (drawSpecifcSegment)
                    {
                        Gizmos.color = Color.blue;
                        for (int p = 0; p < segmentsPerPolygon_simplyfied.Length; p++)
                        {
                            int i = drawSpecificSegmentNum;
                            if (segmentsPerPolygon_simplyfied[p].Length > i)
                            {
                                Gizmos.DrawLine(new Vector3(pointsVoronoiPerPolygon_simplyfied[p][segmentsPerPolygon_simplyfied[p][i].GetIndexFrom()].x, 0, pointsVoronoiPerPolygon_simplyfied[p][segmentsPerPolygon_simplyfied[p][i].GetIndexFrom()].y), new Vector3(pointsVoronoiPerPolygon_simplyfied[p][segmentsPerPolygon_simplyfied[p][i].GetIndexTo()].x, 0, pointsVoronoiPerPolygon_simplyfied[p][segmentsPerPolygon_simplyfied[p][i].GetIndexTo()].y));
                            }
                        }
                    }

                    if (drawSpecificPoint)
                    {
                        Gizmos.color = Color.blue;
                        for (int p = 0; p < pointsVoronoiPerPolygon_simplyfied.Length; p++)
                        {
                            if (specificPointIndex < pointsVoronoiPerPolygon_simplyfied[p].Length)
                            {
                                Gizmos.DrawSphere(new Vector3(pointsVoronoiPerPolygon_simplyfied[p][specificPointIndex].x, 0, pointsVoronoiPerPolygon_simplyfied[p][specificPointIndex].y), specificPointSize);
                            }
                        }
                    }

                    if (drawSpecificOutsideSegment)
                    {
                        Gizmos.color = Color.blue;
                        for (int p = 0; p < outsideSegmentsPerPolygon_simplyfied.Length; p++)
                        {
                            int i = specificOutsideSegmentIndex;
                            if (outsideSegmentsPerPolygon_simplyfied[p].Length > i)
                            {
                                if (p == 3)
                                {
                                    //Debug.LogWarning(p + "outside Segment: " + outsideSegmentsPerPolygon_simplyfied[p][i].GetIndexFrom() + " to " + outsideSegmentsPerPolygon_simplyfied[p][i].GetIndexTo());
                                }
                                Gizmos.DrawLine(new Vector3(pointsVoronoiPerPolygon_simplyfied[p][outsideSegmentsPerPolygon_simplyfied[p][i].GetIndexFrom()].x, 0, pointsVoronoiPerPolygon_simplyfied[p][outsideSegmentsPerPolygon_simplyfied[p][i].GetIndexFrom()].y), new Vector3(pointsVoronoiPerPolygon_simplyfied[p][outsideSegmentsPerPolygon_simplyfied[p][i].GetIndexTo()].x, 0, pointsVoronoiPerPolygon_simplyfied[p][outsideSegmentsPerPolygon_simplyfied[p][i].GetIndexTo()].y));
                            }
                        }
                    }
                }
            }
        }
        if (drawInnerSplitting && checkForSmallCorners)
        {
            Gizmos.color = Color.red;
            ////Debug.Log("points: " + pointsVoronoi.Count + ", segment: " + segments.Count);
            for (int p = 0; p < smallCorners.Length; p++)
            {
                for (int i = 0; i < smallCorners[p].Count; i++)
                {
                    Gizmos.DrawLine(new Vector3(smallCorners[p][i][0].x, 0, smallCorners[p][i][0].y), new Vector3(smallCorners[p][i][1].x, 0, smallCorners[p][i][1].y));
                }
            }
        }

        if (drawInnerSplitting && showVorornoiSplitsForSpecificRoomTEST)
        {
            Gizmos.color = Color.red;
            for (int p = 0; p < splittingPointsTEST.Count; p++)
            {

                for (int i = 0; i < splittingPointsTEST[p].Length; i++)
                {
                    Gizmos.DrawSphere(new Vector3(splittingPointsTEST[p][i].x, 0, splittingPointsTEST[p][i].y), 0.25f);
                }

                if (splittingPointsTEST[p].Length > 1)
                {
                    for (int i = 1; i < splittingPointsTEST[p].Length; i++)
                    {
                        if (isCutTEST[p][i - 1] == false)
                        {
                            Gizmos.color = Color.green;
                        }
                        Gizmos.DrawLine(new Vector3(splittingPointsTEST[p][i].x, 0, splittingPointsTEST[p][i].y), new Vector3(splittingPointsTEST[p][i - 1].x, 0, splittingPointsTEST[p][i - 1].y));
                        Gizmos.color = Color.red;
                    }
                }
                else
                {
                    //Debug.LogWarning("NO");
                    for (int i = 0; i < splittingPointsTEST[p].Length; i++)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(new Vector3(splittingPointsTEST[p][i].x, 0, splittingPointsTEST[p][i].y), 0.15f);
                        Gizmos.color = Color.red;
                    }
                }
            }
        }
        
    }
}

