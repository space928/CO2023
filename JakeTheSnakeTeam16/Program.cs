using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace JakeTheSnakeTeam16
{
    internal static class Program
    {
        private const int MAZE_GENERATOR_SIZE = 29;

        // Test maze from the brief
        private readonly static string[] TEST_DATA =
        {
            "+-+-+-+",
            "|     |",
            "+ +-+ +",
            "|  F| |",
            "+-+-+ +",
            "|$    |",
            "+-+-+-+"
        };

        // Constant offsets used for maze exploration
        private readonly static Vec2[] offsets = new Vec2[]
        {
            new(0, 1),
            new(0, -1),
            new(1, 0),
            new(-1, 0),
        };

        /// <summary>
        /// Entry method, when compiled in Debug runs the maze generator and solver and prints tohe results to the console.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string[] data;
#if DEBUG
            data = TEST_DATA;

            // Generate a random maze to explore
            data = GenerateTestMaze(MAZE_GENERATOR_SIZE, MAZE_GENERATOR_SIZE);
            foreach (var line in data)
            {
                Console.WriteLine(line);
            }
#else
            
#endif
            var result = SolveMaze(data);
            foreach(var item in result)
            {
                Console.WriteLine(item);
            }

#if DEBUG
            Vec2 start = new();
            for (int x = 0; x < data.Length; x++)
                for (int y = 0; y < data[x].Length; y++)
                    if (data[x][y] == '$')
                        start = new(y, x);

            Console.SetCursorPosition(0,0);
            Console.SetCursorPosition(start.x, start.y);
            foreach (var item in result.SkipLast(1))
            {
                switch (item)
                {
                    case "left":
                        Console.CursorLeft--; break;
                    case "right":
                        Console.CursorLeft++; break;
                    case "up":
                        Console.CursorTop--; break;
                    case "down":
                        Console.CursorTop++; break;
                }
                Console.Write("#");
                Console.CursorLeft--;
            }
#endif

#if DEBUG
            Console.ReadKey();
            Console.Clear();
            Main(new string[0]);
#endif
        }

        /// <summary>
        /// Generates a maze in the same form as the brief.
        /// </summary>
        /// <param name="width">width of the maze to generate, this should always be odd</param>
        /// <param name="height">height of the maze to generate, this should always be odd</param>
        /// <returns>an array of strings containing the ascii art of the maze</returns>
        private static string[] GenerateTestMaze(int width, int height)
        {
            var result = new string[height];
            StringBuilder sb = new();

            for (int x = 0; x < width; x++)
                sb.Append((x&1)==1? '=' : '+');
            result[0] = sb.ToString();
            sb.Clear();

            Random rnd = new();

            for(int y = 1; y < height-1; y++)
            {
                sb.Append((y & 1) == 1 ? '|' : '+');
                for (int x = 1; x < width-1; x++)
                {
                    if((y&1) == 1)
                        sb.Append((x&1)==1 ? ' ' : (((rnd.Next() > int.MaxValue >> 2) ? ' ' : '|')));
                    else
                        sb.Append((x&1) == 1 ? (((rnd.Next() > int.MaxValue >> 2) ? ' ' : '=')) : '+');
                }
                sb.Append((y & 1) == 1 ? '|' : '+');
                result[y] = sb.ToString();
                sb.Clear();
            }

            for (int x = 0; x < width; x++)
                sb.Append((x & 1) == 1 ? '=' : '+');
            result[height-1] = sb.ToString();

            (int sx, int sy) = (rnd.Next()%(width-2)|1, rnd.Next()%(height-2)|1);
            (int gx, int gy) = (rnd.Next()%(width-2)|1, rnd.Next()%(height-2)|1);
            // Prevent collisions lazily
            while(gx == sx && gy == sy)
                (gx, gy) = (rnd.Next() % (width - 2) | 1, rnd.Next() % (height - 2) | 1);

            result[sy] = result[sy].Remove(sx, 1).Insert(sx, "$");
            result[gy] = result[gy].Remove(gx, 1).Insert(gx, "F");

            return result;
        }

        /// <summary>
        /// Solves a maze given input as an array of strings in the format given in the brief. <para/>
        /// Implements the A* algorithm to solve the maze efficiently.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>An enumerable of strings with the directions to move to solve the maze.</returns>
        public static IEnumerable<string> SolveMaze(string[] data)
        {
            // Find the snake and the objective in the grid
            Vec2 start = new();
            Vec2 goal = new();

            for (int x = 0; x < data.Length; x++)
            {
                for (int y = 0; y < data[x].Length; y++)
                {
                    if (data[x][y] == '$')
                    {
                        start = new(x, y);
                    }
                    if (data[x][y] == 'F')
                    {
                        goal = new(x, y);
                    }
                }
            }

            // Now start exploring the maze, using the A* algorithm, from Wikipedia lol
            PriorityQueue<Vec2, float> openSet = new();
            // A dict mapping each node to the adjacent node which is the cheapest way to the start
            Dictionary<Vec2, Vec2> cameFrom = new();
            // A dict mapping each node to the cost of the cheapest path to the start
            Dictionary<Vec2, float> gScore = new();
            // fScore[n] = gScore[n] + h(n)
            Dictionary<Vec2, float> fScore = new();
            openSet.Enqueue(start, 0);

            gScore[start] = 0;
            fScore[start] = 0;

            // Iterate through the problem space until the maze is explored
            while (openSet.Count > 0)
            {
                Vec2 curr = openSet.Dequeue();

                if (curr == goal)
                    return ReconstructPath(cameFrom, curr);

                // Examine our neighbours for a better path
                foreach (var neighbour in GetNeighbours(curr, data))
                {
                    float ngScore = (gScore.ContainsKey(curr) ? gScore[curr] : float.MaxValue) + 1;

                    if(ngScore < (gScore.ContainsKey(neighbour) ? gScore[neighbour] : float.MaxValue))
                    {
                        cameFrom[neighbour] = curr;
                        gScore[neighbour] = ngScore;
                        fScore[neighbour] = ngScore + Heuristic(neighbour, goal);

                        openSet.Enqueue(neighbour, fScore[neighbour]);
                    }
                }
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// The heuristic method used to guide the a* algorithm. In this case it simply returns the euclidean distance to the goal from the current square.
        /// </summary>
        /// <param name="neighbour"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        private static float Heuristic(Vec2 neighbour, Vec2 goal)
        {
            return goal.Dist(neighbour);
        }

        /// <summary>
        /// Reconstructs the path as a series of direction commands from the solved dictionary.
        /// </summary>
        /// <param name="cameFrom"></param>
        /// <param name="curr"></param>
        /// <returns></returns>
        private static IEnumerable<string> ReconstructPath(Dictionary<Vec2, Vec2> cameFrom, Vec2 curr)
        {
            List<string> path = new() { };
            while (cameFrom.ContainsKey(curr))
            {
                var dir = curr - cameFrom[curr];
                switch(dir)
                {
                    case (0, 1):
                        path.Add("right");
                        break;
                    case (0, -1):
                        path.Add("left");
                        break;
                    case (1, 0):
                        path.Add("down");
                        break;
                    case (-1, 0):
                        path.Add("up");
                        break;
                }
                curr = cameFrom[curr];
            }

            return path.Reverse<string>();
        }

        /// <summary>
        /// Gets the neighbours of a given maze cell. 
        /// </summary>
        /// <param name="curr"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static IEnumerable<Vec2> GetNeighbours(Vec2 curr, string[] data)
        {
            foreach (var off in offsets)
            {
                Vec2 next = curr + off;
                if (data[next.x][next.y] == ' ' || data[next.x][next.y] == 'F')
                    yield return next;
            }
        }

        internal struct Vec2 : IEquatable<Vec2>
        {
            public int x;
            public int y;

            public Vec2()
            {
                x = 0;
                y = 0;
            }

            public Vec2(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public void Deconstruct(out int x, out int y)
            {
                x = this.x;
                y = this.y;
            }

            public static Vec2 operator +(Vec2 a, Vec2 b)
            {
                return new Vec2(a.x + b.x, a.y + b.y);
            }

            public static Vec2 operator -(Vec2 a, Vec2 b)
            {
                return new Vec2(a.x - b.x, a.y - b.y);
            }

            public override bool Equals(object? obj)
            {
                return obj is Vec2 vec && Equals(vec);
            }

            public bool Equals(Vec2 other)
            {
                return x == other.x &&
                       y == other.y;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(x, y);
            }

            public static bool operator ==(Vec2 left, Vec2 right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Vec2 left, Vec2 right)
            {
                return !(left == right);
            }

            public override string? ToString()
            {
                return $"x={x}; y={y}";
            }

            internal float Dist(Vec2 neighbour)
            {
                var d = this - neighbour;
                return (float)Math.Sqrt(d.x * d.x + d.y * d.y);
            }
        }
    }
}