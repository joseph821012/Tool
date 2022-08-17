using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic.Interface
{
    interface IDB<T>
    {
        string Error_Msg { get; set; }
        DataTable Get(string column, string where, string order, int start, int count);
        bool Get(string condition, out List<T> model);
        bool Insert(T model);
        bool Update(T model);
        bool Delete(T model);
        void ErrorMsgClear();
    }
}
