using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SegmentDeclaration
{
    public int id_from;
    public int id_to;

    public SegmentType segment_type;
    public RoomType room_type_right;
    public RoomType room_type_left;

    public int segment_id;

    private SegmentDeclaration neighbour_seg_from = null;
    private SegmentDeclaration neighbour_seg_to = null;
    private SegmentDeclaration opposite_seg = null;

    public SegmentDeclaration()
    {
    }

    public SegmentDeclaration(int index_from, int index_to)
    {
        id_from = index_from;
        id_to = index_to;
        room_type_right = RoomType.LivingRoom;
        room_type_left = RoomType.LivingRoom;
        segment_id = 0;
    }

    public void SetFromNeighbourSegment(SegmentDeclaration seg) 
    {
        neighbour_seg_from = seg;
    }

    public void SetToNeighbourSegment(SegmentDeclaration seg)
    {
        neighbour_seg_to = seg;
    }

    public void SetOppositeSegment(SegmentDeclaration seg)
    {
        opposite_seg = seg;
    }

    public SegmentDeclaration GetFromNeighbourSegment() 
    {
        return neighbour_seg_from;
    }

    public SegmentDeclaration GetToNeighbourSegment()
    {
        return neighbour_seg_to;
    }

    public SegmentDeclaration GetOppositeSegment()
    {
        return opposite_seg;
    }
}
