using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace Catalog.Application.Common.Exceptions
{
    public class ValidationException : ApplicationException
    {
        public ValidationException()
            : base("One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }

        public Dictionary<string, string[]> Errors { get; }
    }
}
