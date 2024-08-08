using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    [System.Serializable]
    public class MatrixData
    {
        int value;

        int pos_x;
        int pos_y;

        public MatrixData(int value, int pos_x, int pos_y)
        {
            this.value = value;
            this.pos_x = pos_x;
            this.pos_y = pos_y;
        }

        public int getValue() 
        {
            return this.value;
        }

        public int getPosX() 
        {
            return this.pos_x;
        }

        public int getPosY()
        {
            return this.pos_y;
        }

        public void updateValue(int newValue) 
        {
            this.value = newValue;
        }
    }
}
