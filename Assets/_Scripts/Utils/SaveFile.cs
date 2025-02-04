using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Managers;
using Structs;
using UnityEngine;

namespace Utils
{
    public abstract class SaveFile
    {
        // Key for reading and writing encrypted data.
        // (This is a "hardcoded" secret key. )
        private static readonly byte[] SavedKey = { 0xE6, 0xDA, 0x48, 0x8E, 0xD2, 0x15, 0x8A, 0x26, 0x2E, 0xA4, 0x7D, 0x78, 0x96 ,0x0 ,0xCD, 0x90 };
        
        private static readonly string SaveDataPath = Application.persistentDataPath + "/saveData.json";
        private static readonly string HassConfigPath = Application.persistentDataPath + "/hassConfig.json";
        
        public static List<EntityObject> ReadFile()
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
                EntityObjectListWrapper wrapper = JsonUtility.FromJson<EntityObjectListWrapper>(json);
            
                // Deserialize the JSON data into a pattern matching the GameData class.
                return wrapper.EntityObjects ?? new List<EntityObject>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        private static void WriteFile(List<EntityObject> gameData)
        {
            try
            {
                string json = JsonUtility.ToJson(new EntityObjectListWrapper { EntityObjects = gameData }, true);
                
                File.WriteAllText(SaveDataPath, json);
                
                //Debug.Log(Application.persistentDataPath);
            }
            
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void SaveEntityObjects()
        {
            WriteFile(GameManager.Instance.EntityObjects);
        }

        public static EntityObject ReadEncryptedFile()
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
                return JsonUtility.FromJson<EntityObject>(text);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        public static void WriteEncryptedFile(EntityObject gameData)
        {
            try
            {
#if UNITY_EDITOR
                File.WriteAllText(Application.persistentDataPath + "/saveData.vbi", JsonUtility.ToJson(gameData));
                //Debug.Log(Application.persistentDataPath);
#endif
                
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