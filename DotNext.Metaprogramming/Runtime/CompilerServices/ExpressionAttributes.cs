﻿using System.Linq.Expressions;

namespace DotNext.Runtime.CompilerServices
{
    /// <summary>
    /// Represents compiler-generated attributes associated with every expression.
    /// </summary>
    internal class ExpressionAttributes
    {
        private static readonly UserDataSlot<ExpressionAttributes> AttributesSlot = UserDataSlot<ExpressionAttributes>.Allocate();

        /// <summary>
        /// Indicates that expression contains await expression.
        /// </summary>
        internal bool ContainsAwait;

        /// <summary>
        /// Represents state of the expression
        /// </summary>
        internal uint StateId;

        internal void AttachTo(Expression node)
            => node.GetUserData().Set(AttributesSlot, this);

        internal static ExpressionAttributes Get(Expression node)
            => node.GetUserData().Get(AttributesSlot);
    }
}
