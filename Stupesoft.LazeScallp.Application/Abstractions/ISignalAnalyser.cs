using Stupesoft.LazeScallp.Application.Servicies;
using Stupesoft.LazyScalp.Domain.Instrument;
using Stupesoft.LazyScalp.Domain.Notification;

namespace Stupesoft.LazeScallp.Application.Abstractions;

public interface ISignalAnalyser
{
    ResultOfSignals Analyze(Instrument instrument, Signal signal, Notification notification);
}

