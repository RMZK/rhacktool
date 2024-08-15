using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rhacktool
{
    public interface ICommand
    {
        Task ExecuteAsync();
    }
}
