using Fluxor;

namespace Web.States.Counter;

//[FeatureState]
public record CounterState
{
    public int Count { get; init; }
}

public class CounterFeatureState : Feature<CounterState>
{
    //public int Count { get; init; }
    public override string GetName() => nameof(CounterState);
    protected override CounterState GetInitialState() => new CounterState { Count = 0 };
}
