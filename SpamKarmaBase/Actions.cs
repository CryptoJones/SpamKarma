using System;
using System.Net;
using System.Net.Mail;
using System.IO;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;

namespace SpamKarmaBase
{
    public class Actions
    {
        public static void RespondWithPicture(int count, string naggerAddress, Victim victim, string subject)
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

        public static string RandomPicture()
        {
            var rand = new Random();
            var files = Directory.GetFiles(".\\PayloadImages\\", "*.jpg");
            return files[rand.Next(files.Length)];
        }

        public static void DeleteMessage(UniqueId messageId, Victim vm)
        {
            using (var emailClient = new ImapClient())
            {
                emailClient.Connect("imap.gmail.com", 993, true);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(vm.Address, vm.Password);
                emailClient.Inbox.Open(FolderAccess.ReadWrite);
                emailClient.Inbox.AddFlags(new[] { messageId }, MessageFlags.Seen, true);
                emailClient.Inbox.AddFlags(new[] { messageId }, MessageFlags.Deleted, true);
                emailClient.Inbox.Expunge();
            }
        }

        public static void CheckMail(BlackList bl, Victim vm)
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
                            Actions.RespondWithPicture(na.RetributionLevel, message.From.ToString(), vm, message.Subject);
                            Actions.DeleteMessage(uid, vm);
                        }
                    }

                    foreach (var nd in bl.NaggerDomains)
                    {
                        var returnFire = message.From.ToString().Contains(nd.Domain);

                        if (returnFire)
                        {
                            Actions.RespondWithPicture(nd.RetributionLevel, message.From.ToString(), vm, message.Subject);
                            Actions.DeleteMessage(uid, vm);
                        }
                    }
                }
            }
        }
    }
}

