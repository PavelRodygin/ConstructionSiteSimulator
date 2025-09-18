using System;
using Cysharp.Threading.Tasks;

namespace CodeBase.Core.Infrastructure.Modules
{
    public interface IModuleController : IDisposable
    {
        UniTask Enter(object param);
        UniTask Execute();
        UniTask Exit();
    }
}