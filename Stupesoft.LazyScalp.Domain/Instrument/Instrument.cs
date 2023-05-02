namespace Stupesoft.LazyScalp.Domain.Instrument;

public class Instrument
{
    public Instrument(int id, string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        Id = id;
        Name = name;
    }

    public int Id { get; private set; }
    public string Name { get; }
    public int HighLevel { get; private set; }
    public int LowLevel { get; private set; }
    public DateTime? HighDetectionTime { get; private set; }
    public DateTime? LowDetectionTime { get; private set; }

    public void SetHighLevel(int value, DateTime time)
    {
        if (value == 0)
        {
            HighDetectionTime = null;
        }
        else if (HighLevel == 0)
        {
            HighDetectionTime = time;
        }

        HighLevel = value;
    }

    public void SetLowLevel(int value, DateTime time)
    {
        if (value == 0)
        {
            LowDetectionTime = null;
        }
        else if (LowLevel == 0)
        {
            LowDetectionTime = time;
        }

        LowLevel = value;
    }
}