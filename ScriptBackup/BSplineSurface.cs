using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BSplineSurface
{

    /*
     * THESE VALUES MUST BE SET BEFORE CALCULATING
     */
    //size of the control net (e.g. 3x4 net)
    public int NI = 4; //setting these to high can crash unity
    public int NJ = 4;
    //Grid of control points
    public Vector3[,] controlGrid;
    //The degree in each direction
    public int TI = 3;
    public int TJ = 3;
    //output GRID resolution (e.g. 30x40)
    public int RESOLUTIONI = 20; //setting these to high can crash unity
    public int RESOLUTIONJ = 20;
    //the output Grid
    public Vector3[,] outputGrid;

    /*
     * INTERNAL VALUES
     */
    //internal knots in each direction
    private int[] knotsI;
    private int[] knotsJ;
    //internal variables
    private int i, j, ki, kj;
    private float intervalI, incrementI, intervalJ, incrementJ, bi, bj;

    //FUNCTIONS
    private BSplineMath bmath = new BSplineMath();

    //constructor
    public BSplineSurface()
    {
    }


    //MUST BE CALLED FIRST
    public void Init()
    {

        controlGrid = new Vector3[NI, NJ];
        outputGrid = new Vector3[RESOLUTIONI, RESOLUTIONJ];

        //init step size (the increment steps)
        incrementI = (NI - TI + 2) / ((float)RESOLUTIONI - 1);
        incrementJ = (NJ - TJ + 2) / ((float)RESOLUTIONJ - 1);

        //Calculate KNOTS
        knotsI = bmath.SplineKnots(NI, TI);
        knotsJ = bmath.SplineKnots(NJ, TJ);

    }

    // Use this for random initialization
    public void InitRandomGrid()
    {

        //init control points (random z)
        for (i = 0; i <= NI; i++)
        {
            for (j = 0; j <= NJ; j++)
            {
                controlGrid[i, j].x = i;
                controlGrid[i, j].y = j;
                controlGrid[i, j].z = Random.value;
            }
        }
    }

    // Use this for grid input (remember to set NI and NJ before(the grid size) and call Init)
    public void InitGrid(Vector3[,] inputGrid)
    {
        Debug.Log("Initializing grid, control grid is " + NI.ToString() + " x " + NJ.ToString());

        //init control points
        for (i = 0; i < NI; i++)
        {
            for (j = 0; j < NJ; j++)
            {
                //Debug.Log("Copying inputGrid at " + i.ToString() + " x " + j.ToString() + " to controlGrid");
                controlGrid[i, j] = inputGrid[i, j];
                //Debug.Log(inputGrid[i, j].ToString());
            }
        }
    }


    public void Calculate()
    {
        //MAIN CALCULATIONS
        intervalI = 0;
        for (i = 0; i < RESOLUTIONI-1; i++)
        {
            intervalJ = 0;
            for (j = 0; j < RESOLUTIONJ-1; j++)
            {
                outputGrid[i, j] = Vector3.zero;
                for (ki = 0; ki < NI; ki++)
                {
                    for (kj = 0; kj < NJ-1; kj++)
                    {
                        bi = bmath.SplineBlend(ki, TI, knotsI, intervalI);
                        bj = bmath.SplineBlend(kj, TJ, knotsJ, intervalJ);
                        outputGrid[i, j].x += (controlGrid[ki, kj].x * bi * bj);
                        outputGrid[i, j].y += (controlGrid[ki, kj].y * bi * bj);
                        outputGrid[i, j].z += (controlGrid[ki, kj].z * bi * bj);
                    }
                }
                if (outputGrid[i, j].Equals(Vector3.zero))
                {
                    Debug.Log("WARNING, OUTPUT GRID SET TO ORIGIN. DID YOU MEAN TO DO THIS?");
                }
                intervalJ += incrementJ;
            }
            intervalI += incrementI;
        }

        intervalI = 0;
        for (i = 0; i < RESOLUTIONI; i++)
        {
            outputGrid[i, RESOLUTIONJ - 1] = Vector3.zero;
            for (ki = 0; ki < NI; ki++)
            {
                bi = bmath.SplineBlend(ki, TI, knotsI, intervalI);
                outputGrid[i, RESOLUTIONJ - 1].x += (controlGrid[ki, NJ-1].x * bi);
                outputGrid[i, RESOLUTIONJ - 1].y += (controlGrid[ki, NJ-1].y * bi);
                outputGrid[i, RESOLUTIONJ - 1].z += (controlGrid[ki, NJ-1].z * bi);
            }
            if (outputGrid[i, RESOLUTIONJ - 1].Equals(Vector3.zero))
            {
                Debug.Log("WARNING, OUTPUT GRID SET TO ORIGIN. DID YOU MEAN TO DO THIS 2?");
            }
            intervalI += incrementI;
        }
        outputGrid[i-1, RESOLUTIONJ - 1] = controlGrid[NI-1, NJ-1];

        intervalJ = 0;
        for (j = 0; j < RESOLUTIONJ; j++)
        {
            outputGrid[RESOLUTIONI - 1, j] = Vector3.zero;
            for (kj = 0; kj < NJ; kj++)
            {
                bj = bmath.SplineBlend(kj, TJ, knotsJ, intervalJ);
                outputGrid[RESOLUTIONI - 1, j].x += (controlGrid[NI-1, kj].x * bj);
                outputGrid[RESOLUTIONI - 1, j].y += (controlGrid[NI-1, kj].y * bj);
                outputGrid[RESOLUTIONI - 1, j].z += (controlGrid[NI-1, kj].z * bj);
            }
            if (outputGrid[RESOLUTIONI - 1, j].Equals(Vector3.zero))
            {
                Debug.Log("WARNING, OUTPUT GRID SET TO ORIGIN. DID YOU MEAN TO DO THIS 3?");
            }
            intervalJ += incrementJ;
        }
        outputGrid[RESOLUTIONI - 1, j-1] = controlGrid[NI-1, NJ-1];

    }

    public void setControlGrid(int i, int j)
    {
        NI = i;
        NJ = j;
    }
}
