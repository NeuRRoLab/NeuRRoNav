using UnityEngine;
using System.Collections;

public class GridOverlay : MonoBehaviour
{

    //public GameObject plane;
    GameObject center;
    GameObject grid;

    public bool showMain = true;
    public bool showSub = false;

    public float gridSizeX;
    public float gridSizeY;
    public float gridSizeZ;

    public float smallStep;
    public float largeStep;

    public float startX;
    public float startY;
    public float startZ;

    private Material lineMaterial;

    private Color mainColor = new Color(0f, 1f, 0f, 1f);
    private Color subColor = new Color(0f, 0.5f, 0f, 1f);


    void CreateLineMaterial()
    {

        if (!lineMaterial)
        {
            lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
                "SubShader { Pass { " +
                "    Blend SrcAlpha OneMinusSrcAlpha " +
                "    ZWrite Off Cull Off Fog { Mode Off } " +
                "    BindChannels {" +
                "      Bind \"vertex\", vertex Bind \"color\", color }" +
                "} } }");
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    void OnPostRender()
    {
        CreateLineMaterial();
        // set the current material
        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);

        if (showSub)
        {
            GL.Color(subColor);

            //Layers
            for (float j = 0; j <= gridSizeY; j += smallStep)
            {
                //X axis lines
                for (float i = 0; i <= gridSizeZ; i += smallStep)
                {
                    //GL.Vertex3(startX, Vector3.Lerp(startX, j, startZ + i);
                    GL.Vertex3(gridSizeX, j, startZ + i);
                }

                //Z axis lines
                for (float i = 0; i <= gridSizeX; i += smallStep)
                {
                    GL.Vertex3(startX + i, j, startZ);
                    GL.Vertex3(startX + i, j, gridSizeZ);
                }
            }

            //Y axis lines
            for (float i = 0; i <= gridSizeZ; i += smallStep)
            {
                for (float k = 0; k <= gridSizeX; k += smallStep)
                {
                    GL.Vertex3(startX + k, startY, startZ + i);
                    GL.Vertex3(startX + k, gridSizeY, startZ + i);
                }
            }
        }

        if (showMain)
        {
            GL.Color(mainColor);

            //Layers
            for (float j = 0; j <= gridSizeY; j += largeStep)
            {
                //X axis lines
                for (float i = 0; i <= gridSizeZ; i += largeStep)
                {
                    GL.Vertex3(grid.transform.position.x, j, startZ + i);
                    GL.Vertex3(gridSizeX, j, startZ + i);
                }

                //Z axis lines
                for (float i = 0; i <= gridSizeX; i += largeStep)
                {
                    GL.Vertex3(startX + i, j, startZ);
                    GL.Vertex3(startX + i, j, gridSizeZ);
                }
            }

            //Y axis lines
            for (float i = 0; i <= gridSizeZ; i += largeStep)
            {
                for (float k = 0; k <= gridSizeX; k += largeStep)
                {
                    GL.Vertex3(startX + k, startY, startZ + i);
                    GL.Vertex3(startX + k, gridSizeY, startZ + i);
                }
            }
        }


        GL.End();
    }

    public void setCenter(Vector3 v, Quaternion r, Camera cam)
    {
        GL.ClearWithSkybox(true, cam);
        if(center !=null)
        {
            Destroy(center);
        }
        center = new GameObject();
        if (grid != null)
        {
            Destroy(center);
        }
        grid = new GameObject();
        center.transform.position = v;
        center.transform.rotation = r;
        grid.transform.position = center.transform.position;
        grid.transform.rotation = center.transform.rotation;
        grid.transform.parent = center.transform;

        grid.transform.position = new Vector3(grid.transform.localPosition.x - gridSizeX / 2, grid.transform.localPosition.y - gridSizeY/2, grid.transform.localPosition.z);
    }

    public void setDraw(bool main, bool sub, Camera cam)
    {
        GL.ClearWithSkybox(true, cam);
        showMain = main;
        showSub = sub;
    }
}