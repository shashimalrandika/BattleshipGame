namespace BattleshipAPI.Models
{
    public class GameState
    {
        public List<Ship> Ships { get; set; } = new List<Ship>();
        public HashSet<(int, int)> FiredShots { get; set; } = new HashSet<(int, int)>();

    }
}
