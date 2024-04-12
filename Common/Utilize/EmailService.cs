using Common.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.Utilize
{
    public class EmailService
    {
        public static ResponseModel EmailBuilder(string email, string to, string subject, string body, string password, string smtpServer, int port)
        {
            var request = new EmailDTO()
            {
                From = email,
                To = to,
                Subject = subject,
                Body = body
            };

            var smtp = new SmtpDTO()
            {
                Email = email,
                Password = password,
                SMTPServer = smtpServer,
                Port = port
            };

            return new ResponseModel { Data = new { request, smtp} };
        }

        public static ResponseModel SendEmail(EmailDTO request, SmtpDTO smtpDTO)
        {
            var response = new ResponseModel() { IsSuccessful = true, Message = "OTP Required" };
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(request.From));
                email.To.Add(MailboxAddress.Parse(request.To));
                email.Subject = request.Subject;
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = request.Body
                };

                using var smtp = new SmtpClient();
                smtp.Connect(smtpDTO.SMTPServer, smtpDTO.Port, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(smtpDTO.Email, smtpDTO.Password);
                smtp.Send(email);
                smtp.Disconnect(true);
            }
            catch(Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public static bool ValidateEmail(string email)
        {
            var regex = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b";
            if (Regex.IsMatch(email, regex, RegexOptions.IgnoreCase))
            {
                return true;
            }
            return false;

        }
    }
}
