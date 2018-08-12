namespace MazeEditor
{
    public class GridBoard
    {
        public int SizeX { get; }

        public int SizeY { get; }

        public NodeType[,] Grid;

        public GridBoard(int sizex, int sizey)
        {
            SizeX = sizex;
            SizeY = sizey;

            Grid = new NodeType[sizex, sizey];
        }

        public void SetGridNodeType(int x, int y, NodeType nodetype)
        {
            Grid[x, y] = nodetype;
        }

    }
}
