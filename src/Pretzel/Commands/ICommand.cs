using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pretzel.Commands
{
    public interface ICommand
    {
        Task Execute();
    }
}
