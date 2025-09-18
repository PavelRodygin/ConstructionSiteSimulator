using System;

namespace CodeBase.Editor.ModuleCreator.Tasks
{
    [Serializable]
    public abstract class Task
    {
        public bool WaitForCompilation { get; protected set; }
        public abstract void Execute();
    }
}