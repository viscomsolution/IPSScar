using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace TGMTcs
{
    class TGMTemail
    {
        MailAddress m_fromEmail;
        SmtpClient m_client;

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public TGMTemail(string fromEmail, string password, string server = "smtp.gmail.com", int port = 587)
        {
            m_fromEmail = new MailAddress(fromEmail);
            m_client = new SmtpClient();
            m_client.Port = port;
            m_client.Host = server;
            m_client.Credentials = new NetworkCredential(fromEmail, password);
            m_client.EnableSsl = true;
            m_client.DeliveryMethod = SmtpDeliveryMethod.Network;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool SendMail(string toEmail, string subject, string content)
        {
            try
            {
                MailAddress mailTo = new MailAddress(toEmail);

                MailMessage mail = new MailMessage(m_fromEmail, mailTo);
                mail.IsBodyHtml = false;


                mail.Subject = subject;
                mail.Body = content;
                mail.SubjectEncoding = Encoding.UTF8;
                mail.BodyEncoding = Encoding.UTF8;
                m_client.Send(mail);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
            
        }
    }
}
