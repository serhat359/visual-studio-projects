using System;
using System.Collections.Generic;
using System.Linq;
using static CasualConsole.LaytonBridgePuzzle;

namespace CasualConsole
{
    public static class LaytonBridgePuzzle
    {
        public static int[][] GetInitialRiver()
        {
            return new int[][] {
                new int[]{0,0,0,0,0,0,0,0,0,1,0,0},
                new int[]{0,0,0,0,0,1,0,0,0,1,0,0},
                new int[]{0,0,1,0,1,0,0,0,0,1,0,0},
                new int[]{1,0,0,0,1,0,0,0,1,0,0,0},
                new int[]{0,1,0,0,0,0,0,1,0,0,1,0},
                new int[]{0,0,0,1,0,0,0,1,0,0,1,0},
                new int[]{0,0,0,0,1,0,0,0,0,1,0,0},
                new int[]{0,0,1,0,0,0,0,1,0,0,0,0},
                new int[]{0,0,1,0,0,0,0,0,0,0,0,0}
            };
        }

        public static int[] GetSpeeds()
        {
            return new int[] {
                0,
                3,
                -4,
                1,
                4,
                -2,
                -3,
                4,
                0
            };
        }

        public enum MovementType
        {
            Up,
            DoubleUp,
            DoubleDown,
            Down,
            None
        }

        public static void Solve()
        {
            var stage = GetInitialRiver();

            var speeds = GetSpeeds();

            if (stage.Length != speeds.Length)
            {
                throw new Exception();
            }

            var pos = new Position(8, 2);
            var dest = new Position(0, 9);

            var positions = Enumerable.Repeat(pos, 1).ToList();

            while (!positions.Contains(dest))
            {
                var newPositions = new List<Position>();

                foreach (var currentposition in positions)
                {
                    var isTopAvailable = Try(() => stage[currentposition.row - 1][currentposition.col] == 1);
                    var isBottomAvailable = Try(() => stage[currentposition.row + 1][currentposition.col] == 1);
                    var isLeftAvailable = Try(() => stage[currentposition.row][currentposition.col - 1] == 1);
                    var isRightAvailable = Try(() => stage[currentposition.row][currentposition.col + 1] == 1);

                    if (isTopAvailable)
                    {
                        newPositions.Add(new Position(currentposition.row - 1, currentposition.col, currentposition.moves, MovementType.Up));

                        isTopAvailable = Try(() => stage[currentposition.row - 2][currentposition.col] == 1);
                        if (isTopAvailable)
                            newPositions.Add(new Position(currentposition.row - 2, currentposition.col, currentposition.moves, MovementType.DoubleUp));
                    }
                    if (isBottomAvailable)
                    {
                        newPositions.Add(new Position(currentposition.row + 1, currentposition.col, currentposition.moves, MovementType.Down));

                        isBottomAvailable = Try(() => stage[currentposition.row + 2][currentposition.col] == 1);
                        if(isBottomAvailable)
                            newPositions.Add(new Position(currentposition.row + 2, currentposition.col, currentposition.moves, MovementType.DoubleDown));
                    }

                    var speed = speeds[currentposition.row];
                    if (speed != 0)
                    {
                        newPositions.Add(new Position(currentposition.row, currentposition.col, currentposition.moves, MovementType.None));
                    }
                }

                positions = newPositions;

                #region MoveCharacter
                foreach (var position in positions)
                {
                    var speed = speeds[position.row];
                    position.col += speed;
                }

                var colLength = stage[0].Length;
                positions.RemoveAll(x => x.col >= colLength);
                positions.RemoveAll(x => x.col <= -1);
                #endregion

                #region MoveBoats
                var newStage = Enumerable.Range(0, stage.Length).Select(x => new int[stage[0].Length]).ToArray();

                for (int rowNumber = 0; rowNumber < stage.Length; rowNumber++)
                {
                    for (int colNumber = 0; colNumber < colLength; colNumber++)
                    {
                        if (stage[rowNumber][colNumber] == 1)
                        {
                            var speed = speeds[rowNumber];
                            var newColNumber = colNumber + speed;
                            newColNumber += colLength;
                            newColNumber = newColNumber % colLength;
                            newStage[rowNumber][newColNumber] = 1;
                        }
                    }
                }

                stage = newStage;
                #endregion

                if (!positions.Any())
                {
                    throw new Exception();
                }
            }

            var result = positions.First(x => x.Equals(dest));
            var allResults = positions.Where(x => x.Equals(dest)).ToList();

        }

        public static bool Try(Func<bool> func)
        {
            try
            {
                return func();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string Debug(this int[][] stage)
        {
            return string.Join("\n", stage.Select(x => string.Join(",", x)));
        }
    }

    public class Position : IEquatable<Position>
    {
        public int row;
        public int col;
        public List<MovementType> moves = new List<MovementType>();

        public Position(int row, int col)
        {
            this.row = row;
            this.col = col;
        }

        public Position(int row, int col, List<MovementType> oldMoves, MovementType move)
        {
            this.row = row;
            this.col = col;
            this.moves = oldMoves.ToList();
            this.moves.Add(move);
        }

        public bool Equals(Position other)
        {
            return this.row == other.row && this.col == other.col;
        }
    }
}
