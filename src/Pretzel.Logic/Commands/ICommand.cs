using System.Threading.Tasks;

namespace Pretzel.Logic.Commands
{
    public interface ICommand
    {
        Task<int> Execute(ICommandArguments arguments);
    }

    public abstract class Command<T> : ICommand where T : ICommandArguments
    {
        public Task<int> Execute(ICommandArguments arguments)
            => Execute((T)arguments);

        protected abstract Task<int> Execute(T arguments);
    }

}
