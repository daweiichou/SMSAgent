using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMSAgentAPI.Controllers;
using SMSAgentAPI;
using System;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using com.cht.messaging.sns;
using System.Diagnostics;
using System.Web;
using Azure;
using System.Net;
using System.Text;
using System.Configuration;
//using SMSAgentAPI.Models;

namespace SMSAgentApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SMSAgentController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private const string strToRandom = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private readonly ILogger<SMSAgentController> _logger;

        public static string Encrypt(string plainText, byte[] key, byte[] iv)
        {
            byte[] encrypted;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }

                        encrypted = ms.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string cipherText, string key, string iv)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            string plaintext = null;

            
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.ASCII.GetBytes(key);
                aes.IV = Encoding.ASCII.GetBytes(iv);

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                try
                {
                    /*
                    using (var ms = new MemoryStream(cipherBytes))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            Stream stream;
                            plaintext = stream.Read(cs);
                        }
                    }
                    */
                    using (MemoryStream msDecrypt = new MemoryStream(Encoding.ASCII.GetBytes(cipherText)))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {

                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                }
                catch (Exception ex)
                { 
                    
                };
               
            }

            return plaintext;
        }


        //call cht api
        Sms_Client snsClient = new Sms_Client("203.66.172.133", 8001);

        private const string account = "10427";
        private const string password = "UDJp8738";
        private const string iv = "047xjsutiqr139ly";

        public SMSAgentController(ILogger<SMSAgentController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "getMessageSample")]
        public IEnumerable<SMSAgent> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new SMSAgent
            {
                agentSendTime = DateTime.Now.AddDays(index),
                encryptKey = "1234567890123456",
                //encryptKey = Enumerable.Repeat(strToRandom, 16).ToString(),
                //.Select(s => s[Random.Shared.Next(s.Length)]).ToArray(),
                phoneNumber = "0968200776",
                messageContent = Summaries[Random.Shared.Next(Summaries.Length)]
            })
                  .ToArray();
        }

        [Route("sendMessage")]
        [HttpPost]
        public HttpResponseMessage sendMessage(string key, string encryptPhone, string message)
        {
            byte[] encryptKey = Encoding.UTF8.GetBytes(key);
            string phoneNumber = Decrypt(message, key, iv);
            string encryptMessage = message;

            //HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "value");
            //response.Content = new StringContent("hello", Encoding.Unicode);
            //response.Headers.CacheControl = new CacheControlHeaderVa.lue();

            //send via CHT
            snsClient.submitMessage(account, password, phoneNumber, message);
            //snsClient.qryMessageStatus

            //return response;
            return new HttpResponseMessage()
            {
                Content = new StringContent("hello", Encoding.Unicode)
            };
        }

        
    }
}
