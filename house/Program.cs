using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace house
{
    class Game
    {
        public class Menu
        {
            public enum VerbType { Quit, GoTo, Buy, Sell, Take, Leave };
            public static string[] VerbMsg = { "Quit", "Go To", "Buy", "Sell", "Take", "Leave" };
            Dictionary<int, MenuItem> menu = new Dictionary<int, MenuItem>();

            public class MenuItem
            {
                public VerbType Verb { get; set; }
                public System.Object Item { get; set; }

                public MenuItem(VerbType verb, System.Object item)
                {
                    this.Verb = verb;
                    this.Item = item;
                }
            }

            public Menu(Room room, Player player)
            {
                
                // add quit option to menu
                menu.Add(1, new MenuItem(VerbType.Quit, null));

                int i = 2;
                // go thru rooms & add GoTo verb plus Room object
                foreach (Room adjacentRoom in room.Navigable)
                {
                    menu.Add(i, new MenuItem(VerbType.GoTo, adjacentRoom));
                    i++;
                }

                // go thru items & add buy or take plus Item object
                foreach (Item item in room.Pickable)
                {
                    if (!item.IsFixed)
                    {
                        if (room.IsMerchant)
                        {
                            menu.Add(i, new MenuItem(VerbType.Buy, item));
                        }

                        else
                        {
                            menu.Add(i, new MenuItem(VerbType.Take, item));
                        }
                    }

                    i++;
                }

                // go through player backpack & add sell if applicable
                foreach (Item item in player.Backpack)
                {
                    if (room.IsMerchant)
                    {
                        menu.Add(i, new MenuItem(VerbType.Sell, item));
                    }

                    else
                    {
                        menu.Add(i, new MenuItem(VerbType.Leave, item));
                    }

                    i++;
                }
            }

            public bool ProcessInput(string input, Room room, Player player)
            {
                bool result = true;
                bool invalid = false;

                int response;
                if (Int32.TryParse(input, out response))
                {
                    if (response <= menu.Count)
                    {
                        KeyValuePair<int, MenuItem> choice = menu.ElementAt(response - 1);
                        switch (choice.Value.Verb)
                        {
                            case VerbType.Quit:
                                result = false;
                                break;
                            case VerbType.Take:
                                Item takeItem = (Item)choice.Value.Item;
                                player.Backpack.Add(takeItem);
                                room.Pickable.Remove(takeItem);
                                break;
                            case VerbType.Leave:
                                Item leaveItem = (Item)choice.Value.Item;
                                player.Backpack.Remove(leaveItem);
                                room.Pickable.Add(leaveItem);
                                break;
                            case VerbType.GoTo:
                                Room dest = (Room)choice.Value.Item;
                                player.Location = dest;
                                break;
                            case VerbType.Sell:
                                Item sellItem = (Item)choice.Value.Item;
                                player.Cash += sellItem.Value;
                                player.Backpack.Remove(sellItem);
                                room.Pickable.Add(sellItem);
                                break;
                            case VerbType.Buy:
                                Item buyItem = (Item)choice.Value.Item;
                                if (player.Cash >= buyItem.Value)
                                {
                                    player.Cash -= buyItem.Value;
                                    player.Backpack.Add(buyItem);
                                    room.Pickable.Remove(buyItem);
                                }

                                else 
                                {
                                    Console.WriteLine("Sorry. You don't have enough cash. ");
                                }

                                break;

                            default:
                                invalid = true;
                                break;

                        }
                    }

                    else
                    {
                        invalid = true;
                    }
                }

                else
                {
                    invalid = true;
                }

                if (invalid)
                {
                    Console.WriteLine("Sorry. That action is not available.");
                }

                return result;
            }

            public Dictionary<int, MenuItem> Value { get { return menu; } }

            public void Display()
            {
                foreach (KeyValuePair<int, MenuItem> mi in menu)
                {
                    string VerbName = Menu.VerbMsg[(int)mi.Value.Verb];

                    switch (mi.Value.Verb)
                    {
                        case VerbType.Quit:
                            Console.WriteLine("{0}. {1}", mi.Key, "Quit");
                            break;

                        case VerbType.GoTo:
                            Room dest = (Room)mi.Value.Item;
                            Console.WriteLine("{0}. {1} {2}", mi.Key, VerbName, dest.Name);
                            break;

                        default:
                            Item item = (Item)mi.Value.Item;
                            Console.WriteLine("{0}. {1} {2}", mi.Key, VerbName, item.Name);
                            break;
                    }
                }
            }
        }


        public class Player
        {
            List<Item> backpack = new List<Item>();
            public Room Location { get; set; }
            public List<Item> Backpack { get { return backpack; } }
            public int Cash { get; set; }

            public Player()
            {
                Cash = 0;
            }
        }


        public class Item
        {
            string name;
            int value;
            bool isFixed;
            bool isFree;

            public Item(string name, int value, bool isFixed, bool isFree)
            {
                this.name = name;
                this.value = value;
                this.isFixed = isFixed;
                this.isFree = isFree;
            }

            public bool IsFree { get { return isFree; } }
            public bool IsFixed { get { return isFixed; } }
            public int Value { get { return value; } }
            public string Name { get { return name; } }
        }


        public class Room
        {
            string name;
            string description;
            List<Room> navigable;
            List<Item> pickable;
            bool isMerchant;

            public Room(string name, string description, bool isMerchant)
            {
                this.name = name;
                this.description = description;
                this.isMerchant = isMerchant;
                navigable = new List<Room>();
                pickable = new List<Item>();
            }

            public void ListNavigable()
            {
                foreach (Room room in Navigable)
                {
                    Console.WriteLine(room.Name);
                }
            }

            public string Name { get { return name; } }
            public string Description { get { return description; } }
            public List<Room> Navigable { get { return navigable; } }
            public List<Item> Pickable { get { return pickable; } }
            public bool IsMerchant { get { return isMerchant; } }
        }

        Room food, pawn, atrium, sports;
        Item casio, tree, frisbee;
        Player player;

        public Game()
        {
            //init rooms
            food = new Room("Food Court", "Several restaurants are here, and a number of tables and chairs. ", false);
            pawn = new Room("Pawn Shop", "A sign overhead reads, \"We buy and sell most everything\". ", true);
            atrium = new Room("Atrium", "A sign here reads, \"Finders keepers, losers weepers\".", false);
            sports = new Room("Sporting Goods Store", "The store appears to be closed.", false);

            //interconnect rooms
            atrium.Navigable.Add(food);
            atrium.Navigable.Add(pawn);
            atrium.Navigable.Add(sports);
            pawn.Navigable.Add(atrium);
            food.Navigable.Add(atrium);
            sports.Navigable.Add(atrium);

            //init items
            casio = new Item("Casio watch", 10, false, false);
            tree = new Item("Tree", 0, true, false);
            frisbee = new Item("Frisbee", 10, false, true);

            //add stuff to rooms
            atrium.Pickable.Add(frisbee);
            pawn.Pickable.Add(casio);
            atrium.Pickable.Add(tree);

            player = new Player();
            player.Location = atrium;
        }


        public void ListItems(List<Item> items)
        {
            int last = items.Count;

            for (int i = 0; i < last; i++)
            {
                string suffix = "";
                switch (last - i - 1)
                {
                    //last item
                    case 0:
                        suffix = ". ";
                        break;

                    //second to last
                    case 1:
                        suffix = ", and ";
                        break;

                    //list item
                    default:
                        suffix = ", ";
                        break;
                }

                string item = items.ElementAt(i).Name + suffix;
                Console.Write("{0}", item);
            }
        }


        public void ListRoomItems(Room room)
        {
            if (room.Pickable.Count > 0)
            {
                Console.Write("Items at this location include: ");
                ListItems(room.Pickable);
            }

            else
            {
                Console.Write("There are no items in this location. ");
            }

        }


        public void ListBackpack(Player player)
        {

            if (player.Backpack.Count > 0)
            {
                Console.Write("Items in your possession include: ");
                ListItems(player.Backpack);
            }

            else
            {
                Console.Write("Your backpack is empty. ");
            }


            Console.Write("You have ${0} cash. ", player.Cash);
        }

        public static void ListDirections(Room room)
        {
            Console.Write("From here you may go to: ");

            int last = room.Navigable.Count;

            for (int i = 0; i < last; i++)
            {
                string suffix = "";
                switch (last - i - 1)
                {
                    //last item
                    case 0:
                        suffix = ". ";
                        break;

                    //second to last
                    case 1:
                        suffix = ", or ";
                        break;

                    //list item
                    default:
                        suffix = ", ";
                        break;
                }

                string direction = room.Navigable.ElementAt(i).Name + suffix;
                Console.Write("{0}", direction);
            }
        }

        public static void ListLocation(Room room)
        {
            Console.Write("You are in the {0}. {1} ", room.Name, room.Description);
        }


        public void ListAll(Room room, Player player)
        {
            ListLocation(room);
            ListRoomItems(room);
            ListBackpack(player);
            ListDirections(room);
        }


        public void RunGame()
        {
            // game loop
            bool run = true;
            
            while (run)
            {
                ListAll(player.Location, player);

                Console.WriteLine("What do you want to do?");
                Menu menu = new Menu(player.Location, player);
                menu.Display();
                string input = Console.ReadLine();

                run = menu.ProcessInput(input, player.Location, player);
            }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.RunGame();

        }
    }
}


