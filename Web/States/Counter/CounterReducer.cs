using Fluxor;

using Web.States.Counter;

namespace Web.States.Counter;

public static class CounterReducer
{
    [ReducerMethod]
    public static CounterState OnIncreaseCounter(CounterState state, IncreaseCounter action) => state with
    {
        Count = state.Count + action.Step
    };
}
