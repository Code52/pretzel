using System.Threading.Tasks;

namespace Pretzel.Logic.Commands
{
    public interface IPretzelCommand
    {
        Task Execute();
    }
}
