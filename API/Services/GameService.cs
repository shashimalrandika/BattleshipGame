using BattleshipAPI.Models;

namespace BattleshipAPI.Services
{
    public class GameService
    {
        private readonly GameState _gameState = new(); // Stores the current game state
        private readonly Random _random = new();

        public GameService()
        {
            SetupGame();
        }

        public void SetupGame()
        {
            _gameState.Ships.Clear();
            _gameState.FiredShots.Clear();
            PlaceShips();

            //THis is not need now this was i added because ship.Positions keep removing the values and when i reload it didn't reset properly
            //foreach (var ship in _gameState.Ships)
            //{
            //    if (ship.Positions.Count == 0)
            //    {
            //        Console.WriteLine($"Warning: {ship.Name} has an empty positions list! Restoring...");
            //        ship.Positions = ship.OriginalPositions.Select(p => new int[] { p[0], p[1] }).ToList();
            //    }
            //}
        }

        private void PlaceShips()
        {
            List<(string Name, int Size)> shipList = new()
            {
                ("Battleship", 5),
                ("Destroyer1", 4),
                ("Destroyer2", 4)
            };


            foreach (var (name, size) in shipList)
            {
                while (true)
                {

                    bool horizontal = _random.Next(2) == 0;  // Randomly decide horizontal or vertical 
                    bool positiveDirection = _random.Next(2) == 0;  // Decide placement direction (left-right or right-left)

                    int x = positiveDirection ? _random.Next(0, 11 - size) : _random.Next(size - 1, 10);
                    int y = positiveDirection ? _random.Next(0, 11 - size) : _random.Next(size - 1, 10);

                    var positions = new List<int[]>();

                    for (int i = 0; i < size; i++)
                    {
                        int newX = horizontal ? (positiveDirection ? x + i : x - i) : x;
                        int newY = horizontal ? y : (positiveDirection ? y + i : y - i);

                        // Check if the position overlaps with existing ships
                        if (_gameState.Ships.Any(s => s.Positions.Any(p => p[0] == newX && p[1] == newY)))
                        {
                            Console.WriteLine($"Overlapping detected for {name} at ({newX},{newY}). Retrying...");
                            positions.Clear();
                            break;
                        }

                        positions.Add(new[] { newX, newY });
                    }

                    if (positions.Count == size)
                    {
                        Console.WriteLine($"Placed {name}: {string.Join(", ", positions.Select(p => $"({p[0]},{p[1]})"))}");


                        _gameState.Ships.Add(new Ship
                        {
                            Name = name,
                            Positions = positions.Select(p => new int[] { p[0], p[1] }).ToList(),
                            OriginalPositions = positions.Select(p => new int[] { p[0], p[1] }).ToList()
                        });

                        break; // Stop trying once the ship is placed successfully
                    }
                }
            }
        }

        public object FireShot(string input)
        {
            var (x, y) = ConvertInput(input); // Convert input format (e.g A5) to grid coordinates

            Console.WriteLine($"Shot received at ({x}, {y})");

            if (_gameState.FiredShots.Contains((x, y)))
                return new { message = "Already fired here!" };

            _gameState.FiredShots.Add((x, y));

            foreach (var ship in _gameState.Ships)
            {
                Console.WriteLine($"Before Shot: {ship.Name} has positions: {string.Join(", ", ship.Positions.Select(p => $"({p[0]},{p[1]})"))}");
                var targetPosition = ship.Positions.FirstOrDefault(p => p[0] == x && p[1] == y);
                if (targetPosition != null)
                {
                    ship.Positions.Remove(targetPosition); // Remove the hit position
                    Console.WriteLine($"After Shot: {ship.Name} has positions: {string.Join(", ", ship.Positions.Select(p => $"({p[0]},{p[1]})"))}");

                    if (ship.Positions.Count == 0) // Ship has been sunk
                    {
                        Console.WriteLine($"{ship.Name} Sunk!");
                        return new
                        {
                            message = $"{ship.Name} Sunk!",
                            sunkShipName = ship.Name,
                            sunkPositions = ship.OriginalPositions
                        };
                    }

                    return new { message = "Hit!" };
                }
            }

            return new { message = "Miss!" };
        }


        private static readonly Dictionary<char, int> ColumnMapping = new()
{
    { 'A', 0 }, { 'B', 1 }, { 'C', 2 }, { 'D', 3 }, { 'E', 4 },
    { 'F', 5 }, { 'G', 6 }, { 'H', 7 }, { 'I', 8 }, { 'J', 9 }
};
        private (int, int) ConvertInput(string input)
        {
            int x = ColumnMapping[input[0]];
            int y = int.Parse(input.Substring(1)) - 1;

            Console.WriteLine($"API Received Shot '{input}' , Converted to (Row: {y}, Col: {x})");
            return (y, x);
        }

        public List<Ship> GetShips() => _gameState.Ships;
    }
}
