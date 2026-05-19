namespace CratePuzzleSolver;

readonly record struct Point(sbyte row, sbyte col)
{
    public bool Up(out Point point)
    {
        point = new Point((sbyte)(row - 1), col);
        return row >= 1;
    }
    public bool Down(out Point point)
    {
        point = new Point((sbyte)(row + 1), col);
        return row < 4;
    }
    public bool Left(out Point point)
    {
        point = new Point(row, (sbyte)(col - 1));
        return col >= 1;
    }
    public bool Right(out Point point)
    {
        point = new Point(row, (sbyte)(col + 1));
        return col < 4;
    }
}

record struct State(
    sbyte mrow, sbyte mcol
  , sbyte b1row, sbyte b1col
  , sbyte b2row, sbyte b2col
  , sbyte b3row, sbyte b3col
  , sbyte b4row, sbyte b4col)
{
    public int FindBox(Point p)
    {
        if (p.row == b1row & p.col == b1col)
            return 1;
        if (p.row == b2row & p.col == b2col)
            return 2;
        if (p.row == b3row & p.col == b3col)
            return 3;
        if (p.row == b4row & p.col == b4col)
            return 4;
        return -1;
    }

    public void SetBox(int i, Point p)
    {
        switch (i)
        {
            case 1:
                b1row = p.row;
                b1col = p.col;
                return;
            case 2:
                b2row = p.row;
                b2col = p.col;
                return;
            case 3:
                b3row = p.row;
                b3col = p.col;
                return;
            case 4:
                b4row = p.row;
                b4col = p.col;
                return;
            default:
                throw new Exception();
        }
    }
}

record class Node(Direction dir, Node? next);

enum Direction
{
    None,
    Up,
    Right,
    Down,
    Left,
}

class Program
{
    const int IMMOVABLE = 1;

    static int[][] stage;

    static void Main(string[] args)
    {
        stage = new[]
        {
            new []{1,0,0,0,1},
            new []{0,0,0,0,0},
            new []{0,0,1,0,0},
            new []{0,0,0,0,0},
            new []{1,0,0,0,0},
        };

        var states = new List<(State state, Node? node)> { (new State(2, 0, 1, 1, 1, 3, 3, 1, 3, 3), null) };
        int iteration = 0;

        while (true)
        {
            var solutions = states.Where(x => IsComplete(x.state)).ToList();
            if (solutions.Count > 0)
            {
                Console.WriteLine($"We found the solution: iteration count: {iteration}");
                foreach (var solution in solutions)
                {
                    var moves = BuildMoves(solution.node);
                    foreach (var (i, move) in moves.Index())
                    {
                        if (i > 0 && i % 5 == 0)
                            Console.WriteLine();
                        Console.WriteLine($"{i+1}: {move}");
                    }
                }
                return;
            }

            var newStates = new List<(State state, Node? node)>();
            var uniqueStates = new HashSet<State>();
            foreach (var (state, lastNode) in states)
            {
                if (TryMove(Direction.Up, state, out var newState))
                {
                    if (uniqueStates.Add(newState))
                        newStates.Add((newState, new Node(Direction.Up, lastNode)));
                }
                if (TryMove(Direction.Down, state, out newState))
                {
                    if (uniqueStates.Add(newState))
                        newStates.Add((newState, new Node(Direction.Down, lastNode)));
                }
                if (TryMove(Direction.Left, state, out newState))
                {
                    if (uniqueStates.Add(newState))
                        newStates.Add((newState, new Node(Direction.Left, lastNode)));
                }
                if (TryMove(Direction.Right, state, out newState))
                {
                    if (uniqueStates.Add(newState))
                        newStates.Add((newState, new Node(Direction.Right, lastNode)));
                }
            }

            states = newStates;

            iteration++;
            if (iteration >= 100)
                throw new Exception("iteration count exceeded 100");
            Console.WriteLine($"Iteration: {iteration}, state count: {states.Count}");
        }
    }

    static List<Direction> BuildMoves(Node? node)
    {
        var moves = new List<Direction>();
        while (node != null)
        {
            moves.Add(node.dir);
            node = node.next;
        }
        moves.Reverse();
        return moves;
    }

    static bool TryMove(Direction dir, State state, out State newState)
    {
        var position = new Point(row: state.mrow, col: state.mcol);
        switch (dir)
        {
            case Direction.Up:
                {
                    if (position.Up(out var p))
                    {
                        if (stage[p.row][p.col] == IMMOVABLE)
                        {
                            newState = default;
                            return false;
                        }
                        var i = state.FindBox(p);
                        if (i < 0)
                        {
                            // no box, just empty
                            newState = state with { mrow = p.row, mcol = p.col };
                            return true;
                        }
                        if (p.Up(out var p2))
                        {
                            // has box
                            if (stage[p2.row][p2.col] == IMMOVABLE)
                            {
                                newState = default;
                                return false;
                            }
                            var i2 = state.FindBox(p2);
                            if (i2 >= 0)
                            {
                                newState = default;
                                return false;
                            }

                            newState = state with { mrow = p.row, mcol = p.col };
                            newState.SetBox(i, p2);
                            return true;
                        }
                    }
                    break;
                }
            case Direction.Down:
                {
                    if (position.Down(out var p))
                    {
                        if (stage[p.row][p.col] == IMMOVABLE)
                        {
                            newState = default;
                            return false;
                        }
                        var i = state.FindBox(p);
                        if (i < 0)
                        {
                            // no box, just empty
                            newState = state with { mrow = p.row, mcol = p.col };
                            return true;
                        }
                        if (p.Down(out var p2))
                        {
                            // has box
                            if (stage[p2.row][p2.col] == IMMOVABLE)
                            {
                                newState = default;
                                return false;
                            }
                            var i2 = state.FindBox(p2);
                            if (i2 >= 0)
                            {
                                newState = default;
                                return false;
                            }

                            newState = state with { mrow = p.row, mcol = p.col };
                            newState.SetBox(i, p2);
                            return true;
                        }
                    }
                    break;
                }
            case Direction.Left:
                {
                    if (position.Left(out var p))
                    {
                        if (stage[p.row][p.col] == IMMOVABLE)
                        {
                            newState = default;
                            return false;
                        }
                        var i = state.FindBox(p);
                        if (i < 0)
                        {
                            // no box, just empty
                            newState = state with { mrow = p.row, mcol = p.col };
                            return true;
                        }
                        if (p.Left(out var p2))
                        {
                            // has box
                            if (stage[p2.row][p2.col] == IMMOVABLE)
                            {
                                newState = default;
                                return false;
                            }
                            var i2 = state.FindBox(p2);
                            if (i2 >= 0)
                            {
                                newState = default;
                                return false;
                            }

                            newState = state with { mrow = p.row, mcol = p.col };
                            newState.SetBox(i, p2);
                            return true;
                        }
                    }
                    break;
                }
            case Direction.Right:
                {
                    if (position.Right(out var p))
                    {
                        if (stage[p.row][p.col] == IMMOVABLE)
                        {
                            newState = default;
                            return false;
                        }
                        var i = state.FindBox(p);
                        if (i < 0)
                        {
                            // no box, just empty
                            newState = state with { mrow = p.row, mcol = p.col };
                            return true;
                        }
                        if (p.Right(out var p2))
                        {
                            // has box
                            if (stage[p2.row][p2.col] == IMMOVABLE)
                            {
                                newState = default;
                                return false;
                            }
                            var i2 = state.FindBox(p2);
                            if (i2 >= 0)
                            {
                                newState = default;
                                return false;
                            }

                            newState = state with { mrow = p.row, mcol = p.col };
                            newState.SetBox(i, p2);
                            return true;
                        }
                    }
                    break;
                }
            default:
                throw new Exception();
        }
        newState = default;
        return false;
    }

    static bool IsComplete(State state)
    {
        if (state.b1col != 4)
            return false;
        if (state.b2col != 4)
            return false;
        if (state.b3col != 4)
            return false;
        if (state.b4col != 4)
            return false;
        return true;
    }
}
