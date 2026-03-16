using System;
using System.Collections.Generic;

namespace house;

internal static class Program
{
    private const int Height = 20;
    private const int SidebarOffset = 60;
    private const int SidebarWidth = 60;
    private const ConsoleColor InputColoUr = ConsoleColor.DarkRed;
    private const ConsoleColor OutputColoUr = ConsoleColor.Yellow;

    private static void Main()
    {
#pragma warning disable CA1416
        Console.WindowWidth = SidebarOffset + SidebarWidth;
#pragma warning restore CA1416
        string run;
        House house;
        Player player;
        
        // decide what to do
        do
        {
            Console.Write("build house, or run: ");
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
            Room currRoom = house.GetRoom("first room");
            player = new(currRoom);
        }
        else
        {
            house = new House();
            Room currRoom = house.GetRoom("first room");
            player = new(currRoom);
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
        bool repeat = true;
        do
        {
            Console.SetCursorPosition(SidebarOffset, 0);
            string selectedOption = Input(true).ToLower();
            
            ClearSideBar();
            
            switch (selectedOption)
            {
                case "help":
                case "h":
                    WriteOffset("help - h - list commands" +
                                "\ndoors - d - list doors of current room" +
                                "\nmove - m - move rooms" +
                                "\nfind items - f - search for items in the room" + 
                                "\nquit - q - leave the program");
                    break;
                
                case "doors":
                case "d":
                    WriteOptions(player.GetRoom().GetDoors(), " : Leads to ");
                    break;
                
                case "move":
                case "m":
                    MoveRoom(player);
                    break;
                
                case "find items":
                case "f":
                    WriteOptions(player.GetRoom().GetItems(), " : ");
                    break;
                
                case "quit":
                case "q":
                    repeat = false;
                    break;
                
                default:
                    WriteOffset(" --> Sorry, this command isn't valid, try \"help\"\nfor a list of commands.", coloUr: OutputColoUr);
                    break;
            }
        } while (repeat);
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
        Room newRoomQ = currRoom.UseDoor(selected);
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
            WriteOffset("/", i, SidebarOffset - 2);
        }
    }


    /// <summary>
    /// Write line with offset
    /// </summary>
    /// <param name="text">text, this can be multiple lines, but each line must be less than SidebarWidth</param>
    /// <param name="offsetX">the offset on the X axis, from the left</param>
    /// <param name="offsetY">the offset on the Y axis, from the top</param>
    /// <param name="coloUr">ColoUr, defaults to white, and will always return to white at the end</param>
    private static int WriteOffset(string text, int offsetY = 1, int offsetX = SidebarOffset, ConsoleColor coloUr = ConsoleColor.White)
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
        return offsetY;
    }


    /// <summary>
    /// gets user input (coloUrs input)
    /// </summary>
    /// <param name="clearQ">clear after inputted? defaults to false</param>
    /// <param name="offsetY">Y offset</param>
    /// <param name="offsetX">X offset</param>
    /// <returns>input</returns>
    private static string Input(bool clearQ = false, int offsetY = 0, int offsetX = SidebarOffset)
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
            Console.SetCursorPosition(SidebarOffset, y); 
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
    
    public Player(Room startRoom)
    {
        _currRoom = startRoom;
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
internal class Item(string name)
{
    public readonly string Name = name;
}



/// <summary>
/// Creates house
/// </summary>
internal class House
{
    private readonly List<Room> _rooms = [];
    public House()
    {
        _rooms.Add(new Room("first room"));
        _rooms.Add(new Room("second room"));
        
        List<Item> thirdRoomItems = [new Item("cheese"), new Item("eggs")];
        _rooms.Add(new Room("third room"));
        _rooms[2].SetItems(thirdRoomItems);
        
        _rooms[0].CreateDoor(_rooms[1]);
        _rooms[0].CreateDoor(_rooms[2]);
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
internal class Room(string roomName, int length = 5, int height = 5)
{
    public readonly string RoomName = roomName;
    public readonly int Length = length;
    public readonly int Height = height;
    
    private readonly List<Door> _doors = [];
    private List<Item> _items = [];

    public void CreateDoor(Room room, bool locked = false, bool reverse = false)
    {
        Door d = new Door(this, room, locked);
        _doors.Add(d);
        if (!reverse) room.CreateDoor(this, locked, true);
    }
    
    /// <summary>
    /// moves room 
    /// </summary>
    /// <param name="d">integer, the index of the door to be opened (see GetDoors())</param>
    /// <returns>returns room the door led to, if error, null</returns>
    public Room UseDoor(string d)
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
/// a door, when made a reverse door will also be made
/// </summary>
/// <param name="r1">where the door is</param>
/// <param name="r2">where the door will lead</param>
/// <param name="locked">if the door cannot be opened, defaults to locked</param>
internal class Door(Room r1, Room r2, bool locked)
{
    private bool _locked = locked;

    public Room UseDoor(Room r)
    {
        if (_locked) return r;
        
        if (r == r1) return r2;
        
        return r1;
    }

    public void SetLocked(bool locked)
    {
        _locked = locked;
    }
}