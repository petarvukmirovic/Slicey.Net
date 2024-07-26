using Slicey.Net.SliceCloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Slicey.Net.Selector
{
    internal class SliceSelector<TRoot, TSlice, TResult> : Selector<TSlice, TResult>, IUpdateableSelector<TRoot>
    {
        private readonly Func<TRoot, TSlice> compiedSelector;
        internal SliceSelector(TRoot initialState, Expression<Func<TRoot, TSlice>> sliceSelector, Expression<Func<TSlice, TResult>> selectorExpression, ICloner cloner) 
            : base(sliceSelector.Compile()(initialState), selectorExpression, cloner)
        {
            compiedSelector = sliceSelector.Compile();
        }

        void IUpdateableSelector<TRoot>.UpdateSelectorResult(TRoot newState)
        {
            var updatableSlice = (IUpdateableSelector<TSlice>)this;
            updatableSlice.UpdateSelectorResult(compiedSelector(newState));
        }
    }
}
