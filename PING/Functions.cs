using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace PING
{
    class Functions
    {
        public bool MailGonder(List<string> EpostaList, string konu, string icerik)
        {
            bool kontrol = true;
            try
            {
                MailMessage ePosta = new MailMessage();
                ePosta.From = new MailAddress("support@ceibateleicu.com");

                if (EpostaList.Count != 0)//Check if TO list is empty
                {
                    foreach (var i in EpostaList)
                    {
                        ePosta.To.Add(i);
                    }
                }
                else {
                    kontrol = false;
                    return kontrol;
                }
                ePosta.Subject = konu;
                ePosta.Body = icerik;

                SmtpClient smtp = new SmtpClient("mail.teknolojim.com", 587);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.EnableSsl = false;
                smtp.Credentials = new System.Net.NetworkCredential("support@ceibateleicu.com", "f135791");

                smtp.Send(ePosta);
            }
            catch (Exception ex)
            {
                kontrol = false;
                Console.WriteLine("HATA!!! MAIL GONDERILEMEDI {0}", ex);
            }
            return kontrol;
        }
        public bool CCMailGonder(List<string> EpostaListCC, string konu, string icerik) {
            bool kontrol = true;
            try
            {
                MailMessage ePosta = new MailMessage();
                ePosta.From = new MailAddress("support@ceibateleicu.com");

                if (EpostaListCC.Count != 0)//Check if TO list is empty
                {
                    foreach (var i in EpostaListCC)
                    {
                        ePosta.CC.Add(i);
                    }
                }
                else
                {
                    kontrol = false;
                    return kontrol;
                }
                ePosta.Subject = konu;
                ePosta.Body = icerik;

                SmtpClient smtp = new SmtpClient("mail.teknolojim.com", 587);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.EnableSsl = false;
                smtp.Credentials = new System.Net.NetworkCredential("support@ceibateleicu.com", "f135791");

                smtp.Send(ePosta);
            }
            catch (Exception ex)
            {
                kontrol = false;
                Console.WriteLine("HATA!!! MAIL GONDERILEMEDI {0}", ex);
            }
            return kontrol;
        }

    }
}
