using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace house;

internal static class Program
{
    private const int Height = 20;
    private const int MapWidth = 59;
    private const int BarPosition = 60;
    private const int SidebarPosition = 62;
    private const int SidebarWidth = 50;
    private const ConsoleColor InputColoUr = ConsoleColor.DarkRed;
    private const ConsoleColor OutputColoUr = ConsoleColor.Yellow;
    private static readonly char[,] Map = new char[MapWidth, Height];
    

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
        
        // set up the uhh line in the middle
        Setup(); 
        
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
            Room currRoom = house.GetRoom("first room");
            player = new(currRoom, 0, 0);
        }

        Run(player);
        Console.SetCursorPosition(0,0);
    }


    /// <summary>
    /// running the house
    /// </summary>
    /// <param name="player">The player</param>
    private static void Run(Player player)
    {
        
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                Map[x, y] = ' ';
            }
        }
        DrawRoom(player.GetRoom());
        Draw();
        
        bool repeat = true;
        do
        {
            Console.SetCursorPosition(BarPosition, 0);
            string selectedOption = Input(true).ToLower();
            
            ClearSideBar();
            
            switch (selectedOption)
            {
                case "h":
                    WriteOffset("help - h - list commands" +
                                "\nlist doors - l - list doors of current room" +
                                "\nmove - m - move rooms" +
                                "\nfind items - f - search for items in the room" + 
                                "\nquit - q - leave the program");
                    break;
                
                case "l":
                    WriteOptions(player.GetRoom().GetDoors(), " : Leads to ");
                    break;
                
                case "m":
                    MoveRoom(player);
                    DrawRoom(player.GetRoom());
                    Draw();
                    break;
                
                case "f":
                    WriteOptions(player.GetRoom().GetItems(), " : ");
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

    private static void DrawRoom(Room room)
    {
        for (int y = room.Y; y < room.Y + room.Height; y++)
        {
            for (int x = room.X; x < room.X + room.Length; x++)
            {
                if (x == room.X || y == room.Y || y == room.Y + room.Height - 1 || x == room.X + room.Length - 1)
                {
                    Map[x, y] = 'X';
                }
            }
        }
    }

    private static void Draw()
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
    /// move room
    /// </summary>
    /// <param name="player"></param>
    private static void MoveRoom(Player player)
    {
        Room currRoom = player.GetRoom();
        // get wanted room and write
        WriteOffset("Your current door options are:");
        int offsetY = WriteOptions(currRoom.GetDoors(), " : Leads to ", 2);
        
        // give player choice
        WriteOffset("What door would you like to open (X to cancel)?: ", offsetY);
        string selected = Console.ReadLine()!;
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
    private static void Setup()
    {
        Console.Clear();
        WriteOffset("Map here maybe", 4, 20);
        for (int i = 0; i < Height; i++)
        {
            WriteOffset("/", i, BarPosition);
        }
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
    /// <returns>input</returns>
    private static string Input(bool clearQ = false, int offsetY = 0, int offsetX = SidebarPosition)
    {
        Console.SetCursorPosition(offsetX, offsetY);
        Console.ForegroundColor = InputColoUr;
        string value = Console.ReadLine()!;
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
    
    // ReSharper disable once ConvertToPrimaryConstructor
    public Player(Room startRoom, int xPosition, int yPosition)
    {
        _currRoom = startRoom;
        _x = xPosition;
        _y = yPosition;
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
internal class Item(string name, int xCoord = 0, int yCoord = 0)
{
    public readonly string Name = name;
    public int X = xCoord;
    public int Y = yCoord;
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
        
        
        _rooms.Add(new Room("first room",5,7, 4, 16));
        _rooms.Add(new Room("second room",0,0));
        
        Player = new Player(_rooms[0], 0, 0);
        
        List<Item> thirdRoomItems = [new Item("cheese"), new Item("eggs")];
        _rooms.Add(new Room("third room",0,0));
        _rooms[2].SetItems(thirdRoomItems);
        new Door(_rooms[0], _rooms[1], 0, 0);
    }

    public string[] MakeMap(int height, int width)
    {
        string[] map = new string[height];
        for (int i = 0; i < height; i++)
        {
            map[i] = new string(' ', width);
        }
        return map;
    }
    
    public Room GetRoom(string roomName)
    {
        roomName = roomName.ToLower();
        foreach (Room r in _rooms)
            if (r.RoomName == roomName) return r;
        
        return null!;
        
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

    public string[] GetDoors()
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

    public void RemoveItem(Item item)
    {
        _items.Remove(item);
    }
    
    public string[] GetItems()
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
    private bool _locked;
    private readonly Room _r1;
    private readonly Room _r2;
    
    public int X;
    public int Y;


    /// <param name="r1">where the door is</param>
    /// <param name="r2">where the door will lead</param>
    /// <param name="xPosition">x coord of door</param>
    /// <param name="yPosition">y coord of door</param>
    /// <param name="locked">if the door cannot be opened, defaults to locked</param>
    public Door(Room r1, Room r2, int xPosition, int yPosition, bool locked = false)
    {
        _locked = locked;
        _r1 = r1;
        _r2 = r2;
        _r1.AddDoor(this);
        _r2.AddDoor(this);
        
        X = xPosition;
        Y = yPosition;
        
    }
    
    public Room UseDoor(Room r)
    {
        if (_locked) return r;
        
        if (r == _r1) return _r2;
        
        return _r1;
    }

    public void SetLocked(bool locked)
    {
        _locked = locked;
    }
}