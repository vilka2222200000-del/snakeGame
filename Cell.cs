using System;

namespace snakeGame
{
    public struct Cell : IEquatable<Cell>
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Cell other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Cell)
                return Equals((Cell)obj);

            return false;
        }

        public override int GetHashCode()
        {
            return X * 397 ^ Y;
        }

        public static bool operator ==(Cell left, Cell right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Cell left, Cell right)
        {
            return !left.Equals(right);
        }
    }
}
