using System.Collections.Generic;
using Common;
using Common.Abstract;
using MCTS2016.Common.Abstract;
using MCTS2016.Optimizations.UCT;

namespace MCTS2016.SP_MCTS.Optimizations.Utils
{
	/// <summary>
	/// The object pool is a list of already instantiated object of the same type.
	/// </summary>
    public class ObjectPool
    {
	    //the list of objects.
	    private List<Opt_SP_UCTTreeNode> pooledObjects;

	    //sample of the actual object to store.
	    //used if we need to grow the list.
	    //private OptUCTTreeNode pooledObj;
	    
	    //maximum number of objects to have in the list.
	    private int maxPoolSize;

	    public bool NeedToClean;

	    //initial and default number of objects to have in the list.
	    //private int initialPoolSize;
	    
	    /// <summary>
	    /// Constructor for creating a new Object Pool.
	    /// </summary>
	    ///// <param name="node">ITreeNode for this pool</param>
	    /// <param name="iniPoolSize">Initial and default size of the pool.</param>
	    /// <param name="maxPoolSize">Maximum number of objects this pool can contain.</param>
	    /// <param name="nodeRecycling">Using or not node recycling optimization</param>
	    /// <param name="memoryBudget">Initial and default size of the pool if using node recyling optimization.</param>
	    //public ObjectPool(OptUCTTreeNode node, int initialPoolSize, int maxPoolSize)
	    public ObjectPool(int iniPoolSize, int maxPoolSize, bool nodeRecycling, int memoryBudget)
	    {
		    //instantiate a new list of objects to store our pooled objects in.
		    pooledObjects = new List<Opt_SP_UCTTreeNode>();

		    //initial number of objects to have in the list.
		    int initialPoolSize = nodeRecycling ? memoryBudget : iniPoolSize;

		    //create and add an object based on initial size.
		    for (int i = 0; i < initialPoolSize; i++)
		    {
			    //instantiate and create an object with useless attributes.
			    //these should be reset anyways.
			    //make sure the object isn't active.
			    Opt_SP_UCTTreeNode objNode = new Opt_SP_UCTTreeNode(null, null, null, null, false, false, 0, false, 1, 20000, generateUntriedMoves: false);

			    //add the object too our list.
			    pooledObjects.Add(objNode);
		    }

		    //store our other variables that are useful.
		    this.maxPoolSize = maxPoolSize;
		    NeedToClean = false;
		    //pooledObj = node;
		    //this.initialPoolSize = initialPoolSize;
	    }
	    
	    /// <summary>
	    /// Returns an active object from the object pool without resetting any of its values.
	    /// You will need to set its values and set it inactive again when you are done with it.
	    /// </summary>
	    /// <returns>ITreeNode of requested type if it is available, otherwise null.</returns>
	    public Opt_SP_UCTTreeNode GetObject(IPuzzleMove move, Opt_SP_UCTTreeNode parent, IPuzzleState state, MersenneTwister rng, bool ucb1Tuned, bool rave, double raveThreshold, bool nodeRecycling, double const_C, double const_D)
	    {
		    //iterate through all pooled objects.
		    foreach (Opt_SP_UCTTreeNode node in pooledObjects)
		    {
				//look for the first one that is inactive.
			    if (node.SetActive) continue;
			    //set the object to active.
			    node.SetActive = true;
			    
			    //set object's values
			    node.Move = move;
			    node.Parent = parent;
			    node.untriedMoves = state.GetMoves();
			    node.Rnd = rng;
			    node.Ucb1Tuned = ucb1Tuned;
			    node.Rave = rave;
                node.RaveThreshold = raveThreshold;
			    node.NodeRecycling = nodeRecycling;
			    node.ConstC = const_C;
			    node.ConstD = const_D;
			    
			    //return the object we found.
			    return node;
		    }
		    //if we make it this far, we obviously didn't find an inactive object.
		    //so we need to see if we can grow beyond our current count.
		    //if we reach the maximum size we didn't have any inactive objects.
		    //we also were unable to grow, so return null as we can't return an object.
		    if (maxPoolSize <= pooledObjects.Count) return null;
		    
		    //Instantiate a new object.
		    //set it to active since we are about to use it.
		    Opt_SP_UCTTreeNode objNode = new Opt_SP_UCTTreeNode(move, parent, state, rng, ucb1Tuned, rave, raveThreshold, nodeRecycling, const_C, const_D) {SetActive = true};
		    
		    //add it to the pool of objects
		    pooledObjects.Add(objNode);
		    
		    //return the object to the requestor.
		    return objNode;
	    }

	    public void CleanObjectPool()
	    {
		    //iterate through all pooled objects.
		    foreach (Opt_SP_UCTTreeNode node in pooledObjects)
		    {
			    //reset all nodes in the pool to the initial values
			    node.ResetNode();
		    }
	    }
    }
}