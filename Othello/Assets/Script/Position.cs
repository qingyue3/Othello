

using System.Data;

public class Position
{
    public int Row { get; }
    public int Column { get; }

    public Position(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public override bool Equals(object obj)
    {
        if (obj is Position other)
        {
            return Row == other.Row && Column == other.Column;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return 8 * Row + Column;
    }
}