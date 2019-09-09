using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Newtonsoft.Json;

namespace SpamKarma
{
    public class Victim
    {
        public string Address = "aaron.kingsley.clark@gmail.com";
        public string Name = "Aaron K. Clark";
        public string Password = "wgrxbyyroibejkig";
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var vm = new Victim();
            BlackList bl;

            try
            {
                string json = File.ReadAllText("BlackList.json");
                bl = JsonConvert.DeserializeObject<BlackList>(json);
                CheckMail(bl, vm);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(-1);
            }
        }

        private static void CheckMail(BlackList bl, Victim vm)
        {
            using (var emailClient = new ImapClient())
            {
                emailClient.Connect("imap.gmail.com", 993, true);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(vm.Address, vm.Password);
                emailClient.Inbox.Open(FolderAccess.ReadWrite);

                var uids = emailClient.Inbox.Search(SearchQuery.All);

                foreach (var uid in uids)
                {
                    var message = emailClient.Inbox.GetMessage(uid);


                    foreach (var na in bl.NaggerAddresses)
                    {
                        var returnFire = message.From.ToString().Contains(na.User + "@" + na.Domain);

                        if (returnFire)
                        {
                            RespondWithPicture(na.RetributionLevel, message.From.ToString(), vm, message.Subject);
                            DeleteMessage(uid, vm);
                        }
                    }

                    foreach (var nd in bl.NaggerDomains)
                    {
                        var returnFire = message.From.ToString().Contains(nd.Domain);

                        if (returnFire)
                        {
                            RespondWithPicture(nd.RetributionLevel, message.From.ToString(), vm, message.Subject);
                            DeleteMessage(uid, vm);
                        }
                    }
                }
            }
        }

        private static void RespondWithPicture(int count, string naggerAddress, Victim victim, string subject)
        {
            for (var i = 0; i < count; i++)
                using (var message = new MailMessage())
                {
                    message.To.Add(new MailAddress(naggerAddress, naggerAddress));
                    message.From = new MailAddress(victim.Address, victim.Name);
                    message.Subject = "RE: " + subject;
                    message.Body = "";
                    message.IsBodyHtml = true;
                    message.Attachments.Add(new Attachment(RandomPicture()));

                    using (var client = new SmtpClient("smtp.gmail.com"))
                    {
                        client.Port = 587;
                        client.Credentials = new NetworkCredential(victim.Address, victim.Password);
                        client.EnableSsl = true;
                        client.Send(message);
                    }
                }
        }

        private static string RandomPicture()
        {
            var rand = new Random();
            var files = Directory.GetFiles(".\\PayloadImages\\", "*.jpg");
            return files[rand.Next(files.Length)];
        }

        private static void DeleteMessage(UniqueId messageId, Victim vm)
        {
            using (var emailClient = new ImapClient())
            {
                emailClient.Connect("imap.gmail.com", 993, true);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(vm.Address, vm.Password);
                emailClient.Inbox.Open(FolderAccess.ReadWrite);
                emailClient.Inbox.AddFlags(new[] { messageId }, MessageFlags.Seen, true);
                emailClient.Inbox.AddFlags(new[] {messageId}, MessageFlags.Deleted, true);
                emailClient.Inbox.Expunge();
            }
        }
    }

    public class BlackList
    {
        public List<NaggerAddress> NaggerAddresses = new List<NaggerAddress>();
        public List<NaggerDomain> NaggerDomains = new List<NaggerDomain>();
    }

    public class NaggerAddress
    {
        public string Domain;
        public int RetributionLevel;
        public string User;
    }

    public class NaggerDomain
    {
        public string Domain;
        public int RetributionLevel;
        public string Tld;
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