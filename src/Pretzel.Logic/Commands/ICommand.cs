using System.Threading.Tasks;

namespace Pretzel.Logic.Commands
{
    /// <summary>
    /// Defines a custom command for pretzel. Implementors should be decorated with the <see cref="CommandInfoAttribute"/>
    /// Also see <see cref="Command{T}"/> for easier implementation.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes the specified command with arguments provided defined in <see cref="CommandInfoAttribute.CommandArgumentsType"/>.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns></returns>
        Task<int> Execute(ICommandArguments arguments);
    }

    /// <summary>
    /// Defines a custom command for pretzel. Implementors should be decorated with the <see cref="CommandInfoAttribute"/>
    /// </summary>
    public abstract class Command<T> : ICommand where T : ICommandArguments
    {
        public Task<int> Execute(ICommandArguments arguments)
            => Execute((T)arguments);

        protected abstract Task<int> Execute(T arguments);
    }

}
