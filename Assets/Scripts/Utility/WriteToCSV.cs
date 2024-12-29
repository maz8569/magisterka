using System;
using System.IO;
using UnityEngine;

public class WriteToCSV : MonoBehaviour 
{
    public string filename = "STATS";
    public TextAsset textAssetData;

    void Start()
    {
        //filename = Application.dataPath + "/Stats/" + filename + "_" + DateTime.Now.ToString("dd-MM-yy_hh-mm-ss") + ".csv";
        filename = Application.dataPath + "/Stats/" + filename + ".csv";
    }

    public void StartCSV()
    {
        if(File.Exists(filename))
        {
            File.Delete(filename);
        }

        TextWriter tw = new StreamWriter(filename, false);
        tw.WriteLine("Novelty, Fitness Score");
        tw.Close();
    }

    public void WriteLineCSV(float novelty, float fitness)
    {
        TextWriter tw = new StreamWriter(filename, true);
        tw.WriteLine($"{novelty}, {fitness}");
        tw.Close();
    }
}
