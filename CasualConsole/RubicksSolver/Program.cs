namespace RubicksSolver;

internal class Program
{
    static void Main(string[] args)
    {
        FixRubicksCubeVar();
    }

    const byte Red = 16;
    const byte Blue = 17;
    const byte Yellow = 18;
    const byte Green = 19;
    const byte Orange = 20;
    const byte White = 21;

    enum Move
    {
        Left,
        LeftR,
        Right,
        RightR,
        Top,
        TopR,
        Bottom,
        BottomR,
        Front,
        FrontR,
        Back,
        BackR,
    }

    class CubeState
    {
        byte cubeFront1;
        byte cubeFront2;
        byte cubeFront3;
        byte cubeFront4;
        byte cubeBack1;
        byte cubeBack2;
        byte cubeBack3;
        byte cubeBack4;
        byte cubeLeft1;
        byte cubeLeft2;
        byte cubeLeft3;
        byte cubeLeft4;
        byte cubeRight1;
        byte cubeRight2;
        byte cubeRight3;
        byte cubeRight4;
        byte cubeTop1;
        byte cubeTop2;
        byte cubeTop3;
        byte cubeTop4;
        byte cubeBottom1;
        byte cubeBottom2;
        byte cubeBottom3;
        byte cubeBottom4;
        public Stack<Move> stack = new Stack<Move>(50);

        public CubeState(byte cubeFront1, byte cubeFront2, byte cubeFront3, byte cubeFront4, byte cubeBack1, byte cubeBack2, byte cubeBack3, byte cubeBack4, byte cubeLeft1, byte cubeLeft2, byte cubeLeft3, byte cubeLeft4, byte cubeRight1, byte cubeRight2, byte cubeRight3, byte cubeRight4, byte cubeTop1, byte cubeTop2, byte cubeTop3, byte cubeTop4, byte cubeBottom1, byte cubeBottom2, byte cubeBottom3, byte cubeBottom4)
        {
            this.cubeFront1 = cubeFront1;
            this.cubeFront2 = cubeFront2;
            this.cubeFront3 = cubeFront3;
            this.cubeFront4 = cubeFront4;
            this.cubeBack1 = cubeBack1;
            this.cubeBack2 = cubeBack2;
            this.cubeBack3 = cubeBack3;
            this.cubeBack4 = cubeBack4;
            this.cubeLeft1 = cubeLeft1;
            this.cubeLeft2 = cubeLeft2;
            this.cubeLeft3 = cubeLeft3;
            this.cubeLeft4 = cubeLeft4;
            this.cubeRight1 = cubeRight1;
            this.cubeRight2 = cubeRight2;
            this.cubeRight3 = cubeRight3;
            this.cubeRight4 = cubeRight4;
            this.cubeTop1 = cubeTop1;
            this.cubeTop2 = cubeTop2;
            this.cubeTop3 = cubeTop3;
            this.cubeTop4 = cubeTop4;
            this.cubeBottom1 = cubeBottom1;
            this.cubeBottom2 = cubeBottom2;
            this.cubeBottom3 = cubeBottom3;
            this.cubeBottom4 = cubeBottom4;
        }

        public CubeState Clone()
        {
            return new CubeState(
                cubeFront1,
                cubeFront2,
                cubeFront3,
                cubeFront4,
                cubeBack1,
                cubeBack2,
                cubeBack3,
                cubeBack4,
                cubeLeft1,
                cubeLeft2,
                cubeLeft3,
                cubeLeft4,
                cubeRight1,
                cubeRight2,
                cubeRight3,
                cubeRight4,
                cubeTop1,
                cubeTop2,
                cubeTop3,
                cubeTop4,
                cubeBottom1,
                cubeBottom2,
                cubeBottom3,
                cubeBottom4
                );
        }

        public bool isSolved()
        {
            return cubeFront1 == cubeFront2
                && cubeFront1 == cubeFront3
                && cubeFront1 == cubeFront4

                && cubeBack1 == cubeBack2
                && cubeBack1 == cubeBack3
                && cubeBack1 == cubeBack4

                && cubeLeft1 == cubeLeft2
                && cubeLeft1 == cubeLeft3
                && cubeLeft1 == cubeLeft4

                && cubeRight1 == cubeRight2
                && cubeRight1 == cubeRight3
                && cubeRight1 == cubeRight4

                && cubeTop1 == cubeTop2
                && cubeTop1 == cubeTop3
                && cubeTop1 == cubeTop4

                && cubeBottom1 == cubeBottom2
                && cubeBottom1 == cubeBottom3
                && cubeBottom1 == cubeBottom4
                ;
        }

        public void front()
        {
            byte top3 = cubeTop3;
            byte top4 = cubeTop4;
            cubeTop3 = cubeLeft4;
            cubeTop4 = cubeLeft2;
            cubeLeft2 = cubeBottom1;
            cubeLeft4 = cubeBottom2;
            cubeBottom1 = cubeRight3;
            cubeBottom2 = cubeRight1;
            cubeRight3 = top4;
            cubeRight1 = top3;

            byte t1 = cubeFront1;
            cubeFront1 = cubeFront3;
            cubeFront3 = cubeFront4;
            cubeFront4 = cubeFront2;
            cubeFront2 = t1;
        }
        public void frontR()
        {
            byte top3 = cubeTop3;
            byte top4 = cubeTop4;
            cubeTop3 = cubeRight1;
            cubeTop4 = cubeRight3;
            cubeRight1 = cubeBottom2;
            cubeRight3 = cubeBottom1;
            cubeBottom2 = cubeLeft4;
            cubeBottom1 = cubeLeft2;
            cubeLeft2 = top4;
            cubeLeft4 = top3;

            byte t1 = cubeFront1;
            cubeFront1 = cubeFront2;
            cubeFront2 = cubeFront4;
            cubeFront4 = cubeFront3;
            cubeFront3 = t1;
        }

        public void back()
        {
            byte top1 = cubeTop1;
            byte top2 = cubeTop2;
            cubeTop1 = cubeRight2;
            cubeTop2 = cubeRight4;
            cubeRight2 = cubeBottom4;
            cubeRight4 = cubeBottom3;
            cubeBottom4 = cubeLeft3;
            cubeBottom3 = cubeLeft1;
            cubeLeft3 = top1;
            cubeLeft1 = top2;

            byte t1 = cubeBack1;
            cubeBack1 = cubeBack3;
            cubeBack3 = cubeBack4;
            cubeBack4 = cubeBack2;
            cubeBack2 = t1;
        }
        public void backR()
        {
            byte top1 = cubeTop1;
            byte top2 = cubeTop2;
            cubeTop1 = cubeLeft3;
            cubeTop2 = cubeLeft1;
            cubeLeft3 = cubeBottom4;
            cubeLeft1 = cubeBottom3;
            cubeBottom4 = cubeRight2;
            cubeBottom3 = cubeRight4;
            cubeRight2 = top1;
            cubeRight4 = top2;

            byte t1 = cubeBack1;
            cubeBack1 = cubeBack2;
            cubeBack2 = cubeBack4;
            cubeBack4 = cubeBack3;
            cubeBack3 = t1;
        }

        public void left()
        {
            byte front1 = cubeFront1;
            byte front3 = cubeFront3;
            cubeFront1 = cubeTop1;
            cubeFront3 = cubeTop3;
            cubeTop1 = cubeBack1;
            cubeTop3 = cubeBack3;
            cubeBack1 = cubeBottom1;
            cubeBack3 = cubeBottom3;
            cubeBottom1 = front1;
            cubeBottom3 = front3;

            byte t1 = cubeLeft1;
            cubeLeft1 = cubeLeft3;
            cubeLeft3 = cubeLeft4;
            cubeLeft4 = cubeLeft2;
            cubeLeft2 = t1;
        }
        public void leftR()
        {
            byte front1 = cubeFront1;
            byte front3 = cubeFront3;
            cubeFront1 = cubeBottom1;
            cubeFront3 = cubeBottom3;
            cubeBottom1 = cubeBack1;
            cubeBottom3 = cubeBack3;
            cubeBack1 = cubeTop1;
            cubeBack3 = cubeTop3;
            cubeTop1 = front1;
            cubeTop3 = front3;

            byte t1 = cubeLeft1;
            cubeLeft1 = cubeLeft2;
            cubeLeft2 = cubeLeft4;
            cubeLeft4 = cubeLeft3;
            cubeLeft3 = t1;
        }

        public void right()
        {
            byte front2 = cubeFront2;
            byte front4 = cubeFront4;
            cubeFront2 = cubeBottom2;
            cubeFront4 = cubeBottom4;
            cubeBottom2 = cubeBack2;
            cubeBottom4 = cubeBack4;
            cubeBack2 = cubeTop2;
            cubeBack4 = cubeTop4;
            cubeTop2 = front2;
            cubeTop4 = front4;

            byte t1 = cubeRight1;
            cubeRight1 = cubeRight3;
            cubeRight3 = cubeRight4;
            cubeRight4 = cubeRight2;
            cubeRight2 = t1;
        }
        public void rightR()
        {
            byte front2 = cubeFront2;
            byte front4 = cubeFront4;
            cubeFront2 = cubeTop2;
            cubeFront4 = cubeTop4;
            cubeTop2 = cubeBack2;
            cubeTop4 = cubeBack4;
            cubeBack2 = cubeBottom2;
            cubeBack4 = cubeBottom4;
            cubeBottom2 = front2;
            cubeBottom4 = front4;

            byte t1 = cubeRight1;
            cubeRight1 = cubeRight2;
            cubeRight2 = cubeRight4;
            cubeRight4 = cubeRight3;
            cubeRight3 = t1;
        }

        public void top()
        {
            byte left1 = cubeLeft1;
            byte left2 = cubeLeft2;
            cubeLeft1 = cubeFront1;
            cubeLeft2 = cubeFront2;
            cubeFront1 = cubeRight1;
            cubeFront2 = cubeRight2;
            cubeRight1 = cubeBack4;
            cubeRight2 = cubeBack3;
            cubeBack4 = left1;
            cubeBack3 = left2;

            byte t1 = cubeTop1;
            cubeTop1 = cubeTop3;
            cubeTop3 = cubeTop4;
            cubeTop4 = cubeTop2;
            cubeTop2 = t1;
        }
        public void topR()
        {
            byte left1 = cubeLeft1;
            byte left2 = cubeLeft2;
            cubeLeft1 = cubeBack4;
            cubeLeft2 = cubeBack3;
            cubeBack3 = cubeRight2;
            cubeBack4 = cubeRight1;
            cubeRight1 = cubeFront1;
            cubeRight2 = cubeFront2;
            cubeFront1 = left1;
            cubeFront2 = left2;

            byte t1 = cubeTop1;
            cubeTop1 = cubeTop2;
            cubeTop2 = cubeTop4;
            cubeTop4 = cubeTop3;
            cubeTop3 = t1;
        }

        public void bottom()
        {
            byte right3 = cubeRight3;
            byte right4 = cubeRight4;
            cubeRight3 = cubeFront3;
            cubeRight4 = cubeFront4;
            cubeFront3 = cubeLeft3;
            cubeFront4 = cubeLeft4;
            cubeLeft3 = cubeBack2;
            cubeLeft4 = cubeBack1;
            cubeBack2 = right3;
            cubeBack1 = right4;

            byte t1 = cubeBottom1;
            cubeBottom1 = cubeBottom3;
            cubeBottom3 = cubeBottom4;
            cubeBottom4 = cubeBottom2;
            cubeBottom2 = t1;
        }
        public void bottomR()
        {
            byte right3 = cubeRight3;
            byte right4 = cubeRight4;
            cubeRight3 = cubeBack2;
            cubeRight4 = cubeBack1;
            cubeBack2 = cubeLeft3;
            cubeBack1 = cubeLeft4;
            cubeLeft3 = cubeFront3;
            cubeLeft4 = cubeFront4;
            cubeFront3 = right3;
            cubeFront4 = right4;

            byte t1 = cubeBottom1;
            cubeBottom1 = cubeBottom2;
            cubeBottom2 = cubeBottom4;
            cubeBottom4 = cubeBottom3;
            cubeBottom3 = t1;
        }

        public void TrySolve()
        {
            int tryMoveCount = 1;

            while (true)
            {
                Console.WriteLine($"Trying {tryMoveCount} move solutions");
                bool isSuccess = TrySolution(tryMoveCount);
                if (isSuccess)
                {
                    break;
                }
                tryMoveCount++;
            }

            foreach (var item in stack.Reverse())
            {
                Console.WriteLine(item);
            }
        }

        bool TrySolution(int count)
        {
            bool willCheckForSolution = count == 1;

            var stackpeek = stack.Peek();
            if (stackpeek != Move.TopR)
            {
                top();
                stack.Push(Move.Top);
                if (willCheckForSolution) { if (isSolved()) return true; }
                else { bool isSuccess = TrySolution(count - 1); if (isSuccess) return true; }
                topR();
                stack.Pop(); 
            }

            if (stackpeek != Move.Top)
            {
                topR();
                stack.Push(Move.TopR);
                if (willCheckForSolution) { if (isSolved()) return true; }
                else { bool isSuccess = TrySolution(count - 1); if (isSuccess) return true; }
                top();
                stack.Pop(); 
            }

            if (stackpeek != Move.BottomR)
            {
                bottom();
                stack.Push(Move.Bottom);
                if (willCheckForSolution) { if (isSolved()) return true; }
                else { bool isSuccess = TrySolution(count - 1); if (isSuccess) return true; }
                bottomR();
                stack.Pop(); 
            }

            if (stackpeek != Move.Bottom)
            {
                bottomR();
                stack.Push(Move.BottomR);
                if (willCheckForSolution) { if (isSolved()) return true; }
                else { bool isSuccess = TrySolution(count - 1); if (isSuccess) return true; }
                bottom();
                stack.Pop(); 
            }

            if (stackpeek != Move.LeftR)
            {
                left();
                stack.Push(Move.Left);
                if (willCheckForSolution) { if (isSolved()) return true; }
                else { bool isSuccess = TrySolution(count - 1); if (isSuccess) return true; }
                leftR();
                stack.Pop(); 
            }

            if (stackpeek != Move.Left)
            {
                leftR();
                stack.Push(Move.LeftR);
                if (willCheckForSolution) { if (isSolved()) return true; }
                else { bool isSuccess = TrySolution(count - 1); if (isSuccess) return true; }
                left();
                stack.Pop(); 
            }

            if (stackpeek != Move.RightR)
            {
                right();
                stack.Push(Move.Right);
                if (willCheckForSolution) { if (isSolved()) return true; }
                else { bool isSuccess = TrySolution(count - 1); if (isSuccess) return true; }
                rightR();
                stack.Pop(); 
            }

            if (stackpeek != Move.Right)
            {
                rightR();
                stack.Push(Move.RightR);
                if (willCheckForSolution) { if (isSolved()) return true; }
                else { bool isSuccess = TrySolution(count - 1); if (isSuccess) return true; }
                right();
                stack.Pop(); 
            }

            if (stackpeek != Move.FrontR)
            {
                front();
                stack.Push(Move.Front);
                if (willCheckForSolution) { if (isSolved()) return true; }
                else { bool isSuccess = TrySolution(count - 1); if (isSuccess) return true; }
                frontR();
                stack.Pop(); 
            }

            if (stackpeek != Move.Front)
            {
                frontR();
                stack.Push(Move.FrontR);
                if (willCheckForSolution) { if (isSolved()) return true; }
                else { bool isSuccess = TrySolution(count - 1); if (isSuccess) return true; }
                front();
                stack.Pop(); 
            }

            if (stackpeek != Move.BackR)
            {
                back();
                stack.Push(Move.Back);
                if (willCheckForSolution) { if (isSolved()) return true; }
                else { bool isSuccess = TrySolution(count - 1); if (isSuccess) return true; }
                backR();
                stack.Pop(); 
            }

            if (stackpeek != Move.Back)
            {
                backR();
                stack.Push(Move.BackR);
                if (willCheckForSolution) { if (isSolved()) return true; }
                else { bool isSuccess = TrySolution(count - 1); if (isSuccess) return true; }
                back();
                stack.Pop(); 
            }

            return false;
        }
    }

    private static void FixRubicksCubeVar()
    {
        var cube = new CubeState(
            // front
            Red,
            Red,
            Yellow,
            White,

            // back
            Green,
            Green,
            Green,
            Orange,

            // left
            Yellow,
            White,
            Orange,
            Blue,

            // right
            White,
            Yellow,
            Orange,
            Orange,

            // top
            Red,
            Blue,
            Blue,
            Green,

            // bottom
            Red,
            Blue,
            White,
            Yellow);

        var tasks = new List<Task>();
        tasks.Add(Task.Run(() =>
        {
            var clone = cube.Clone();
            clone.top();
            clone.stack.Push(Move.Top);
            clone.TrySolve();
        }));
        tasks.Add(Task.Run(() =>
        {
            var clone = cube.Clone();
            clone.topR();
            clone.stack.Push(Move.TopR);
            clone.TrySolve();
        }));
        tasks.Add(Task.Run(() =>
        {
            var clone = cube.Clone();
            clone.bottom();
            clone.stack.Push(Move.Bottom);
            clone.TrySolve();
        }));
        tasks.Add(Task.Run(() =>
        {
            var clone = cube.Clone();
            clone.bottomR();
            clone.stack.Push(Move.BottomR);
            clone.TrySolve();
        }));
        tasks.Add(Task.Run(() =>
        {
            var clone = cube.Clone();
            clone.left();
            clone.stack.Push(Move.Left);
            clone.TrySolve();
        }));
        tasks.Add(Task.Run(() =>
        {
            var clone = cube.Clone();
            clone.leftR();
            clone.stack.Push(Move.LeftR);
            clone.TrySolve();
        }));
        tasks.Add(Task.Run(() =>
        {
            var clone = cube.Clone();
            clone.right();
            clone.stack.Push(Move.Right);
            clone.TrySolve();
        }));
        tasks.Add(Task.Run(() =>
        {
            var clone = cube.Clone();
            clone.rightR();
            clone.stack.Push(Move.RightR);
            clone.TrySolve();
        }));
        tasks.Add(Task.Run(() =>
        {
            var clone = cube.Clone();
            clone.front();
            clone.stack.Push(Move.Front);
            clone.TrySolve();
        }));
        tasks.Add(Task.Run(() =>
        {
            var clone = cube.Clone();
            clone.frontR();
            clone.stack.Push(Move.FrontR);
            clone.TrySolve();
        }));
        tasks.Add(Task.Run(() =>
        {
            var clone = cube.Clone();
            clone.back();
            clone.stack.Push(Move.Back);
            clone.TrySolve();
        }));
        tasks.Add(Task.Run(() =>
        {
            var clone = cube.Clone();
            clone.backR();
            clone.stack.Push(Move.BackR);
            clone.TrySolve();
        }));

        Console.WriteLine("Started looking for a solution..");
        var finishedTask = Task.WaitAny(tasks.ToArray());
        Console.WriteLine("Found a solution!");

        //Console.WriteLine("Here is the solution:");
        //cube.TrySolve();


    }
}