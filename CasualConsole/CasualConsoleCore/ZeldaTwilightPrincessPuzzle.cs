using System;
using System.Collections.Generic;
using System.Linq;

namespace CasualConsoleCore;

public class ZeldaTwilightPrincessPuzzle
{
    public static void Solve()
    {
        var stage = new int[][]
        {
            new int[]{1,1,0,1,1,},
            new int[]{1,1,1,1,1,},
            new int[]{1,1,1,1,1,},
            new int[]{0,1,1,1,0,},
            new int[]{0,1,1,1,0,},
            new int[]{0,0,1,0,0,},
        };
        var initialState = new State(new Point(3, 2), new Point(5, 2), new Point(1, 2));
        var states = new List<Path>() { new(initialState, null!) };
        while (true)
        {
            states = GetNextStates(states, stage);
            var list = states.Where(x => x.state.IsDone()).ToList();
            if (list.Count > 0)
            {
                var length = list[0].directions.GetLength();
                Console.WriteLine($"Found {list.Count} solutions for {length} moves");

                foreach (var (state, directions) in list)
                {
                    Console.WriteLine($"{directions}, link: {state.link}");
                }

                if (length >= 12) // Change this number to see solutions with higher number of moves
                {
                    return;
                }
            }
            //states.RemoveAll(x => x.state.IsDone());
        }
    }

    static readonly Direction[] allDirections = { Direction.Right, Direction.Left, Direction.Up, Direction.Down };
    static List<Path> GetNextStates(List<Path> states, int[][] stage)
    {
        var newStates = new List<Path>();
        foreach (var (state, directions) in states)
        {
            foreach (var dir in allDirections)
            {
                if (state.CanMove(dir, stage))
                {
                    var newState = state.Move(dir, stage);
                    if (newState.IsValid())// && !newStates.Any(x => x.state == newState))
                        newStates.Add(new(newState, Append(directions, dir)));
                }
            }
        }
        return newStates;
    }

    static DirectionNode Append(DirectionNode? source, Direction direction)
    {
        return new DirectionNode(direction, source);
    }

    static Direction OppositeOf(Direction dir)
    {
        return dir switch
        {
            Direction.Right => Direction.Left,
            Direction.Left => Direction.Right,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            _ => throw new Exception(),
        };
    }

    enum Direction : byte
    {
        Right,
        Left,
        Up,
        Down,
    }

    record class DirectionNode(Direction direction, DirectionNode? next)
    {
        public int GetLength()
        {
            var pointer = next;
            int count = 1;
            while (pointer != null)
            {
                pointer = pointer.next;
                count++;
            }
            return count;
        }

        public override string ToString()
        {
            var list = new List<Direction>();
            list.Add(this.direction);
            var pointer = this.next;
            while (pointer is not null)
            {
                list.Add(pointer.direction);
                pointer = pointer.next;
            }
            list.Reverse();
            return string.Join(",", list);
        }
    }

    readonly record struct Point(byte row, byte col)
    {
        public bool IsEqualTo(byte row, byte col)
        {
            return this.row == row && this.col == col;
        }

        public bool IsIn(int[][] stage)
        {
            // No need to check less than 0 since byte is unsigned so it would become 255 after decrement
            if (this.row >= stage.Length) return false;
            if (this.col >= stage[0].Length) return false;
            return stage[this.row][this.col] == 1;
        }

        public Point GetAdjacent(Direction dir)
        {
            return dir switch
            {
                Direction.Right => new Point(this.row, (byte)(this.col + 1)),
                Direction.Left => new Point(this.row, (byte)(this.col - 1)),
                Direction.Up => new Point((byte)(this.row - 1), this.col),
                Direction.Down => new Point((byte)(this.row + 1), this.col),
                _ => throw new Exception(),
            };
        }
    }

    readonly record struct State(Point link, Point sameGuard, Point oppositeGuard)
    {
        public bool IsDone()
        {
            return (this.sameGuard.IsEqualTo(1, 3) && this.oppositeGuard.IsEqualTo(1, 1))
             || (this.oppositeGuard.IsEqualTo(1, 3) && this.sameGuard.IsEqualTo(1, 1));
        }

        public bool IsValid()
        {
            if (this.link.Equals(this.sameGuard)) return false;
            if (this.link.Equals(this.oppositeGuard)) return false;
            return true;
        }

        public bool CanMove(Direction direction, int[][] stage)
        {
            return this.link.GetAdjacent(direction).IsIn(stage);
        }

        public State Move(Direction direction, int[][] stage)
        {
            var link = this.link.GetAdjacent(direction);
            var sameGuard = this.sameGuard.GetAdjacent(direction);
            var oppositeGuard = this.oppositeGuard.GetAdjacent(OppositeOf(direction));

            // If guard cannot move then reset their position
            if (!sameGuard.IsIn(stage))
                sameGuard = this.sameGuard;
            if (!oppositeGuard.IsIn(stage))
                oppositeGuard = this.oppositeGuard;

            return new State(link, sameGuard, oppositeGuard);
        }
    }

    readonly record struct Path(State state, DirectionNode directions);
}