using EmployeeLeaveProcessing.Data;
using EmployeeLeaveProcessing.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveProcessing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Ensures that only authenticated users can access these endpoints
    public class LeaveController : ControllerBase
    {
        private readonly EmployeeDbContext _dbContext;

        private readonly EmailService _emailService;
        public LeaveController(EmployeeDbContext dbContext)
        {
            _dbContext = dbContext;
            string connectionString = "endpoint=https://infinion-comm-service.africa.communication.azure.com/;accesskey=/rTcnY6XnrmgxrzHi7f+jnFiAgPpOT9zdsB3KHj98ZjMHn+Y9O0tre/8Jp00Xg33YJh27ufcgVwqDlpAEiXIhg==";
            _emailService = new EmailService(connectionString);
        }
        private static List<Employee> employees = new List<Employee>();

        [HttpPost("apply")]
        public async Task<IActionResult> ApplyLeave([FromBody] LeaveApplicationRequest request)
        {
            try
            {
                var userEmailClaim = User?.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
                var employee = _dbContext.Employees.Include(e => e.Leaves).FirstOrDefault(e => e.Email == userEmailClaim);

                DateTime startDate = request.StartDate;
                DateTime endtDate = request.EndDate;

                // Assuming you have a method to retrieve employeeId based on email
                if (employee != null)
                {
                    if (endtDate <= startDate)
                    {
                        return Ok(new { EmployeeId = employee.Id, Message = "Please select a Valid Leave Dtate Thanks" });
                    }
                    else
                    {
                        // Verify if the requested leave is within the available leave balance
                        var requestedLeaveDays = (int)(request.EndDate - request.StartDate).TotalDays;
                        if (requestedLeaveDays <= employee.LeaveBalance)
                        {
                            var leave = new EmployeeLeave
                            {
                                EmployeeId = employee.Id,
                                StartDate = request.StartDate,
                                EndDate = request.EndDate,
                                Status = "Approved",

                            };
                            int yearlyEntitlement = 15;
                            employee.Leaves.Add(leave);
                            employee.LeaveBalance -= requestedLeaveDays;
                            employee.LeaveUsed = yearlyEntitlement - employee.LeaveBalance;
                            //Save To Database
                            _dbContext.SaveChanges();
                            //Send Mail 
                            var recipientEmail = userEmailClaim;
                            var emailSendOperation = await _emailService.SendLeaveApprovalEmail(userEmailClaim, employee.FullName, employee.LeaveBalance); ;

                            return Ok(new { EmployeeId = employee.Id, Message = "Leave approved successfully" });
                        }
                        else if (requestedLeaveDays > employee.LeaveBalance)
                        {
                            var leave = new EmployeeLeave
                            {
                                Status = "Reject",
                            };
                            //Send Mail 
                            var recipientEmail = userEmailClaim;
                            var emailSendOperation = await _emailService.SendLeaveRejectedEmail(userEmailClaim, employee.FullName, employee.LeaveBalance);
                            return BadRequest(new { Message = "Insufficient leave balance" });
                        }
                        else
                        {
                            var leave = new EmployeeLeave
                            {
                                Status = "Pending",
                            };
                            //var gmailService = GmailServiceFactory.GetGmailService();
                            // EmailService.CreateAndSendEmailAsync(userEmailClaim, employee.FullName);
                            return BadRequest(new { Message = "Ivalid Leave Requested" });
                        }
                    }

                }
                //SG.d - sD5CDARcWqBYFBuMHhsA.z0cVxrftWmIABF2BD9oCyDrMW81dZsc - Tz9LHCEEP5o
                //d-sD5CDARcWqBYFBuMHhsA
                else
                {
                    return BadRequest(new { Message = "Email claim not found in the token" });
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { Message = "Internal server error", Exception = ex.Message });
            }
        }
        private Employee GetEmployeeByEmail(string email)
        {
            var existingEmployee = employees.FirstOrDefault(e => e.Email == "preferred_username");

            if (existingEmployee == null)
            {
                // If the employee does not exist, create a new one with default leave balance
                var newEmployee = new Employee
                {
                    Email = email,
                    LeaveBalance = 15, // Set your default leave balance here
                };

                employees.Add(newEmployee);
                return newEmployee;
            }

            // Update leave balance annually
            existingEmployee.UpdateLeaveBalance();

            return existingEmployee;
        }

        // Assuming you have a method to retrieve employeeId based on email

        [HttpGet("entitlement")]
        public IActionResult GetYearlyLeaveEntitlement()
        {
            // Retrieve employeeId from the authenticated user
            var employeeClaim = User?.Claims.FirstOrDefault(c => c.Type == "EmployeeId");

            // Implement logic to retrieve yearly leave entitlement based on employeeId
            // ...

            int yearlyEntitlement = 15; // For demonstration

            return Ok(new { YearlyLeaveEntitlement = yearlyEntitlement });
        }

        [HttpGet("remaining")]
        public IActionResult GetRemainingLeaveForYear()
        {
            // Assuming email is the identifier for an employee
            var userEmailClaim = User?.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

            if (userEmailClaim == null)
            {
                // Handle the case where the user email claim is not present
                return BadRequest("User email claim not found");
            }
            var existingEmployee = _dbContext.Employees.FirstOrDefault(e => e.Email == userEmailClaim);

            if (existingEmployee == null)
            {
                return NotFound($"Employee with email {userEmailClaim} not found");
            }

            int remainingLeave = CalculateRemainingLeave(existingEmployee);

            return Ok(new { LeaveBalance = remainingLeave });
        }
        private int CalculateRemainingLeave(Employee employee)
        {
            // Assuming a yearly entitlement of 15
            int yearlyEntitlement = 15;
            int remainingLeave;
            // Placeholder logic, you should replace this with your actual calculation
            if (employee.LeaveBalance == yearlyEntitlement)
            {
                remainingLeave = 15;
            }
            else
            {
                remainingLeave = employee.LeaveBalance;
            }


            return remainingLeave;

        }
        [HttpGet("Used")]
        public IActionResult GetUsedLeaveForYear()
        {
            // Assuming email is the identifier for an employee
            var userEmailClaim = User?.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

            if (userEmailClaim == null)
            {
                // Handle the case where the user email claim is not present
                return BadRequest("User email claim not found");
            }
            var existingEmployee = _dbContext.Employees.FirstOrDefault(e => e.Email == userEmailClaim);

            if (existingEmployee == null)
            {
                return NotFound($"Employee with email {userEmailClaim} not found");
            }

            int remainingLeave = CalculateUsedLeave(existingEmployee);

            return Ok(new { LeaveUsed = remainingLeave });
        }
        private int CalculateUsedLeave(Employee employee)
        {
            // Assuming a yearly entitlement of 20
            int yearlyEntitlement = 15;
            int remainingLeave;
            // Placeholder logic, you should replace this with your actual calculation
            if (employee.LeaveBalance == yearlyEntitlement)
            {
                remainingLeave = 15;
            }
            else
            {
                remainingLeave = yearlyEntitlement - employee.LeaveBalance;
            }


            return remainingLeave;

        }
        /*
          private void SendEmail(GmailService service, string userId, MimeMessage mimeMessage)
        {
            try
            {
                var rawMessage = mimeMessage.ToString();
                var message = new Google.Apis.Gmail.v1.Data.Message { Raw = Base64UrlEncode(rawMessage) };

                service.Users.Messages.Send(message, userId).Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email. Error: {ex.Message}");
            }
        }

        private string Base64UrlEncode(string input)
        {
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
            return base64.Replace('+', '-').Replace('/', '_').Replace("=", "");
        }

         */

    }



}
