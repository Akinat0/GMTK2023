using System.Collections.Generic;
using System.Linq;

public class GridPathCost
{
    public GridController Grid { get; }

    public GridPathCost(GridController grid)
    {
        Grid = grid;
    }

    public void Evaluate(GridItem startItem)
    {
        foreach (var item in Grid.Items.Where(item => item != null))
            item.PathCost = -1;


        List<GridItem> openList = new List<GridItem>(); 
        List<GridItem> closedList = new List<GridItem>();
        
        openList.Add(startItem); //add start item
        startItem.PathCost = 0;

        int counter = 0;
        
        while (openList.Count > 0)
        {
            counter++;
            if(counter > 10000)
                return;
            
            GridItem currentItem = openList[0];
            closedList.Add(currentItem);
            openList.RemoveAt(0);
            
            foreach (GridItem neighbour in currentItem.GetNeighbours())
            {
                if(closedList.Contains(neighbour))
                    continue;
                
                openList.Add(neighbour);
                neighbour.PathCost = currentItem.PathCost + 1;
            }

            
            openList.Sort((lhs, rhs) => lhs.PathCost.CompareTo(rhs.PathCost));
        }
    }


}
