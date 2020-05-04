using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
    public class Unit
    {
        public string name;
        public Unit_Class unit_class;
        public int experience;
        public int relationship_bonus; // 0 = strength, 1 = defence, 2 = magic, 3 = resistance, 4 = speed, 5 = agility, 6 = skill, 7 = trickiness, 8 = fortune
        public int max_hp;
        public int health;
        public int hp_growth;
        public int strength;
        public int strength_growth;
        public int strength_bonus;
        public int defence;
        public int defence_growth;
        public int defence_bonus;
        public int magic;
        public int magic_growth;
        public int magic_bonus;
        public int resistance;
        public int resistance_growth;
        public int resistance_bonus;
        public int speed;
        public int speed_growth;
        public int speed_bonus;
        public int agility;
        public int agility_growth;
        public int agility_bonus;
        public int skill;
        public int skill_growth;
        public int skill_bonus;
        public int trickiness;
        public int trickiness_growth;
        public int trickiness_bonus;
        public int fortune;
        public int fortune_growth;
        public int fortune_bonus;
        public Weapon weapon1;
        public Weapon weapon2;
        public Item inventory1;
        public Item inventory2;
        public int x_pos;
        public int y_pos;
        public int status; // 0 = ready, 1 = wait, 2 = dead
        public int team; // 0 = player1, 1 = player2, 2 = player3
    }
}
