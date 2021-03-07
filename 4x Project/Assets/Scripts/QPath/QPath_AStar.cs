using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QPath
{
    public class QPath_AStar
    {
        //does the actual pathfinding work
        public QPath_AStar( 
            IQPathWorld world, 
            IQPathUnit unit, 
            IQPathTile startTile, 
            IQPathTile endTile,
            CostEstimateDelegate costEstimateFunc
        )
        {
            //Do setup

            this.world = world;
            this.unit = unit;
            this.startTile = startTile;
            this.endTile = endTile;
            this.costEstimateFunc = costEstimateFunc;

            //Do we need to explicitily create a graph? (maybe not)
            
        }

        IQPathWorld world;
        IQPathUnit unit;
        IQPathTile startTile;
        IQPathTile endTile;
        CostEstimateDelegate costEstimateFunc;

        Queue<IQPathTile> path;

        public void DoWork()
        {
            Debug.Log("QPath_AStar::DoWork");
            path = new Queue<IQPathTile>();

            HashSet< IQPathTile > closedSet = new HashSet<IQPathTile>();//an array that is guaranteed to be unique

            //for openset use quill18's pathfinding priority queue
            PathfindingPriorityQueue<IQPathTile> openSet = new PathfindingPriorityQueue<IQPathTile>();
            //openSet of all our tiles is organized based on shortest distance first
            openSet.Enqueue(startTile, 0);

            Dictionary<IQPathTile, IQPathTile> came_From = new Dictionary<IQPathTile, IQPathTile>();

            Dictionary<IQPathTile, float> g_score = new Dictionary<IQPathTile, float>();
            g_score[startTile] = 0; //cost to get to our start tile is 0

            Dictionary<IQPathTile, float> f_score = new Dictionary<IQPathTile, float>(); 
            f_score[startTile] = costEstimateFunc(startTile, endTile); //includes the estimated cost of getting to that tile
        
            while (openSet.Count > 0)
            {
                IQPathTile current = openSet.Dequeue();

                if (current == endTile)
                {
                    Reconstruct_path(came_From, current);
                    return;
                }

                closedSet.Add(current);
                foreach (IQPathTile edge_neighbour in current.GetNeighbours())
                {
                    IQPathTile neighbour = edge_neighbour;

                    if (closedSet.Contains(neighbour))
                    {
                        continue; //ignore this already completed neighbor
                    }
                    
                    float total_pathfinding_cost_to_neighbor =
                        neighbour.AggregateCostToEnter(g_score[current], current, unit);


                    float tentative_g_score = g_score[current] + total_pathfinding_cost_to_neighbor;

                    if (openSet.Contains(neighbour) && tentative_g_score >= g_score[neighbour])
                    {
                        continue;
                    }

                    //This is either a new tile or a cheaper route to it
                    came_From[neighbour] = current;
                    g_score[neighbour] = tentative_g_score;
                    f_score[neighbour] = g_score[neighbour] + costEstimateFunc(neighbour, endTile);

                    openSet.EnqueueOrUpdate(neighbour, f_score[neighbour]);
                }   //foreach neighbour
            }   //while


        }

        private void Reconstruct_path(
        Dictionary<IQPathTile, IQPathTile> came_From,
        IQPathTile current)
        {
            // So at this point, current IS the goal.
            // So what we want to do is walk backwards through the Came_From
            // map, until we reach the "end" of that map...which will be
            // our starting node!
            Queue<IQPathTile> total_path = new Queue<IQPathTile>();
            total_path.Enqueue(current); // This "final" step is the path is the goal!

            while (came_From.ContainsKey(current))
            {
                /*    Came_From is a map, where the
                *    key => value relation is real saying
                *    some_node => we_got_there_from_this_node
                */

                current = came_From[current];
                total_path.Enqueue(current);
            }

            // At this point, total_path is a queue that is running
            // backwards from the END tile to the START tile, so let's reverse it.
            path = new Queue<IQPathTile>(total_path.Reverse());
        }

        public IQPathTile[] GetList()
        {
            return path.ToArray();
        }

    }
}

