using System.Runtime.InteropServices;
using APIContagem.Kafka;

namespace APIContagem;

public class Contador
{
    private static readonly string _LOCAL;
    private static readonly string _KERNEL;
    private static readonly string _FRAMEWORK;

    static Contador()
    {
        _LOCAL = Environment.MachineName;
        _KERNEL = Environment.OSVersion.VersionString;
        _FRAMEWORK = RuntimeInformation.FrameworkDescription;
    }

    public Contador()
    {
        _partition = KafkaExtensions.QtdPartitions > 1 ? -1 : 0;
    }

    private int _valorAtual = 90_000;
    private int _partition;

    public int ValorAtual { get => _valorAtual; }
    public int Partition { get => _partition; }
    public string Local { get => _LOCAL; }
    public string Kernel { get => _KERNEL; }
    public string Framework { get => _FRAMEWORK; }

    public void Incrementar()
    {
        _valorAtual++;
        
        if (KafkaExtensions.QtdPartitions > 1)
        {
            _partition++;
            if (_partition == KafkaExtensions.QtdPartitions)
                _partition = 0;
        }
    }
}