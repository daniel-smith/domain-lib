using System.Threading.Tasks;

namespace DomainLib.Projections
{
    public delegate Task ProjectEvent(object @event);
}