using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CasualConsole
{
    public class LaytonClockPuzzle
    {
        public static void TestLaytonClockPuzzle()
        {
            Clock[,] clocks;
            //clocks = GetFirstSet();
            clocks = GetSecondSet();

            NextIteration(clocks);
            Point current = new Point(0, 0);

            List<Movement> positions = new List<Movement> {
                new Movement {
                    position = current,
                    dirList = new List<Direction>()
                }
            };

            while (true)
            {
                if (positions.Count == 0)
                    throw new Exception("There is no point!!");

                positions = positions.SelectMany(mov => clocks[mov.position.Y, mov.position.X] == null ? new Movement[] { } : clocks[mov.position.Y, mov.position.X].directions.Select(dir =>
                {
                    var newPos = new Point(mov.position.X, mov.position.Y);
                    switch (dir)
                    {
                        case Direction.Up:
                            newPos.Y -= 1;
                            break;
                        case Direction.Down:
                            newPos.Y += 1;
                            break;
                        case Direction.Left:
                            newPos.X -= 1;
                            break;
                        case Direction.Right:
                            newPos.X += 1;
                            break;
                        default:
                            break;
                    }
                    var newList = mov.dirList.ToList();
                    newList.Add(dir);
                    return new Movement { position = newPos, dirList = newList };
                })).ToList();

                var solution = positions.FirstOrDefault(mov => mov.position.X == 6 && mov.position.Y == 3);
                if (solution != null)
                {
                    throw new Exception("We found the solution!!!!");
                }

                positions.RemoveAll(mov => mov.position.X < 0 || mov.position.Y < 0 || mov.position.X >= 6 || mov.position.Y >= 4);

                var s1 = positions.Select(x => x.position).Count();
                var s2 = positions.Select(x => x.position).Distinct().Count();

                NextIteration(clocks);
            }
        }

        private static Clock[,] GetFirstSet()
        {
            Clock[,] clocks = {
                { new Clock(Direction.Up), new Clock(Clock.UpLeft), new Clock(Direction.Left), null, new Clock(Direction.Up), new Clock(Direction.Down) },
                { new Clock(Direction.Up), new Clock(Clock.UpLeft), new Clock(Clock.LeftRight), new Clock(Clock.DownLeft), new Clock(Clock.UpRight), new Clock(Direction.Right) },
                { new Clock(Direction.Right), new Clock(Clock.LeftRight), new Clock(Clock.UpDown), new Clock(Direction.Down), new Clock(Direction.Up), new Clock(Clock.UpRight) },
                { null, new Clock(Direction.Left), new Clock(Direction.Right), new Clock(Direction.Down), new Clock(Clock.DownRight), new Clock(Direction.Up) },
            };

            foreach (var clock in clocks)
            {
                if (clock != null)
                    clock.isClockWise = true;
            }

            return clocks;
        }

        private static Clock[,] GetSecondSet()
        {
            Clock[,] clocks = {
                { new Clock(Direction.Down), new Clock(Clock.UpLeft), new Clock(Clock.UpRight), new Clock(Clock.LeftRight), new Clock(Clock.DownLeft), new Clock(Direction.Up) },
                { new Clock(Clock.UpDown), new Clock(Direction.Down), new Clock(Direction.Right), null, new Clock(Direction.Right), new Clock(Direction.Down) },
                { new Clock(Clock.LeftRight), null, new Clock(Direction.Left), new Clock(Clock.UpLeft), new Clock(Clock.LeftRight), new Clock(Direction.Down) },
                { new Clock(Clock.UpRight), new Clock(Clock.DownRight), new Clock(Clock.DownLeft), new Clock(Clock.UpLeft), new Clock(Clock.UpLeft), new Clock(Clock.DownLeft) },
            };

            int i = 0;
            foreach (var clock in clocks)
            {
                int row = i / 6;
                int col = i % 6;

                if (clock != null)
                    clock.isClockWise = (col % 2 == 1) ^ (row % 2 == 1);
                i++;
            }

            return clocks;
        }

        private static void NextIteration(Clock[,] clocks)
        {
            foreach (var clock in clocks)
            {
                if (clock != null)
                {
                    if (clock.isClockWise)
                        clock.directions = Clock.Next(clock.directions);
                    else
                    {
                        clock.directions = Clock.Next(clock.directions);
                        clock.directions = Clock.Next(clock.directions);
                        clock.directions = Clock.Next(clock.directions);
                    }
                }
            }
        }

    }

    enum Direction
    {
        Up, Down, Left, Right
    }

    class Clock
    {
        public Direction[] directions;
        public bool isClockWise;

        public static readonly Direction[] UpLeft = new Direction[] { Direction.Up, Direction.Left };
        public static readonly Direction[] UpRight = new Direction[] { Direction.Up, Direction.Right };
        public static readonly Direction[] DownLeft = new Direction[] { Direction.Down, Direction.Left };
        public static readonly Direction[] DownRight = new Direction[] { Direction.Down, Direction.Right };
        public static readonly Direction[] LeftRight = new Direction[] { Direction.Left, Direction.Right };
        public static readonly Direction[] UpDown = new Direction[] { Direction.Up, Direction.Down };

        public Clock(params Direction[] directions)
        {
            this.directions = directions.ToArray();
        }

        internal static Direction[] Next(Direction[] directions)
        {
            Func<Direction, Direction> next = x =>
            {
                switch (x)
                {
                    case Direction.Up:
                        return Direction.Right;
                    case Direction.Down:
                        return Direction.Left;
                    case Direction.Left:
                        return Direction.Up;
                    case Direction.Right:
                        return Direction.Down;
                    default:
                        throw new Exception();
                }
            };

            for (int i = 0; i < directions.Length; i++)
            {
                directions[i] = next(directions[i]);
            }

            return directions;
        }
    }

    class Movement
    {
        public Point position;
        public List<Direction> dirList;
    }
}
