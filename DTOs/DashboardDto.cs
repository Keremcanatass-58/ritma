namespace Ritma.Api.DTOs;

public class DashboardDto
{
    public int TodayAppointments { get; set; }
    public int TotalCustomers { get; set; }
    public int ActiveStaff { get; set; }
    public int ActiveServices { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int PendingPaymentCount { get; set; }
    public decimal PendingPaymentAmount { get; set; }
}
