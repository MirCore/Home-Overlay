using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Managers;
using Structs;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Abstract utility class for handling file saving and loading operations, including encryption.
    /// </summary>
    public abstract class SaveFile
    {
        // Key for reading and writing encrypted data.
        // (This is a "hardcoded" secret key.)
        private static readonly byte[] SavedKey = { 0xE6, 0xDA, 0x48, 0x8E, 0xD2, 0x15, 0x8A, 0x26, 0x2E, 0xA4, 0x7D, 0x78, 0x96 ,0x0 ,0xCD, 0x90 };
        
        /// <summary>
        /// The path where the save data file is stored.
        /// </summary>
        private static readonly string SaveDataPath = Path.Combine(Application.persistentDataPath, "saveData.json");
        
        /// <summary>
        /// Flag to indicate if a save operation is currently in progress.
        /// </summary>
        private static bool _isSaving;
        
        /// <summary>
        /// Reads the save file and deserializes it into a list of PanelData objects.
        /// </summary>
        /// <returns>A list of PanelData objects, or null if the file does not exist.</returns>
        public static List<PanelData> ReadFile()
        {
            if (!File.Exists(SaveDataPath))
            {
                Debug.Log("No file found");
                return null;
            }
            try
            {
                // Read the entire file into a String value.
                string json = File.ReadAllText(SaveDataPath);
                // Deserialize the JSON data into a PanelDataListWrapper.
                PanelDataListWrapper wrapper = JsonUtility.FromJson<PanelDataListWrapper>(json);
            
                // Return the list of PanelData objects.
                return wrapper.PanelDatas ?? new List<PanelData>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        /// <summary>
        /// Writes the list of PanelData objects to the save file.
        /// </summary>
        /// <param name="gameData">The list of PanelData objects to save.</param>
        private static void WriteFile(List<PanelData> gameData)
        {
            try
            {                
                // Serialize the list of PanelData objects to JSON.
                string json = JsonUtility.ToJson(new PanelDataListWrapper { PanelDatas = gameData }, true);
                // Write the JSON string to the save file.
                File.WriteAllText(SaveDataPath, json);
            }
            
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        /// <summary>
        /// Marks the panel data as dirty, indicating that it needs to be saved.
        /// </summary>
        public static void SetPanelDataDirty()
        {
            _ = SavePanelData();
        }

        /// <summary>
        /// Asynchronously saves the panel data to the file, yielding to await all changed data in the frame.
        /// </summary>
        private static async Task SavePanelData()
        {
            if (_isSaving) return;

            _isSaving = true;

            try
            {                
                // Yield to the next frame to await all changed data.
                await Task.Yield();
                // Write the panel data to the file.
                WriteFile(PanelManager.Instance.PanelDataList);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SavePanelData] Error while saving: {ex}");
            }
            finally
            {
                _isSaving = false;
            }
        }

        /// <summary>
        /// Reads and decrypts the save file, returning a PanelData object.
        /// </summary>
        /// <returns>The deserialized PanelData object, or null if the file does not exist.</returns>
        public static PanelData ReadEncryptedFile()
        {
            if (!File.Exists(SaveDataPath))
            {
                Debug.Log("No file found");
                return null;
            }
            try
            {
                // Create FileStream for opening files.
                using FileStream dataStream = new (SaveDataPath, FileMode.Open);
            
                // Create new AES instance.
                using Aes aes = Aes.Create();
            
                // Create an array of correct size based on AES IV.
                byte[] outputIv = new byte[aes.IV.Length];
            
                // Read the IV from the file.
                // ReSharper disable once MustUseReturnValue
                dataStream.Read(outputIv, 0, outputIv.Length);
            
                // Create CryptoStream, wrapping FileStream
                using CryptoStream stream = new(dataStream, aes.CreateDecryptor(SavedKey, outputIv), CryptoStreamMode.Read);
            
                // Create a StreamReader, wrapping CryptoStream
                using StreamReader reader = new(stream);
            
                // Read the entire file into a String value.
                string text = reader.ReadToEnd();

                // Close StreamReader.
                reader.Close();
            
                // Deserialize the JSON data into a pattern matching the GameData class.
                return JsonUtility.FromJson<PanelData>(text);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        /// <summary>
        /// Encrypts and writes the PanelData object to the save file.
        /// </summary>
        /// <param name="gameData">The PanelData object to save.</param>
        public static void WriteEncryptedFile(PanelData gameData)
        {
            try
            {
                // Create a FileStream for creating files.
                using FileStream dataStream = new FileStream(SaveDataPath, FileMode.Create);

                // Create new AES instance.
                using Aes aes = Aes.Create();

                // Save the new generated IV.
                byte[] inputIv = aes.IV;

                // Write the IV to the FileStream unencrypted.
                dataStream.Write(inputIv, 0, inputIv.Length);

                // Create CryptoStream, wrapping FileStream.
                using (CryptoStream stream = new(dataStream, aes.CreateEncryptor(SavedKey, aes.IV), CryptoStreamMode.Write))
                {
                    // Create StreamWriter, wrapping CryptoStream.
                    using (StreamWriter streamWriter = new(stream))
                    {
                        // Serialize the object into JSON and save string.
                        string jsonString = JsonUtility.ToJson(gameData);

                        // Write to the innermost stream (which will encrypt).
                        streamWriter.Write(jsonString);
                        // Close StreamWriter.
                        streamWriter.Close();
                    }


                    // Close CryptoStream.
                    stream.Close();
                }

                // Close FileStream.
                dataStream.Close();
            }
            
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}