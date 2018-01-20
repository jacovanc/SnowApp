﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class HeightMapGenerator : MonoBehaviour {

    GameObject settingsPanel;
    // Use this for initialization
    void Start () {
        settingsPanel = GameObject.Find("SettingsPanel");
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateModel()//Called on button click from menu
    {
        var gpsdata = LogHandler.ReadLog();
        foreach(var x in gpsdata) //test purposes, read log to settings page
        {
            //Debug.Log(x.Latitude + " " + x.Longitude + " " + x.Altitude);
            //settingsPanel.GetComponentInChildren<Text>().text += x.Latitude + " " + x.Longitude + " " + x.Altitude;

        }
        //this function should use the gps log data to call CreateHeightMap() and generate a heightmap raster image. This will be stored to 
        //the file system, later to be used to generate the terrain.

        var rasterarray = CreateHeightMap(gpsdata);

    }

    float[,] CreateHeightMap(List<GPSPosition> gpsdata)
    {
        var accuracy = 10000;
        //find size of array
        IEnumerable<GPSPosition> orderedlat = gpsdata.OrderByDescending(x => x.Latitude);
        double highestlat = orderedlat.First().Latitude;
        double lowestlat = orderedlat.Last().Latitude;

        IEnumerable<GPSPosition> orderedlong = gpsdata.OrderByDescending(x => x.Longitude);
        double leftmostlong = orderedlong.Last().Longitude;
        double rightmostlong = orderedlong.First().Longitude;

        double xdisplace = leftmostlong;
        double ydisplace = lowestlat;

        int xheight = Convert.ToInt32((rightmostlong - leftmostlong) * accuracy);
        int yheight = Convert.ToInt32((highestlat - lowestlat) * accuracy);

        //get max and min altitudes
        IEnumerable<GPSPosition> orderedalt = gpsdata.OrderByDescending(x => x.Altitude);
        double high = orderedalt.First().Altitude;
        double low = orderedalt.Last().Altitude;

        Texture2D texture = new Texture2D(xheight + 1, yheight + 1);
        var asdf = texture.GetPixel(1, 1);

        Color32[] colors = CreateBaseColours(xheight, yheight);
        float[,] rasterarray = new float[yheight + 1, xheight + 1]; //create array to that size

        //insert gps data into appropriate index
        foreach (var pos in gpsdata)
        { //add estimates between points: store previous value, compare current and previous, calculate intermediate points, update them with averaged altitude
            int relativelatitude = Convert.ToInt32((pos.Latitude - ydisplace) * accuracy);
            int relativelongitude = Convert.ToInt32((pos.Longitude - xdisplace) * accuracy);

            rasterarray[relativelatitude, relativelongitude] = GetRelativeAltitude(high, low, pos); //array in (y, x) format
            var colorindex = (relativelatitude * (xheight + 1)) + relativelongitude; //find the flattened index of a 2d array

            float altitudefloat = GetRelativeAltitude(high, low, pos);
            Debug.Log(altitudefloat);
            colors[colorindex] = new Color(altitudefloat, altitudefloat, altitudefloat);
        }
        texture.SetPixels32(colors); //set 1d array of colors to 2d texture pixels
        texture.Apply();

        asdf = texture.GetPixel(1, 1);
        File.WriteAllBytes(Application.persistentDataPath + "/gpsraster.png", texture.EncodeToPNG());

        //create terrain
        TerrainData terraindata = new TerrainData {
            size = new Vector3(xheight + 1, 50, yheight + 1), //change value to increase scale in the upwards direction
        };
        terraindata.heightmapResolution = xheight > yheight ? xheight + 1 : yheight + 1;
        terraindata.SetHeights(0, 0, rasterarray);

        //change texture
        var textureInSplats = new SplatPrototype[1];
        var textureIn = new Texture2D[15];

        textureInSplats[0] = new SplatPrototype();
        textureInSplats[0].texture = texture;
        textureInSplats[0].tileSize = new Vector2((xheight + 1) * terraindata.heightmapScale.x, (yheight + 1) * terraindata.heightmapScale.z);
        terraindata.splatPrototypes = textureInSplats;

        var terrain = Terrain.CreateTerrainGameObject(terraindata);
        
        return rasterarray;
    }

    private static float GetRelativeAltitude(double high, double low, GPSPosition pos) //converts the altitude into a scale between 0 and 1 based on max and min altitudes
    {
        var altitudedifference = high - low;
        var b = pos.Altitude - low;
        float altitudefloat = (float)(b / altitudedifference);
        return altitudefloat;
    }

    private static Color32[] CreateBaseColours(int xheight, int yheight)
    {
        var colours = new Color32[(xheight + 1) * (yheight + 1)];
        for (int i = 0; i < colours.Length; i++)
        {
            colours[i] = new Color(0, 0, 0, 1);
        }
        return colours;
    }
}
