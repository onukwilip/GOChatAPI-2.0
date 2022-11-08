using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using MailKit;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace GOChatAPI
{
    public enum ResponseCodes
    {
        //Success or Failure
        Successfull = 200,
        Deleted = 204,
        Unsuccessfull = 400,
        NoUser = 404,
        NoRequests = 404,
        NoChatRoom = 404,
        NotFound = 404,
        //Mail
        MailNotSent = 100,
        //Requests Validation
        RequestValid = 0,
        RequestExists = 1,
        ChatRoomMemberExists = 2,
        UserIsIgnored = 3
    }

    public class General
    {
        public static string IPAddress { get; set; }

        public int Random()
        {
            int random = new Random().Next(100000, 999999);
            return random;
        }

        public string Mail(string toEmail, string fromEmail, string fromName, string toName, string body, string subject)
        {
            string msg = String.Empty;
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                message.Body = new TextPart("html")
                {
                    Text = body
                };

                using (var client = new SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, t) => true;

                    //client.Connect("smtp.gmail.com", 587, false);
                    client.Connect("smtp.gmail.com", 465, true);

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate("onukwilip@gmail.com", "xgtswucwcflackth");

                    client.Send(message);
                    client.Disconnect(true);

                    msg = "Mail sent successfully...";
                }
            }

            catch (Exception ex)
            {
                msg = $"{ex}";//$"Something went wrong...😥";
            }

            return msg;
        }

        public string Mail2(string email, string Body, string subject)
        {
            string msg = String.Empty;
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Go-IT", "onukwilip@gmail.com"));
                message.To.Add(new MailboxAddress(email, email));
                message.Subject = subject;

                message.Body = new TextPart("html")
                {
                    Text = Body
                };

                using (var client = new SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, t) => true;

                    client.Connect("smtp.gmail.com", 25, false);

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate("onukwilip@gmail.com", "vradvsdyewmjpjal");//vradvsdyewmjpjal

                    client.Send(message);
                    client.Disconnect(true);

                    msg = "Mail sent successfully...";
                }
            }
            catch (Exception ex)
            {
                msg = $"Error {ex}";
            }

            return msg;
        }

        public string Encrypt(string encryptString)
        {
            string encryptionKey = "!@#$%^&*()_+=-|}{\\][\":';?></.,'"; //"0123456789abcdefghijklmnopqrstuvwxyzABDEFGHIJKLMNOPQRSTUVWXYZ";
            byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] {
                    0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
                });

                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    encryptString = Convert.ToBase64String(ms.ToArray());
                }
            }

            return encryptString;
        }

        public string Decrypt(string cipherText)
        {
            string encryptionKey = "!@#$%^&*()_+=-|}{\\][\":';?></.,'"; //"0123456789abcdefghijklmnopqrstuvwxyzABDEFGHIJKLMNOPQRSTUVWXYZ";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[]{
                    0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
                });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }

            return cipherText;
        }
    }
}