namespace Stupesoft.LazyScalp.Domain.Notification;

public class Notification
{
    public Notification(int id, string instrumentName)
    {
        if (string.IsNullOrEmpty(instrumentName))
            throw new ArgumentException($"'{nameof(instrumentName)}' cannot be null or empty.", nameof(instrumentName));

        Id = id;
        InstrumentName = instrumentName;
    }

    public int Id { get; private set; }
    public string InstrumentName { get; }
    public DateTime? LastHigh { get; private set; }
    public DateTime? LastLow { get; private set; }

    public void SetLastTimeHigh(DateTime time)
    {
        if (LastHigh > time)
            throw new Exception("New time should bigger then old");

        LastHigh = time;
    }

    public void SetLastTimeLow(DateTime time)
    {
        if (LastLow > time)
            throw new Exception("New time should bigger then old");

        LastLow = time;
    }
}

