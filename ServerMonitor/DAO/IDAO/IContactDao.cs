using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.Models.DAO
{
    public interface IContactDao
    {
        int AddContact(Contact contact);        
    }
}
