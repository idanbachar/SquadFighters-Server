using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquadFighters.Server {
    public class Position {
        public float X; //ציר X
        public float Y; //ציר Y

        /// <summary>
        /// פונקציה המקבלת נקודות בציר X,Y
        /// ומייצרת מיקום
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Position(float x, float y) {
            X = x;
            Y = y;
        }
    }
}
