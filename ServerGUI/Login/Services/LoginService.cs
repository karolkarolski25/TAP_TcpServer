﻿using Microsoft.Extensions.Logging;
using Storage.DAL;
using Storage.Models;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Login.Services
{
    public class LoginService : ILoginService
    {
        private readonly Aes aes;
        private readonly CryptoConfiguration cryptoConfiguration;

        private readonly ILogger<LoginService> _logger;
        private readonly IStorageService _storageService;

        public LoginService(CryptoConfiguration _cryptoConfiguration, ILogger<LoginService> logger,
            IStorageService storageService)
        {
            _logger = logger;
            cryptoConfiguration = _cryptoConfiguration;
            _storageService = storageService;

            aes = Aes.Create();
        }

        /// <summary>
        /// Changes password
        /// </summary>
        /// <param name="data"></param>
        /// <returns>True if operation was succesfull</returns>
        public bool ChangePassword(string data)
        {
            FileStream fileStream = null;
            CryptoStream cryptoStream = null;
            StreamReader streamReader = null;
            string dbData;
            data += "|";
            try
            {
                fileStream = new FileStream("NotPasswords.bin", FileMode.OpenOrCreate);
                cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(cryptoConfiguration.Key, cryptoConfiguration.IV), CryptoStreamMode.Read);
                streamReader = new StreamReader(cryptoStream);
                dbData = streamReader.ReadToEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                _logger.LogInformation(e.Message);

                return false;
            }
            finally
            {
                streamReader.Close();
                cryptoStream.Close();
                fileStream.Close();
            }

            string login = data.Substring(0, data.IndexOf(";"));
            int position = dbData.IndexOf(login);

            dbData = dbData.Remove(position, dbData.IndexOf("|", position));

            dbData = dbData + data;

            try
            {
                fileStream = new FileStream("NotPasswords.bin", FileMode.OpenOrCreate);
                cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(cryptoConfiguration.Key, cryptoConfiguration.IV), CryptoStreamMode.Write);

                byte[] binaryData = new byte[dbData.Length];

                for (int i = 0; i < dbData.Length; i++)
                {
                    binaryData[i] = (byte)dbData[i];
                }

                cryptoStream.Write(binaryData, 0, binaryData.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                _logger.LogInformation(e.Message);

                return false;
            }
            finally
            {
                cryptoStream.Close();
                fileStream.Close();
            }
            return true;
        }

        /// <summary>
        /// Checks if user is already in database
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Information if user is in database</returns>
        public bool CheckData(string data)
        {
            try
            {
                FileStream fileStream = new FileStream("NotPasswords.bin", FileMode.OpenOrCreate);

                CryptoStream cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(cryptoConfiguration.Key,
                    cryptoConfiguration.IV), CryptoStreamMode.Read);

                StreamReader streamReader = new StreamReader(cryptoStream);

                string dbData;

                try
                {
                    dbData = streamReader.ReadToEnd();

                    foreach (var line in dbData.Split('|'))
                    {
                        if (line == data)
                        {
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    _logger.LogInformation(e.Message);
                }
                finally
                {
                    streamReader.Close();
                    cryptoStream.Close();
                    fileStream.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                _logger.LogInformation(e.Message);
            }
            return false;
        }

        /// <summary>
        /// Adds user to database
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Status of registration</returns>
        public bool RegisterAccount(string data)
        {
            FileStream fileStream = null;
            CryptoStream cryptoStream = null;
            StreamReader streamReader = null;
            string dbData;
            data += "|";
            try
            {
                fileStream = new FileStream("NotPasswords.bin", FileMode.OpenOrCreate);
                cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(cryptoConfiguration.Key, cryptoConfiguration.IV), CryptoStreamMode.Read);
                streamReader = new StreamReader(cryptoStream);
                dbData = streamReader.ReadToEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                _logger.LogInformation(e.Message);

                return false;
            }
            finally
            {
                streamReader.Close();
                cryptoStream.Close();
                fileStream.Close();
            }
            dbData = dbData + data;

            try
            {
                fileStream = new FileStream("NotPasswords.bin", FileMode.OpenOrCreate);
                cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(cryptoConfiguration.Key, cryptoConfiguration.IV), CryptoStreamMode.Write);

                byte[] binaryData = new byte[dbData.Length];

                for (int i = 0; i < dbData.Length; i++)
                {
                    binaryData[i] = (byte)dbData[i];
                }

                cryptoStream.Write(binaryData, 0, binaryData.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                _logger.LogInformation(e.Message);

                return false;
            }
            finally
            {
                cryptoStream.Close();
                fileStream.Close();
            }
            return true;
        }
    }
}
