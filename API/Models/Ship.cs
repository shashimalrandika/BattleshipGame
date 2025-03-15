namespace BattleshipAPI.Models
{
    public class Ship
    {
        public string Name { get; set; } = string.Empty;
        public List<int[]> Positions { get; set; } = new();
        public List<int[]> OriginalPositions { get; set; } = new List<int[]>();
    }
}
