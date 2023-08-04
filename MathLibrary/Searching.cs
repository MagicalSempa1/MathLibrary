using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public class Searching
    {
        int[][] list = new int[][]
        {
            new int[] { 1, 2, 3 },
            new int[] { 0, 4},
            new int[] { 0, 4, 5, 6 },
            new int[] { 0, 6 },
            new int[] { 1, 2, 7, 8 },
            new int[] { 2, 7, 8, 9 },
            new int[] { 2, 3, 8, 9 },
            new int[] { 4, 5, 10 },
            new int[] { 4, 5, 6, 10 },
            new int[] { 5, 6, 10 },
            new int[] { 7, 8, 9 }
        };

        bool[] visited = new bool[11];

        public void Clear() => visited = new bool[11];

        public bool BFS(int startNodeIndex, int searchNodeIndex)
        {
            if (startNodeIndex == searchNodeIndex)
                return true;
            visited[startNodeIndex] = true;
            var nodeIndexesList = list[startNodeIndex].Where(x => !visited[x]).ToArray();
            for (int i = 0; i < nodeIndexesList.Length; i++)
            {
                Console.WriteLine($"Из точки {startNodeIndex + 1} в {nodeIndexesList[i] + 1}");
                if (nodeIndexesList[i] == searchNodeIndex)
                    return true;
                visited[nodeIndexesList[i]] = true;
            }
            if (nodeIndexesList.Length == 0)
                return false;
            for (int i = 0; i < nodeIndexesList.Length; i++)
            {
                if (BFS(nodeIndexesList[i], searchNodeIndex))
                    return true;
            }
            return false;
        }

        public bool DFS(int startNodeIndex, int searchNodeIndex)
        {
            if (startNodeIndex == searchNodeIndex)
                return true;
            visited[startNodeIndex] = true;
            var nodeIndexesList = list[startNodeIndex].Where(x => !visited[x]).ToArray();
            for (int i = 0; i < nodeIndexesList.Length; i++)
            {
                Console.WriteLine($"Из точки {startNodeIndex + 1} в {nodeIndexesList[i] + 1}");
                if (nodeIndexesList[i] == searchNodeIndex)
                    return true;
                visited[nodeIndexesList[i]] = true;
                if (nodeIndexesList.Length == 0)
                    return false;
                if (DFS(nodeIndexesList[i], searchNodeIndex))
                    return true;
            }
            return false;
        }
    }
}
