using System.Threading.Tasks;

namespace Grillbot.Services.Initiable
{
    public interface IInitiable
    {
        void Init();
        Task InitAsync();
    }
}
