using Raylib_cs;
using System.Numerics;

namespace MinesweeperRaylib
{
    public class Game
    {
        public const int MAP_SIZE = 16;
        public const int TILE_RES = 32;
        public const float BOMB_CHANCE = 0.2f;
        public static Tile[] MapTiles = [];
        public static bool IsDead = false;

        private static void Main(string[] args)
        {
            RegenerateMap();
            Raylib.InitWindow(MAP_SIZE * TILE_RES, MAP_SIZE * TILE_RES, "Minesweeper");

            bool firstClick = true;
            while (!Raylib.WindowShouldClose())
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    if (IsDead)
                    {
                        firstClick = true;
                        RegenerateMap();
                        IsDead = false;
                    }
                    else
                    {
                        Vector2 mousePos = Raylib.GetMousePosition();
                        for (int i = 0; i < MapTiles.Length; i++)
                        {
                            Rectangle tileRect = new Rectangle(i % MAP_SIZE * TILE_RES, i / MAP_SIZE * TILE_RES, TILE_RES, TILE_RES);
                            if (!MapTiles[i].IsExposed && Raylib.CheckCollisionPointRec(mousePos, tileRect))
                            {
                                if (firstClick)
                                {
                                    while (MapTiles[i].IsBomb || MapTiles[i].NeighborBombs > 0)
                                        RegenerateMap();
                                    firstClick = false;
                                }
                                MapTiles[i].IsExposed = true;
                                MapTiles[i].IsFlagged = false;

                                if (MapTiles[i].IsBomb)
                                    IsDead = true;
                                else if (MapTiles[i].NeighborBombs == 0)
                                    Flood(i % MAP_SIZE, i / MAP_SIZE);
                            }
                        }
                    }
                }

                if (Raylib.IsMouseButtonPressed(MouseButton.Right))
                {
                    if (IsDead)
                    {
                        firstClick = true;
                        RegenerateMap();
                        IsDead = false;
                    }
                    else if (!firstClick)
                    {
                        Vector2 mousePos = Raylib.GetMousePosition();
                        for (int i = 0; i < MapTiles.Length; i++)
                        {
                            Rectangle tileRect = new Rectangle(i % MAP_SIZE * TILE_RES, i / MAP_SIZE * TILE_RES, TILE_RES, TILE_RES);
                            if (!MapTiles[i].IsExposed && Raylib.CheckCollisionPointRec(mousePos, tileRect))
                                MapTiles[i].IsFlagged = !MapTiles[i].IsFlagged;
                        }
                    }
                }

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                for (int i = 0; i < MapTiles.Length; i++)
                    MapTiles[i].Render(i % MAP_SIZE * TILE_RES, i / MAP_SIZE * TILE_RES);

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        public static void RegenerateMap()
        {
            Random rand = new Random();
            MapTiles = new Tile[MAP_SIZE * MAP_SIZE];
            int bombSlots = (int)(MapTiles.Length * BOMB_CHANCE);
            for (int i = 0; i < MapTiles.Length; i++)
                MapTiles[i] = new Tile { IsBomb = rand.NextSingle() <= BOMB_CHANCE && bombSlots-- > 0 };

            for (int x = 0; x < MAP_SIZE; x++)
                for (int y = 0; y < MAP_SIZE; y++)
                    if (MapTiles[x + y * MAP_SIZE].IsBomb)
                        for (int ix = x - 1; ix <= x + 1; ix++)
                            for (int iy = y - 1; iy <= y + 1; iy++)
                                if (ix >= 0 && iy >= 0 && ix < MAP_SIZE && iy < MAP_SIZE && !(ix == x && iy == y))
                                    MapTiles[ix + iy * MAP_SIZE].NeighborBombs++;

        }

        public static void Flood(int x, int y)
        {
            List<(int, int)> floodChecks = [(x, y)];
            for (int i = 0; i < floodChecks.Count; i++)
            {
                x = floodChecks[i].Item1;
                y = floodChecks[i].Item2;
                MapTiles[x + y * MAP_SIZE].IsExposed = true;

                if (MapTiles[x + y * MAP_SIZE].NeighborBombs > 0)
                    continue;

                for (int ix = x - 1; ix <= x + 1; ix++)
                    for (int iy = y - 1; iy <= y + 1; iy++)
                        if (ix >= 0 && iy >= 0 && ix < MAP_SIZE && iy < MAP_SIZE && !floodChecks.Contains((ix, iy)))
                            floodChecks.Add((ix, iy));
            }
        }
    }

    public class Tile
    {
        public bool IsBomb = false;
        public bool IsFlagged = false;
        public bool IsExposed = false;
        public int NeighborBombs = 0;

        public void Render(int x, int y)
        {
            if (!IsExposed)
                Raylib.DrawRectangle(x + 1, y + 1, Game.TILE_RES - 2, Game.TILE_RES - 2, IsFlagged ? Color.Yellow : Color.Gray);
            else if (IsBomb)
                Raylib.DrawRectangle(x + 1, y + 1, Game.TILE_RES - 2, Game.TILE_RES - 2, Color.Red);
            else
            {
                Raylib.DrawRectangle(x + 1, y + 1, Game.TILE_RES - 2, Game.TILE_RES - 2, Color.White);
                if (NeighborBombs > 0)
                {
                    int meas = Raylib.MeasureText(NeighborBombs.ToString(), Game.TILE_RES - 4);
                    Raylib.DrawText(NeighborBombs.ToString(), x + Game.TILE_RES / 2 - meas / 2, y + 2, Game.TILE_RES - 4, Color.Black);
                }
            }
        }
    }
}
