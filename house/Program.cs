// ReSharper disable once RedundantUsingDirective
using System;
// ReSharper disable once RedundantUsingDirective
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace house;

internal static class Program
{
    private const int Height = 20;
    private const int MapWidth = 59;
    private const int BarPosition = 60;
    private const int SidebarPosition = 62;
    private const int SidebarWidth = 57;
    private const ConsoleColor InputColoUr = ConsoleColor.DarkRed;
    private const ConsoleColor OutputColoUr = ConsoleColor.Yellow;
    private static readonly char[,] Map = new char[MapWidth, Height];
    
    private const char WallSymbol = '#';
    private const char PlayerSymbol = 'O';
    

    private static void Main()
    {
#pragma warning disable CA1416
        Console.WindowWidth = SidebarPosition + SidebarWidth;
#pragma warning restore CA1416
        string run;
        House house;
        Player player;
        
        // decide what to do
        do
        {
            Console.Write("build house, or run (use h while running for command list): ");
            run = Console.ReadLine()!;
        } while (run.ToLower() != "run" && run.ToLower() != "build" && 
                 run.ToLower() != "r" &&  run.ToLower() != "b");
        
        
        
        // run, or build then run, build currently does nothing
        if (run.ToLower() == "build" || run.ToLower() == "b")
        {
            WriteOffset("Nothing here yet!");
            house = new House();
            player = house.Player;
        }
        else
        {
            house = new House();
            player = house.Player;
        }
        
        // set up the uhh line in the middle
        Setup(player);
        Run(player);
    }
    
    /// <summary>
    /// running the house
    /// </summary>
    /// <param name="player">The player</param>
    private static void Run(Player player)
    {
        bool repeat = true;
        do
        {
            Console.SetCursorPosition(BarPosition, 0);
            string selectedOption = Input(true, singleCharQ: true).ToLower();
            
            ClearSideBar();
            
            if (selectedOption is "w" or "a" or "s" or "d") // if movement key
            {
                (int selectedX, int selectedY) = player.GetCoords();
                switch (selectedOption) // sets selectedX and selectedY to the coords of the thing moved into
                {
                    case "w":
                        selectedY--;
                        break;
                    case "s":
                        selectedY++;
                        break;
                    case "a":
                        selectedX--;
                        break;
                    case "d":
                        selectedX++;
                        break;
                }

                if (selectedX >= 0 && selectedY >= 0)
                {
                    char selectedObject = Map[selectedX, selectedY];
                    switch (selectedObject)
                    {
                        case ' ':
                            MovePlayer(player, selectedX, selectedY);
                            break;

                        case '-':
                        case '|':
                            TryDoor(player, selectedX, selectedY);
                            break;

                        case WallSymbol:

                            break;

                        default:
                            ItemInteract(player, selectedX, selectedY, selectedOption);
                            break;
                    }
                }

            }
            else switch (selectedOption) // if other
            {
                case "h":
                    WriteOffset("help - h - list commands" +
                                "\nmovement - w/a/s/d - move the player and interact" +
                                "\nlist doors - l - list doors of current room" +
                                "\nmove - m - move rooms" +
                                "\nfind items - f - search for items in the room" + 
                                "\nquit - q - leave the program");
                    break;
                
                case "l":
                    WriteOptions(player.GetRoom().GetDoorNames(), " : Leads to ");
                    break;
                
                case "m":
                    MoveRoom(player);
                    DrawRoom(player.GetRoom());
                    Draw();
                    break;
                
                case "f":
                    WriteOffset("This room contains...");
                    WriteOptions(player.GetRoom().GetItemNames(), " : ", 2);
                    break;
                
                case "i":
                    WriteOffset("You are currently in possession of...");
                    WriteOptions(player.GetItemNames(), " : ", 2);
                    break;
                
                case "q":
                    repeat = false;
                    break;
                
                default:
                    WriteOffset(" --> Sorry, this command isn't valid, try \"h\" for\na list of commands.", coloUr: OutputColoUr);
                    break;
            }
        } while (repeat);
    }

    /// <summary>
    /// interacts with an item, if it can be picked up, gives the player the option to pick it up
    /// </summary>
    /// <param name="player">player</param>
    /// <param name="selectedX">Item location X</param>
    /// <param name="selectedY">Item location Y</param>
    /// <param name="selectedOption">Used to pick up item if walked into again (if possible)</param>
    private static void ItemInteract(Player player, int selectedX, int selectedY, string selectedOption)
    {
        Item item = null!;
        foreach(Item i in player.GetRoom().GetItems()) // get targeted item
        {
            if (i.X == selectedX && i.Y == selectedY)
            {
                item = i;
            }
        }
        
        WriteOffset(item.Text);
        
        
        if (item.PickupQ) // ask to pick up the item
        {
            string pickupRequest = "This item can be picked up. Pick up? (y/N): ";
            WriteOffset(pickupRequest, 2);
            
            string playerOption = Input(offsetY: 2, offsetX: pickupRequest.Length + SidebarPosition, singleCharQ: true).ToLower();

            if (playerOption == "y" || playerOption == selectedOption)
            {
                player.AddItem(item);
                player.GetRoom().RemoveItem(item);
                DrawRoom(player.GetRoom());
                Draw();
            }
            
        }
        else if (item.GetType() == typeof(Safe)) // no safe can be picked up so else can be used
        {
            TrySafe(player, (Safe)item);
        }
    }

    private static void TrySafe(Player player, Safe safe)
    {
        string codeRequest = "Input the safe's code: ";
        WriteOffset(codeRequest, 2);
        string inputtedCode = Input(false, 2, codeRequest.Length + SidebarPosition);
        if (inputtedCode == safe.Code)
        {
            safe.Contents.X = safe.X; // ensure item will be placed at safe, not really needed
            safe.Contents.Y = safe.Y;
            player.GetRoom().RemoveItem(safe);
            player.GetRoom().AddItem(safe.Contents);
            
            DrawRoom(player.GetRoom());
            Draw();
            WriteOffset("This safe has been opened!", 3, coloUr:  OutputColoUr);
            return;
        }
        WriteOffset("The safe does not open, the code must be wrong", 3, coloUr:  OutputColoUr);
    }
    
    
    /// <summary>
    /// Attempts to go through door, if locked, tries to unlock it with a key.
    /// </summary>
    /// <param name="player">The player</param>
    /// <param name="newX">X of the door</param>
    /// <param name="newY">Y of the door</param>
    private static void TryDoor(Player player, int newX, int newY)
    {
        Door door = null!;
        foreach(Door d in player.GetRoom().GetDoors()) // get targeted door
        {
            if (d.X == newX && d.Y == newY)
            {
                door = d;
            }
        }

        if (door.GetLockedQ()) // if door locked, try to use a key, else cancel movement
        {
            foreach (Key k in player.GetItems().OfType<Key>())
            {
                if (k.LockedDoor == door)
                {
                    player.RemoveItem(k);
                    door.SetLockedQ(false);
                    WriteOffset("Door unlocked!");
                    break;
                }
            }
            if (door.GetLockedQ())
            {
                WriteOffset("This door is locked. Maybe find a key first");
                return;
            }
        }
        player.SetRoom(door.UseDoor(player.GetRoom())); // set player room
        DrawRoom(player.GetRoom());
        
        if (player.GetCoords().X > newX) MovePlayer(player, newX - 1, newY); // if door to the left
        else if (player.GetCoords().X < newX) MovePlayer(player, newX + 1, newY); // if door to the right
        else if (player.GetCoords().Y < newY) MovePlayer(player, newX, newY + 1); // if door to the top
        else if (player.GetCoords().Y > newY) MovePlayer(player, newX, newY - 1); // if door to the bottom
    }

    private static void MovePlayer(Player player, int newX, int newY) // WARNING: if door pressed against edge (somehow) room will be set but not location
    {
        if (newX < MapWidth - 1 && newY < Height - 1)
            // I don't know why but this is how it wants to be
        {
            Map[player.GetCoords().X, player.GetCoords().Y] = ' ';
            player.SetCoords(newX, newY);
            Map[newX, newY] = PlayerSymbol;
            Draw();
        }
    }
    
    private static void DrawRoom(Room room)
    {
        
        for (int y = room.Y; y < room.Y + room.Height; y++)
        {
            for (int x = room.X; x < room.X + room.Length; x++)
            {
                if (x == room.X || y == room.Y || y == room.Y + room.Height - 1 || x == room.X + room.Length - 1)
                {
                    Map[x, y] = WallSymbol;
                }
                else if (Map[x, y] != PlayerSymbol)
                {
                    Map[x, y] = ' ';
                }
            }
        }

        foreach (Item i in room.GetItems())
        {
            if (i.X != -1) Map[i.X, i.Y] = i.Symbol; // if so items inside of safe don't get printed, look at "Safe()"
        }

        foreach (Door d in room.GetDoors())
        {
            char symbol = '-';
            if (Map[d.X, d.Y + 1] == WallSymbol) symbol = '|';
            Map[d.X, d.Y] =  symbol;
        }
    }

    private static void Draw() // could prolly change this so characters are written individually, so the walls could be made grey (less busy visually)
    {
        Console.SetCursorPosition(0,0);
        for (int y = 0; y < Height; y++)
        {
            string line = "";
            for (int x = 0; x < MapWidth; x++)
            {
                line += Map[x, y];
            }
            Console.WriteLine(line);
        }
    }
    

    /// <summary>
    /// writes the door numbers and where they lead
    /// </summary>
    /// <param name="names">string array of door options</param>
    /// <param name="text">Text between index and name</param>
    /// <param name="offsetY">vertical offset of text</param>
    /// <returns></returns>
    private static int WriteOptions(string[] names, string text,  int offsetY = 1)
    {
        foreach (string name in names)
        {
            WriteOffset((Array.IndexOf(names, name) + text + name), offsetY);
            offsetY += 1;
        }
        return offsetY;
    }

    
    /// <summary>
    /// move room using command
    /// </summary>
    /// <param name="player"></param>
    private static void MoveRoom(Player player)
    {
        Room currRoom = player.GetRoom();
        // get wanted room and write
        WriteOffset("Your current door options are:");
        int offsetY = WriteOptions(currRoom.GetDoorNames(), " : Leads to ", 2);
        
        // give player choice
        string message = "What door would you like to open (X to cancel)?: ";
        WriteOffset(message, offsetY);
        string selected = Input(offsetY: offsetY, offsetX: message.Length + SidebarPosition);
        offsetY++;
        
        // try and enter room
        Room newRoomQ = currRoom.FromRoomUseDoor(selected);
        if  (newRoomQ != null!)
        {
            player.SetRoom(newRoomQ);
            WriteOffset(("Moved to: " + newRoomQ.RoomName), offsetY, coloUr: OutputColoUr );
        }
        else if (selected.ToLower() == "x") WriteOffset(" --> Cancelled", offsetY, coloUr: OutputColoUr);
        else WriteOffset(" --> Failed", offsetY, coloUr: OutputColoUr);
    }
    
    
    /// <summary>
    /// clear and put middle line down
    /// </summary>
    /// <param name="player">initialised player</param>
    private static void Setup(Player player)
    {
        Console.Clear();
        WriteOffset("Map here maybe", 4, 20);
        for (int i = 0; i < Height; i++)
        {
            WriteOffset("/", i, BarPosition);
        }
        
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                Map[x, y] = ' ';
            }
        }
        DrawRoom(player.GetRoom());
        MovePlayer(player, player.GetCoords().X, player.GetCoords().Y);
        Draw();
        
        
    }


    /// <summary>
    /// Write line with offset
    /// </summary>
    /// <param name="text">text, this can be multiple lines, but each line must be less than SidebarWidth</param>
    /// <param name="offsetY">the offset on the Y axis, from the top</param>
    /// <param name="offsetX">the offset on the X axis, from the left</param>
    /// <param name="coloUr">ColoUr, defaults to white, and will always return to white at the end</param>
    private static void WriteOffset(string text, int offsetY = 1, int offsetX = SidebarPosition,
        ConsoleColor coloUr = ConsoleColor.White)
    {
        Console.ForegroundColor = coloUr;
        string[] lines = text.Split("\n");
        foreach (string t in lines)
        {
            Console.SetCursorPosition(offsetX, offsetY);
            Console.Write(t);
            offsetY++;
        }

        Console.ForegroundColor = ConsoleColor.White;
    }


    /// <summary>
    /// gets user input (coloUrs input)
    /// </summary>
    /// <param name="clearQ">clear after inputted? defaults to false</param>
    /// <param name="offsetY">Y offset</param>
    /// <param name="offsetX">X offset</param>
    /// <param name="singleCharQ">If only a single character is allowed to be inputted</param>
    /// <returns>input</returns>
    private static string Input(bool clearQ = false, int offsetY = 0, int offsetX = SidebarPosition, bool singleCharQ = false)
    {
        Console.SetCursorPosition(offsetX, offsetY);
        Console.ForegroundColor = InputColoUr;
        
        string value = singleCharQ ? Console.ReadKey().KeyChar.ToString() : Console.ReadLine()!;
        
        Console.ForegroundColor = ConsoleColor.White;

        if (clearQ)
        {
            Console.SetCursorPosition(offsetX, offsetY); 
            Console.Write(new string(' ', SidebarWidth));
        }
        
        return value;
    }
    
    
    /// <summary>
    /// clears the sidebar
    /// </summary>
    private static void ClearSideBar()
    {
        for (int y = 0; y < Height; y++)
        {
            Console.SetCursorPosition(SidebarPosition, y); 
            Console.Write(new string(' ', SidebarWidth));
        }
    }
    
}



/// <summary>
/// The player, contains its position and items
/// </summary>
internal class Player
{
    private Room _currRoom;
    private int _x;
    private int _y;
    private readonly List<Item> _items;
    
    // ReSharper disable once ConvertToPrimaryConstructor
    public Player(Room startRoom, int xPos, int yPos, List<Item>? items = null)
    {
        _currRoom = startRoom;
        _x = xPos;
        _y = yPos;
        _items = items ?? new List<Item>();
    }

    public (int X, int Y) GetCoords()
    {
        return (_x, _y);
    }

    public void SetCoords(int x, int y)
    {
        _x = x;
        _y = y;
    }

    public void AddItem(Item item)
    {
        _items.Add(item);
    }

    public void RemoveItem(Item item)
    {
        _items.Remove(item);
    }

    public List<Item> GetItems()
    {
        return _items;
    }
    
    public string[] GetItemNames()
    {
        string[] itemNames = new string[_items.Count];
        int i = 0;
        foreach (Item o in _items)
        {
            itemNames[i] = o.Name;
            i++;
        }
        return itemNames;
    }
    
    public Room GetRoom()
    {
        return _currRoom;
    }
    
    public string GetRoomName()
    {
        return _currRoom.RoomName;
    }

    public void SetRoom(Room newRoom)
    {
        _currRoom = newRoom;
    }
}



/// <summary>
/// an item
/// </summary>
/// <param name="name">the items name</param>
/// <param name="xCoord">X coordinate of the item</param>
/// <param name="yCoord">Y coordinate of the item</param>
internal class Item(string name, int xCoord, int yCoord, string text, bool pickupQ = true, char symbol = '?')
{
    public readonly string Name = name;
    public int X = xCoord;
    public int Y = yCoord;
    public readonly string Text = text;
    public readonly bool PickupQ = pickupQ;
    public readonly char Symbol = symbol;
}

internal class Message(string name, int xCoord, int yCoord, string text) : Item(name, xCoord, yCoord, text, false, '=') {}



/// <summary>
/// the contents X coord MUST be set to -1
/// </summary>
/// <param name="contents">the item to drop when opened</param>
/// <param name="code">the code that must be inputted to open it</param>
internal class Safe(string name, int xCoord, int yCoord, Item contents, string text, string code) : Item(name, xCoord, yCoord, text, false, '@')
{
    public readonly Item Contents = contents;
    public readonly string Code = code;
}



/// <summary>
/// A key for a door, inherits properties from Item
/// </summary>
/// <param name="lockedDoor">Door the key can open</param>
internal class Key(string name, int xCoord, int yCoord, Door lockedDoor, string text) : Item(name, xCoord, yCoord, text, true, '¬')
{
    public readonly Door LockedDoor = lockedDoor;
}



/// <summary>
/// Creates house
/// </summary>
internal class House
{
    private readonly List<Room> _rooms = [];
    public Player Player = null!;
    
    
    public House(bool defaultHouse = true)
    {
        if (defaultHouse) CreateDefaultHouse();
    }

    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    private void CreateDefaultHouse()
    {
        
        _rooms.Add(new Room("Bedroom",5,7, 6, 16));
        _rooms.Add(new Room("Hallway",20,4, 12));
        _rooms.Add(new Room("Bedroom closet", 5, 12, 4, 6));
        
        // player begins in the Bedroom (room 0)
        List<Item> playerItems = [new Item("Pajamas", 4, 4, "")];
        Player = new Player(_rooms[0], 6, 8, playerItems);
        
        // Door for bedroom closet and locked door to leave bedroom
        new Door(_rooms[0], _rooms[2], 7, 12); 
        Door door1To2 = new Door(_rooms[0], _rooms[1], 20, 9, true);

        List<Item> bedroomBed =
        [
            new("Bed00", 6, 9, "You bed's one pillow", false, ']'),
            new("Bed01", 6, 10, "Your bed", false, '%'),
            new("Bed10", 7, 9, "Your bed, it needs to be washed soon", false, '%'),
            new("Bed11", 7, 10, "Your bed, its a single", false, '%'),
            new("Bed20", 8, 9, "Your bed", false, '%'),
            new("Bed21", 8, 10, "Your bed, it has white blankets", false, '%')
        ]; // the bed

        List<Item> bedroomItems =
        [
            new("Bedside Table", 6, 11, "A bedside table. It has a lamp on it", false),
            new("Dressing Table0", 12, 11, "A dressing table", false),
            new("Dressing Table1", 13, 11, "A dressing table", false),
            new("Dirty clothes", 12, 8, "Your dirty clothes, clean up after yourself!"),
            new Message("Door locked message", 19, 8, "You lock your bedroom door with a key in your closet safe")
        ];
        _rooms[0].SetItems(bedroomBed);
        _rooms[0].AddItems(bedroomItems);


        List<Item> thirdRoomItems =
        [
            new("clothes1", 7, 14, "Some clean clothes", false),
            new("clothes2", 9, 13, "Some clean clothes", false),
            new("clothes3", 9, 14, "Some clean clothes", false),
            new Message("Message", 8, 14, "The code for the safe is \"Hello world\""),
            new Safe("Safe", 6, 14, new Key("Key", 0, 0, door1To2, "A key"), "This seems to be a safe", "Hello world")
        ];
        
        _rooms[2].SetItems(thirdRoomItems);
    }
}



/// <summary>
/// A room in the house
/// </summary>
/// <param name="roomName">The selected name of the room</param>
internal class Room(string roomName, int xPosition, int yPosition, int height = 5, int length = 5)
{
    public readonly string RoomName = roomName;
    public readonly int Length = length;
    public readonly int Height = height;
    public readonly int X = xPosition;
    public readonly int Y = yPosition;
    
    private readonly List<Door> _doors = [];
    private List<Item> _items = [];

    public void AddDoor(Door d)
    {
        _doors.Add(d);
    }
    
    /// <summary>
    /// moves room 
    /// </summary>
    /// <param name="d">integer, the index of the door to be opened (see GetDoors())</param>
    /// <returns>returns room the door led to, if error, null</returns>
    public Room FromRoomUseDoor(string d)
    {
        bool isIntQ = int.TryParse(d, out int doorInt);
        if (isIntQ && _doors.Count != 0 && doorInt >= 0)
        {
            return _doors[doorInt].UseDoor(this);
        }

        return null!;
    }

    public List<Door> GetDoors()
    {
        return _doors;
    }
    
    public string[] GetDoorNames()
    {
        string[] roomNames = new string[_doors.Count];
        int i = 0;
        foreach (Door d in _doors)
        {
            roomNames[i] = d.UseDoor(this).RoomName;
            i++;
        }
        return roomNames;
    }

    public void SetItems(List<Item> items)
    {
        _items =  items;
    }

    public void AddItem(Item item)
    {
        _items.Add(item);
    }

    public void AddItems(List<Item> items)
    {
        _items.AddRange(items);
    }

    public void RemoveItem(Item item)
    {
        _items.Remove(item);
    }

    public List<Item> GetItems()
    {
        return _items;
    }
    
    public string[] GetItemNames()
    {
        string[] itemNames = new string[_items.Count];
        int i = 0;
        foreach (Item o in _items)
        {
            itemNames[i] = o.Name;
            i++;
        }
        return itemNames;
    }
}



/// <summary>
/// a door.
/// </summary>
internal class Door
{
    private bool _lockedQ;
    private readonly Room _r1;
    private readonly Room _r2;
    
    public readonly int X;
    public readonly int Y;


    /// <param name="r1">where the door is</param>
    /// <param name="r2">where the door will lead</param>
    /// <param name="xPosition">x coord of door</param>
    /// <param name="yPosition">y coord of door</param>
    /// <param name="lockedQ">if the door cannot be opened, defaults to locked</param>
    public Door(Room r1, Room r2, int xPosition, int yPosition, bool lockedQ = false)
    {
        _lockedQ = lockedQ;
        _r1 = r1;
        _r2 = r2;
        _r1.AddDoor(this);
        _r2.AddDoor(this);
        
        X = xPosition;
        Y = yPosition;
        
    }
    
    public Room UseDoor(Room r)
    {
        return r == _r1 ? _r2 : _r1; // unlocking door is handled in TryDoor() in Program
    }

    public void SetLockedQ(bool lockedQ)
    {
        _lockedQ = lockedQ;
    }

    public bool GetLockedQ()
    {
        return _lockedQ;
    }
}