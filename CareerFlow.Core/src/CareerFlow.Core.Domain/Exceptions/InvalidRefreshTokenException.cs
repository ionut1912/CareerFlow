using System;
using System.Collections.Generic;
using System.Text;

namespace CareerFlow.Core.Domain.Exceptions;

public class InvalidRefreshTokenException(string message) : Exception(message)
{
}
