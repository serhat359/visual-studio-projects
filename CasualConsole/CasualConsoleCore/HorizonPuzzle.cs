using System;
using System.Collections.Generic;

namespace CasualConsoleCore;

public class HorizonPuzzle
{
    private enum NodeType
    {
        Narrow, // 60 degrees angle
        Wide, // 120 degrees angle
    }
    private enum EdgeDirection
    {
        UpperLeft,
        Upper,
        UpperRight,
        LowerRight,
        Lower,
        LowerLeft,
    }
    private class Node
    {
        public NodeType type;
        public int number;
        public bool canMove;

        public readonly Node?[] neighbors = new Node?[6];

        public Node(NodeType type, int number, bool canMove = true)
        {
            this.type = type;
            this.number = number;
            this.canMove = canMove;
        }
    }
    private class ArrayHashSet
    {
        private readonly bool[] visitedNodes = new bool[15];

        public ArrayHashSet(int visitedNode)
        {
            visitedNodes[visitedNode] = true;
        }

        public ArrayHashSet(ArrayHashSet set)
        {
            Array.Copy(set.visitedNodes, this.visitedNodes, this.visitedNodes.Length);
        }

        public bool Contains(int number)
        {
            return visitedNodes[number];
        }

        public void Add(int number)
        {
            //if (visitedNodes[number])
            //    throw new Exception();
            visitedNodes[number] = true;
        }
    }

    private record struct Passage(EdgeDirection direction, Node arrivingNode);
    private record struct FullPath(IReadOnlyList<Passage> path, ArrayHashSet visitedNodes);

    public static void SolveHorizonPuzzle()
    {
        var nodeList = new List<Node>
        {
            new Node(NodeType.Narrow, 0, canMove: false), // This is the destination
            new Node(NodeType.Narrow, 1),
            new Node(NodeType.Narrow, 2),
            new Node(NodeType.Narrow, 3),
            new Node(NodeType.Narrow, 4),
            new Node(NodeType.Narrow, 5),
            new Node(NodeType.Wide, 6),
            new Node(NodeType.Wide, 7),
            new Node(NodeType.Narrow, 8),
            new Node(NodeType.Wide, 9),
            new Node(NodeType.Wide, 10),
            new Node(NodeType.Wide, 11),
            new Node(NodeType.Wide, 12),
            new Node(NodeType.Narrow, 13),
            new Node(NodeType.Narrow, 14),
        };

        // Lower edges
        AddEdge(nodeList[1], EdgeDirection.Lower, nodeList[2]);
        AddEdge(nodeList[3], EdgeDirection.Lower, nodeList[4]);
        AddEdge(nodeList[4], EdgeDirection.Lower, nodeList[5]);
        AddEdge(nodeList[5], EdgeDirection.Lower, nodeList[6]);
        AddEdge(nodeList[7], EdgeDirection.Lower, nodeList[0]);
        AddEdge(nodeList[0], EdgeDirection.Lower, nodeList[9]);
        AddEdge(nodeList[9], EdgeDirection.Lower, nodeList[10]);
        AddEdge(nodeList[11], EdgeDirection.Lower, nodeList[12]);
        AddEdge(nodeList[12], EdgeDirection.Lower, nodeList[13]);
        AddEdge(nodeList[13], EdgeDirection.Lower, nodeList[14]);

        // Topright edges
        AddEdge(nodeList[1], EdgeDirection.UpperRight, nodeList[3]);
        AddEdge(nodeList[4], EdgeDirection.UpperRight, nodeList[7]);
        AddEdge(nodeList[7], EdgeDirection.UpperRight, nodeList[11]);
        AddEdge(nodeList[2], EdgeDirection.UpperRight, nodeList[5]);
        AddEdge(nodeList[5], EdgeDirection.UpperRight, nodeList[0]);
        AddEdge(nodeList[0], EdgeDirection.UpperRight, nodeList[12]);
        AddEdge(nodeList[12], EdgeDirection.UpperRight, nodeList[8]);
        AddEdge(nodeList[6], EdgeDirection.UpperRight, nodeList[9]);
        AddEdge(nodeList[9], EdgeDirection.UpperRight, nodeList[13]);
        AddEdge(nodeList[10], EdgeDirection.UpperRight, nodeList[14]);

        // Topleft edges
        AddEdge(nodeList[8], EdgeDirection.UpperLeft, nodeList[11]);
        AddEdge(nodeList[12], EdgeDirection.UpperLeft, nodeList[7]);
        AddEdge(nodeList[7], EdgeDirection.UpperLeft, nodeList[3]);
        AddEdge(nodeList[13], EdgeDirection.UpperLeft, nodeList[0]);
        AddEdge(nodeList[0], EdgeDirection.UpperLeft, nodeList[4]);
        AddEdge(nodeList[4], EdgeDirection.UpperLeft, nodeList[1]);
        AddEdge(nodeList[14], EdgeDirection.UpperLeft, nodeList[9]);
        AddEdge(nodeList[9], EdgeDirection.UpperLeft, nodeList[5]);
        AddEdge(nodeList[10], EdgeDirection.UpperLeft, nodeList[6]);
        AddEdge(nodeList[6], EdgeDirection.UpperLeft, nodeList[2]);

        var firstFullPath = new FullPath(new[] { new Passage(EdgeDirection.Lower, nodeList[9]), }, new ArrayHashSet(nodeList[9].number));

        Solve(firstFullPath);
    }

    private static void Solve(FullPath firstFullPath)
    {
        IReadOnlyList<FullPath> currentFullPathList = new List<FullPath> { firstFullPath };
        while (true)
        {
            var fullPathList = GetNextPaths(currentFullPathList);
            foreach (var fullPath in fullPathList)
            {
                if (fullPath.path[^1].arrivingNode.number == 0)
                {
                    foreach (var path in fullPath.path)
                    {
                        Console.WriteLine(path.direction);
                    }

                    Console.WriteLine("We found the solution!!!");
                    return;
                }
            }

            if (fullPathList.Count == 0)
                throw new Exception("No solution found");

            currentFullPathList = fullPathList;
        }
    }

    static void AddEdge(Node from, EdgeDirection direction, Node to)
    {
        switch (direction)
        {
            case EdgeDirection.UpperLeft:
                from.neighbors[(int)EdgeDirection.UpperLeft] = to;
                to.neighbors[(int)EdgeDirection.LowerRight] = from;
                break;
            case EdgeDirection.UpperRight:
                from.neighbors[(int)EdgeDirection.UpperRight] = to;
                to.neighbors[(int)EdgeDirection.LowerLeft] = from;
                break;
            case EdgeDirection.Lower:
                from.neighbors[(int)EdgeDirection.Lower] = to;
                to.neighbors[(int)EdgeDirection.Upper] = from;
                break;
            default:
                throw new Exception();
        }
    }

    static int NormalizeIndex(int index)
    {
        return (index + 6) % 6;
    }

    static void CheckAndAddToList(int newDirection, Node arrivingNode, FullPath fullPath, List<FullPath> newList)
    {
        var newNode = arrivingNode.neighbors[newDirection];
        if (newNode != null && !fullPath.visitedNodes.Contains(newNode.number))
        {
            if (newNode.canMove || (newNode.number == 0 && newDirection == 0))
            {
                var fullPathCopy = new List<Passage>(fullPath.path);
                fullPathCopy.Add(new Passage((EdgeDirection)newDirection, newNode));

                var visitedNodesCopy = new ArrayHashSet(fullPath.visitedNodes);
                visitedNodesCopy.Add(newNode.number);

                newList.Add(new FullPath(fullPathCopy, visitedNodesCopy));
            }
        }
    }

    static IReadOnlyList<FullPath> GetNextPaths(IReadOnlyList<FullPath> fullPathList)
    {
        var newList = new List<FullPath>();
        foreach (var fullPath in fullPathList)
        {
            var (path, visitedNodes) = fullPath;
            var (direction, arrivingNode) = path[^1];
            var directionIndex = (int)direction - 3;
            var newDirection1 = directionIndex + (arrivingNode.type == NodeType.Wide ? 2 : 1);
            var newDirection2 = directionIndex - (arrivingNode.type == NodeType.Wide ? 2 : 1);

            newDirection1 = NormalizeIndex(newDirection1);
            newDirection2 = NormalizeIndex(newDirection2);

            CheckAndAddToList(newDirection1, arrivingNode, fullPath, newList);
            CheckAndAddToList(newDirection2, arrivingNode, fullPath, newList);
        }

        return newList;
    }
}
