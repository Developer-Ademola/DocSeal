using Azure.Communication.Email;
using Azure;

namespace EmployeeLeaveProcessing.Model
{
    public class EmailService
    {
        private readonly EmailClient _emailClient;

        public EmailService(string connectionString)
        {
            _emailClient = new EmailClient(connectionString);
        }
        public async Task<EmailSendOperation> SendLeaveApprovalEmail(string email, string name, int leavebalance)
        {
            // This code demonstrates how to send email using Azure Communication Services.
            string connectionString = "endpoint=https://communicationemployee.unitedstates.communication.azure.com/;accesskey=0IFImUNOx/9C81WbousurUFA+7nj1o7s6r9k3/NOQcBWEXYTIXQTBhZ3TuAKlCJMiuI7QicGVUx5Af95Hhl5sw==";
            var emailClient = new EmailClient(connectionString);

            var sender = "donotreply@69973990-cc6f-41ec-a72a-d3ecd53c4816.azurecomm.net";
            var recipient = email;
            var subject = "Leave Approved";
            var htmlContent = $"<html><h1>Dear {name} \n Your leave requested has been approved \n Your balance for this year is {leavebalance}day's \n Thanks!</h1></html>";

            try
            {
                var emailSendOperation = await emailClient.SendAsync(
                    wait: WaitUntil.Completed,
                    senderAddress: sender, // The email address of the domain registered with the Communication Services resource
                    recipientAddress: recipient,
                    subject: subject,
                    htmlContent: htmlContent);
                Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

                /// Get the OperationId so that it can be used for tracking the message for troubleshooting
                string operationId = emailSendOperation.Id;
                Console.WriteLine($"Email operation id = {operationId}");
            }
            catch (RequestFailedException ex)
            {
                /// OperationID is contained in the exception message and can be used for troubleshooting purposes
                Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            }
            return null;
        }
        public async Task<EmailSendOperation> SendLeaveRejectedEmail(string email, string name, int leavebalance)
        {
            // This code demonstrates how to send email using Azure Communication Services.
            string connectionString = "endpoint=https://communicationemployee.unitedstates.communication.azure.com/;accesskey=0IFImUNOx/9C81WbousurUFA+7nj1o7s6r9k3/NOQcBWEXYTIXQTBhZ3TuAKlCJMiuI7QicGVUx5Af95Hhl5sw==";
            var emailClient = new EmailClient(connectionString);

            var sender = "donotreply@69973990-cc6f-41ec-a72a-d3ecd53c4816.azurecomm.net";
            var recipient = email;
            var subject = "Leave Rejected";
            var htmlContent = $"<html><h1>Dear {name} \n Your leave has been rejected \n due to Insufficient leave remain for the year \n Your balance for this year is {leavebalance}day's \n Please wait till next year before you can apply \n for another leave!</h1></html>";

            //List<string> emailList = new List<string>
            //{
            //     "rherdeymohlar@gmail.com",
            //     "risiaka@infinion.co",
            //     "ridwanisiaq01@gmail.com"
            //};
            try
            {
                var emailSendOperation = await emailClient.SendAsync(
                    wait: WaitUntil.Completed,
                    senderAddress: sender, // The email address of the domain registered with the Communication Services resource
                    recipientAddress: recipient,
                    subject: subject,
                    htmlContent: htmlContent);
                Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

                /// Get the OperationId so that it can be used for tracking the message for troubleshooting
                string operationId = emailSendOperation.Id;
                Console.WriteLine($"Email operation id = {operationId}");
            }
            catch (RequestFailedException ex)
            {
                /// OperationID is contained in the exception message and can be used for troubleshooting purposes
                Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            }
            return null;
        }
        //sending multiple email
        /* public async Task<EmailSendOperation> SendLeaveRejectedEmail(string name, int leavebalance)
        {
            // This code demonstrates how to send email using Azure Communication Services.
            string connectionString = "endpoint=https://infinion-comm-service.africa.communication.azure.com/;accesskey=/rTcnY6XnrmgxrzHi7f+jnFiAgPpOT9zdsB3KHj98ZjMHn+Y9O0tre/8Jp00Xg33YJh27ufcgVwqDlpAEiXIhg==";
            var emailClient = new EmailClient(connectionString);

            var sender = "donotreply@salenco.co";
             var subject = "Leave Rejected";

            List<string> emailList = new List<string>
            {
                 "rherdeymohlar@gmail.com",
                 "risiaka@infinion.co",
                 "ridwanisiaq01@gmail.com"
            };
            var emailOperations = new List<EmailSendOperation>();

            try
            {
                foreach (var email in emailList)
                {
                    var htmlContent = $"<html><h1>Dear {name} \n Your leave has been rejected \n due to Insufficient" +
                                    $"leave remain for the year \n Your balance for this year is {leavebalance} \n Please wait till next year " +
                                    " before you can apply \n for another leave!</h1></html>";

                    // Send email for each email address
                    EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                     wait: WaitUntil.Completed,
                     senderAddress: sender,
                     recipientAddress: email, // Pass a single email address or a comma-separated string
                     subject: subject,
                     htmlContent: htmlContent);

                    // Add the EmailSendOperation to the list
                    emailOperations.Add(emailSendOperation);
                }

                // Return a response with the email operation details
                return null;
            }
            catch (RequestFailedException ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");

                // Return a 500 Internal Server Error response
                return null;
            }
        }*/

        public async Task<EmailSendOperation> SendLeavePendingEmail(string email, string name)
        {
            // This code demonstrates how to send email using Azure Communication Services.
            string connectionString = "endpoint=https://communicationemployee.unitedstates.communication.azure.com/;accesskey=0IFImUNOx/9C81WbousurUFA+7nj1o7s6r9k3/NOQcBWEXYTIXQTBhZ3TuAKlCJMiuI7QicGVUx5Af95Hhl5sw==";
            var emailClient = new EmailClient(connectionString);

            var sender = "donotreply@69973990-cc6f-41ec-a72a-d3ecd53c4816.azurecomm.net";
            var recipient = email;
            var subject = "Leave Rejected";
            var htmlContent = $"<html><h1>Dear {name} \n Your leave has been rejected \n due to Insufficient leave remain for the year \n Please wait till next year before you can apply \n for another leave!</h1></html>";

            try
            {
                var emailSendOperation = await emailClient.SendAsync(
                    wait: WaitUntil.Completed,
                    senderAddress: sender, // The email address of the domain registered with the Communication Services resource
                    recipientAddress: recipient,
                    subject: subject,
                    htmlContent: htmlContent);
                Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

                /// Get the OperationId so that it can be used for tracking the message for troubleshooting
                string operationId = emailSendOperation.Id;
                Console.WriteLine($"Email operation id = {operationId}");
            }
            catch (RequestFailedException ex)
            {
                /// OperationID is contained in the exception message and can be used for troubleshooting purposes
                Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            }
            return null;
        }
    }
}
