using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edit
{
    public interface IFramer
    {
        IEnumerable<T> Read<T>(Stream source) where T : class;
        byte[] Write<T>(T e) where T : class;
    }
}
