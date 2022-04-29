using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BepInEx;
using System.IO;

public class SnapCam : MonoBehaviour 
 {

    Camera cam;

    int bildLänge =1000;
    string pictureName = "GorillaPic";
    string fileFormat = "png";
    int bildHöhe =1000;
    string snapCamFolder;
    RenderTexture renderTexture;

    void Start()
    {
        cam = GetComponent<Camera>();

        bildHöhe = cam.targetTexture.height;
        bildLänge = cam.targetTexture.width;
        snapCamFolder = BepInEx.Paths.BepInExRootPath + "/SnapCam";
        var folder = Directory.CreateDirectory(snapCamFolder);
        Debug.Log("Made a new SnapCam Folder");
    }
   public  void TakePic()
    {


        Texture2D pic = new Texture2D(bildLänge, bildHöhe, TextureFormat.RGBA32, false);
        RenderTexture.active = cam.targetTexture;
        pic.ReadPixels(new Rect(0, 0, bildLänge, bildHöhe), 0, 0);

        Color[] pixels = pic.GetPixels();
        for (int p = 0; p < pixels.Length; p++)
        {
            pixels[p] = pixels[p].gamma;
        }
        pic.SetPixels(pixels);
        pic.Apply();
        byte[] bytes = pic.EncodeToPNG();
        
        string fileName = FileName();
        System.IO.File.WriteAllBytes(fileName, bytes);
        
        Debug.Log("Pic Taken! Picture can be found  here : " +  fileName);
    }

   public string FileName()
    {
        return string.Format("{0}/" + pictureName + "_{1}." + fileFormat, snapCamFolder, System.DateTime.Now.ToString("yyyy_MM_dd-mm_ss_f"));

    }
}

