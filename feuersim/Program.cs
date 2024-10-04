using System;
using System.Collections.Specialized;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;

namespace feuersim;

class Program
{
    static void Main(string[] args)
    {
        //für emoji anzeigen
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Elements elements = new Elements();


        bool programRunning = true;
        bool start = true;
        int mapSideLenght = 0;
        string[,] map = null;
        int w = 70;//fire spread chance
        int chanceTreeReSpawn = 75;
        int chanceFire = 50;
        int[,] agedMap;
        mapSideLenght = getMapWidth();
        map = new string[mapSideLenght, mapSideLenght];
        agedMap = new int[mapSideLenght, mapSideLenght];
        while (programRunning)
        {

            if (start)
            {



                generateMap(map);
                printMap(map);
                start = false;
                if (agedMap == null)
                {
                    Console.WriteLine("Error: agedMap is not initialized.");
                    break;
                }
            }





            printMap(map);
            //PrintGrid(agedMap);

            map = feuerGrow(map);
            fireStarter(map, mapSideLenght, chanceFire);
            (map, agedMap) = reGenMap(map, agedMap, chanceTreeReSpawn);



            Thread.Sleep(1000);










        }



        static int getMapWidth()
        {
            Console.WriteLine("Bitte gib an, wie groß der Wald sein soll (Quadrat):");
            int mapSideLength;
            string eingabe = Console.ReadLine();


            if (int.TryParse(eingabe, out mapSideLength))
            {
                Console.WriteLine("Danke");
                return mapSideLength;
            }
            else
            {
                Console.WriteLine("Bitte eine richtige Zahl eingeben.");
                return -1;
            }
        }

        static string[,] generateMap(string[,] map)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    map[i, j] = randomElementPicker();
                }
            }

            GenerateClusters(map, Elements.stein, 2, 10, 20, 80);
            //GenerateClusters(map, Elements.see, 4, 3, 12, 80);
            GenerateLakeClusters(map, 4, 5, 20);


            GenerateRivers(map, 3, 12, 50);

            return map;
        }



        static string randomElementPicker()
        {
            var elements = new (string Element, int CumulativeProbability)[]
            {
                (Elements.baum, 65),     // 65%
                (Elements.stein, 85),    // 20% 
                (Elements.baum, 95)     // 15% 
                
            };

            int rand = getRandNum(0, 101);

            foreach (var (element, cumulativeProbability) in elements)
            {
                if (rand < cumulativeProbability)
                {
                    return element;
                }
            }

            return Elements.hummus;
        }

        static void GenerateClusters(string[,] map, string element, int clusterCount, int minSize, int maxSize, int chance)
        {

            int mapSize = map.GetLength(0);

            for (int c = 0; c < clusterCount; c++)
            {

                int clusterX = getRandNum(0, mapSize);
                int clusterY = getRandNum(0, mapSize);


                int clusterWidth = getRandNum(minSize, maxSize + 1);
                int clusterHeight = getRandNum(minSize, maxSize + 1);

                for (int i = clusterX; i < Math.Min(clusterX + clusterWidth, mapSize); i++)
                {
                    for (int j = clusterY; j < Math.Min(clusterY + clusterHeight, mapSize); j++)
                    {
                        int r = getRandNum(1, 100);
                        if (r < chance)
                        {
                            map[i, j] = element;
                            GetSurroundingCoordinates(i, j, map);
                        }
                    }
                }
            }
        }

        static void GenerateLakeClusters(string[,] map, int clusterCount, int minSize, int maxSize)
        {
            Random random = new Random();
            int mapSize = map.GetLength(0);

            for (int c = 0; c < clusterCount; c++)
            {
                int startX = random.Next(0, mapSize);
                int startY = random.Next(0, mapSize);

                // Random size for the lake
                int size = random.Next(minSize, maxSize + 1);

                // Use a HashSet to keep track of lake positions and avoid duplicates
                HashSet<(int, int)> lakePositions = new HashSet<(int, int)>();
                lakePositions.Add((startX, startY));

                // Grow the lake
                Queue<(int, int)> positionsToCheck = new Queue<(int, int)>();
                positionsToCheck.Enqueue((startX, startY));

                while (positionsToCheck.Count > 0 && lakePositions.Count < size)
                {
                    var currentPos = positionsToCheck.Dequeue();

                    // Check surrounding tiles to grow the lake
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            // Avoid diagonal movement
                            if (Math.Abs(dx) == Math.Abs(dy)) continue;

                            int newX = currentPos.Item1 + dx;
                            int newY = currentPos.Item2 + dy;

                            if (newX >= 0 && newX < mapSize && newY >= 0 && newY < mapSize &&
                                !lakePositions.Contains((newX, newY)))
                            {
                                // Random chance to add this position to the lake
                                if (random.NextDouble() < 0.5) // Adjust this value to control lake shape
                                {
                                    lakePositions.Add((newX, newY));
                                    positionsToCheck.Enqueue((newX, newY));
                                }
                            }
                        }
                    }
                }

                // Fill the map with the lake positions
                foreach (var pos in lakePositions)
                {
                    map[pos.Item1, pos.Item2] = Elements.see;
                }
            }
        }





        static string[,] fireStarter(string[,] map, int mapSideLenght, int chanceFire)
        {

            Random random = new Random();
            int fireOption = random.Next(0, mapSideLenght - 1);


            int fireOption1 = random.Next(0, mapSideLenght - 1);

            int rand2 = random.Next(0, 102);




            if (map[fireOption, fireOption1] != Elements.feuer)
            {
                //fire chance 

                if (rand2 < chanceFire)
                {
                    //making shure fire starts on trees!!
                    if (map[fireOption, fireOption1] == Elements.baum)
                    {


                        map[fireOption, fireOption1] = Elements.feuer;
                        chanceFire -= chanceFire / 4;
                    }
                }

            }





            return map;
        }

        static void printMap(string[,] map)
        {
            //whole string for buffer  
            StringBuilder mapStringBuilder = new StringBuilder();

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    mapStringBuilder.Append(map[i, j]);
                }

                mapStringBuilder.AppendLine();
            }

            Console.Clear();
            Console.Write(mapStringBuilder.ToString());
        }



        string[,] feuerGrow(string[,] map)
        {

            List<vec2> tempFireCoordinates = new List<vec2>();
            string[,] dCopyMap = DeepCopy(map);

            //jede crodiante anschauen
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] == Elements.feuer)
                    {


                        tempFireCoordinates = GetSurroundingCoordinates(i, j, map);
                        for (int k = 0; k < tempFireCoordinates.Count; k++)
                        {
                            if (agedMap[k, tempFireCoordinates.Count] <= 0)
                            {
                                int rand1 = getRandNum(1, 100);
                                if (rand1 < w)
                                {
                                    dCopyMap[tempFireCoordinates[k].x, tempFireCoordinates[k].y] = Elements.feuer;
                                }

                            }


                        }



                    }

                }
            }





            return dCopyMap;
        }

        static List<vec2> GetSurroundingCoordinates(int x, int y, string[,] map)
        {
            List<vec2> neighbors = new List<vec2>();


            int maxX = map.GetLength(0);
            int maxY = map.GetLength(1);


            int[,] offsets = new int[,]
            {
                {-1,  0}, // Up
                { 1,  0}, // Down
                { 0, -1}, // Left
                { 0,  1}, // Right
                {-1, -1}, // Up-Left
                {-1,  1}, // Up-Right
                { 1, -1}, // Down-Left
                { 1,  1}  // Down-Right
            };


            for (int i = 0; i < offsets.GetLength(0); i++)
            {
                int newX = x + offsets[i, 0];
                int newY = y + offsets[i, 1];


                if (newX >= 0 && newX < maxX && newY >= 0 && newY < maxY)
                {

                    if (map[newX, newY] == Elements.baum)
                    {
                        neighbors.Add(new vec2(newX, newY));
                    }
                }
            }

            return neighbors;
        }


        static int getRandNum(int von, int bis)
        {
            Random random = new Random();
            int rand = random.Next(von, bis);

            return rand;
        }

        static void GenerateRivers(string[,] map, int riverCount, int minLength, int maxLength)
        {
            Random random = new Random();
            int mapSize = map.GetLength(0);

            for (int r = 0; r < riverCount; r++)
            {
                // Start the river on a random edge
                int startX = 0, startY = 0;
                int direction = random.Next(4); // 0=Top 1=Bottom  2=Left 3=Right

                switch (direction)
                {
                    case 0: // topedge
                        startX = random.Next(mapSize);
                        startY = 0;
                        break;
                    case 1: // botttom edge
                        startX = random.Next(mapSize);
                        startY = mapSize - 1;
                        break;
                    case 2: // leftedge
                        startX = 0;
                        startY = random.Next(mapSize);
                        break;
                    case 3: // right edge 
                        startX = mapSize - 1;
                        startY = random.Next(mapSize);
                        break;
                }

                int length = random.Next(minLength, maxLength + 1);
                int currentX = startX;
                int currentY = startY;

                // logik richtung
                for (int i = 0; i < length; i++)
                {
                    // check das im range map arary 
                    if (currentX >= 0 && currentX < mapSize && currentY >= 0 && currentY < mapSize)
                    {
                        map[currentX, currentY] = Elements.river;
                    }

                    // random direction 
                    int turn = random.Next(3);
                    switch (direction)
                    {
                        case 0: //t-b
                            currentY++;
                            if (turn == 1 && currentX > 0) currentX--; // left
                            if (turn == 2 && currentX < mapSize - 1) currentX++; // right
                            break;
                        case 1: // b-t
                            currentY--;
                            if (turn == 1 && currentX > 0) currentX--; // ^left
                            if (turn == 2 && currentX < mapSize - 1) currentX++; // right
                            break;
                        case 2: // r-l
                            currentX++;
                            if (turn == 1 && currentY > 0) currentY--; // Turn up
                            if (turn == 2 && currentY < mapSize - 1) currentY++; // Turn down
                            break;
                        case 3: // l-r
                            currentX--;
                            if (turn == 1 && currentY > 0) currentY--; // Turn up
                            if (turn == 2 && currentY < mapSize - 1) currentY++; // Turn down
                            break;
                    }

                    //  check if out of bound
                    if (currentX < 0 || currentX >= mapSize || currentY < 0 || currentY >= mapSize)
                    {
                        break;
                    }
                }
            }
        }

        static (string[,], int[,]) reGenMap(string[,] map, int[,] mapHistorie, int treechance)
        {
            if (map.GetLength(0) != mapHistorie.GetLength(0) || map.GetLength(1) != mapHistorie.GetLength(1))
            {
                throw new ArgumentException("map and mapHistorie must be the same dimensions.");
            }


            string[,] dCopy = DeepCopy(map);

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {

                    if (map[i, j] == Elements.feuer)
                    {
                        mapHistorie[i, j]++;
                        if (mapHistorie[i, j] == 5)
                        {
                            mapHistorie[i, j] = 0;
                            map[i, j] = Elements.ash;
                        }
                    }

                    if (map[i, j] == Elements.hummus)
                    {
                        int lol = getRandNum(1, 100);
                        mapHistorie[i, j]++;
                        if (lol < treechance)
                        {
                            if (mapHistorie[i, j] == 5)
                            {
                                mapHistorie[i, j] = 0;
                                map[i, j] = Elements.baum;
                            }
                        }
                        else
                        {
                            mapHistorie[i, j] = 0;
                        }
                    }

                    if (map[i, j] == Elements.ash)
                    {


                        mapHistorie[i, j]++;

                        if (mapHistorie[i, j] == 6)
                        {
                            mapHistorie[i, j] = 0;
                            map[i, j] = Elements.hummus;


                            mapHistorie[i, j] = 0;

                        }
                    }


                }
            }

            return (map, mapHistorie);
        }



        static string[,] DeepCopy(string[,] input)
        {
            string[,] deepCopy = new string[input.GetLength(0), input.GetLength(1)];

            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    deepCopy[i, j] = input[i, j]; // Copy each element
                }
            }
            return deepCopy;
        }


        static void PrintGrid(int[,] array)
        {
            int rows = array.GetLength(0);
            int columns = array.GetLength(1);

            // Loop each 
            for (int i = 0; i < rows; i++)
            {

                for (int j = 0; j < columns; j++)
                {

                    Console.Write(array[i, j].ToString().PadRight(4));
                }

                Console.WriteLine();
            }
        }

    }
}

public class vec2
{
    public int x { get; set; }
    public int y { get; set; }

    public vec2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}




class Elements
{

    public static string baum = "\ud83c\udf33";
    public static string stein = "\ud83e\udea8";
    public static string boden = "\ud83d\udfe9";
    public static string feuer = "\ud83d\udd25";
    public static string hummus = "\ud83d\udfeb";
    public static string see = "\ud83d\udfe6";
    public static string river = "\ud83d\udfe6";
    public static string ash = "🔺";

    public static int spawnChanceBaum;
    public static int spawnChanceStein;
    public static int spawnChanceFeuer;
    public static int spawnChanceHummus;

}


