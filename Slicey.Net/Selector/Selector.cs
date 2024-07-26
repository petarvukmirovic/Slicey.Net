using Force.DeepCloner;
using Slicey.Net.SliceCloning;
using System.Linq.Expressions;

namespace Slicey.Net.Selector
{
    /// <summary>
    /// Based on current state value, computes a new value. It gets updated every time the state is updated
    /// and every time computed value is updated <code>SelectorUpdated</code> event is raised.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class Selector<TState, TResult> : IUpdateableSelector<TState>
    {
        internal Selector(TState initialState, Expression<Func<TState, TResult>> selectorExpression, ICloner cloner)
        {
            compiledSelectorExpression = selectorExpression.Compile();
            currentSelectorResult = compiledSelectorExpression(initialState);
            this.cloner = cloner;
        }

        private readonly Func<TState, TResult> compiledSelectorExpression;
        public event EventHandler<TResult>? SelectorUpdated;

        private TResult currentSelectorResult;
        private readonly ICloner cloner;

        public static implicit operator TResult(Selector<TState, TResult> selector) => selector.currentSelectorResult.DeepClone();

        void IUpdateableSelector<TState>.UpdateSelectorResult(TState newState)
        {
            var newResult = compiledSelectorExpression.Invoke(newState);
            if ((newResult == null && currentSelectorResult != null) ||
                (newResult != null && !newResult.Equals(currentSelectorResult)))
            {
                currentSelectorResult = newResult;
                SelectorUpdated?.Invoke(this, cloner.Clone(newResult));
            }
        }
    }
}
