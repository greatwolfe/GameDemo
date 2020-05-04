using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
    public class Weapon
    {
        public string name;
        public bool magical; // 0 = physical, 1 = magic
        public int power;
        public int weight;
        public int hit;
        public int crit;
        public string ability;
        public int type; //0 = sword, 1 = lance, 2 = axe
    }
}
