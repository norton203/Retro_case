namespace RetroSite.Models;

public enum RoomHazard { None, Wumpus, Pit, Bats }

public class WumpusGame
{
    // The classic 20-room dodecahedron cave map — each room connects to exactly three others.
    // This layout is the original 1972 Hunt the Wumpus design (public domain).
    private static readonly Dictionary<int, int[]> Adjacency = new()
    {
        [1] = new[] { 2, 5, 8 },
        [2] = new[] { 1, 3, 10 },
        [3] = new[] { 2, 4, 12 },
        [4] = new[] { 3, 5, 14 },
        [5] = new[] { 1, 4, 6 },
        [6] = new[] { 5, 7, 15 },
        [7] = new[] { 6, 8, 17 },
        [8] = new[] { 1, 7, 9 },
        [9] = new[] { 8, 10, 18 },
        [10] = new[] { 2, 9, 11 },
        [11] = new[] { 10, 12, 19 },
        [12] = new[] { 3, 11, 13 },
        [13] = new[] { 12, 14, 20 },
        [14] = new[] { 4, 13, 15 },
        [15] = new[] { 6, 14, 16 },
        [16] = new[] { 15, 17, 20 },
        [17] = new[] { 7, 16, 18 },
        [18] = new[] { 9, 17, 19 },
        [19] = new[] { 11, 18, 20 },
        [20] = new[] { 13, 16, 19 },
    };

    private readonly Random _rng = new();

    public int PlayerRoom { get; private set; }
    public int WumpusRoom { get; private set; }
    public HashSet<int> PitRooms { get; private set; } = new();
    public HashSet<int> BatRooms { get; private set; } = new();
    public int Arrows { get; private set; } = 5;
    public bool IsOver { get; private set; }
    public bool Won { get; private set; }
    public List<string> Log { get; } = new();

    public int[] AdjacentRooms => Adjacency[PlayerRoom];

    public WumpusGame()
    {
        var rooms = Enumerable.Range(1, 20).OrderBy(_ => _rng.Next()).ToList();
        PlayerRoom = rooms[0];
        WumpusRoom = rooms[1];
        PitRooms = new HashSet<int> { rooms[2], rooms[3] };
        BatRooms = new HashSet<int> { rooms[4], rooms[5] };

        Log.Add("You creep into the cave mouth. Somewhere in these twenty rooms:");
        Log.Add("a Wumpus sleeps, two floors are bottomless pits, and two rooms host giant bats.");
        Log.Add("You have 5 arrows. Shoot the Wumpus before it — or the cave — gets you.");
        DescribeRoom();
    }

    private void DescribeRoom()
    {
        Log.Add($"— You are in room {PlayerRoom}. Tunnels lead to {string.Join(", ", AdjacentRooms)}. —");

        if (AdjacentRooms.Contains(WumpusRoom))
            Log.Add("You smell something terrible nearby.");
        if (AdjacentRooms.Any(r => PitRooms.Contains(r)))
            Log.Add("You feel a cold draft.");
        if (AdjacentRooms.Any(r => BatRooms.Contains(r)))
            Log.Add("You hear a rustling of wings.");
    }

    public void Move(int room)
    {
        if (IsOver || !AdjacentRooms.Contains(room)) return;

        PlayerRoom = room;

        if (PlayerRoom == WumpusRoom)
        {
            Log.Add("The Wumpus wakes and drags you into the dark. You have died.");
            IsOver = true;
            return;
        }

        if (PitRooms.Contains(PlayerRoom))
        {
            Log.Add("The floor gives way. You fall forever into a bottomless pit.");
            IsOver = true;
            return;
        }

        if (BatRooms.Contains(PlayerRoom))
        {
            Log.Add("Giant bats snatch you up and dump you somewhere else in the cave!");
            PlayerRoom = _rng.Next(1, 21);
            // Landing on a hazard after a bat drop is possible, per the original game — re-check.
            if (PlayerRoom == WumpusRoom || PitRooms.Contains(PlayerRoom))
            {
                Move(PlayerRoom); // re-run hazard check on the new room without consuming another move
                return;
            }
        }

        DescribeRoom();
    }

    public void Shoot(int room)
    {
        if (IsOver) return;
        if (!AdjacentRooms.Contains(room))
        {
            Log.Add("Your arrow clatters uselessly — that tunnel isn't connected to this room.");
            return;
        }

        Arrows--;
        Log.Add($"Thwip! Your arrow flies into room {room}...");

        if (room == WumpusRoom)
        {
            Log.Add("A shriek echoes through the cave. You got the Wumpus! You win!");
            IsOver = true;
            Won = true;
            return;
        }

        Log.Add("...and misses.");

        if (Arrows <= 0)
        {
            Log.Add("Out of arrows. The cave falls silent, then something finds you. Game over.");
            IsOver = true;
            return;
        }

        // Startled, the Wumpus has a chance to lumber into an adjacent room.
        if (_rng.Next(4) == 0)
        {
            WumpusRoom = Adjacency[WumpusRoom][_rng.Next(3)];
            if (WumpusRoom == PlayerRoom)
            {
                Log.Add("You hear a startled snort right next to you...");
                Log.Add("The Wumpus stumbles into your room and finds you first. You have died.");
                IsOver = true;
            }
        }
    }
}