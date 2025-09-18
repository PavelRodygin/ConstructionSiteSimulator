using System;
using Cysharp.Threading.Tasks;

namespace CodeBase.Core.Patterns.Architecture.MVP
{
    public interface IPresenter : IDisposable
    {
        UniTask Enter(object param);
        UniTask Exit();
    }
}