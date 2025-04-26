namespace Project41225.Application.Interfaces;

public interface IInterviewService
{
    Task<string> StartInterviewAsync(string role);
}
