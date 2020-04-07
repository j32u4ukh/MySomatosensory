using UnityEngine;
using System.Collections;
using System.IO;
using System;

/// <summary>
/// Background color image is component that displays the color camera feed on GUI texture, usually the scene background.
/// </summary>
public class BackgroundColorImage : MonoBehaviour 
{
	[Tooltip("RawImage used to display the color camera feed.")]
	public UnityEngine.UI.RawImage backgroundImage;


	void Start()
	{
		if (backgroundImage == null) 
		{
			backgroundImage = GetComponent<UnityEngine.UI.RawImage>();
		}
	}


	void Update () 
	{
        KinectManager manager = KinectManager.Instance;

        if (manager && manager.IsInitialized())
        {
            if (backgroundImage && (backgroundImage.texture == null))
            {
                backgroundImage.texture = manager.GetUsersClrTex();
                backgroundImage.rectTransform.localScale = manager.GetColorImageScale();
                backgroundImage.color = Color.white;
            }
        }

        //string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
        //                                "Somatosensory/Picture");
        //saveImage(directory);
    }

    void saveImage(string _directory)
    {
        KinectManager manager = KinectManager.Instance;
        Texture2D texture = manager.GetUsersClrTex2D();
        byte[] data = texture.EncodeToJPG();

        if (!Directory.Exists(_directory))
        {
            Directory.CreateDirectory(_directory);
        }

        string file_name = Path.Combine(_directory, string.Format("{0}.png", getTimestamp()));

        File.WriteAllBytes(file_name, data);
    }

    long getTimestamp()
    {
        var curr_time = DateTime.Now.ToFileTime();

        return curr_time;
    }

    void takePhoto(string save_dir)
    {
        KinectManager manager = KinectManager.Instance;
        Texture2D texture = manager.GetUsersClrTex2D();
        byte[] data = texture.EncodeToJPG();

        if (!Directory.Exists(save_dir))
        {
            Directory.CreateDirectory(save_dir);
        }

        string file_name = Path.Combine(save_dir, string.Format("{0}.png", getTimestamp()));

        File.WriteAllBytes(file_name, data);
    }
}
