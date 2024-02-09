namespace EmployeeLeaveProcessing.Model
{
    public class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int LeaveEntitlement { get; set; }
        public int LeaveUsed { get; set; } = 0;
        public int LeaveBalance { get; set; }
        public DateTime LastLeaveBalanceUpdate { get; set; }
        public List<EmployeeLeave> Leaves { get; set; }

        public void UpdateLeaveBalance()
        {
            // Assuming the leave balance is updated annually
            if (LastLeaveBalanceUpdate.Year < DateTime.Now.Year)
            {
                // Implement your logic to update leave balance
                // For example, set the leave balance to the default yearly entitlement
                LeaveBalance = LeaveEntitlement;

                // Update the last balance update year to the current year
                LastLeaveBalanceUpdate = DateTime.Now;
            }
        }
    }
}