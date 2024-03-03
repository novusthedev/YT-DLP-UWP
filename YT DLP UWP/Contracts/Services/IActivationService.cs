namespace YT_DLP_UWP.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
