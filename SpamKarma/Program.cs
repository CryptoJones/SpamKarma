using System;
using System.Net;
using System.Net.Mail;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using MimeKit;
using MailKit;
using MailKit.Search;

namespace SpamKarma
{
    public class Victim
    {
        public String Name = "Aaron K. Clark";
        public String Address = "aaron.kingsley.clark@gmail.com";
        public String Password = "MyD@ddyL0v3sM3";
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
            using (var emailClient = new MailKit.Net.Imap.ImapClient())
            {
                emailClient.Connect("imap.gmail.com", 993, true);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(vm.Address, vm.Password);
                emailClient.Inbox.Open(FolderAccess.ReadWrite);
                emailClient.Inbox.FirstOrDefault();
                //int numberOfEmails = emailClient.Inbox.Count();
                var uids = emailClient.Inbox.Search(SearchQuery.All);

                foreach (var uid in uids)
                {
                    MimeMessage message = emailClient.Inbox.GetMessage(uid);




                    foreach (NaggerAddress na in bl.NaggerAddresses)
                    {
                        bool returnFire = false;

                        if (message.From.ToString().Contains(na.User + "@" + na.Domain))
                        {
                            returnFire = true;
                        }

                        if (returnFire)
                        {
                            RespondWithPicture(na.RetributionLevel, message.From.ToString(), vm, message.Subject);
                            DeleteMessage(uid, vm);
                        }
                    }

                    foreach (NaggerDomain nd in bl.NaggerDomains)
                    {
                        bool returnFire = false;

                        if (message.From.ToString().Contains(nd.Domain))
                        {
                            returnFire = true;
                        }

                        if (returnFire)
                        {
                            RespondWithPicture(nd.RetributionLevel, message.From.ToString(), vm, message.Subject);
                            DeleteMessage(uid, vm);
                        }
                    }
                }
            }
                         
        }

        static void RespondWithPicture(int count, string naggerAddress, Victim victim, string subject)
        {
            for (int i = 0; i < count; i++)
            {
                System.Threading.Thread.Sleep(5000);
                using (var message = new MailMessage())
                {
                    message.To.Add(new MailAddress(naggerAddress, naggerAddress));
                    message.From = new MailAddress(victim.Address, victim.Name);
                    message.Subject = "RE: " + subject;
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


        static string RandomPicture()
        {
            var rand = new Random();
            var files = Directory.GetFiles(".\\PayloadImages\\", "*.jpg");
            return files[rand.Next(files.Length)];

        }

        static void DeleteMessage(UniqueId messageId, Victim vm)
        {
            using (var emailClient = new MailKit.Net.Imap.ImapClient())
            {
                emailClient.Connect("imap.gmail.com", 993, true);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(vm.Address, vm.Password);
                emailClient.Inbox.Open(FolderAccess.ReadWrite);
                emailClient.Inbox.AddFlags(new UniqueId[] { messageId }, MessageFlags.Deleted, true);
                emailClient.Inbox.Expunge();
                
                
            }
        }


    }

    public class BlackList {
        public List<NaggerAddress> NaggerAddresses = new List<NaggerAddress>();
        public List<NaggerDomain> NaggerDomains = new List<NaggerDomain>();

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
        public bool Unread { get; set; }
    }
}
