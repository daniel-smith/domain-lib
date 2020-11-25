using System.Threading.Tasks;

namespace DomainLib.Projections
{
    public interface IStreamLifecycleHandler
    {
        Task OnStarted();
        Task OnCaughtUp();
    }
}