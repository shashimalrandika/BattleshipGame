using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

class Program
{
    private static readonly HttpClient _httpClient = new() { BaseAddress = new Uri("https://localhost:7080/api/Game/") };
  
    private static Dictionary<(int, int), string> _board = new();
    private static int _destroyersSunk = 0;

    // Board Symbols
    private const string SHIP = "S";   
    private const string HIT = "X";    
    private const string MISS = "O";   
    private const string EMPTY = ".";  
    private const string SUNK = "#";   

    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("🔥 Welcome to Battleship! 🔥");
        Console.WriteLine("Enter coordinates (A1 - J10) to fire shots.");
        Console.WriteLine("Type 'reset' to restart, or 'exit' to quit.\n");

        await FetchShips();
        await PlayGame();
    }

    // Fetch ships and initialize the board
    static async Task FetchShips()
    {
        try
        {
            await _httpClient.PostAsync("reset", null); 
            Console.WriteLine("Game Reset!");

            var response = await _httpClient.GetStringAsync("ships");
            var ships = JsonSerializer.Deserialize<List<Ship>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (ships == null || ships.Count == 0)
            {
                Console.WriteLine("No ships found!");
                return;
            }

            _board.Clear();
            foreach (var ship in ships)
            {
                //  Show only the Battleship (size 5)
                if (ship.Positions.Count == 5)
                {
                    foreach (var pos in ship.Positions)
                    {
                        _board[(pos[0], pos[1])] = SHIP; // Place only battleship
                    }
                }
            }

            DrawBoard();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading ships: {ex.Message}");
        }
    }

    // Main game loop
    static async Task PlayGame()
    {
        while (true)
        {
            Console.Write("\nEnter your shot (e.g., A5): ");
            string input = Console.ReadLine()?.ToUpper();

            if (input == "EXIT") break;
            if (input == "RESET") { await FetchShips(); continue; }
            if (!IsValidInput(input)) { Console.WriteLine("Invalid input!"); continue; }

            var coords = ConvertInput(input);

            if (_board.ContainsKey(coords) && _board[coords] == SHIP)
            {
                Console.WriteLine("You cannot fire at your own battleship!");
                continue;
            }

            if (_board.ContainsKey(coords) && (_board[coords] == HIT || _board[coords] == MISS || _board[coords] == SUNK))
            {
                Console.WriteLine("Already fired at this position!");
                continue;
            }

            string result = await FireShot(input);
            UpdateBoard(coords, result);
            DrawBoard();

            if (_destroyersSunk == 2)
            {
                Console.WriteLine("\n BOOM! YOU SANK THEM ALL!");
                Console.WriteLine("Press ENTER to restart or type 'EXIT' to quit.");

                string finalInput = Console.ReadLine()?.ToUpper();
                if (finalInput == "EXIT") break;
                else await FetchShips(); 
            }
        }

        Console.WriteLine("Thanks for playing! Press ENTER to exit.");
        Console.ReadLine();
    }

    // Draw the game board
    static void DrawBoard()
    {
        Console.Clear();
        // added 3 spaces before to align with the row numbers
        Console.WriteLine("   A B C D E F G H I J");

        for (int row = 0; row < 10; row++)
        {
            Console.Write($"{row + 1,2} "); // row number (1-10) will have 2 spaces
            for (int col = 0; col < 10; col++)
            {
                Console.Write(_board.TryGetValue((row, col), out var value) ? $"{value} " : $"{EMPTY} ");
            }
            Console.WriteLine();
        }
    }

    // Fire a shot and process response
    static async Task<string> FireShot(string input)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("fire", new { position = input });

            if (!response.IsSuccessStatusCode)
                return $"API Error: {response.StatusCode}";

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    //Update board based on shot result
    static void UpdateBoard((int, int) coords, string result)
    {
        if (result.Contains("Sunk"))
        {
            var sunkPositions = ExtractSunkPositions(result);

            foreach (var pos in sunkPositions)
            {
                _board[pos] = SUNK; // Mark all ship positions as Sunk
            }

            _destroyersSunk++;
        }
        else if (result.Contains("Hit"))
        {
            _board[coords] = HIT;
        }
        else
        {
            _board[coords] = MISS;
        }
    }

    static List<(int, int)> ExtractSunkPositions(string response)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(response);
            var root = jsonDoc.RootElement;

            var sunkPositions = new List<(int, int)>();

            if (root.TryGetProperty("sunkPositions", out JsonElement positionsArray))
            {
                foreach (var pos in positionsArray.EnumerateArray())
                {
                    int row = pos[0].GetInt32();
                    int col = pos[1].GetInt32();
                    sunkPositions.Add((row, col));
                }
            }

            return sunkPositions;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing sunk positions: {ex.Message}");
            return new List<(int, int)>();
        }
    }


    // Input validation
    static bool IsValidInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length < 2 || input.Length > 3)
            return false; // Input must be 2 or 3 characters (e.g., A1, J10)

        char column = input[0]; // First character (A-J)
        string rowPart = input.Substring(1); // Remaining part (1-10)

        if (column < 'A' || column > 'J')
            return false; // Column must be A-J

        if (!int.TryParse(rowPart, out int row) || row < 1 || row > 10)
            return false; // Row must be a number between 1 and 10

        return true; // ✅ Input is valid
    }


    // Convert "A5" → (Row, Col)
    static (int, int) ConvertInput(string input)
    {
        char column = input[0]; // First character (A-J)
        int row = int.Parse(input.Substring(1)); // Extract the number part

        int rowIndex = row - 1; // Convert 1-based row to 0-based index
        int colIndex = ColumnMapping[column];

        return (rowIndex, colIndex);
    }

    private static readonly Dictionary<char, int> ColumnMapping = new()
{
    { 'A', 0 }, { 'B', 1 }, { 'C', 2 }, { 'D', 3 }, { 'E', 4 },
    { 'F', 5 }, { 'G', 6 }, { 'H', 7 }, { 'I', 8 }, { 'J', 9 }
};


}


class Ship
{
    public string Name { get; set; } = string.Empty;
    public List<int[]> Positions { get; set; } = new();
}
