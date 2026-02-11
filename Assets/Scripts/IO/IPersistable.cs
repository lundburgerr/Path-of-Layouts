using System.Threading;
using System.Threading.Tasks;

public interface IPersistable
{
    bool IsDirty { get; }
    Task SaveAsync(CancellationToken token);
    void MarkClean();
    string Name { get; }
}