namespace Data
{
    public record LevelParametersDTO
    {
        public int LevelNumber { get; init; }
        public int GridSize { get; init; }
        public int PieceAmount { get; init; }
    }
}