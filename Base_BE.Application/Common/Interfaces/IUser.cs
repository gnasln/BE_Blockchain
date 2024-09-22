using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_BE.Application.Common.Interfaces
{
    public interface IUser
    {
        string? Id { get; }
        string? UserName { get; }
    }
}
