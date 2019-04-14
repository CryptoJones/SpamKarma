using System;
using System.Net;
using System.Net.Mail;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using MimeKit;

namespace SpamKarma
{
    public class Victim
    {
        public String Name = "Aaron K. Clark";
        public String Address = "aaron.kingsley.clark@gmail.com";
        public String Password = "IL0v3G0@tS3x!!";
    }

    class Program
    {
        static void Main(string[] args)
        {
            Victim vm = new Victim();
            BlackList bl;

            string json = String.Empty;
            try
            {
                json = File.ReadAllText("BlackList.json");
                bl = JsonConvert.DeserializeObject<BlackList>(json);
                CheckMail(bl, vm);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(-1);
            }

            

        }

        static void CheckMail(BlackList bl, Victim vm)
        {
            using (var emailClient = new MailKit.Net.Pop3.Pop3Client())
            {
                emailClient.Connect("pop.gmail.com", 995, true);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(vm.Address, vm.Password);
                List<EmailMessage> emails = new List<EmailMessage>();
                for (int i = 0; i < emailClient.Count && i < 50; i++)
                {
                    var message = emailClient.GetMessage(i);
                    var emailMessage = new EmailMessage
                    {
                        Content = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody,
                        Subject = message.Subject
                    };
                    emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
                    emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));

                    foreach (NaggerAddress item in bl.NaggerAddresses)
                    {
                        string email = item.User + "@" + item.Domain;
                       if (email == emailMessage.FromAddresses.ToString())
                        {
                            RespondWithPicture(1, "", vm);
                            DeleteMessage(message.MessageId, vm);
                        }
                    }
                }
                
            }
        }

        static void RespondWithPicture(int count, string naggerAddress, Victim victim)
        {
            for (int i = 0; i < count; i++)
            {
                using (var message = new MailMessage())
                {
                    message.To.Add(new MailAddress(naggerAddress, naggerAddress));
                    message.From = new MailAddress(victim.Address, victim.Name);
                    message.Subject = GetSubject();
                    message.Body = "";
                    message.IsBodyHtml = true;
                    message.Attachments.Add(new System.Net.Mail.Attachment(RandomPicture()));

                    using (var client = new SmtpClient("smtp.gmail.com"))
                    {
                        client.Port = 587;
                        client.Credentials = new NetworkCredential(victim.Address, victim.Password);
                        client.EnableSsl = true;
                        client.Send(message);
                    }
                }
            }
        }

        static string GetSubject()
        {
            string subject = String.Empty;

            return subject;
        }

        static string RandomPicture()
        {
            var rand = new Random();
            var files = Directory.GetFiles(".\\PayloadImages\\", "*.jpg");
            return files[rand.Next(files.Length)];

        }

        static void DeleteMessage(string messageId, Victim vm)
        {
            using (var emailClient = new MailKit.Net.Pop3.Pop3Client())
            {
                emailClient.Connect("pop.gmail.com", 995, true);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(vm.Address, vm.Password);
                emailClient.DeleteMessage(0);
            }
        }


    }

    public class BlackList {
        public ArrayList NaggerAddresses = new ArrayList();
        public ArrayList NaggerDomains = new ArrayList();

       public BlackList()
        {
          
        }

    }

   public class NaggerAddress
    {
        public string User;
        public string Domain;
        public int RetributionLevel;
    }

    public class NaggerDomain
    {
        public string Domain;
        public string Tld;
        public int RetributionLevel;
    }

    public class EmailAddress
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class EmailMessage
    {
        public EmailMessage()
        {
            ToAddresses = new List<EmailAddress>();
            FromAddresses = new List<EmailAddress>();
        }

        public List<EmailAddress> ToAddresses { get; set; }
        public List<EmailAddress> FromAddresses { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}
