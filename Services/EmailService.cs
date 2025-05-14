using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using LMS_backend.Dtos;
using LMS_backend.Services.Interfaces;

namespace LMS_backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _smtpHost = configuration["Email:SmtpHost"] ?? throw new ArgumentNullException("SMTP host configuration is missing");
            _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? throw new ArgumentNullException("SMTP port configuration is missing"));
            _smtpUsername = configuration["Email:Username"] ?? throw new ArgumentNullException("SMTP username configuration is missing");
            _smtpPassword = configuration["Email:Password"] ?? throw new ArgumentNullException("SMTP password configuration is missing");
            _fromEmail = configuration["Email:FromEmail"] ?? throw new ArgumentNullException("From email configuration is missing");
            _fromName = configuration["Email:FromName"] ?? throw new ArgumentNullException("From name configuration is missing");
        }

        public async Task SendOrderConfirmationAsync(OrderResponseDto order)
        {
            var subject = $"Order Confirmation - Claim Code: {order.ClaimCode}";
            var body = GenerateOrderConfirmationEmail(order);
            await SendEmailAsync(order.Email, subject, body);
        }

        public async Task SendOrderCancellationAsync(OrderDto order)
        {
            var subject = "Order Cancellation Notice";
            var body = GenerateOrderCancellationEmail(order);
            await SendEmailAsync(order.User.Email, subject, body);
        }

        public async Task SendOrderReadyForPickupAsync(OrderDto order)
        {
            var subject = "Your Order is Ready for Pickup";
            var body = GenerateOrderReadyForPickupEmail(order);
            await SendEmailAsync(order.User.Email, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetCode)
        {
            var subject = "Password Reset Request";
            var body = GeneratePasswordResetEmail(resetCode);
            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }

        private string GenerateOrderConfirmationEmail(OrderResponseDto order)
        {
            return $@"
                <h2>Order Confirmation</h2>
                <p>Thank you for your order!</p>
                <p><strong>Order Details:</strong></p>
                <ul>
                    <li>Order ID: {order.Id}</li>
                    <li>Claim Code: {order.ClaimCode}</li>
                    <li>Order Date: {order.OrderDate}</li>
                    <li>Subtotal: ${order.SubTotal:F2}</li>
                    <li>Discount: ${order.DiscountAmount:F2}</li>
                    <li>Final Total: ${order.FinalTotal:F2}</li>
                </ul>
                <h3>Items:</h3>
                <ul>
                    {string.Join("", order.Items.Select(item => $@"
                        <li>{item.Book.Title} - Quantity: {item.Quantity} - ${item.PriceAtTime:F2}</li>
                    "))}
                </ul>
                <p><strong>Important:</strong> Please present your membership ID and claim code ({order.ClaimCode}) at the store to collect your order.</p>";
        }

        private string GenerateOrderCancellationEmail(OrderDto order)
        {
            return $@"
                <h2>Order Cancellation Notice</h2>
                <p>Your order has been cancelled.</p>
                <p><strong>Order Details:</strong></p>
                <ul>
                    <li>Order ID: {order.Id}</li>
                    <li>Claim Code: {order.ClaimCode}</li>
                    <li>Cancellation Date: {order.CancellationDate}</li>
                </ul>";
        }

        private string GenerateOrderReadyForPickupEmail(OrderDto order)
        {
            return $@"
                <h2>Your Order is Ready for Pickup</h2>
                <p>Good news! Your order is now ready for pickup at our store.</p>
                <p><strong>Order Details:</strong></p>
                <ul>
                    <li>Order ID: {order.Id}</li>
                    <li>Claim Code: {order.ClaimCode}</li>
                    <li>Order Date: {order.CreatedAt}</li>
                </ul>
                <p><strong>Important:</strong> Please present your membership ID and claim code ({order.ClaimCode}) at the store to collect your order.</p>";
        }

        private string GeneratePasswordResetEmail(string resetCode)
        {
            return $@"
                <h2>Password Reset Request</h2>
                <p>You have requested to reset your password. Use the following code to reset your password:</p>
                <h3 style='color: #007bff; font-size: 24px;'>{resetCode}</h3>
                <p>This code will expire in 15 minutes.</p>
                <p>If you did not request this password reset, please ignore this email.</p>";
        }
    }
}