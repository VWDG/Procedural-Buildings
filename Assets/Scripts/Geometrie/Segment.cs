using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    [System.Serializable]
    public class Segment
    {
        private int indexFrom;
        private int indexTo;
        private RoomType roomType;
        private int roomID;

        public Segment(int index_from, int index_to) 
        {
            indexFrom = index_from;
            indexTo = index_to;
            roomType = RoomType.Undefined;
            roomID = -1;
            possible();
        }

        public Segment(int index_from, int index_to, RoomType r)
        {
            indexFrom = index_from;
            indexTo = index_to;
            roomType = r;
            roomID = -1;
            possible();
        }

        public Segment(int index_from, int index_to, RoomType r, int id)
        {
            indexFrom = index_from;
            indexTo = index_to;
            roomType = r;
            roomID = id;
            possible();
        }

        public int GetIndexFrom() 
        {
            return indexFrom;
        }

        public int GetIndexTo() 
        {
            return indexTo;
        }

        public void SetIndexFrom(int index) 
        {
            indexFrom = index;
            possible();
        }

        public void SetIndexTo(int index) 
        {
            indexTo = index;
            possible();
        }

        public RoomType getRoomType() 
        {
            return roomType;
        }

        public void setRoomType(RoomType r) 
        {
            roomType = r;
        }

        public int getRoomID() 
        {
            return roomID;
        }

        public void setRoomID(int id) 
        {
            roomID = id;
        }

        public Segment copy() 
        {
            Segment s = new Segment(this.indexFrom, this.indexTo);
            s.setRoomID(this.roomID);
            s.setRoomType(this.roomType);

            return s;
        }

        private void possible() 
        {
            if (indexFrom == indexTo && indexFrom != -1)
            {
                //Debug.LogError("WARNING WARNING WARNING --> segments start (" + indexFrom + ") == segment end (" + indexTo + ")");
            }
        }
    }
}
