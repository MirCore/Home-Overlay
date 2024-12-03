using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Reflection;
using SimpleFileBrowser;
using UI;

public abstract class FileBrowserUtility
{
	// Warning: paths returned by FileBrowser dialogs do not contain a trailing '\' character
	// Warning: FileBrowser can only show 1 dialog at a time

	private void Awake()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
    typeof(SimpleFileBrowser.FileBrowserHelpers).GetField("m_shouldUseSAF", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, (bool?)false);
#endif

    }

    public static void LoadStringFromFile(SettingsUI settingsUI)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
    typeof(SimpleFileBrowser.FileBrowserHelpers).GetField("m_shouldUseSAF", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, (bool?)false);

    using var buildVersion = new AndroidJavaClass("android.os.Build$VERSION");
    using var buildCodes = new AndroidJavaClass("android.os.Build$VERSION_CODES");

    //Check SDK version > 29
    if (buildVersion.GetStatic<int>("SDK_INT") > buildCodes.GetStatic<int>("Q"))
    {
        using var environment = new AndroidJavaClass("android.os.Environment");
        //сhecking if permission already exists
        if (!environment.CallStatic<bool>("isExternalStorageManager"))
        {
            using var settings = new AndroidJavaClass("android.provider.Settings");
            using var uri = new AndroidJavaClass("android.net.Uri");
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var parsedUri = uri.CallStatic<AndroidJavaObject>("parse", $"package:{Application.identifier}");
            using var intent = new AndroidJavaObject("android.content.Intent",
                settings.GetStatic<string>("ACTION_MANAGE_APP_ALL_FILES_ACCESS_PERMISSION"),
                parsedUri);
            currentActivity.Call("startActivity", intent);
        }
    }
#endif
        
        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog),
        // if all the dialogs will be using the same filters
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Text Files", ".txt", ".pdf"));

        // Set default filter that is selected when the dialog is shown (optional)
        // Returns true if the default filter is set successfully
        // In this case, set Images filter as the default filter
        FileBrowser.SetDefaultFilter(".txt");

        // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        // Note that when you use this function, .lnk and .tmp extensions will no longer be
        // excluded unless you explicitly add them as parameters to the function
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        // !!! Uncomment any of the examples below to show the file browser !!!

        // Example 1: Show a save file dialog using callback approach
        // onSuccess event: not registered (which means this dialog is pretty useless)
        // onCancel event: not registered
        // Save file/folder: file, Allow multiple selection: false
        // Initial path: "C:\", Initial filename: "Screenshot.png"
        // Title: "Save As", Submit button text: "Save"
        // FileBrowser.ShowSaveDialog( null, null, FileBrowser.PickMode.Files, false, "C:\\", "Screenshot.png", "Save As", "Save" );

        // Example 2: Show a select folder dialog using callback approach
        // onSuccess event: print the selected folder's path
        // onCancel event: print "Canceled"
        // Load file/folder: folder, Allow multiple selection: false
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Select Folder", Submit button text: "Select"
        FileBrowser.ShowLoadDialog((paths) =>
            {
                Debug.Log( "Selected: " + paths[0] );
                
                string filePath = paths[0];
                
                byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(filePath);
                
                settingsUI.OnTokenLoadedFromFile(bytes);
            },
            () =>
            {
                Debug.Log( "Canceled" );
            },
            FileBrowser.PickMode.Files, false, null, null, "Select File", "Select" );

        // Example 3: Show a select file dialog using coroutine approach
        // StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, true, null, null, "Select Files", "Load");

        // Dialog is closed
        // Print whether the user has selected some files or cancelled the operation (FileBrowser.Success)
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
            OnFilesSelected(FileBrowser.Result); // FileBrowser.Result is null, if FileBrowser.Success is false
    }

    void OnFilesSelected(string[] filePaths)
    {
        // Print paths of the selected files
        for (int i = 0; i < filePaths.Length; i++)
            Debug.Log(filePaths[i]);

        // Get the file path of the first selected file
        string filePath = filePaths[0];

        // Read the bytes of the first file via FileBrowserHelpers
        // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
        byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(filePath);

        // Or, copy the first file to persistentDataPath
        string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(filePath));
        FileBrowserHelpers.CopyFile(filePath, destinationPath);
    }
}