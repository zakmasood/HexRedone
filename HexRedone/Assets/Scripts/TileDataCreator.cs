using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class TileDataCreator : MonoBehaviour
{
    class Storage
    {
        static void Main()
        {
            string filePath = "Assets/Data.json"; // Specify the path to Data
            string jsonString = File.ReadAllText(filePath); // Read the JSON data from the file

            List<Tile> tiles = JsonConvert.DeserializeObject<List<Tile>>(jsonString); // Deserialize the JSON string into a list of tiles
            
            foreach (var tile in tiles) // Access and print data for each tile
            {

            }
        }
    }

    // Define classes to represent the JSON structure
    public class Tile
    {
        [JsonProperty("coordinates")]
        public Coordinates Coordinates { get; set; }

        [JsonProperty("resources")]
        public Resources Resources { get; set; }

        [JsonProperty("building")]
        public Building Building { get; set; }
    }

    public class Coordinates
    {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("z")]
        public int Z { get; set; }
    }

    public class Resources
    {
        [JsonProperty("oil")]
        public int Oil { get; set; }

        [JsonProperty("stone")]
        public int Stone { get; set; }

        [JsonProperty("wood")]
        public int Wood { get; set; }
    }

    public class Building // Can also make seperate classes for different buildings
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("capacity")]
        public int Capacity { get; set; }
    }
}