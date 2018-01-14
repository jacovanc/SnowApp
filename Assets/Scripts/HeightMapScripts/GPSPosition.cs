﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPSPosition {

    public double Longitude;
    public double Latitude;
    public double Altitude;

    public GPSPosition(double longitude, double latitude, double altitude)
    {
        Longitude = (longitude);
        Latitude = (latitude);
        Altitude = (altitude);
    }
}
