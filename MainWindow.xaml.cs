using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;

namespace Final_Game
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Image> CurrentScreenImages = new List<Image> { };
        List<Label> CurrentScreenLabels = new List<Label> { };
        List<Image> CurmapGrid = new List<Image> { };
        List<Unit2> CurrentUnits = new List<Unit2> { };
        List<Image> ColouredSpots = new List<Image> { };
        List<Image> InfoImages = new List<Image> { };
        List<Label> MenuLabels = new List<Label> { };
        List<Node> Path = new List<Node> { };
        //Lists for battles
        List<int[]> BattleList = new List<int[]> { };
        List<Unit2> BattleParticipants = new List<Unit2> { };
        List<int> BattleOrder = new List<int> { };
        List<WriteableBitmap> Unit1Animation = new List<WriteableBitmap> { };
        List<WriteableBitmap> Unit2Animation = new List<WriteableBitmap> { };
        // 0 is background 1,2 is character1 and their weapon, 3,4 is character2 and their weapon
        List<Image> BattlePics = new List<Image> { };
        List<Image> BattleInfoPics = new List<Image> { };
        List<Label> BattleInfoLabel = new List<Label> { };


        //Global variables
        // int array for current map being played and top left ints, and the grid for the terrain of the map
        int[] curmap;
        int map_x = 0;
        int map_y = 0;
        int map_width = 0;
        int map_height = 0;
        int[] curmap_grid_ForTerrain;
        int curteamturn = -1; // 0 = player1, 1 = player2, 2 = player3
        int cur_x = 0;
        int cur_y = 0;
        int team1colour = 1;
        int team2colour = 7;
        bool animation_occuring = false;
        bool battle_occuring = false;
        int battleturn = 0;
        bool inventorymenu = false;
        
        //function for enemy ai
        private void enemy_turn()
        {
            //use ai_type;;;;; ******
            Unit2 guy = null;
            Unit2 dude = null;
            List<Unit2> People = new List<Unit2> { };
            foreach(Unit2 person in CurrentUnits)
            {
                if (person.team == 1 && person.status == 0)
                {
                    guy = person;
                    break;
                }
            }
            if (guy == null)
            {
                end_turn();
                return;
            }
            ActiveMovement(guy);
            foreach(Image x in ColouredSpots)
            {
                dude = playeratcoord(Grid.GetColumn(x), Grid.GetRow(x));
                if(dude != null)
                {
                    People.Add(dude);
                }
            }
            ClearColouredSpots();
            dude = getweakest(People, guy);
            //determine whether they should retreat or attack
            if(People.Count > 0)
            {
                if (Damage(dude, guy) < guy.health * 2)
                {
                    selected = guy;
                    ActiveMovement(guy);
                    if ((Math.Sqrt(Math.Pow((selected.x_pos - dude.x_pos), 2.0) + Math.Pow((selected.y_pos - dude.y_pos), 2.0)) <= 1))
                    {
                        BattleParticipants.Add(selected);
                        BattleParticipants.Add(dude);
                        battle_occuring = true;
                        Pathfinding(selected, selected.x_pos, selected.y_pos);
                    }
                    else if (bluespotexists(Grid.GetColumn(dude.char_im), Grid.GetRow(dude.char_im) - 1))
                    {
                        BattleParticipants.Add(selected);
                        BattleParticipants.Add(dude);
                        battle_occuring = true;
                        Pathfinding(selected, Grid.GetColumn(dude.char_im), Grid.GetRow(dude.char_im) - 1);
                    }
                    else if (bluespotexists(Grid.GetColumn(dude.char_im), Grid.GetRow(dude.char_im) + 1))
                    {
                        BattleParticipants.Add(selected);
                        BattleParticipants.Add(dude);
                        battle_occuring = true;
                        Pathfinding(selected, Grid.GetColumn(dude.char_im), Grid.GetRow(dude.char_im) + 1);
                    }
                    else if (bluespotexists(Grid.GetColumn(dude.char_im) - 1, Grid.GetRow(dude.char_im)))
                    {
                        BattleParticipants.Add(selected);
                        BattleParticipants.Add(dude);
                        battle_occuring = true;
                        Pathfinding(selected, Grid.GetColumn(dude.char_im) - 1, Grid.GetRow(dude.char_im));
                    }
                    else if (bluespotexists(Grid.GetColumn(dude.char_im) + 1, Grid.GetRow(dude.char_im)))
                    {
                        BattleParticipants.Add(selected);
                        BattleParticipants.Add(dude);
                        battle_occuring = true;
                        Pathfinding(selected, Grid.GetColumn(dude.char_im) + 1, Grid.GetRow(dude.char_im));
                    }
                }
                else
                {
                    Wait(guy);
                }
                //else if the character's health is lower than 50%
                //{
                //    //retreat
                //}
            }
            else
            {
                Wait(guy);
            }
        }
        private Unit2 getweakest(List<Unit2> people, Unit2 dude)
        {
            Unit2 guy = null;
            foreach(Unit2 person in people)
            {
                if(guy == null)
                {
                    guy = person;
                    continue;
                }
                if(strengthvalue(guy, dude) < strengthvalue(guy, person))
                {
                    guy = person;
                }
            }

            return guy;
        }
        private int strengthvalue(Unit2 guy, Unit2 dude)
        {
            int x = 0;
            x += Damage(dude, guy);
            if(Damage(dude, guy) >= guy.health)
            {
                x *= 5;
            }
            x += Crit(dude, guy);
            if(Hit(dude, guy) < 50)
            {
                x /= 2;
            }

            return x;
        }
        //function for ending and beginning turns
        private void end_turn()
        {
            bool x = false;
            bool y = false;
            foreach(Unit2 guy in CurrentUnits)
            {
                if(guy.team == curteamturn && guy.status == 0)
                {
                    y = true;
                }
                if (guy.team == 1 && guy.status != 2) x = true;
            }
            if(x && !y)
            {
                begin_turn();
                return;
            }
            if (x && y) return;
            if(!x) win_animation();
        }
        private void win_animation()
        {
            Create_InfoTextLabel(10, 4, map_x, map_y + 12, "Winner, press esc to close", false, false);
        }
        private void begin_turn()
        {
            animation_occuring = true;
            Image panel = Create_Image(32, 3, @"pack://siteoforigin:,,,/Resources/PlayerTurnPhase.png", 0, 7);
            Grid.SetZIndex(panel, 200);
            BattlePics.Add(panel);
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 0;
            animation.To = 1.5;
            animation.Duration = new Duration(TimeSpan.FromSeconds(1.5));
            animation.AutoReverse = true;

            board_move = new Storyboard();
            board_move.Children.Add(animation);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Image.OpacityProperty));
            Storyboard.SetTarget(animation, panel);

            animation.Completed += new EventHandler(beginturnfinished);

            if(curteamturn == 0)
            {
                curteamturn = 1;
                foreach(Unit2 guy in CurrentUnits)
                {
                    if (guy == null) break;
                    if (guy.team == 0)
                    {
                        var animation2 = new ObjectAnimationUsingKeyFrames();
                        animation2.BeginTime = TimeSpan.FromSeconds(0);
                        animation2.RepeatBehavior = RepeatBehavior.Forever;
                        animation2.AutoReverse = true;
                        Storyboard.SetTarget(animation2, guy.char_im);
                        Storyboard.SetTargetProperty(animation2, new PropertyPath(Image.SourceProperty));
                        animation2.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", guy.unit_class.animation_name, "Map1.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team1colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))));
                        animation2.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", guy.unit_class.animation_name, "Map2.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team1colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))));
                        animation2.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", guy.unit_class.animation_name, "Map1.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team1colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))));
                        board.Children.Add(animation2);
                        board.Begin();
                    }
                    else
                    {
                        remove_buffs(guy);
                        add_buffs(guy);
                    }
                    if (guy.status == 2) ;
                    else guy.status = 0;
                }
                panel.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/EnemyTurnPhase.png"));
            }
            else
            {
                curteamturn = 0;
                foreach (Unit2 guy in CurrentUnits)
                {
                    if (guy == null) break;
                    if(guy.team != 0)
                    {
                        var animation2 = new ObjectAnimationUsingKeyFrames();
                        animation2.BeginTime = TimeSpan.FromSeconds(0);
                        animation2.RepeatBehavior = RepeatBehavior.Forever;
                        animation2.AutoReverse = true;
                        Storyboard.SetTarget(animation2, guy.char_im);
                        Storyboard.SetTargetProperty(animation2, new PropertyPath(Image.SourceProperty));
                        animation2.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", guy.unit_class.animation_name, "Map1.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team2colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))));
                        animation2.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", guy.unit_class.animation_name, "Map2.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team2colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))));
                        animation2.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", guy.unit_class.animation_name, "Map1.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team2colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))));
                        board.Children.Add(animation2);
                        board.Begin();
                    }
                    else
                    {
                        remove_buffs(guy);
                        add_buffs(guy);
                    }
                    if (guy.status == 2) ;
                    else guy.status = 0;
                }
            }
            board_move.Begin();
        }
        private void remove_buffs(Unit2 guy)
        {
            guy.strength_bonus = 0;
            guy.defence_bonus = 0;
            guy.magic_bonus = 0;
            guy.resistance_bonus = 0;
            guy.speed_bonus = 0;
            guy.agility_bonus = 0;
            guy.skill_bonus = 0;
            guy.trickiness_bonus = 0;
            guy.fortune_bonus = 0;
        }
        private void add_buffs(Unit2 guy)
        {
            if(guy.inventory1 != null)
            {
                find_buff(guy, guy.inventory1);
            }
            if(guy.inventory2 != null)
            {
                find_buff(guy, guy.inventory2);
            }
        }
        private void find_buff(Unit2 guy, Item thing)
        {
            if (thing.stat == 0) guy.strength_bonus = thing.buff;
            else if (thing.stat == 1) guy.defence_bonus = thing.buff;
            else if (thing.stat == 2) guy.magic_bonus = thing.buff;
            else if (thing.stat == 3) guy.resistance_bonus = thing.buff;
            else if (thing.stat == 4) guy.speed_bonus = thing.buff;
            else if (thing.stat == 5) guy.agility_bonus = thing.buff;
            else if (thing.stat == 6) guy.skill_bonus = thing.buff;
            else if (thing.stat == 7) guy.trickiness_bonus = thing.buff;
            else if (thing.stat == 8) guy.fortune_bonus = thing.buff;
        }
        private void beginturnfinished(object sender, EventArgs e)
        {
            animation_occuring = false;
            ClearListImages(BattlePics);
            if (curteamturn == 1) enemy_turn();
            //start enemy turn
        }
        //function for finding the curmap_grid_ForTerrain of the grid at position x, y
        private int posforcurcoord(int x, int y)
        {
            int x1 = x - map_x;
            int y1 = y - map_y;

            return curmap_grid_ForTerrain[x1 + (y1 * map_width)];
        }
        private int tiledefenceconvert(int x)
        {
            //fix this once all tiles exist
            return 0;
        }

        // Units, will change this later
        //Unit2 player1;
        //Unit2 player2;
        Unit2 selected;
        Unit2 InfoSelected;
        Image gone;

        //storyboard for animations
        private Storyboard board;
        private Storyboard board_move;
        private Storyboard board_attack;

        //Functions for moving a character in each of the four directions
        private void Move(int direction, Unit2 guy)
        {
            animation_occuring = true;
            //0 = down, 1 = left, 2 = up, 3 = right
            double squaresize = SystemParameters.PrimaryScreenHeight / 18;
            var animation = new ThicknessAnimation();
            if (direction == 2)
            {
                guy.y_pos -= 1;
                Grid.SetRow(guy.char_im, guy.y_pos);
                animation.From = new Thickness(0, squaresize, 0, -squaresize);
                animation.To = new Thickness(0, 0, 0, 0);
            }
            else if(direction == 1)
            {
                guy.x_pos -= 1;
                Grid.SetColumn(guy.char_im, guy.x_pos);
                animation.From = new Thickness(squaresize, 0, -squaresize, 0);
                animation.To = new Thickness(0, 0, 0, 0);
            }
            else if(direction == 0)
            {
                guy.y_pos += 1;
                Grid.SetRow(guy.char_im, guy.y_pos);
                animation.From = new Thickness(0, -squaresize, 0, squaresize);
                animation.To = new Thickness(0, 0, 0, 0);
            }
            else
            {
                guy.x_pos += 1;
                Grid.SetColumn(guy.char_im, guy.x_pos);
                animation.From = new Thickness(-squaresize, 0, squaresize, 0);
                animation.To = new Thickness(0, 0, 0, 0);
            }
            animation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            animation.Completed += new EventHandler(MovementFinished);
            board_move = new Storyboard();
            Storyboard.SetTargetName(animation, guy.char_im.Name);
            Storyboard.SetTarget(animation, guy.char_im);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Image.MarginProperty));
           // this.RegisterName(guy.char_im.Name, guy.char_im);
            board_move.Children.Add(animation);
            board_move.Begin(this);
        }
        private void MovementFinished(object sender, EventArgs e)
        {
            Unit2 guy = selected;
            board_move.Remove(guy.char_im);
            if(Path.Count == 0)
            {
                //finished
                ClearColouredSpots();
                animation_occuring = false;
                if(battle_occuring)
                {
                    Battle(BattleParticipants[0], BattleParticipants[1]);
                    BattleSetup(BattleParticipants[0], BattleParticipants[1]);
                }
                else
                {
                    Load_Wait_menu(selected);
                }
            }
            else
            {
                if (Path[Path.Count - 1].x < guy.x_pos) // move left
                {
                    clear_furthest_parent();
                    Move(1, guy);
                }
                else if (Path[Path.Count - 1].x > guy.x_pos) // move right
                {
                    clear_furthest_parent();
                    Move(3, guy);
                }
                else if (Path[Path.Count - 1].y < guy.y_pos) // move up
                {
                    clear_furthest_parent();
                    Move(2, guy);
                }
                else if (Path[Path.Count - 1].y > guy.y_pos) // move down
                {
                    clear_furthest_parent();
                    Move(0, guy);
                }
            }
        }

        private void BattleSetup(Unit2 attacker, Unit2 defender)
        {
            string curdirname = Directory.GetCurrentDirectory();
            string[] expend = Directory.GetFiles(String.Concat(curdirname, "/Resources/", attacker.unit_class.animation_name), "*.png");
            BattlePics.Add(Create_Image(16, 8, @"pack://siteoforigin:,,,/Resources/MainBackground.png", 8, 5));
            Grid.SetZIndex(BattlePics[0], 50);
            foreach (string s in expend)
            {
                Unit1Animation.Add(ColourShiftedSource(s, string.Concat(@"pack://siteoforigin:,,,/Resources/", attacker.unit_class.animation_name, "Palette.png"), string.Concat(@"pack://siteoforigin:,,,/Resources/", attacker.name, "Palette.png"), 130, 80));
            }
            expend = Directory.GetFiles(String.Concat(curdirname, "/Resources/", defender.unit_class.animation_name), "*.png");
            foreach (string s in expend)
            {
                Unit2Animation.Add(ColourShiftedSource(s, string.Concat(@"pack://siteoforigin:,,,/Resources/", defender.unit_class.animation_name, "Palette.png"), string.Concat(@"pack://siteoforigin:,,,/Resources/", defender.name, "Palette.png"), 130, 80));
            }
            LoadCharacterandWeapons(attacker, defender);
            BattleAnimation();
        }
        private void LoadCharacterandWeapons(Unit2 attacker, Unit2 defender)
        {
            if(attacker.unit_class.animation_name == "Noble1")
            {
                BattlePics.Add(Create_battle_image(16, 8, Unit1Animation[0], 8, 5, false));
            }
            if(defender.unit_class.animation_name == "Noble1")
            {
                BattlePics.Add(Create_battle_image(16, 8, Unit2Animation[0], 8, 5, true));
            }
        }

        private void BattleAnimation()
        {
            battle_occuring = true;
            animation_occuring = true;
            int x = BattleOrder[battleturn];
            Unit2 attacker;
            Unit2 defender;
            if(x == 0)
            {
                attacker = BattleParticipants[x];
                defender = BattleParticipants[1];
                Grid.SetZIndex(BattlePics[1], 99);
                Grid.SetZIndex(BattlePics[2], 98);
                if (String.Equals(attacker.unit_class.animation_name, "Noble1")) Noble1BattleAnimation(BattlePics[1], 1, defender, BattlePics[2]);
            }
            else
            {
                attacker = BattleParticipants[0];
                defender = BattleParticipants[x];
                Grid.SetZIndex(BattlePics[2], 99);
                Grid.SetZIndex(BattlePics[1], 98);
                if (String.Equals(attacker.unit_class.animation_name, "Noble1")) Noble1BattleAnimation(BattlePics[2], -1, attacker, BattlePics[1]);
            }
        }
        private int DodgeAnimation(Unit2 guy, Image guy2, int x_direction, double starttime)
        {
            if(String.Equals(guy.unit_class.animation_name, "Noble1"))
            {
                return Noble1DodgeAnimation(guy2, x_direction, starttime);
            }
            return 0;
        }
        private int Noble1DodgeAnimation(Image guy, int x_direction, double starttime)
        {
            List<WriteableBitmap> sourcelist = new List<WriteableBitmap> { };
            if (BattleOrder[battleturn] == 0)
            {
                sourcelist = Unit2Animation;
            }
            else
            {
                sourcelist = Unit1Animation;
            }
            double squaresize = SystemParameters.PrimaryScreenWidth / 32;
            board_attack = new Storyboard();
            ObjectAnimationUsingKeyFrames animation4 = new ObjectAnimationUsingKeyFrames();
            // Source of the person
            Storyboard.SetTarget(animation4, guy);
            Storyboard.SetTargetProperty(animation4, new PropertyPath(Image.SourceProperty));
            animation4.BeginTime = TimeSpan.FromSeconds(starttime);
            board_attack.Children.Add(animation4);
            board_attack.Begin();

            // return the animation length
            return 4;
        }
        private void Noble1BattleAnimation(Image guy, int x_direction, Unit2 defender, Image guy2)//1 is moving left, -1 is moving right, defender is for the dodge animation
        {
            List<WriteableBitmap> sourcelist = new List<WriteableBitmap> { };
            board_attack = new Storyboard();
            if (BattleOrder[battleturn] == 0)
            {
                sourcelist = Unit1Animation;
            }
            else
            {
                sourcelist = Unit2Animation;
            }
            double squaresize = SystemParameters.PrimaryScreenWidth / 32;
            // Source of the Person
            ObjectAnimationUsingKeyFrames animation4 = new ObjectAnimationUsingKeyFrames();
            Storyboard.SetTarget(animation4, guy);
            Storyboard.SetTargetProperty(animation4, new PropertyPath(Image.SourceProperty));
            animation4.BeginTime = TimeSpan.FromSeconds(0);
            if (BattleList[battleturn][1] == 0) // no crit
            {
                int delay = 0;
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[1], TimeSpan.FromSeconds(1)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[2], TimeSpan.FromSeconds(1.025)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[3], TimeSpan.FromSeconds(1.05)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[4], TimeSpan.FromSeconds(1.075)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[5], TimeSpan.FromSeconds(1.1)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[6], TimeSpan.FromSeconds(1.125)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[7], TimeSpan.FromSeconds(1.15)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[8], TimeSpan.FromSeconds(1.175)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[9], TimeSpan.FromSeconds(1.2)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[10], TimeSpan.FromSeconds(1.225)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[11], TimeSpan.FromSeconds(1.25)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[12], TimeSpan.FromSeconds(1.275)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[13], TimeSpan.FromSeconds(1.3)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[14], TimeSpan.FromSeconds(1.325)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[15], TimeSpan.FromSeconds(1.35)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[16], TimeSpan.FromSeconds(1.375)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[17], TimeSpan.FromSeconds(1.4)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[18], TimeSpan.FromSeconds(1.425)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[19], TimeSpan.FromSeconds(1.45)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[20], TimeSpan.FromSeconds(1.475)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[21], TimeSpan.FromSeconds(1.5)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[22], TimeSpan.FromSeconds(1.525)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[23], TimeSpan.FromSeconds(1.55)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[24], TimeSpan.FromSeconds(1.575)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[25], TimeSpan.FromSeconds(1.6)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[26], TimeSpan.FromSeconds(1.625)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[27], TimeSpan.FromSeconds(1.65)));
                if (BattleList[battleturn][0] == 0)
                {
                    delay = DodgeAnimation(defender, guy2, x_direction * -1, 1.65);
                }
                else
                {
                    defender.health -= BattleList[battleturn][2];
                }
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[28], TimeSpan.FromSeconds(2 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[29], TimeSpan.FromSeconds(2.05 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[30], TimeSpan.FromSeconds(2.1 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[31], TimeSpan.FromSeconds(2.15 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[32], TimeSpan.FromSeconds(2.2 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[33], TimeSpan.FromSeconds(2.25 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[34], TimeSpan.FromSeconds(2.3 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[35], TimeSpan.FromSeconds(2.35 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[36], TimeSpan.FromSeconds(2.4 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[37], TimeSpan.FromSeconds(2.45 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[38], TimeSpan.FromSeconds(2.5 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[39], TimeSpan.FromSeconds(2.55 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[40], TimeSpan.FromSeconds(2.6 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[41], TimeSpan.FromSeconds(2.65 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[42], TimeSpan.FromSeconds(2.7 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[43], TimeSpan.FromSeconds(2.75 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[44], TimeSpan.FromSeconds(2.8 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[45], TimeSpan.FromSeconds(2.85 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[46], TimeSpan.FromSeconds(2.9 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[47], TimeSpan.FromSeconds(2.95 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[48], TimeSpan.FromSeconds(3 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[49], TimeSpan.FromSeconds(3.05 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[50], TimeSpan.FromSeconds(3.1 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[51], TimeSpan.FromSeconds(3.15 + delay)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[0], TimeSpan.FromSeconds(3.2 + delay)));
            }
            else
            {
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[0], TimeSpan.FromSeconds(0.8)));
                animation4.KeyFrames.Add(new DiscreteObjectKeyFrame(sourcelist[1], TimeSpan.FromSeconds(5)));
            }
            animation4.Completed += new EventHandler(BattleAnimationfinished);

            board_attack.Children.Add(animation4);
            board_attack.Begin();
        }
        private void BattleAnimationfinished(object sender, EventArgs e)
        {
            battleturn++;
            if(BattleParticipants[0].health <= 0)
            {
                deathanimation(BattleParticipants[0].char_im);
                CurrentUnits.Remove(BattleParticipants[0]);
                battleturn = 0;
                Unit1Animation.Clear();
                Unit2Animation.Clear();
                BattleList.Clear();
                BattleOrder.Clear();
                battle_occuring = false;
                BattleParticipants.Clear();
                ClearBattlePics();
                Wait(selected);
            }
            else if(BattleParticipants[1].health <= 0)
            {
                deathanimation(BattleParticipants[1].char_im);
                CurrentUnits.Remove(BattleParticipants[1]);
                battleturn = 0;
                Unit1Animation.Clear();
                Unit2Animation.Clear();
                BattleList.Clear();
                BattleOrder.Clear();
                battle_occuring = false;
                BattleParticipants.Clear();
                ClearBattlePics();
                Wait(selected);
            }
            else if (battleturn < BattleList.Count)
            {
                BattleAnimation();
            }
            else
            {
                //battle over check if characters have died and then clear them
                battleturn = 0;
                Unit1Animation.Clear();
                Unit2Animation.Clear();
                BattleList.Clear();
                BattleOrder.Clear();
                battle_occuring = false;
                animation_occuring = false;
                BattleParticipants.Clear();
                ClearBattlePics();
                Wait(selected);
               // end_turn();
            }
        }

        //death animation
        private void deathanimation(Image guy)
        {
            board_attack = new Storyboard();
            DoubleAnimation animation4 = new DoubleAnimation();
            Storyboard.SetTarget(animation4, guy);
            Storyboard.SetTargetProperty(animation4, new PropertyPath(Image.OpacityProperty));
            animation4.BeginTime = TimeSpan.FromSeconds(0);
            animation4.Completed += new EventHandler(deathanimationfinished);
            animation4.From = 1.5;
            animation4.To = 0;
            animation4.Duration = TimeSpan.FromSeconds(1);
            board_attack.Children.Add(animation4);
            board_attack.Begin();
        }
        private void deathanimationfinished(object sender, EventArgs e)
        {
            Grid_Main.Children.Remove(gone);
            gone = null;
            animation_occuring = false;
            end_turn();
        }

        //pathfinding
        private void Pathfinding(Unit2 guy, int destx, int desty)
        {
            ClearColouredSpots();
            pathfinderwrapper(guy, destx, desty);
            CleanUpList(destx, desty);
            get_shortest_path();
            flush_pathlist();
            if(guy.x_pos == destx && guy.y_pos == desty)
            {
                ClearColouredSpots();
                Battle(BattleParticipants[0], BattleParticipants[1]);
                BattleSetup(BattleParticipants[0], BattleParticipants[1]);
                return;
            }
            if (Path[Path.Count - 1].x < guy.x_pos) // move left
            {
                clear_furthest_parent();
                Move(1, guy);
            }
            else if (Path[Path.Count - 1].x > guy.x_pos) // move right
            {
                clear_furthest_parent();
                Move(3, guy);
            }
            else if (Path[Path.Count - 1].y < guy.y_pos) // move up
            {
                clear_furthest_parent();
                Move(2, guy);
            }
            else if (Path[Path.Count - 1].y > guy.y_pos) // move down
            {
                clear_furthest_parent();
                Move(0, guy);
            }
        }

        private Node get_shortest_path()
        {
            Node a = Path[0];
            foreach(Node n in Path)
            {
                if(a.move < n.move)
                {
                    a = n;
                }
            }
            Path.Clear();
            Path.Add(a);
            return a;
        }
        private void flush_pathlist()
        {
            List<Node> path2 = new List<Node> { };
            for(int x = 0; x < Path.Count; x++)
            {
                if(Path[x].parent != null)
                {
                    Path.Add(Path[x].parent);
                }
            }
            clear_furthest_parent();
            clear_furthest_parent();
        }
        private void clear_furthest_parent()
        {
            Path.Remove(Path[Path.Count - 1]);
        }
        private void CleanUpList(int destx, int desty)
        {
            List<Node> Path2 = new List<Node> { };
            foreach(Node n in Path)
            {
                if(n.x == destx && n.y == desty)
                {
                    Path2.Add(n);
                }
            }
            Path = Path2;
        }

        private void pathfinderwrapper(Unit2 guy, int destx, int desty)
        {
            int startx = guy.x_pos;
            int starty = guy.y_pos;
            int movement = guy.unit_class.movement;
            Node first = new Node
            {
                move = movement,
                x = startx,
                y = starty
            };
            PathfindingHelper(guy, movement, startx, starty, 1, first, destx, desty);
            PathfindingHelper(guy, movement, startx, starty, 2, first, destx, desty);
            PathfindingHelper(guy, movement, startx, starty, 3, first, destx, desty);
            PathfindingHelper(guy, movement, startx, starty, 4, first, destx, desty);
            Path.Add(first);
        }
        private void PathfindingHelper(Unit2 guy, int move_remain, int x, int y, int priordirection, Node helper, int destx, int desty)
        {
            int cost = 0;
            Node first = new Node
            {
                move = move_remain
            };
            first.parent = helper;
            first.x = x;
            first.y = y;
            if(Math.Sqrt(Math.Pow((x - destx), 2.0) - Math.Pow((y - desty), 2.0)) > move_remain)
            {
                return;
            }
            Path.Add(first);
            //0 = no prior direction, 1 = up, 2 = right, 3 = down, 4 = left
            //four functions
            //PathfindingHelper(guy, move_remain - Move_Cost(guy, x + 1, y), x + 1, y, 4, first);
            //PathfindingHelper(guy, move_remain - Move_Cost(guy, x - 1, y), x - 1, y, 2, first);
            //PathfindingHelper(guy, move_remain - Move_Cost(guy, x, y + 1), x, y + 1, 1, first);
            //PathfindingHelper(guy, move_remain - Move_Cost(guy, x, y - 1), x, y - 1, 3, first);
            if (priordirection != 3)
            {
                if (y - map_y + 1 < map_height)
                {
                    cost = Move_Cost(guy, x, y + 1);
                    if (cost == 200)
                    {
                        PathfindingHelper(guy, move_remain - 1, x, y + 1, 1, first, destx, desty);
                    }
                    else if (cost <= move_remain)
                    {
                        PathfindingHelper(guy, move_remain - cost, x, y + 1, 1, first, destx, desty);
                    }
                }
            }
            if (priordirection != 2)
            {
                if(x - map_x + 1 < map_width)
                {
                    cost = Move_Cost(guy, x + 1, y);
                    if(cost == 200)
                    {
                        PathfindingHelper(guy, move_remain - 1, x + 1, y, 4, first, destx, desty);
                    }
                    else if(cost <= move_remain)
                    {
                        PathfindingHelper(guy, move_remain - cost, x + 1, y, 4, first, destx, desty);
                    }
                }
            }
            if (priordirection != 1)
            {
                if(y - map_y - 1 >= 0)
                {
                    cost = Move_Cost(guy, x, y - 1);
                    if(cost == 200)
                    {
                        PathfindingHelper(guy, move_remain - 1, x, y - 1, 3, first, destx, desty);
                    }
                    else if(cost <= move_remain)
                    {
                        PathfindingHelper(guy, move_remain - cost, x, y - 1, 3, first, destx, desty);
                    }
                }
            }
            if (priordirection != 4)
            {
                if(x - map_x - 1 >= 0)
                {
                    cost = Move_Cost(guy, x - 1, y);
                    if(cost == 200)
                    {
                        PathfindingHelper(guy, move_remain - 1, x - 1, y, 2, first, destx, desty);
                    }
                    else if(cost <= move_remain)
                    {
                        PathfindingHelper(guy, move_remain - cost, x - 1, y, 2, first, destx, desty);
                    }
                }
            }
        }


        //palette swapping, can only be done on smaller images, images are then scaled to be larger
        private byte[] Bitmapsourcetoarray(BitmapSource bs)
        {
            int stride = (int)bs.PixelWidth * (bs.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[(int)bs.PixelHeight * stride];
            bs.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        private WriteableBitmap Arraytobitmapsource(byte[] pixels, int width, int height)
        {
            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * bitmap.Format.BitsPerPixel / 8, 0);
            return bitmap;
        }

        private WriteableBitmap ColourShiftedSource(string original_image, string palette1source, string palette2source, int width, int height)
        {
            byte[] pixels = Bitmapsourcetoarray((new BitmapImage(new Uri(original_image))));
            byte[] palette1 = Bitmapsourcetoarray((new BitmapImage(new Uri(palette1source))));
            byte[] palette2 = Bitmapsourcetoarray((new BitmapImage(new Uri(palette2source))));

            for (int i = 0; i < pixels.Length / 4; i++)
            {
                byte b = pixels[i * 4];
                byte g = pixels[i * 4 + 1];
                byte r = pixels[i * 4 + 2];

                for (int x = 0; x < palette1.Length / 4; x++)
                {
                    byte b2 = palette1[x * 4];
                    byte g2 = palette1[x * 4 + 1];
                    byte r2 = palette1[x * 4 + 2];

                    if (b == b2 && g == g2 && r == r2)
                    {
                        pixels[i * 4] = palette2[x * 4];
                        pixels[i * 4 + 1] = palette2[x * 4 + 1];
                        pixels[i * 4 + 2] = palette2[x * 4 + 2];
                    }
                }
            }
            return (Arraytobitmapsource(pixels, width, height));
        }

        // clears current screens images and labels, create images and labels, scaled to be correct size, enlarged by nearest neighbour property
        private void ClearCurrentScreenImages()
        {
            int x = CurrentScreenImages.Count;
            for (int y = 0; y < x; y++)
            {
                if(!string.IsNullOrEmpty(CurrentScreenImages[y].Name))
                {
                    CurrentScreenImages[y].UnregisterName(CurrentScreenImages[y].Name);
                }
                Grid_Main.Children.Remove(CurrentScreenImages[y]);
            }
            CurrentScreenImages.Clear();
        }
        private void ClearListImages(List<Image> a)
        {
            int x = a.Count;
            for(int y = 0; y < x; y++)
            {
                if(!string.IsNullOrEmpty(a[y].Name))
                {
                    a[y].UnregisterName(a[y].Name);
                }
                Grid_Main.Children.Remove(a[y]);
            }
            a.Clear();
        }
        private void ClearCurmapGrid()
        {
            int x = CurmapGrid.Count;
            for(int y = 0; y < x; y++)
            {
                if(!string.IsNullOrEmpty(CurmapGrid[y].Name))
                {
                    CurmapGrid[y].UnregisterName(CurmapGrid[y].Name);
                }
                Grid_Main.Children.Remove(CurmapGrid[y]);
            }
            CurmapGrid.Clear();
        }
        private void ClearCurrentScreenLabels()
        {
            int x = CurrentScreenLabels.Count;
            for (int y = 0; y < x; y++)
            {
                if(!string.IsNullOrEmpty(CurrentScreenLabels[y].Name))
                {
                    CurrentScreenLabels[y].UnregisterName(CurrentScreenLabels[y].Name);
                }
                Grid_Main.Children.Remove(CurrentScreenLabels[y]);
            }
            CurrentScreenLabels.Clear();
        }
        private void ClearListLabels(List<Label> a)
        {
            int x = a.Count;
            for (int y = 0; y < x; y++)
            {
                if (!string.IsNullOrEmpty(a[y].Name))
                {
                    a[y].UnregisterName(a[y].Name);
                }
                Grid_Main.Children.Remove(a[y]);
            }
            a.Clear();
        }
        private void ClearMenuLabels()
        {
            int x = MenuLabels.Count;
            for (int y = 0; y < x; y++)
            {
                if (!string.IsNullOrEmpty(MenuLabels[y].Name))
                {
                    MenuLabels[y].UnregisterName(MenuLabels[y].Name);
                }
                Grid_Main.Children.Remove(MenuLabels[y]);
            }
            MenuLabels.Clear();
        }
        private void ClearCurrentTeamUnits()
        {
            int x = CurrentUnits.Count;
            foreach(Unit2 guy in CurrentUnits)
            {
                if(!string.IsNullOrEmpty(guy.char_im.Name))
                {
                    guy.char_im.UnregisterName(guy.char_im.Name);
                }
                Grid_Main.Children.Remove(guy.char_im);
            }
            CurrentUnits.Clear();
        }
        private void ClearColouredSpots()
        {
            int x = ColouredSpots.Count;
            for (int y = 0; y < x; y++)
            {
                if (!string.IsNullOrEmpty(ColouredSpots[y].Name))
                {
                    ColouredSpots[y].UnregisterName(ColouredSpots[y].Name);
                }
                Grid_Main.Children.Remove(ColouredSpots[y]);
            }
            ColouredSpots.Clear();
        }
        private void ClearInfoImages()
        {
            int x = InfoImages.Count;
            for(int y = 0; y < x; y++)
            {
                if(!string.IsNullOrEmpty(InfoImages[y].Name))
                {
                    InfoImages[y].UnregisterName(InfoImages[y].Name);
                }
                Grid_Main.Children.Remove(InfoImages[y]);
            }
            InfoImages.Clear();
        }
        private void ClearBattlePics()
        {
            int x = BattlePics.Count;
            for(int y = 0; y < x; y++)
            {
                if(!string.IsNullOrEmpty(BattlePics[y].Name))
                {
                    BattlePics[y].UnregisterName(BattlePics[y].Name);
                }
                Grid_Main.Children.Remove(BattlePics[y]);
            }
            BattlePics.Clear();
        }

        private Unit2 convertUnit_toUnit2(Unit guy1)
        {
            Unit2 guy2 = new Unit2
            {
                name = guy1.name,
                unit_class = guy1.unit_class,
                experience = guy1.experience,
                relationship_bonus = guy1.relationship_bonus,
                max_hp = guy1.max_hp,
                health = guy1.health,
                hp_growth = guy1.hp_growth,
                strength = guy1.strength,
                strength_growth = guy1.strength_growth,
                strength_bonus = guy1.strength_bonus,
                defence = guy1.defence,
                defence_growth = guy1.defence_growth,
                defence_bonus = guy1.defence_bonus,
                magic = guy1.magic,
                magic_growth = guy1.magic_growth,
                magic_bonus = guy1.magic_bonus,
                resistance = guy1.resistance,
                resistance_growth = guy1.resistance_growth,
                resistance_bonus = guy1.resistance_bonus,
                speed = guy1.speed,
                speed_growth = guy1.speed_growth,
                speed_bonus = guy1.speed_bonus,
                agility = guy1.agility,
                agility_growth = guy1.agility_growth,
                agility_bonus = guy1.agility_bonus,
                skill = guy1.skill,
                skill_growth = guy1.skill_growth,
                skill_bonus = guy1.skill_bonus,
                trickiness = guy1.trickiness,
                trickiness_growth = guy1.trickiness_growth,
                trickiness_bonus = guy1.trickiness_bonus,
                fortune = guy1.fortune,
                fortune_growth = guy1.fortune_growth,
                fortune_bonus = guy1.fortune_bonus,
                weapon1 = guy1.weapon1,
                weapon2 = guy1.weapon2,
                inventory1 = guy1.inventory1,
                inventory2 = guy1.inventory2,
                x_pos = guy1.x_pos,
                y_pos = guy1.y_pos,
                status = guy1.status,
            };

            return guy2;
        }

        private Image Create_Image(int x_span, int y_span, string source, int grid_x, int grid_y)
        {
           // double grid_size = System.Windows.SystemParameters.PrimaryScreenHeight / 18;
            Image pic = new Image();
            int squaresize = Convert.ToInt32(SystemParameters.PrimaryScreenHeight) / 18;
            pic.Source = (new BitmapImage(new Uri(source)));
            pic.Height = squaresize * y_span;
            pic.Width = squaresize * x_span;
            pic.Stretch.Equals(Stretch.None);
            RenderOptions.SetBitmapScalingMode(pic, BitmapScalingMode.NearestNeighbor);
            ScaleTransform transform = new ScaleTransform();
            double y = SystemParameters.PrimaryScreenHeight / squaresize;
            double x = SystemParameters.PrimaryScreenWidth / squaresize;
            transform.ScaleX = x / 32;
            transform.ScaleY = y / 18;
            pic.RenderTransform = transform;
            pic.HorizontalAlignment.Equals(HorizontalAlignment.Center);
            pic.VerticalAlignment.Equals(VerticalAlignment.Center);
            Grid.SetRow(pic, grid_y);
            Grid.SetColumn(pic, grid_x);
            Grid.SetColumnSpan(pic, x_span);
            Grid.SetRowSpan(pic, y_span);
            Grid.SetZIndex(pic, 0);
            pic.RenderTransformOrigin = new Point(0.5, 0.5);
            Grid_Main.Children.Add(pic);

            return pic;
        }
        private Image Create_battle_image(int x_span, int y_span, WriteableBitmap source, int grid_x, int grid_y, bool inverted)
        {
            Image pic = new Image();
            int squaresize = Convert.ToInt32(SystemParameters.PrimaryScreenHeight) / 18;
            pic.Source = source;
            pic.Height = squaresize * y_span;
            pic.Width = squaresize * x_span;
            pic.Stretch.Equals(Stretch.None);
            RenderOptions.SetBitmapScalingMode(pic, BitmapScalingMode.NearestNeighbor);
            ScaleTransform transform = new ScaleTransform();
            double y = SystemParameters.PrimaryScreenHeight / squaresize;
            double x = SystemParameters.PrimaryScreenWidth / squaresize;
            if (inverted)
            {
                transform.ScaleX = x / 32 * -1;
            }
            else
            {
                transform.ScaleX = x / 32;
            }
            transform.ScaleY = y / 18;
            pic.RenderTransform = transform;
            pic.HorizontalAlignment.Equals(HorizontalAlignment.Center);
            pic.VerticalAlignment.Equals(VerticalAlignment.Center);
            Grid.SetRow(pic, grid_y);
            Grid.SetColumn(pic, grid_x);
            Grid.SetColumnSpan(pic, x_span);
            Grid.SetRowSpan(pic, y_span);
            Grid.SetZIndex(pic, 0);
            pic.RenderTransformOrigin = new Point(0.5, 0.5);
            Grid_Main.Children.Add(pic);

            return pic;
        }

        private Image Create_InfoImage(int x_span, int y_span, int grid_x, int grid_y, string source)
        {
            Image pic = new Image();
            pic.Source = new BitmapImage(new Uri(source));
            pic.Height = 1;
            pic.Width = 10;
            pic.Stretch.Equals(Stretch.None);
            RenderOptions.SetBitmapScalingMode(pic, BitmapScalingMode.NearestNeighbor);
            ScaleTransform transform = new ScaleTransform();
            double y = System.Windows.SystemParameters.PrimaryScreenHeight / 1;
            double x = System.Windows.SystemParameters.PrimaryScreenWidth / 1;
            transform.ScaleX = x * x_span / 32;
            transform.ScaleY = y * y_span / 18;
            pic.RenderTransform = transform;
            pic.HorizontalAlignment.Equals(HorizontalAlignment.Center);
            pic.VerticalAlignment.Equals(VerticalAlignment.Center);
            Grid.SetRow(pic, grid_y);
            Grid.SetColumn(pic, grid_x);
            Grid.SetColumnSpan(pic, x_span);
            Grid.SetRowSpan(pic, y_span);
            pic.RenderTransformOrigin = new Point(0.5, 0.5);
            Grid_Main.Children.Add(pic);
            Grid.SetZIndex(pic, 10);

            return pic;
        }

        private Image Create_Character_Image(int x_span, int y_span, string source_name, string palette, int grid_x, int grid_y, string nm)
        {
            Image pic = new Image();
            int squaresize = Convert.ToInt32(SystemParameters.PrimaryScreenHeight) / 18;
            pic.Source = ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", source_name, "Map1.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", palette, 30, 30);
            pic.Height = squaresize * y_span;
            pic.Width = squaresize * x_span;
            pic.Name = nm;
            pic.Stretch.Equals(Stretch.None);
            RenderOptions.SetBitmapScalingMode(pic, BitmapScalingMode.NearestNeighbor);
            ScaleTransform transform = new ScaleTransform();
            double y = SystemParameters.PrimaryScreenHeight / squaresize;
            double x = SystemParameters.PrimaryScreenWidth / squaresize;
            transform.ScaleX = x / 32;
            transform.ScaleY = y / 18;
            pic.RenderTransform = transform;
            pic.HorizontalAlignment.Equals(HorizontalAlignment.Center);
            pic.VerticalAlignment.Equals(VerticalAlignment.Center);
            Grid.SetRow(pic, grid_y);
            Grid.SetColumn(pic, grid_x);
            Grid.SetColumnSpan(pic, x_span);
            Grid.SetRowSpan(pic, y_span);
            Grid.SetZIndex(pic, 2);
            pic.RenderTransformOrigin = new Point(0.5, 0.5);
            Grid_Main.Children.Add(pic);
            //pic.RegisterName(nm, pic);
            var animation = new ObjectAnimationUsingKeyFrames();
            animation.BeginTime = TimeSpan.FromSeconds(0);
            animation.RepeatBehavior = RepeatBehavior.Forever;
            animation.AutoReverse = true;
            board = new Storyboard();
            Storyboard.SetTarget(animation, pic);
            Storyboard.SetTargetName(animation, nm);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Image.SourceProperty));
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", source_name, "Map1.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", palette, 30, 30),
                KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))));
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", source_name, "Map2.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", palette, 30, 30),
                KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))));
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", source_name, "Map1.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", palette, 30, 30),
                KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))));
            board.Children.Add(animation);
            board.Begin(this, true);

            return pic;
        }

        private Label Create_MainMenu_Label(int x_span, int y_span, string source, int grid_x, int grid_y, string content)
        {
            Label pic = new Label();
            int squaresize = Convert.ToInt32(SystemParameters.PrimaryScreenHeight) / 18;
            ImageBrush brush = new ImageBrush();
            RenderOptions.SetBitmapScalingMode(pic, BitmapScalingMode.NearestNeighbor);
            ScaleTransform transform = new ScaleTransform();
            brush.ImageSource = new BitmapImage(new Uri(source));
            pic.Background = brush;
            pic.Height = squaresize * y_span;
            pic.Width = squaresize * x_span;
            pic.HorizontalAlignment = HorizontalAlignment.Center;
            pic.VerticalAlignment = VerticalAlignment.Center;
            pic.HorizontalContentAlignment = HorizontalAlignment.Center;
            pic.VerticalContentAlignment = VerticalAlignment.Center;
            pic.FontStretch = FontStretches.Normal;
            pic.FontSize = SystemParameters.PrimaryScreenWidth / 60;
            Grid.SetColumn(pic, grid_x);
            Grid.SetRow(pic, grid_y);
            Grid.SetColumnSpan(pic, x_span);
            Grid.SetRowSpan(pic, y_span);
            double y = SystemParameters.PrimaryScreenHeight / squaresize;
            double x = SystemParameters.PrimaryScreenWidth / squaresize;
            transform.ScaleX = x / 32;
            transform.ScaleY = y / 18;
            pic.RenderTransformOrigin = new Point(0.5, 0.5);
            pic.RenderTransform = transform;
            Grid_Main.Children.Add(pic);
            pic.Content = content;
            pic.MouseEnter += new MouseEventHandler(MainMenuButtonMouseEnter);
            pic.MouseLeave += new MouseEventHandler(MainMenuButtonMouseLeave);
            pic.MouseLeftButtonDown += new MouseButtonEventHandler(MainMenuButtonMouseClick);

            return pic;
        }

        private Label Create_InfoTextLabel(int x_span, int y_span, int grid_x, int grid_y, string content, bool buff, bool left)
        {
            double squaresize = SystemParameters.PrimaryScreenWidth / 32;
            Label pic = new Label();
            ScaleTransform transform = new ScaleTransform();
            pic.Height = squaresize * y_span;
            pic.Width = squaresize * x_span;
            pic.FontSize = SystemParameters.PrimaryScreenWidth / 80;
            pic.Foreground = new SolidColorBrush(Colors.White);
            pic.HorizontalAlignment = HorizontalAlignment.Center;
            pic.VerticalAlignment = VerticalAlignment.Center;
            if (left)
            {
                pic.HorizontalContentAlignment = HorizontalAlignment.Left;
            }
            else
            {
                pic.HorizontalContentAlignment = HorizontalAlignment.Center;
            }
            pic.VerticalContentAlignment = VerticalAlignment.Center;
            Grid.SetColumn(pic, grid_x);
            Grid.SetRow(pic, grid_y);
            Grid.SetColumnSpan(pic, x_span);
            Grid.SetRowSpan(pic, y_span);
            Grid_Main.Children.Add(pic);
            Grid.SetZIndex(pic, 11);
            if (buff && Convert.ToInt32(content) > 0)
            {
                pic.Foreground = new SolidColorBrush(Colors.Green);
                pic.Content = content;
            }
            else if (buff)
            {
                pic.Foreground = new SolidColorBrush(Colors.Red);
                pic.Content = content;
            }
            else
            {
                pic.Content = content;
            }
            pic.MouseEnter += new MouseEventHandler(InfoTextMouseEnter);
            pic.MouseLeave += new MouseEventHandler(InfoTextMouseLeave);
            pic.MouseLeftButtonDown += new MouseButtonEventHandler(InfoTextMouseDown);

            return pic;
        }
        private Label Create_TextLabel(int x_span, int y_span, int grid_x, int grid_y, string content, bool buff, bool left, bool right)
        {
            double squaresize = SystemParameters.PrimaryScreenWidth / 32;
            Label pic = new Label();
            ScaleTransform transform = new ScaleTransform();
            pic.Height = squaresize;
            pic.Width = squaresize * x_span;
            pic.FontSize = System.Windows.SystemParameters.PrimaryScreenWidth / 80;
            pic.Foreground = new SolidColorBrush(Colors.White);
            pic.HorizontalAlignment = HorizontalAlignment.Center;
            pic.VerticalAlignment = VerticalAlignment.Center;
            if(left)
            {
                pic.HorizontalContentAlignment = HorizontalAlignment.Left;
            }
            else
            {
                pic.HorizontalContentAlignment = HorizontalAlignment.Center;
            }
            if(right)
            {
                pic.HorizontalContentAlignment = HorizontalAlignment.Right;
            }
            pic.VerticalContentAlignment = VerticalAlignment.Center;
            Grid.SetColumn(pic, grid_x);
            Grid.SetRow(pic, grid_y);
            Grid.SetColumnSpan(pic, x_span);
            Grid.SetRowSpan(pic, y_span);
            Grid_Main.Children.Add(pic);
            Grid.SetZIndex(pic, 11);
            if(buff && Convert.ToInt32(content) > 0)
            {
                pic.Foreground = new SolidColorBrush(Colors.Green);
                pic.Content = content;
            }
            else if(buff)
            {
                pic.Foreground = new SolidColorBrush(Colors.Red);
                pic.Content = content;
            }
            else
            {
                pic.Content = content;
            }

            return pic;
        }
        private Label Create_BattleTextLabel(int x_span, int y_span, int grid_x, int grid_y, string content, bool bottom)
        {
            double squaresize = SystemParameters.PrimaryScreenWidth / 32;
            Label pic = new Label();
            ScaleTransform transform = new ScaleTransform();
            pic.Height = squaresize;
            pic.Width = squaresize * x_span;
            pic.FontSize = System.Windows.SystemParameters.PrimaryScreenWidth / 80;
            pic.Foreground = new SolidColorBrush(Colors.White);
            pic.HorizontalAlignment = HorizontalAlignment.Center;
            pic.VerticalAlignment = VerticalAlignment.Center;
            pic.HorizontalContentAlignment = HorizontalAlignment.Center;
            pic.VerticalContentAlignment = VerticalAlignment.Top;
            if (bottom)
            {
                pic.VerticalContentAlignment = VerticalAlignment.Bottom;
            }
            Grid.SetColumn(pic, grid_x);
            Grid.SetRow(pic, grid_y);
            Grid.SetColumnSpan(pic, x_span);
            Grid.SetRowSpan(pic, y_span);
            Grid_Main.Children.Add(pic);
            Grid.SetZIndex(pic, 2);
            pic.Content = content;

            return pic;
        }

        // Load specific menu screens
        private void Load_main()
        {
            CurrentScreenImages.Add(Create_Image(32, 18, @"pack://siteoforigin:,,,/Resources/MainBackground.png", 0, 0));
            CurrentScreenLabels.Add(Create_MainMenu_Label(8, 2, @"pack://siteoforigin:,,,/Resources/Button.png", 12, 4, "Story Mode"));
            CurrentScreenLabels.Add(Create_MainMenu_Label(8, 2, @"pack://siteoforigin:,,,/Resources/Button.png", 12, 7, "Arcade"));
            CurrentScreenLabels.Add(Create_MainMenu_Label(8, 2, @"pack://siteoforigin:,,,/Resources/Button.png", 12, 10, "Test"));
            CurrentScreenLabels.Add(Create_MainMenu_Label(8, 2, @"pack://siteoforigin:,,,/Resources/Button.png", 12, 13, "Exit to Desktop"));
        }

        private void Load_Arcade()
        {
            CurrentScreenLabels.Add(Create_MainMenu_Label(4, 1, @"pack://siteoforigin:,,,/Resources/Button.png", 0, 17, "Main Menu"));
        }

        private void Load_Singleplayer()
        {
            CurrentScreenLabels.Add(Create_MainMenu_Label(4, 1, @"pack://siteoforigin:,,,/Resources/Button.png", 0, 17, "Main Menu"));
        }

        // Functions for Map creation & Gameplay

        //map creation
        private void Load_Map(string map1)
        {
            ClearCurrentScreenImages();
            ClearCurrentScreenLabels();
            string path1 = Directory.GetCurrentDirectory();
            string path2 = System.IO.Path.Combine(path1, string.Concat(map1, ".xml"));
            //specific for current map
            int starty = ((18 - 10) / 2);
            int startx = ((32 - 10) / 2);
            map_x = startx;
            map_y = starty;
            //
            CurrentScreenImages.Add(Create_Image(10, 10, @"pack://siteoforigin:,,,/Resources/TestMap.png", startx, starty));
            //Load teams into the map
            Load_team();
            if (File.Exists(path2))
            {
                var reader = new XmlSerializer(typeof(int[]));
                var helper = new FileStream(path2, FileMode.Open);
                curmap = (int[])reader.Deserialize(helper);
                helper.Close();
            }
            int width = curmap[0];
            map_width = width;
            int height = curmap[1];
            map_height = height;
            int z = curmap.Length;
            curmap_grid_ForTerrain = new int[z - 3];
            for(int t = 3; t < z; t++)
            {
                curmap_grid_ForTerrain[t - 3] = curmap[t];
            }
            for(int x = 0; x < height; x++)
            {
                Create_GridRow(width, x, height);
            }
            begin_turn();
        }

        private void Create_GridRow(int width, int rownum, int height)
        {
            int mapstarty = ((18 - height) / 2);
            int mapstartx = ((32 - width) / 2);
            for(int x = 0; x < width; x++)
            {
                CurmapGrid.Add(Create_MapGridSpot(x + mapstartx, rownum + mapstarty));
            }
        }

        private Image Create_MapGridSpot(int rownum, int y)
        {
            int squaresize = 1;
            Image pic = new Image();
            pic.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/MenuCursor.png"));
            pic.Opacity = 0;
            pic.Height = squaresize;
            pic.Width = squaresize;
            pic.Stretch.Equals(Stretch.None);
            RenderOptions.SetBitmapScalingMode(pic, BitmapScalingMode.NearestNeighbor);
            ScaleTransform transform = new ScaleTransform();
            double y1 = System.Windows.SystemParameters.PrimaryScreenHeight / squaresize;
            double x = System.Windows.SystemParameters.PrimaryScreenWidth / squaresize;
            transform.ScaleX = x / 32;
            transform.ScaleY = y1 / 18;
            pic.RenderTransform = transform;
            pic.HorizontalAlignment.Equals(HorizontalAlignment.Center);
            pic.VerticalAlignment.Equals(VerticalAlignment.Center);
            Grid.SetRow(pic, y);
            Grid.SetColumn(pic, rownum);
            Grid.SetColumnSpan(pic, 1);
            Grid.SetRowSpan(pic, 1);
            Grid.SetZIndex(pic, 3);
            pic.RenderTransformOrigin = new Point(0.5, 0.5);
            pic.MouseEnter += new MouseEventHandler(MapGridMouseEnter);
            pic.MouseLeave += new MouseEventHandler(MapGridMouseLeave);
            pic.MouseLeftButtonDown += new MouseButtonEventHandler(MapGridMouseClick);
            pic.MouseRightButtonDown += new MouseButtonEventHandler(MapGridMouseRightClick);
            Grid_Main.Children.Add(pic);

            return pic;
        }

        //function for loading teams into game
        private void Load_team()
        {
            //concrete for now, add fluidity later.
            string path1 = Directory.GetCurrentDirectory();
            string path2 = System.IO.Path.Combine(path1, "Ahron.xml");
            if (File.Exists(path2))
            {
                var reader = new XmlSerializer(typeof(Unit));
                var helper = new FileStream(path2, FileMode.Open);
                Unit expend = (Unit)reader.Deserialize(helper);
                CurrentUnits.Add(convertUnit_toUnit2(expend));
                helper.Close();
                helper = new FileStream(path2, FileMode.Open);
                expend = (Unit)reader.Deserialize(helper);
                CurrentUnits.Add(convertUnit_toUnit2(expend));
                helper.Close();
                helper = new FileStream(path2, FileMode.Open);
                expend = (Unit)reader.Deserialize(helper);
                CurrentUnits.Add(convertUnit_toUnit2(expend));
                CurrentUnits.Add(convertUnit_toUnit2(expend));
                CurrentUnits.Add(convertUnit_toUnit2(expend));
                CurrentUnits.Add(convertUnit_toUnit2(expend));
                CurrentUnits.Add(convertUnit_toUnit2(expend));
                helper.Close();
            }
            Weapon SilverSword = new Weapon
            {
                name = "Silver Sword",
                magical = false,
                power = 12,
                weight = 10,
                type = 0,
                ability = null,
                crit = 0,
                hit = 80
            };
            //change this stuff later
            CurrentUnits[0].char_im = (Create_Character_Image(1, 1, CurrentUnits[0].unit_class.animation_name, @"pack://siteoforigin:,,,/Resources/Palette1.png", map_x + 6, map_y + 9, "player1"));
            CurrentUnits[0].x_pos = map_x + 6;
            CurrentUnits[0].y_pos = map_y + 9;
            CurrentUnits[0].team = 0;
            CurrentUnits[0].unit_class.movement = 6;
            CurrentUnits[0].fortune = 20;
            CurrentUnits[0].skill = 15;
            CurrentUnits[0].max_hp = 35;
            CurrentUnits[0].health = 35;
            add_buffs(CurrentUnits[0]);

            CurrentUnits[1].name = "Red Soldier";
            CurrentUnits[1].char_im = (Create_Character_Image(1, 1, CurrentUnits[1].unit_class.animation_name, @"pack://siteoforigin:,,,/Resources/Palette7.png", map_x + 1, map_y + 1, "player2"));
            CurrentUnits[1].x_pos = map_x + 1;
            CurrentUnits[1].y_pos = map_y + 1;
            CurrentUnits[1].team = 1;
            CurrentUnits[1].fortune = 20;
            CurrentUnits[1].skill = 15;
            CurrentUnits[1].speed = 20;
            CurrentUnits[1].trickiness = 10;
            CurrentUnits[1].inventory2 = null;
            CurrentUnits[1].weapon2 = null;
            CurrentUnits[1].weapon1 = SilverSword;
            CurrentUnits[1].agility = 9;
            CurrentUnits[1].max_hp = 22;
            CurrentUnits[1].health = 22;
            add_buffs(CurrentUnits[1]);

            // CurrentUnits[2].unit_class.animation_name = "LanceSoldier";
            CurrentUnits[2].name = "Soldier";
            CurrentUnits[2].char_im = (Create_Character_Image(1, 1, CurrentUnits[2].unit_class.animation_name, @"pack://siteoforigin:,,,/Resources/Palette1.png", map_x + 7, map_y + 9, "player3"));
            CurrentUnits[2].x_pos = map_x + 7;
            CurrentUnits[2].y_pos = map_y + 9;
            CurrentUnits[2].team = 0;
            CurrentUnits[2].unit_class.movement = 6;
            CurrentUnits[2].fortune = 20;
            CurrentUnits[2].skill = 15;
            CurrentUnits[2].inventory1 = null;
            CurrentUnits[2].inventory2 = null;
            CurrentUnits[2].weapon2 = null;
            add_buffs(CurrentUnits[2]);

            // CurrentUnits[3].unit_class.animation_name = "LanceSoldier";
            CurrentUnits[3].name = "Red Soldier";
            CurrentUnits[3].char_im = (Create_Character_Image(1, 1, CurrentUnits[2].unit_class.animation_name, @"pack://siteoforigin:,,,/Resources/Palette7.png", map_x + 2, map_y + 5, "player3"));
            CurrentUnits[3].x_pos = map_x + 2;
            CurrentUnits[3].y_pos = map_y + 5;
            CurrentUnits[3].team = 1;
            CurrentUnits[3].unit_class.movement = 6;
            CurrentUnits[3].fortune = 20;
            CurrentUnits[3].skill = 15;
            CurrentUnits[3].inventory1 = null;
            CurrentUnits[3].inventory2 = null;
            CurrentUnits[3].weapon2 = null;
            add_buffs(CurrentUnits[3]);

            CurrentUnits[4].name = "Red Soldier";
            CurrentUnits[4].char_im = (Create_Character_Image(1, 1, CurrentUnits[2].unit_class.animation_name, @"pack://siteoforigin:,,,/Resources/Palette7.png", map_x + 2, map_y + 2, "player3"));
            CurrentUnits[4].x_pos = map_x + 2;
            CurrentUnits[4].y_pos = map_y + 2;
            CurrentUnits[4].team = 1;
            CurrentUnits[4].unit_class.movement = 3;
            CurrentUnits[4].fortune = 20;
            CurrentUnits[4].skill = 15;
            CurrentUnits[4].inventory1 = null;
            CurrentUnits[4].inventory2 = null;
            CurrentUnits[4].weapon2 = null;
            CurrentUnits[4].strength = 8;
            CurrentUnits[4].defence = 1;
            add_buffs(CurrentUnits[4]);

            CurrentUnits[5].name = "Red Soldier";
            CurrentUnits[5].char_im = (Create_Character_Image(1, 1, CurrentUnits[2].unit_class.animation_name, @"pack://siteoforigin:,,,/Resources/Palette7.png", map_x, map_y + 2, "player3"));
            CurrentUnits[5].x_pos = map_x;
            CurrentUnits[5].y_pos = map_y + 2;
            CurrentUnits[5].team = 1;
            CurrentUnits[5].unit_class.movement = 3;
            CurrentUnits[5].fortune = 20;
            CurrentUnits[5].skill = 15;
            CurrentUnits[5].inventory1 = null;
            CurrentUnits[5].inventory2 = null;
            CurrentUnits[5].weapon2 = null;
            CurrentUnits[5].speed = 8;
            CurrentUnits[5].agility = 2;
            add_buffs(CurrentUnits[5]);

            CurrentUnits[6].name = "Red Soldier";
            CurrentUnits[6].char_im = (Create_Character_Image(1, 1, CurrentUnits[2].unit_class.animation_name, @"pack://siteoforigin:,,,/Resources/Palette7.png", map_x + 8, map_y + 7, "player3"));
            CurrentUnits[6].x_pos = map_x + 8;
            CurrentUnits[6].y_pos = map_y + 7;
            CurrentUnits[6].team = 1;
            CurrentUnits[6].unit_class.movement = 5;
            CurrentUnits[6].fortune = 20;
            CurrentUnits[6].skill = 15;
            CurrentUnits[6].inventory1 = null;
            CurrentUnits[6].inventory2 = null;
            CurrentUnits[6].weapon2 = null;
            CurrentUnits[6].strength = 1;
            CurrentUnits[6].speed = 2;
            CurrentUnits[6].agility = 2;
            add_buffs(CurrentUnits[6]);
        }

        //functions for working with units on the map
        private Unit2 playeratcoord (int x, int y)
        {
            foreach(Unit2 guy in CurrentUnits)
            {
                if(guy.x_pos == x && guy.y_pos == y)
                {
                    return guy;
                }
            }
            return null;
        }

        //Functions for moving and fighting
        private void onerangehighlight(Unit2 guy, int x, int y)
        {
            Unit2 person = null;
            if (y - 1 >= map_y)
            {
                person = playeratcoord(x, y - 1);
                if (!colouredspotexists(x, y - 1, false) && (person == null || person.team != guy.team))
                {
                    ColouredSpots.Add(Create_MapColouredSpot(x, y - 1, false));
                }
            }
            if (y + 1 < map_y + map_height)
            {
                person = playeratcoord(x, y + 1);
                if (!colouredspotexists(x, y + 1, false) && (person == null || person.team != guy.team))
                {
                    ColouredSpots.Add(Create_MapColouredSpot(x, y + 1, false));
                }
            }
            if (x - 1 >= map_x)
            {
                person = playeratcoord(x - 1, y);
                if (!colouredspotexists(x - 1, y, false) && (person == null || person.team != guy.team))
                {
                    ColouredSpots.Add(Create_MapColouredSpot(x - 1, y, false));
                }
            }
            if (x + 1 < map_x + map_width)
            {
                person = playeratcoord(x + 1, y);
                if (!colouredspotexists(x + 1, y, false) && (person == null || person.team != guy.team))
                {
                    ColouredSpots.Add(Create_MapColouredSpot(x + 1, y, false));
                }
            }
        }
        private void ActiveMovement(Unit2 guy)
        {
            if (guy == null) return;
            int startx = guy.x_pos;
            int starty = guy.y_pos;
            int movement = guy.unit_class.movement;
            ActiveMovementHelper(guy, movement, startx, starty, 0);
        }
        private void ActiveMovementHelper(Unit2 guy, int move_remain, int x, int y, int priordirection)
        {
            int cost = 0;
            //0 = no prior direction, 1 = up, 2 = right, 3 = down, 4 = left
            if(priordirection != 3)
            {
                if(y - map_y - 1 >= 0)
                {
                    cost = Move_Cost(guy, x, y - 1);
                    if(cost == 200)
                    {
                        if(move_remain > 1) ActiveMovementHelper(guy, move_remain - 1, x, y - 1, 1);
                    }
                    else if(cost <= move_remain)
                    {
                        if(!colouredspotexists(x, y - 1, true))
                        {
                            ColouredSpots.Add(Create_MapColouredSpot(x, y - 1, true));
                        }
                        ActiveMovementHelper(guy, move_remain - cost, x, y - 1, 1);
                    }
                    else
                    {
                        if (!colouredspotexists(x, y - 1, false))
                        {
                            ColouredSpots.Add(Create_MapColouredSpot(x, y - 1, false));
                        }
                    }
                }
            }
            if(priordirection != 2)
            {
                if(x - map_x + 1 < map_width)
                {
                    cost = Move_Cost(guy, x + 1, y);
                    if(cost == 200)
                    {
                        if (move_remain > 1) ActiveMovementHelper(guy, move_remain - 1, x + 1, y, 4);
                    }
                    else if(cost <= move_remain)
                    {
                        if (!colouredspotexists(x + 1, y, true))
                        {
                            ColouredSpots.Add(Create_MapColouredSpot(x + 1, y, true));
                        }
                        ActiveMovementHelper(guy, move_remain - cost, x + 1, y, 4);
                    }
                    else
                    {
                        if (!colouredspotexists(x + 1, y, false))
                        {
                            ColouredSpots.Add(Create_MapColouredSpot(x + 1, y, false));
                        }
                    }
                }
            }
            if (priordirection != 1)
            {
                if(y - map_y + 1 < map_height)
                {
                    cost = Move_Cost(guy, x, y + 1);
                    if(cost == 200)
                    {
                        if (move_remain > 1) ActiveMovementHelper(guy, move_remain - 1, x, y + 1, 3);
                    }
                    else if (cost <= move_remain)
                    {
                        if (!colouredspotexists(x, y + 1, true))
                        {
                            ColouredSpots.Add(Create_MapColouredSpot(x, y + 1, true));
                        }
                        ActiveMovementHelper(guy, move_remain - cost, x, y + 1, 3);
                    }
                    else
                    {
                        if (!colouredspotexists(x, y + 1, false))
                        {
                            ColouredSpots.Add(Create_MapColouredSpot(x, y + 1, false));
                        }
                    }
                }
            }
            if (priordirection != 4)
            {
                if(x - map_x - 1 >= 0)
                {
                    cost = Move_Cost(guy, x - 1, y);
                    if(cost == 200)
                    {
                        if (move_remain > 1) ActiveMovementHelper(guy, move_remain - 1, x - 1, y, 2);
                    }
                    else if (cost <= move_remain)
                    {
                        if (!colouredspotexists(x - 1, y, true))
                        {
                            ColouredSpots.Add(Create_MapColouredSpot(x - 1, y, true));
                        }
                        ActiveMovementHelper(guy, move_remain - cost, x - 1, y, 2);
                    }
                    else
                    {
                        if (!colouredspotexists(x - 1, y, false))
                        {
                            ColouredSpots.Add(Create_MapColouredSpot(x - 1, y, false));
                        }
                    }
                }
            }
        }
        private int Move_Cost(Unit2 guy, int x, int y)
        {
            Unit2 person = occupied_by_unit(x, y);
            if(guy != null && person != null)
            {
                if(person.team == guy.team)
                {
                    return 200;
                }
                return 100;
            }
            int z = posforcurcoord(x, y);
            if(z == 0)
            {
                return guy.unit_class.sand_move;
            }
            else if(z == 1)
            {
                return 1;
            }
            else if (z == 2)
            {
                return guy.unit_class.floor_move;
            }
            else if (z == 3)
            {
                return guy.unit_class.tree_move;
            }
            else if (z == 4)
            {
                return guy.unit_class.water_move;
            }
            return 100;
        }
        private void Load_Info(Unit2 guy)
        {
            InfoImages.Add(Create_InfoImage(12, 12, 10, 3, @"pack://siteoforigin:,,,/Resources/InfoBackground.png"));
            InfoImages.Add(Create_InfoImage(4, 4, 10, 3, String.Concat(@"pack://siteoforigin:,,,/Resources/", guy.name, "Portrait", ".png")));
            CurrentScreenLabels.Add(Create_TextLabel(4, 1, 16, 3, guy.name, false, false, false));
            CurrentScreenLabels.Add(Create_InfoTextLabel(4, 1, 16, 4, guy.unit_class.name, false, false));
            CurrentScreenLabels.Add(Create_InfoTextLabel(3, 1, 16, 5, "Level", false, false));
            CurrentScreenLabels.Add(Create_TextLabel(1, 1, 18, 5, Convert.ToString((guy.experience / 100) + 1), false, false, false));
            CurrentScreenLabels.Add(Create_InfoTextLabel(3, 1, 18, 7, "Experience", false, true));
            CurrentScreenLabels.Add(Create_TextLabel(1, 1, 21, 7, Convert.ToString(guy.experience % 100), false, false, false));
            CurrentScreenLabels.Add(Create_InfoTextLabel(3, 1, 14, 7, "Health", false, true));
            CurrentScreenLabels.Add(Create_TextLabel(2, 1, 16, 7, String.Concat(Convert.ToString(guy.health), "/", Convert.ToString(guy.max_hp)), false, false, true));

            CurrentScreenLabels.Add(Create_InfoTextLabel(3, 1, 14, 8, "Strength", false, true));
            if (guy.strength_bonus != 0)
            {
                CurrentScreenLabels.Add(Create_TextLabel(1, 1, 17, 8, Convert.ToString(guy.strength_bonus + guy.strength), true, false, false));
            }
            else CurrentScreenLabels.Add(Create_TextLabel(1, 1, 17, 8, Convert.ToString(guy.strength), false, false, false));
            CurrentScreenLabels.Add(Create_InfoTextLabel(3, 1, 18, 8, "Defence", false, true));
            if (guy.defence_bonus != 0)
            {
                CurrentScreenLabels.Add(Create_TextLabel(1, 1, 21, 8, Convert.ToString(guy.defence_bonus + guy.defence), true, false, false));
            }
            else CurrentScreenLabels.Add(Create_TextLabel(1, 1, 21, 8, Convert.ToString(guy.defence), false, false, false));

            CurrentScreenLabels.Add(Create_InfoTextLabel(3, 1, 14, 9, "Magic", false, true));
            if (guy.magic_bonus != 0)
            {
                CurrentScreenLabels.Add(Create_TextLabel(1, 1, 17, 9, Convert.ToString(guy.magic_bonus + guy.magic), true, false, false));
            }
            else CurrentScreenLabels.Add(Create_TextLabel(1, 1, 17, 9, Convert.ToString(guy.magic), false, false, false));
            CurrentScreenLabels.Add(Create_InfoTextLabel(3, 1, 18, 9, "Resistance", false, true));
            if (guy.resistance_bonus != 0)
            {
                CurrentScreenLabels.Add(Create_TextLabel(1, 1, 21, 9, Convert.ToString(guy.resistance_bonus + guy.resistance), true, false, false));
            }
            else CurrentScreenLabels.Add(Create_TextLabel(1, 1, 21, 9, Convert.ToString(guy.resistance), false, false, false));

            CurrentScreenLabels.Add(Create_InfoTextLabel(3, 1, 14, 10, "Speed", false, true));
            if (guy.speed_bonus != 0)
            {
                CurrentScreenLabels.Add(Create_TextLabel(1, 1, 17, 10, Convert.ToString(guy.speed_bonus + guy.speed), true, false, false));
            }
            else CurrentScreenLabels.Add(Create_TextLabel(1, 1, 17, 10, Convert.ToString(guy.speed), false, false, false));
            CurrentScreenLabels.Add(Create_InfoTextLabel(3, 1, 18, 10, "Agility", false, true));
            if (guy.agility_bonus != 0)
            {
                CurrentScreenLabels.Add(Create_TextLabel(1, 1, 21, 10, Convert.ToString(guy.agility_bonus + guy.agility), true, false, false));
            }
            else CurrentScreenLabels.Add(Create_TextLabel(1, 1, 21, 10, Convert.ToString(guy.agility), false, false, false));

            CurrentScreenLabels.Add(Create_InfoTextLabel(3, 1, 14, 11, "Skill", false, true));
            if (guy.skill_bonus != 0)
            {
                CurrentScreenLabels.Add(Create_TextLabel(1, 1, 17, 11, Convert.ToString(guy.skill_bonus + guy.skill), true, false, false));
            }
            else CurrentScreenLabels.Add(Create_TextLabel(1, 1, 17, 11, Convert.ToString(guy.skill), false, false, false));
            CurrentScreenLabels.Add(Create_InfoTextLabel(3, 1, 18, 11, "Trickiness", false, true));
            if (guy.trickiness_bonus != 0)
            {
                CurrentScreenLabels.Add(Create_TextLabel(1, 1, 21, 11, Convert.ToString(guy.trickiness_bonus + guy.trickiness), true, false, false));
            }
            else CurrentScreenLabels.Add(Create_TextLabel(1, 1, 21, 11, Convert.ToString(guy.trickiness), false, false, false));

            CurrentScreenLabels.Add(Create_InfoTextLabel(3, 1, 14, 12, "Fortune", false, true));
            if (guy.fortune_bonus != 0)
            {
                CurrentScreenLabels.Add(Create_TextLabel(1, 1, 17, 12, Convert.ToString(guy.fortune_bonus + guy.fortune), true, false, false));
            }
            else CurrentScreenLabels.Add(Create_TextLabel(1, 1, 17, 12, Convert.ToString(guy.fortune), false, false, false));

            if (guy.weapon1 != null) CurrentScreenLabels.Add(Create_InfoTextLabel(4, 1, 18, 12, guy.weapon1.name, false, true));

            if(guy.inventory1 != null) CurrentScreenLabels.Add(Create_InfoTextLabel(4, 1, 10, 8, guy.inventory1.name, false, false));
            if(guy.inventory2 != null) CurrentScreenLabels.Add(Create_InfoTextLabel(4, 1, 10, 9, guy.inventory2.name, false, false));
            if (guy.weapon2 != null) CurrentScreenLabels.Add(Create_InfoTextLabel(4, 1, 10, 10, guy.weapon2.name, false, false));
            //put characters skill here


            CurrentScreenLabels.Add(Create_InfoTextLabel(10, 2, 11, 13, "", false, true));
        }
        private void Load_BattleInfo(Unit2 attacker, Unit2 defender)
        {
            BattleInfoPics.Add(Create_Image(4, 7, @"pack://siteoforigin:,,,/Resources/BattleInfoBackground.png", 0, 5));
            BattleInfoLabel.Add(Create_BattleTextLabel(4, 1, 0, 5, attacker.name, false));
            BattleInfoLabel.Add(Create_BattleTextLabel(2, 1, 1, 6, "Health", false));
            BattleInfoLabel.Add(Create_BattleTextLabel(1, 1, 0, 6, Convert.ToString(defender.health), false));
            BattleInfoLabel.Add(Create_BattleTextLabel(1, 1, 3, 6, Convert.ToString(attacker.health), false));
            BattleInfoLabel.Add(Create_BattleTextLabel(2, 1, 1, 7, "Damage", false));
            BattleInfoLabel.Add(Create_BattleTextLabel(1, 1, 0, 7, Convert.ToString(Damage(defender, attacker)), false));
            BattleInfoLabel.Add(Create_BattleTextLabel(1, 1, 3, 7, Convert.ToString(Damage(attacker, defender)), false));
            BattleInfoLabel.Add(Create_BattleTextLabel(2, 1, 1, 8, "Hit", false));
            BattleInfoLabel.Add(Create_BattleTextLabel(1, 1, 0, 8, Convert.ToString(Hit(defender, attacker)), false));
            BattleInfoLabel.Add(Create_BattleTextLabel(1, 1, 3, 8, Convert.ToString(Hit(attacker, defender)), false));
            BattleInfoLabel.Add(Create_BattleTextLabel(2, 1, 1, 9, "Crit", false));
            BattleInfoLabel.Add(Create_BattleTextLabel(1, 1, 0, 9, Convert.ToString(Crit(defender, attacker)), false));
            BattleInfoLabel.Add(Create_BattleTextLabel(1, 1, 3, 9, Convert.ToString(Crit(attacker, defender)), false));
            BattleInfoLabel.Add(Create_BattleTextLabel(2, 1, 1, 10, "Double", false));
            BattleInfoLabel.Add(Create_BattleTextLabel(1, 1, 0, 10, Convert.ToString(Double(defender, attacker)), false));
            BattleInfoLabel.Add(Create_BattleTextLabel(1, 1, 3, 10, Convert.ToString(Double(attacker, defender)), false));
            BattleInfoLabel.Add(Create_BattleTextLabel(4, 1, 0, 11, defender.name, true));
        }
        //returns whether or not there is already a coloured spot at that location
        private bool colouredspotexists(int x, int y, bool blue)
        {
            foreach(Image spot in ColouredSpots)
            {
                if(Grid.GetRow(spot) == y && Grid.GetColumn(spot) == x)
                {
                    if(blue)
                    {
                        spot.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/BlueTile.png"));
                    }
                    return true;
                }
            }
            return false;
        }
        private bool bluespotexists(int x, int y)
        {
            foreach(Image spot in ColouredSpots)
            {
                if(Grid.GetRow(spot) == y && Grid.GetColumn(spot) == x)
                {
                    if (String.Equals(Convert.ToString(spot.Source), Convert.ToString(new BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/BlueTile.png")))))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private Unit2 occupied_by_unit(int x, int y)
        {
            foreach(Unit2 guy in CurrentUnits)
            {
                if(guy.x_pos == x && guy.y_pos == y)
                {
                    return guy;
                }
            }
            return null;
        }
        private Image Create_MapColouredSpot(int x, int y, bool blue)
        {
            int squaresize = 1;
            Image pic = new Image();
            if(blue)
            {
                pic.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/BlueTile.png"));
            }
            else
            {
                pic.Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/RedTile.png"));
            }
            pic.Height = squaresize;
            pic.Width = squaresize;
            pic.Stretch.Equals(Stretch.None);
            RenderOptions.SetBitmapScalingMode(pic, BitmapScalingMode.NearestNeighbor);
            ScaleTransform transform = new ScaleTransform();
            double y1 = System.Windows.SystemParameters.PrimaryScreenHeight / squaresize;
            double x1 = System.Windows.SystemParameters.PrimaryScreenWidth / squaresize;
            transform.ScaleX = x1 / 32;
            transform.ScaleY = y1 / 18;
            pic.RenderTransform = transform;
            pic.HorizontalAlignment.Equals(HorizontalAlignment.Center);
            pic.VerticalAlignment.Equals(VerticalAlignment.Center);
            Grid.SetRow(pic, y);
            Grid.SetColumn(pic, x);
            Grid.SetColumnSpan(pic, 1);
            Grid.SetRowSpan(pic, 1);
            Grid.SetZIndex(pic, 1);
            pic.RenderTransformOrigin = new Point(0.5, 0.5);
            Grid_Main.Children.Add(pic);

            return pic;
        }

        //Functions for battle
        private int Hit(Unit2 attacker, Unit2 defender)
        {
            return Math.Min(Math.Max(((attacker.skill + attacker.skill_bonus) - (defender.trickiness + defender.trickiness_bonus))
                * 5 + attacker.weapon1.hit - 20 - tiledefenceconvert(posforcurcoord(defender.x_pos, defender.y_pos)), 0), 100);
        }
        private int Crit(Unit2 attacker, Unit2 defender)
        {
            return Math.Min(Math.Max(((attacker.skill + attacker.skill_bonus) * 5 + attacker.weapon1.crit - (defender.fortune + defender.fortune_bonus) * 5), 0), 100);
        }
        private int Double(Unit2 attacker, Unit2 defender)
        {
            return Math.Min(Math.Max((((attacker.speed + attacker.speed_bonus - attacker.weapon1.weight) - (defender.agility + defender.agility_bonus - defender.weapon1.weight)) * 20), 0), 100);
        }
        private int Damage(Unit2 attacker, Unit2 defender)
        {
            if(attacker.weapon1.magical)
            {
                return Math.Max((attacker.magic + attacker.magic_bonus + attacker.weapon1.power) - (defender.resistance + defender.resistance_bonus), 1);
            }
            return Math.Max((attacker.strength + attacker.strength_bonus + attacker.weapon1.power) - (defender.defence + defender.defence_bonus), 1);
        }
        private void Battle(Unit2 attacker, Unit2 defender)
        {
            int aturns = 0;
            int bturns = 0;
            Random rnd = new Random();
            battle_occuring = true;
            for(bool x = true; x;)
            {
                if (aturns < 1 || (aturns < 2 && rnd.Next(101) <= Double(attacker, defender)))
                {
                    BattleList.Add(BattleHelper(attacker, defender));
                    BattleOrder.Add(0);
                }
                aturns++;
                if (bturns < 1 || (bturns < 2 && rnd.Next(101) <= Double(defender, attacker)))
                {
                    BattleList.Add(BattleHelper(defender, attacker));
                    BattleOrder.Add(1);
                }
                bturns++;
                if (aturns >= 2 && bturns >= 2)
                {
                    x = false;
                }
            }
        }
        private int[] BattleHelper(Unit2 attacker, Unit2 defender)
        {
            Random rnd = new Random();
            //first digit is hit or miss, second digit is crit or not, third digit is health afterwards.
            int[] scenario = new int[3] { 0, 0, defender.health };
            if(rnd.Next(101) <= Hit(attacker, defender))
            {
                scenario[0] = 1;
            }
            else
            {
                return scenario;
            }
            if(rnd.Next(101) <= Crit(attacker, defender))
            {
                scenario[1] = 1;
                scenario[2] = defender.health - (Damage(attacker, defender) * 3);
                return scenario;
            }
            else
            {
                scenario[2] = Damage(attacker, defender);
                return scenario;
            }
        }

        //Loading the wait menu
        private void Load_Wait_menu(Unit2 guy)
        {
            //add item
            int x = 8;
            foreach (Unit2 person in CurrentUnits)
            {
                if(person.team != guy.team && (Math.Sqrt(Math.Pow((guy.x_pos - person.x_pos), 2.0) + Math.Pow((guy.y_pos - person.y_pos), 2.0)) <= 1))
                {
                    MenuLabels.Add(Create_MainMenu_Label(4, 1, @"pack://siteoforigin:,,,/Resources/Button.png", 27, x, "Attack"));
                    x++;
                    break;
                }
            }
            MenuLabels.Add(Create_MainMenu_Label(4, 1, @"pack://siteoforigin:,,,/Resources/Button.png", 27, x, "Inventory"));
            x++;
            MenuLabels.Add(Create_MainMenu_Label(4, 1, @"pack://siteoforigin:,,,/Resources/Button.png", 27, x, "Wait"));
            x++;
        }
        private void Wait(Unit2 guy)
        {
            ClearInfoImages();
            if (guy == null) return;
            ClearMenuLabels();
            guy.status = 1;
            var animation = new ObjectAnimationUsingKeyFrames();
            animation.BeginTime = TimeSpan.FromSeconds(0);
            animation.RepeatBehavior = RepeatBehavior.Forever;
            animation.AutoReverse = true;
            Storyboard.SetTarget(animation, guy.char_im);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Image.SourceProperty));
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", guy.unit_class.animation_name, "Map5.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team1colour, ".png"), 30, 30),
                KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))));
            board.Children.Add(animation);
            board.Begin();
            selected = null;
            InfoSelected = null;
            if(curteamturn != 0)
            {
                enemy_turn();
            }
        }
        private void Load_Inventory_menu(Unit2 guy)
        {
            inventorymenu = true;
            InfoImages.Add(Create_InfoImage(8, 8, 12, 5, @"pack://siteoforigin:,,,/Resources/InventoryScreen.png"));
            if (selected.weapon1 != null) CurrentScreenLabels.Add(Create_InfoTextLabel(4, 1, 14, 5, selected.weapon1.name, false, false));
            if (selected.weapon2 != null) CurrentScreenLabels.Add(Create_InfoTextLabel(4, 1, 14, 6, selected.weapon2.name, false, false));
            if (selected.inventory1 != null) CurrentScreenLabels.Add(Create_InfoTextLabel(4, 1, 14, 7, selected.inventory1.name, false, false));
            if (selected.inventory2 != null) CurrentScreenLabels.Add(Create_InfoTextLabel(4, 1, 14, 8, selected.inventory2.name, false, false));
            CurrentScreenLabels.Add(Create_InfoTextLabel(4, 1, 14, 9, "", false, false));
            CurrentScreenLabels.Add(Create_InfoTextLabel(4, 1, 14, 10, "", false, false));
            CurrentScreenLabels.Add(Create_InfoTextLabel(4, 1, 14, 11, "", false, false));
            CurrentScreenLabels.Add(Create_InfoTextLabel(4, 1, 14, 12, "", false, false));
        }

        public MainWindow()
        {
            InitializeComponent();
            Load_main();
        }

        //events for the info tables
        private string weapon_descript(Weapon thing)
        {
            string s = "";
            s = string.Concat("Power: ", Convert.ToString(thing.power), " Hit: ", Convert.ToString(thing.hit), " Crit: ", Convert.ToString(thing.crit), " Weight: ", Convert.ToString(thing.weight));

            return s;
        }
        private string item_descript(Item thing)
        {
            string s = "";
            if (thing.stat == 0) s = String.Concat(s, "Boosts this unit's strength by ");
            else if (thing.stat == 1) s = String.Concat(s, "Boosts this unit's defence by ");
            else if (thing.stat == 2) s = String.Concat(s, "Boosts this unit's magic by ");
            else if (thing.stat == 3) s = String.Concat(s, "Boosts this unit's resistance by ");
            else if (thing.stat == 4) s = String.Concat(s, "Boosts this unit's speed by ");
            else if (thing.stat == 5) s = String.Concat(s, "Boosts this unit's agility by ");
            else if (thing.stat == 6) s = String.Concat(s, "Boosts this unit's skill by ");
            else if (thing.stat == 7) s = String.Concat(s, "Boosts this unit's trickiness by ");
            else if (thing.stat == 8) s = String.Concat(s, "Boosts this unit's fortune by ");

            s = string.Concat(s, Convert.ToString(thing.buff));
            return s;
        }
        private string class_descript(Unit_Class thing)
        {
            string s = "";
            if (String.Equals(thing.name, "Noble")) s = "4 Movement, Can wield swords";

            return s;
        }
        private void InfoTextMouseEnter(object sender, RoutedEventArgs e)
        {
            Label Src = e.Source as Label;
            if (inventorymenu)
            {
                string a = "";
                string b = "";
                string c = "";
                string d = "";
                if(string.Equals(Src.Content, selected.weapon1.name))
                {
                    CurrentScreenLabels[CurrentScreenLabels.Count - 4].Content = String.Concat("Power: ", Convert.ToInt16(selected.weapon1.power));
                    CurrentScreenLabels[CurrentScreenLabels.Count - 3].Content = String.Concat("Hit: ", Convert.ToInt16(selected.weapon1.hit));
                    CurrentScreenLabels[CurrentScreenLabels.Count - 2].Content = String.Concat("Crit: ", Convert.ToInt16(selected.weapon1.crit));
                    CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = String.Concat("Weight: ", Convert.ToInt16(selected.weapon1.weight));
                }
                else if(string.Equals(Src.Content, selected.weapon2.name))
                {
                    CurrentScreenLabels[CurrentScreenLabels.Count - 4].Content = String.Concat("Power: ", Convert.ToInt16(selected.weapon2.power));
                    CurrentScreenLabels[CurrentScreenLabels.Count - 3].Content = String.Concat("Hit: ", Convert.ToInt16(selected.weapon2.hit));
                    CurrentScreenLabels[CurrentScreenLabels.Count - 2].Content = String.Concat("Crit: ", Convert.ToInt16(selected.weapon2.crit));
                    CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = String.Concat("Weight: ", Convert.ToInt16(selected.weapon2.weight));
                }
                return;
            }
            if (String.Equals(Src.Content, "Health")) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = "The current health of this unit";
            else if (String.Equals(Src.Content, "Experience")) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = "The amount of experience that this unit currently has";
            else if (String.Equals(Src.Content, "Strength")) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = "Affects the power of physical attacks";
            else if (String.Equals(Src.Content, "Defence")) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = "Affects damage taken by physical attacks";
            else if (String.Equals(Src.Content, "Magic")) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = "Affects the power of magical attacks";
            else if (String.Equals(Src.Content, "Resistance")) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = "Affects damage taken by magical attacks";
            else if (String.Equals(Src.Content, "Speed")) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = "Affects unit's ability to double";
            else if (String.Equals(Src.Content, "Agility")) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = "Affects enemy's ability to double";
            else if (String.Equals(Src.Content, "Skill")) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = "Affects unit's hit chance";
            else if (String.Equals(Src.Content, "Trickiness")) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = "Affects enemy's hit chance";
            else if (String.Equals(Src.Content, "Fortune")) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = "Affects enemy's critical chance";

            if (InfoSelected == null) return;
            //weapons
            else if (InfoSelected.weapon1 != null && String.Equals(Src.Content, InfoSelected.weapon1.name)) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = weapon_descript(InfoSelected.weapon1);
            else if (InfoSelected.weapon2 != null && String.Equals(Src.Content, InfoSelected.weapon2.name)) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = weapon_descript(InfoSelected.weapon2);
            //items
            else if (InfoSelected.inventory1 != null && string.Equals(Src.Content, InfoSelected.inventory1.name)) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = item_descript(InfoSelected.inventory1);
            else if (InfoSelected.inventory2 != null && string.Equals(Src.Content, InfoSelected.inventory2.name)) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = item_descript(InfoSelected.inventory2);
            //add skills later
            //change to show class' stats
            else if (String.Equals(Src.Content, InfoSelected.unit_class.name)) CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = class_descript(InfoSelected.unit_class);
        }
        private void InfoTextMouseLeave(object sender, RoutedEventArgs e)
        {
            if (inventorymenu)
            {
                CurrentScreenLabels[CurrentScreenLabels.Count - 4].Content = "";
                CurrentScreenLabels[CurrentScreenLabels.Count - 3].Content = "";
                CurrentScreenLabels[CurrentScreenLabels.Count - 2].Content = "";
                CurrentScreenLabels[CurrentScreenLabels.Count - 1].Content = "";
                return;
            }
            int x = CurrentScreenLabels.Count() - 1;
            if(CurrentScreenLabels.Count > 0) CurrentScreenLabels[x].Content = "";
        }
        private void InfoTextMouseDown(object sender, RoutedEventArgs e)
        {;
            Label Src = e.Source as Label;
            if (inventorymenu)
            {
                if (CurrentScreenLabels.Count == 5) return;
                if (String.Equals(Src.Content, CurrentScreenLabels[CurrentScreenLabels.Count - 8].Content)) return;
                if (String.Equals(Src.Content, CurrentScreenLabels[CurrentScreenLabels.Count - 7].Content))
                {
                    Weapon helper = selected.weapon1;
                    selected.weapon1 = selected.weapon2;
                    selected.weapon2 = helper;
                }
                inventorymenu = false;
                ClearCurrentScreenLabels();
                ClearInfoImages();
                return;
            }
        }

        //events for map interaction
        private void MapGridMouseEnter(object sender, RoutedEventArgs e)
        {
            if(curteamturn == 0 && !animation_occuring)
            {
                Image Src = e.Source as Image;
                Src.Opacity = 1;
                int a = Grid.GetRow(Src);
                int b = Grid.GetColumn(Src);
                Unit2 c = playeratcoord(b, a);
                if (c != null)
                {
                    //change all of this later
                    if (c.team == 0 && c.status != 1 && c != selected)
                    {
                        var animation = new ObjectAnimationUsingKeyFrames();
                        animation.BeginTime = TimeSpan.FromSeconds(0);
                        animation.RepeatBehavior = RepeatBehavior.Forever;
                        animation.AutoReverse = true;
                        Storyboard.SetTarget(animation, c.char_im);
                        Storyboard.SetTargetProperty(animation, new PropertyPath(Image.SourceProperty));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", c.unit_class.animation_name, "Map4.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team1colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", c.unit_class.animation_name, "Map3.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team1colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", c.unit_class.animation_name, "Map4.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team1colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))));
                        board.Children.Add(animation);
                        board.Begin();
                    }
                    else if(c.team == 1)
                    {
                        if (selected == null)
                        {
                            ActiveMovement(c);
                        }
                        else
                        {
                            Load_BattleInfo(selected, c);
                        }
                        //change later
                        var animation = new ObjectAnimationUsingKeyFrames();
                        animation.BeginTime = TimeSpan.FromSeconds(0);
                        animation.RepeatBehavior = RepeatBehavior.Forever;
                        animation.AutoReverse = true;
                        Storyboard.SetTarget(animation, c.char_im);
                        Storyboard.SetTargetProperty(animation, new PropertyPath(Image.SourceProperty));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", c.unit_class.animation_name, "Map4.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team2colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", c.unit_class.animation_name, "Map3.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team2colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", c.unit_class.animation_name, "Map4.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team2colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))));
                        board.Children.Add(animation);
                        board.Begin();
                    }
                }
            }
        }
        private void MapGridMouseLeave(object sender, RoutedEventArgs e)
        {
            if(curteamturn == 0)
            {
                Image Src = e.Source as Image;
                Src.Opacity = 0;
                int a = Grid.GetRow(Src);
                int b = Grid.GetColumn(Src);
                Unit2 c = playeratcoord(b, a);
                ClearListImages(BattleInfoPics);
                ClearListLabels(BattleInfoLabel);
                if (c != null)
                {
                    //change all of this later
                    if (c.team == 0 && c != selected && c.status != 1)
                    {
                        var animation = new ObjectAnimationUsingKeyFrames();
                        animation.BeginTime = TimeSpan.FromSeconds(0);
                        animation.RepeatBehavior = RepeatBehavior.Forever;
                        animation.AutoReverse = true;
                        Storyboard.SetTarget(animation, c.char_im);
                        Storyboard.SetTargetProperty(animation, new PropertyPath(Image.SourceProperty));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", c.unit_class.animation_name, "Map1.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team1colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", c.unit_class.animation_name, "Map2.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team1colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", c.unit_class.animation_name, "Map1.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team1colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))));
                        board.Children.Add(animation);
                        board.Begin();
                    }
                    else if (c != selected && c.status != 1)
                    {
                        if (selected == null)
                        {
                            ClearColouredSpots();
                        }
                        //change later
                        var animation = new ObjectAnimationUsingKeyFrames();
                        animation.BeginTime = TimeSpan.FromSeconds(0);
                        animation.RepeatBehavior = RepeatBehavior.Forever;
                        animation.AutoReverse = true;
                        Storyboard.SetTarget(animation, c.char_im);
                        Storyboard.SetTargetProperty(animation, new PropertyPath(Image.SourceProperty));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", c.unit_class.animation_name, "Map1.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team2colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", c.unit_class.animation_name, "Map2.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team2colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", c.unit_class.animation_name, "Map1.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team2colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))));
                        board.Children.Add(animation);
                        board.Begin();
                    }
                }
            }
        }
        private void MapGridMouseClick(object sender, RoutedEventArgs e)
        {
            if (curteamturn == 0 && !animation_occuring)
            {
                Image Src = e.Source as Image;
                int a = Grid.GetRow(Src);
                int b = Grid.GetColumn(Src);
                Unit2 c = playeratcoord(b, a);
                if (c != null)
                {
                    if (c == selected)
                    {
                        ClearColouredSpots();
                        ClearInfoImages();
                        ClearCurrentScreenLabels();
                        Load_Wait_menu(c);
                    }
                    //change all of this later
                    else if (c.team == curteamturn && c.status == 0 && selected == null)
                    {
                        //change later
                        selected = c;
                        cur_x = c.x_pos;
                        cur_y = c.y_pos;
                        ActiveMovement(c);
                        // load movement for that character
                    }
                    else if (selected != null && c.team != selected.team)
                    {
                        if ((Math.Sqrt(Math.Pow((selected.x_pos - c.x_pos), 2.0) + Math.Pow((selected.y_pos - c.y_pos), 2.0)) <= 1))
                        {
                            BattleParticipants.Add(selected);
                            BattleParticipants.Add(c);
                            battle_occuring = true;
                            Pathfinding(selected, selected.x_pos, selected.y_pos);
                        }
                        //move to adjacent space and attack character
                        else if (bluespotexists(Grid.GetColumn(Src), Grid.GetRow(Src) - 1))
                        {
                            BattleParticipants.Add(selected);
                            BattleParticipants.Add(c);
                            battle_occuring = true;
                            Pathfinding(selected, Grid.GetColumn(Src), Grid.GetRow(Src) - 1);
                        }
                        else if (bluespotexists(Grid.GetColumn(Src), Grid.GetRow(Src) + 1))
                        {
                            BattleParticipants.Add(selected);
                            BattleParticipants.Add(c);
                            battle_occuring = true;
                            Pathfinding(selected, Grid.GetColumn(Src), Grid.GetRow(Src) + 1);
                        }
                        else if (bluespotexists(Grid.GetColumn(Src) - 1, Grid.GetRow(Src)))
                        {
                            BattleParticipants.Add(selected);
                            BattleParticipants.Add(c);
                            battle_occuring = true;
                            Pathfinding(selected, Grid.GetColumn(Src) - 1, Grid.GetRow(Src));
                        }
                        else if (bluespotexists(Grid.GetColumn(Src) + 1, Grid.GetRow(Src)))
                        {
                            BattleParticipants.Add(selected);
                            BattleParticipants.Add(c);
                            battle_occuring = true;
                            Pathfinding(selected, Grid.GetColumn(Src) + 1, Grid.GetRow(Src));
                        }
                    }
                }
                if (selected != null && bluespotexists(Grid.GetColumn(Src), Grid.GetRow(Src))) // move that character
                {
                    Pathfinding(selected, Grid.GetColumn(Src), Grid.GetRow(Src));
                    foreach (Image n in ColouredSpots)
                    {
                        n.Opacity = 0;
                    }
                }
                else
                {
                    //open options menu
                }
            }
        }
        private void MapGridMouseRightClick(object sender, RoutedEventArgs e)
        {
            Image Src = e.Source as Image;
            int a = Grid.GetRow(Src);
            int b = Grid.GetColumn(Src);
            Unit2 c = playeratcoord(b, a);
            inventorymenu = false;
            if (c != null && selected == null)
            {
                InfoSelected = c;
                Load_Info(c);
            }
        }

        //Main Menu Events
        private void MainMenuButtonMouseEnter(object sender, RoutedEventArgs e)
        {
            Label SourceLabel = e.Source as Label;
            ImageBrush updated = new ImageBrush();
            updated.ImageSource = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/ButtonSelected.png"));
            SourceLabel.Background = updated;
        }
        private void MainMenuButtonMouseLeave(object sender, RoutedEventArgs e)
        {
            Label SourceLabel = e.Source as Label;
            ImageBrush updated = new ImageBrush();
            updated.ImageSource = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/Button.png"));
            SourceLabel.Background = updated;
        }
        private void MainMenuButtonMouseClick(object sender, RoutedEventArgs e)
        {
            ClearCurrentScreenLabels();
            Label SourceLabel = e.Source as Label;
            //Top Menu
            if (String.Equals(Convert.ToString(SourceLabel.Content), "Story Mode"))
            {
                Load_Singleplayer();
            }
            else if (String.Equals(Convert.ToString(SourceLabel.Content), "Arcade"))
            {
                Load_Arcade();
            }
            else if (String.Equals(Convert.ToString(SourceLabel.Content), "Test"))
            {
                Load_Map("Test Map");
            }
            //Single Player Menu

            //Arcade Menu

            else if (string.Equals(Convert.ToString(SourceLabel.Content), "Main Menu"))
            {
                Load_main();
            }
            else if(string.Equals(Convert.ToString(SourceLabel.Content), "Attack"))
            {
                ClearMenuLabels();
                onerangehighlight(selected, selected.x_pos, selected.y_pos);
            }
            else if (string.Equals(Convert.ToString(SourceLabel.Content), "Wait"))
            {
                Wait(selected);
                end_turn();
            }
            else if(string.Equals(Convert.ToString(SourceLabel.Content), "Inventory"))
            {
                Load_Inventory_menu(selected);
            }
            else
            {
                Close();
            }
        }

        //Map Events


        // Testing Keys
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.L)
            {
                ClearCurmapGrid();
                ClearCurrentTeamUnits();
                ClearColouredSpots();
                ClearCurrentTeamUnits();
                ClearMenuLabels();
                ClearInfoImages();
                ClearBattlePics();
                Load_main();
            }
            else if(e.Key == Key.Back)
            {
                ClearCurrentScreenImages();
                ClearCurrentScreenLabels();
                ClearCurmapGrid();
                ClearCurrentTeamUnits();
                ClearInfoImages();
                ClearMenuLabels();
            }
            else if(e.Key == Key.M)
            {
                curteamturn = 0;
            }
            else if(e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(!animation_occuring && !battle_occuring)
            {
                ClearMenuLabels();
                ClearColouredSpots();
                if (selected != null)
                {
                    if(selected.team == 0)
                    {
                        var animation = new ObjectAnimationUsingKeyFrames();
                        animation.BeginTime = TimeSpan.FromSeconds(0);
                        animation.RepeatBehavior = RepeatBehavior.Forever;
                        animation.AutoReverse = true;
                        Storyboard.SetTarget(animation, selected.char_im);
                        Storyboard.SetTargetProperty(animation, new PropertyPath(Image.SourceProperty));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", selected.unit_class.animation_name, "Map1.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team1colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", selected.unit_class.animation_name, "Map2.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team1colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", selected.unit_class.animation_name, "Map1.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team1colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))));
                        board.Children.Add(animation);
                        board.Begin();
                    }
                    else if(selected.team == 1)
                    {
                        var animation = new ObjectAnimationUsingKeyFrames();
                        animation.BeginTime = TimeSpan.FromSeconds(0);
                        animation.RepeatBehavior = RepeatBehavior.Forever;
                        animation.AutoReverse = true;
                        Storyboard.SetTarget(animation, selected.char_im);
                        Storyboard.SetTargetProperty(animation, new PropertyPath(Image.SourceProperty));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", selected.unit_class.animation_name, "Map1.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team2colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", selected.unit_class.animation_name, "Map2.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team2colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))));
                        animation.KeyFrames.Add(new DiscreteObjectKeyFrame(ColourShiftedSource(String.Concat(@"pack://siteoforigin:,,,/Resources/", selected.unit_class.animation_name, "Map1.png"), @"pack://siteoforigin:,,,/Resources/Palette.png", String.Concat(@"pack://siteoforigin:,,,/Resources/Palette", team2colour, ".png"), 30, 30),
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))));
                        board.Children.Add(animation);
                        board.Begin();
                    }
                    selected.x_pos = cur_x;
                    selected.y_pos = cur_y;
                    Grid.SetColumn(selected.char_im, cur_x);
                    Grid.SetRow(selected.char_im, cur_y);
                }
                selected = null;
            }
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            InfoSelected = null;
            if (InfoImages.Count > 0)
            {
                ClearInfoImages();
                ClearCurrentScreenLabels();
            }
        }
    }
}
