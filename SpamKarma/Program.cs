using System;
using System.Net;
using System.Net.Mail;
using System.Collections;

namespace SpamKarma
{
    class Program
    {
        
        static void Main(string[] args)
        {
            CheckMail();            
        }

        static void CheckMail()
        {
            ArrayList emailAddresses = new ArrayList();
            ArrayList naggerDomains = new ArrayList();

        }

        static void RespondWithPicture(int count, string naggerAddress, string naggerName)
        {
            for (int i = 0; i < count; i++)
            {
                using (var message = new MailMessage())
                {
                    message.To.Add(new MailAddress(naggerAddress, naggerName));
                    message.From = new MailAddress("aaron.kingsley.clark@gmail.com", "Aaron Kingsley Clark");
                    message.Subject = GetSubject();
                    message.Body = "Body";
                    message.IsBodyHtml = true;
                    message.Attachments.Add(new System.Net.Mail.Attachment(RandomPicture()));

                    using (var client = new SmtpClient("smtp.gmail.com"))
                    {
                        client.Port = 587;
                        client.Credentials = new NetworkCredential("aaron.kingsley.clark@gmail.com", "Il0v3G0@tS3x!!");
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
            string fileName = String.Empty;

            return fileName;
        }


    }
}
