using EmployeeLeaveProcessing.Data;
using EmployeeLeaveProcessing.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Formats.Asn1;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using CsvHelper.Configuration;
using CsvHelper;

namespace EmployeeLeaveProcessing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeDbContext _dbContext;

        public EmployeeController(EmployeeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Empty file");
                }

                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        BadDataFound = null, // Ignore bad data
                        MissingFieldFound = null,
                    };

                    using (var csv = new CsvReader(reader, csvConfig))
                    {
                        csv.Read(); // Read the header row
                        csv.ReadHeader(); // Read header and move to the next row


                        var records = csv.GetRecords<Employee>().ToList();
                        var employees = records.Select(record => new Employee
                        {
                            //Id = int.Parse(record.id),
                            FullName = record.FullName,
                            Email = record.Email,
                            Role = record.Role,
                            LeaveEntitlement = record.LeaveEntitlement,
                            LeaveUsed = record.LeaveUsed,
                            LeaveBalance = record.LeaveBalance
                        }).ToList();

                        // Save records to the database
                        _dbContext.Employees.AddRange(records);
                        await _dbContext.SaveChangesAsync();
                        // Convert CSV records to binary data
                        byte[] binaryData;
                        using (var memoryStream = new MemoryStream())
                        using (var writer = new StreamWriter(memoryStream, Encoding.UTF8)) // Specify encoding
                        {
                            var csvWriter = new CsvWriter(writer, csvConfig);
                            csvWriter.WriteRecords(records);
                            writer.Flush();
                            binaryData = memoryStream.ToArray();
                        }
                        // Process or save records as needed
                        // For demonstration, we are returning the records in the response
                        return Ok(new { Message = "File uploaded successfully", CsvData = records });

                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Exception: {ex.Message}");

                // Log the inner exception details
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("employees")]
        public ActionResult<IEnumerable<Employee>> GetEmployees()
        {
            var employees = _dbContext.Employees
                .Select(e => new
                {
                    e.Id,
                    e.FullName,
                    e.Email,
                    e.Role,
                    e.LeaveEntitlement,
                    e.LeaveUsed,
                    e.LeaveBalance,

                    Leave = e.Leaves.Select(i => new
                    {
                        i.StartDate,
                        i.EndDate,
                    }).ToList()
                })
                .ToList();
            var cleanedEmployees = RemoveIdPropertiesFromCollection(employees);

            var jsonSerializerOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                // Add other serialization options if needed
                IgnoreNullValues = true, // Ignore null values
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Convert property names to camelCase
            };
            if(employees == null)
            {
                return StatusCode(200, new {Message = "Employee Not Found" });
            }

            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)); // Serialize enums as strings in camelCase format

            return new JsonResult(cleanedEmployees, jsonSerializerOptions);
        }

        [HttpGet("employees/{id}")]
        public ActionResult<Employee> GetEmployee(int id)
        {
            var employee = _dbContext.Employees
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    Employee = new
                    {
                        e.FullName,
                        e.Email,
                        e.Role,
                        e.LeaveUsed,
                        e.LeaveEntitlement,
                        e.LeaveBalance
                    },
                    Leaves = e.Leaves
                         .Select(l => new
                         {
                             l.StartDate,
                             l.EndDate
                         })
                         .ToList()

                })
                    .FirstOrDefault();

            if (employee == null)
                return Ok(new { Message = "Employee With Id Not Found" });

            // Removing Id properties from the result
            var cleanedEmployee = RemoveIdProperties(employee);

            var jsonSerializerOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                // Add other serialization options if needed
                IgnoreNullValues = true, // Ignore null values
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Convert property names to camelCase
            };

            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)); // Serialize enums as strings in camelCase format

            return new JsonResult(cleanedEmployee, jsonSerializerOptions);
        }

        [HttpPost("employees")]
        public ActionResult CreateEmployee([FromBody] Employee employee)
        {
            var newEmployee = new Employee
            {
                FullName = employee.FullName,
                Email = employee.Email,
                Role = employee.Role,
                LeaveEntitlement = employee.LeaveEntitlement,
                LeaveUsed = employee.LeaveUsed,
                LeaveBalance = employee.LeaveBalance,
                LastLeaveBalanceUpdate = DateTime.Now
                //JoinDate = DateTime.Today
                // Add other desired fields as needed
            };
            if (string.IsNullOrEmpty(newEmployee.FullName) ||
                string.IsNullOrEmpty(newEmployee.Email) ||
                string.IsNullOrEmpty(newEmployee.Role) ||
                newEmployee.LeaveEntitlement.ToString() == "0" ||
                newEmployee.LeaveBalance.ToString() == "0")
            {
                return BadRequest(new { Message = "Employee details are required." });
            }
            _dbContext.Employees.Add(employee);
            _dbContext.SaveChanges();

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);


        }


        [HttpPut("employees/{id}")]
        public ActionResult UpdateEmployee(int id, [FromBody] Employee employee)
        {
            if (id != employee.Id)
                return BadRequest(new { Message = "Employee details with Id Not Found." });

            _dbContext.Entry(employee).State = EntityState.Modified;

            try
            {
                _dbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_dbContext.Employees.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }


        [HttpDelete("employees/{id}")]
        public IActionResult DeleteEmployee(int id)
        {
            var employee = _dbContext.Employees.FirstOrDefault(e => e.Id == id);

            if (employee == null)
            {
                return NotFound(new { Message = $"Employee with ID {id} not found" });
            }
            _dbContext.Employees.Remove(employee);
            _dbContext.SaveChanges();

            return Ok(new { Message = "Employee deleted successfully", Employee = employee });
        }
        private object RemoveIdPropertiesFromCollection(IEnumerable<object> collection)
        {
            var jsonString = JsonSerializer.Serialize(collection);

            return JsonSerializer.Deserialize<object>(jsonString);
        }
        private object RemoveIdProperties(object obj)
        {
            if (obj == null)
                return null;

            // Serialize the object to JSON
            var jsonString = JsonSerializer.Serialize(obj);

            // Deserialize the JSON back to an object without Id properties
            return JsonSerializer.Deserialize<ExpandoObject>(jsonString);
        }


    }

    }
/*
 public ActionResult<Employee> GetEmployee(int id)
    {
        var employee = _dbContext.Employees.Include(e => e.Leaves).FirstOrDefault(e => e.Id == id);

        if (employee == null)
            return NotFound();

        return new JsonResult(employee, new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            // Add other serialization options if needed
        });
    }
 */