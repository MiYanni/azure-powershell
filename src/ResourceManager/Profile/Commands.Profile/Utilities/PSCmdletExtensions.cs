using System;
using System.Linq.Expressions;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Profile.Utilities
{
    // ReSharper disable once InconsistentNaming
    public static class PSCmdletExtensions
    {
        // ReSharper disable once InconsistentNaming
        // https://blogs.msdn.microsoft.com/csharpfaq/2010/03/11/how-can-i-get-objects-and-property-values-from-expression-trees/
        public static bool IsParamBound<TPSCmdlet, TProp>(this TPSCmdlet cmdlet, Expression<Func<TPSCmdlet, TProp>> propertySelector) where TPSCmdlet : PSCmdlet
        {
            var propName = ((MemberExpression)propertySelector.Body).Member.Name;
            return cmdlet.MyInvocation.BoundParameters.ContainsKey(propName);
        }
    }
}
